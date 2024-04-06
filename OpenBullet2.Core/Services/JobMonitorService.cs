﻿using Newtonsoft.Json;
using RuriLib.Functions.Crypto;
using RuriLib.Models.Jobs.Monitor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace OpenBullet2.Core.Services;

/// <summary>
///     Monitors jobs, checks defined triggers every second and executes the corresponding actions.
/// </summary>
public class JobMonitorService : IDisposable
{
    private readonly string fileName = "UserData/triggeredActions.json";
    private readonly JobManagerService jobManager;

    private readonly JsonSerializerSettings jsonSettings = new() {
        TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented
    };

    private readonly Timer saveTimer;

    private readonly Timer timer;
    private byte[] lastSavedHash = Array.Empty<byte>();

    public JobMonitorService(JobManagerService jobManager)
    {
        this.jobManager = jobManager;
        RestoreTriggeredActions();
        timer = new Timer(_ => CheckAndExecute(), null, 1000, 1000);
        saveTimer = new Timer(_ => SaveStateIfChanged(), null, 5000, 5000);
    }

    /// <summary>
    ///     The list of triggered actions that can be executed by the job monitor.
    /// </summary>
    public List<TriggeredAction> TriggeredActions { get; set; } = new();

    public void Dispose()
    {
        timer?.Dispose();
        saveTimer?.Dispose();
        GC.SuppressFinalize(this);
    }

    private void CheckAndExecute()
    {
        for (var i = 0; i < TriggeredActions.Count; i++)
        {
            var action = TriggeredActions[i];

            if (action.IsActive && !action.IsExecuting && (action.IsRepeatable || action.Executions == 0))
                action.CheckAndExecute(jobManager.Jobs).ConfigureAwait(false);
        }
    }

    private void RestoreTriggeredActions()
    {
        if (!File.Exists(fileName)) return;

        try
        {
            var json = File.ReadAllText(fileName);
            TriggeredActions = JsonConvert.DeserializeObject<TriggeredAction[]>(json, jsonSettings).ToList();
        }
        catch
        {
            Console.WriteLine("Failed to deserialize triggered actions from json, recreating them");
        }
    }

    private void SaveStateIfChanged()
    {
        var json = JsonConvert.SerializeObject(TriggeredActions.ToArray(), jsonSettings);
        var hash = Crypto.MD5(Encoding.UTF8.GetBytes(json));

        if (hash != lastSavedHash)
            try
            {
                File.WriteAllText(fileName, json);
                lastSavedHash = hash;
            }
            catch
            {
                // File probably in use
            }
    }
}