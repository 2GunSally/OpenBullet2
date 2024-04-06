﻿using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Repositories;
using RuriLib.Functions.Conversion;
using RuriLib.Helpers;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Services;

// TODO: The config service should also be in charge of calling methods of the IConfigRepository
/// <summary>
///     Manages the list of available configs.
/// </summary>
public class ConfigService
{
    private readonly IConfigRepository configRepo;
    private readonly OpenBulletSettingsService openBulletSettingsService;

    private Config selectedConfig;

    public ConfigService(IConfigRepository configRepo, OpenBulletSettingsService openBulletSettingsService)
    {
        this.configRepo = configRepo;
        this.openBulletSettingsService = openBulletSettingsService;
    }

    /// <summary>
    ///     The list of available configs.
    /// </summary>
    public List<Config> Configs { get; set; } = new();

    /// <summary>
    ///     The currently selected config.
    /// </summary>
    public Config SelectedConfig
    {
        get => selectedConfig;
        set
        {
            selectedConfig = value;
            OnConfigSelected?.Invoke(this, selectedConfig);
        }
    }

    /// <summary>
    ///     Called when a new config is selected.
    /// </summary>
    public event EventHandler<Config> OnConfigSelected;

    /// <summary>
    ///     Called when all configs from configured remote endpoints are loaded.
    /// </summary>
    public event EventHandler OnRemotesLoaded;

    /// <summary>
    ///     Reloads all configs from the <see cref="IConfigRepository" /> and remote endpoints.
    /// </summary>
    public async Task ReloadConfigs()
    {
        // Load from the main repository
        Configs = (await configRepo.GetAll()).ToList();
        SelectedConfig = null;

        // Load from remotes (fire and forget)
        LoadFromRemotes();
    }

    private async void LoadFromRemotes()
    {
        List<Config> remoteConfigs = new();

        var func = new Func<RemoteConfigsEndpoint, Task>(async endpoint =>
        {
            try
            {
                // Get the file
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("Api-Key", endpoint.ApiKey);
                using var response = await client.GetAsync(endpoint.Url);

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException();

                if (response.StatusCode == HttpStatusCode.NotFound) throw new FileNotFoundException();

                var fileStream = await response.Content.ReadAsStreamAsync();

                // Unpack the archive in memory
                using ZipArchive archive = new(fileStream, ZipArchiveMode.Read);
                foreach (var entry in archive.Entries)
                {
                    if (!entry.Name.EndsWith(".opk")) continue;

                    try
                    {
                        using var entryStream = entry.Open();
                        var config = await ConfigPacker.Unpack(entryStream);

                        // Calculate the hash of the metadata of the remote config to use as id.
                        // This is done to have a consistent id through successive pulls of configs
                        // from remotes, so that jobs can reference the id and retrieve the correct one
                        config.Id = HexConverter.ToHexString(config.Metadata.GetUniqueHash());
                        config.IsRemote = true;

                        // If a config with the same hash is not already present (e.g. same exact config
                        // from another source) add it to the list
                        if (!remoteConfigs.Any(c => c.Id == config.Id)) remoteConfigs.Add(config);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{endpoint.Url}] Failed to pull configs from endpoint: {ex.Message}");
            }
        });

        var tasks = openBulletSettingsService.Settings.RemoteSettings.ConfigsEndpoints
            .Select(endpoint => func.Invoke(endpoint));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        lock (Configs) Configs.AddRange(remoteConfigs);

        OnRemotesLoaded?.Invoke(this, EventArgs.Empty);
    }
}