using Blazored.Modal;
using Blazored.Modal.Services;
using GridBlazor;
using GridBlazor.Pages;
using GridMvc.Server;
using GridShared;
using GridShared.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using OpenBullet2.Auth;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Helpers;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using RuriLib.Models.Data;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Jobs;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Pages;

public partial class Wordlists
{
    private CGrid<WordlistEntity> grid;

    private GridComponent<WordlistEntity> gridComponent;
    private Task gridLoad;
    private WordlistEntity selectedWordlist;
    private int uid = -1;

    private List<WordlistEntity> wordlists = new();
    [Inject] private IModalService Modal { get; set; }
    [Inject] private IWordlistRepository WordlistRepo { get; set; }
    [Inject] private IGuestRepository GuestRepo { get; set; }
    [Inject] private RuriLibSettingsService RuriLibSettings { get; set; }
    [Inject] private JobManagerService JobManager { get; set; }
    [Inject] private JobManagerService Manager { get; set; }
    [Inject] private AuthenticationStateProvider Auth { get; set; }
    [Inject] private VolatileSettingsService VolatileSettings { get; set; }

    protected async override Task OnParametersSetAsync()
    {
        uid = await ((OBAuthenticationStateProvider)Auth).GetCurrentUserId();

        wordlists = uid == 0
            ? await WordlistRepo.GetAll().Include(w => w.Owner).ToListAsync()
            : await WordlistRepo.GetAll().Include(w => w.Owner).Where(w => w.Owner.Id == uid).ToListAsync();

        Action<IGridColumnCollection<WordlistEntity>> columns = c =>
        {
            c.Add(w => w.Name).Titled(Loc["Name"]);
            c.Add(w => w.Type).Titled(Loc["Type"]);
            c.Add(w => w.Purpose).Titled(Loc["Purpose"]);
            c.Add(w => w.Total).Titled(Loc["Lines"]);
            c.Add(w => w.FileName).Titled(Loc["FileName"]);

            if (uid == 0)
                c.Add(w => w.Owner.Username).Titled(Loc["Owner"]);
        };

        var query = new QueryDictionary<StringValues>();
        query.Add("grid-page", "1");

        var client = new GridClient<WordlistEntity>(q => GetGridRows(columns, q), query, false, "wordlistsGrid",
                columns, CultureInfo.CurrentCulture)
            .Sortable()
            .Filterable()
            .WithMultipleFilters()
            .SetKeyboard(true)
            .ChangePageSize(true)
            .WithGridItemsCount()
            .ExtSortable()
            .Selectable(true, false, false);
        grid = client.Grid;

        // Try to set a previous filter
        if (VolatileSettings.GridQueries.ContainsKey((uid, "wordlistsGrid")))
            grid.Query = VolatileSettings.GridQueries[(uid, "wordlistsGrid")];

        // Set new items to grid
        gridLoad = client.UpdateGrid();
        await gridLoad;
    }

    private ItemsDTO<WordlistEntity> GetGridRows(Action<IGridColumnCollection<WordlistEntity>> columns,
        QueryDictionary<StringValues> query)
    {
        VolatileSettings.GridQueries[(uid, "wordlistsGrid")] = query;

        var server = new GridServer<WordlistEntity>(wordlists, new QueryCollection(query),
            true, "wordlistsGrid", columns, 10).Sortable().Filterable().WithMultipleFilters();

        // Return items to displays
        return server.ItemsToDisplay;
    }

    protected void OnWordlistSelected(object item)
    {
        if (item.GetType() == typeof(WordlistEntity)) selectedWordlist = (WordlistEntity)item;
    }

    private async Task RefreshList()
    {
        wordlists = uid == 0
            ? await WordlistRepo.GetAll().ToListAsync()
            : await WordlistRepo.GetAll().Include(w => w.Owner).Where(w => w.Owner.Id == uid).ToListAsync();

        await gridComponent.UpdateGrid();
        StateHasChanged();
    }

    private async Task AddWordlist()
    {
        var modal = Modal.Show<WordlistAdd>(Loc["AddWordlist"]);
        var result = await modal.Result;

        if (!result.Cancelled)
        {
            var entity = result.Data as WordlistEntity;
            entity.Owner = await GuestRepo.Get(uid);
            await WordlistRepo.Add(entity);
            wordlists.Add(entity);
            await js.AlertSuccess(Loc["Added"], Loc["AddedWordlist"]);
        }

        await RefreshList();
    }

    private async Task EditWordlist()
    {
        if (selectedWordlist == null)
        {
            await ShowNoWordlistSelectedWarning();
            return;
        }

        var parameters = new ModalParameters();
        parameters.Add(nameof(WordlistEdit.Wordlist), selectedWordlist);

        var modal = Modal.Show<WordlistEdit>(Loc["EditWordlist"], parameters);
        var result = await modal.Result;

        if (!result.Cancelled)
        {
            await WordlistRepo.Update(selectedWordlist);

            // Update the wordlist in existing jobs
            foreach (var job in JobManager.Jobs.Where(j => j.Status == JobStatus.Idle && j is MultiRunJob)
                         .Cast<MultiRunJob>())
                if (job.DataPool is WordlistDataPool dp && dp.Wordlist.Id == selectedWordlist.Id)
                {
                    var type = RuriLibSettings.Environment.WordlistTypes.First(w => w.Name == selectedWordlist.Type);
                    job.DataPool = new WordlistDataPool(new Wordlist(
                        selectedWordlist.Name, selectedWordlist.FileName, type, selectedWordlist.Purpose, false) {
                        Id = selectedWordlist.Id, Total = selectedWordlist.Total
                    });

                    await JobManager.SaveMultiRunJobOptions(job);
                }
        }

        await RefreshList();
    }

    private async Task DeleteWordlist()
    {
        if (selectedWordlist == null)
        {
            await ShowNoWordlistSelectedWarning();
            return;
        }

        var jobsUsingSelectedWordlist = Manager.Jobs.OfType<MultiRunJob>()
            .Where(j => j.DataPool is WordlistDataPool wl && wl.Wordlist.Id == selectedWordlist.Id);

        if (jobsUsingSelectedWordlist.Any(j => j.Status != JobStatus.Idle))
        {
            await js.AlertError(Loc["Uh-Oh"], Loc["WordlistInUse"]);
            return;
        }

        foreach (var job in jobsUsingSelectedWordlist.Where(j => j.Status == JobStatus.Idle))
            job.DataPool = new InfiniteDataPool();

        if (await js.Confirm(Loc["AreYouSure"], $"{Loc["ReallyDelete"]} {selectedWordlist.Name}?", Loc["Cancel"]))
        {
            // Only prompt this for uploaded lists
            var deleteFile = selectedWordlist.FileName.StartsWith("UserData/Wordlists/") && await js.Confirm(
                Loc["AlsoDeleteFile"],
                $"{Loc["DeleteFileText1"]} {selectedWordlist.FileName} {Loc["DeleteFileText2"]}", Loc["KeepFile"]);

            // Delete the wordlist from the DB and disk
            await WordlistRepo.Delete(selectedWordlist, deleteFile);
        }

        await RefreshList();
    }

    private async Task DeleteNotFound()
    {
        var toDelete = wordlists.Where(w => !File.Exists(w.FileName)).ToList();

        foreach (var wordlist in toDelete)
            // This can throw but it's not an essential operation so we can safely ignore exceptions
            try
            {
                var jobsUsingSelectedWordlist = Manager.Jobs.OfType<MultiRunJob>().Where(j =>
                    j.DataPool is WordlistDataPool wl && wl.Wordlist.Id == selectedWordlist.Id);

                if (jobsUsingSelectedWordlist.Any(j => j.Status != JobStatus.Idle))
                {
                    await js.AlertError(Loc["Uh-Oh"], Loc["WordlistInUse"]);
                    return;
                }

                foreach (var job in jobsUsingSelectedWordlist.Where(j => j.Status == JobStatus.Idle))
                    job.DataPool = new InfiniteDataPool();
            }
            catch
            {
            }

        foreach (var wordlist in toDelete) await WordlistRepo.Delete(wordlist);

        await RefreshList();

        await js.AlertSuccess(Loc["Deleted"], $"{Loc["DeletedWordlists"]}: {toDelete.Count}");
    }

    private async Task ShowNoWordlistSelectedWarning()
        => await js.AlertError(Loc["Uh-Oh"], Loc["NoWordlistSelected"]);
}