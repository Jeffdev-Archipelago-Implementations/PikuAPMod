using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Pikuniku.Achievements;
using UnityEngine;

namespace PikunikuAPMod;

// TODO: Define your items here. IDs are relative to ArchipelagoConstants.BaseId
// and must match the apworld's item table.
public enum PikunikuItem
{
    // ProgressiveExample = 0x1,
    // SomeTrap = 0x10,
    // SomeFiller = 0x20,
}

public class ItemHandler : MonoBehaviour
{
    // net35: no ValueTuple, so use KeyValuePair instead of (int, ItemInfo)
    private readonly Queue<KeyValuePair<int, ItemInfo>> cachedItems = new();

    // A resolved item waiting to be applied on the main thread.
    private struct PendingPopup
    {
        public int Index;
        public long InternalId;
        public bool IsHat;
        public bool IsTrophy;
        public Trophies TrophyId;
        public int Credits; // >0 means grant this many coins instead of an asset/trophy
    }

    // Items queued for the in-game popup animation. Filled from ProcessItem (possibly on the
    // socket thread) and drained on the main thread by Update(), where StartCoroutine must run.
    private readonly Queue<PendingPopup> popupQueue = new();
    private readonly object popupLock = new();
    private bool isShowingPopups;

    // True while applying a server-sent item; the location-check patches read this to tell an
    // incoming AP item (grant it) apart from a world pickup (send a check).
    public bool IsReceivingItem { get; set; }

    private bool IsGameReady()
    {
        if (PikunikuAPMod.SaveDataHandler?.SaveData == null)
        {
            Log.Debug("SaveData not ready");
            return false;
        }

        if (PikunikuAPMod.SlotData == null)
        {
            Log.Debug("SlotData not ready");
            return false;
        }

        if (InventoryManager.S == null)
        {
            Log.Debug("InventoryManager not ready");
            return false;
        }

        return true;
    }

    public void HandleItem(int index, ItemInfo item, bool save = true)
    {
        try
        {
            // If game isn't ready, cache the item for later
            if (!IsGameReady())
            {
                Log.Debug($"Game not ready, caching item: {item.ItemName} (index {index})");
                cachedItems.Enqueue(new KeyValuePair<int, ItemInfo>(index, item));
                return;
            }

            // Check if this item was already processed before caching
            if (index < PikunikuAPMod.SaveDataHandler.SaveData.ItemIndex)
            {
                Log.Debug($"Item {index} already processed (current: {PikunikuAPMod.SaveDataHandler.SaveData.ItemIndex}), skipping");
                return;
            }

            // Process any cached items first
            if (cachedItems.Count > 0)
            {
                Log.Message($"Processing {cachedItems.Count} cached items...");
                FlushQueue();
            }

            ProcessItem(index, item, save);
        }
        catch (Exception ex)
        {
            Log.Error($"HandleItem Error: {ex}");
            // Don't rethrow - prevents cascading errors
        }
    }

    public void FlushQueue()
    {
        if (!IsGameReady())
        {
            Log.Warning("Attempted to flush queue but game is not ready");
            return;
        }

        int processedCount = 0;
        while (cachedItems.Count > 0)
        {
            var cached = cachedItems.Dequeue();
            ProcessItem(cached.Key, cached.Value, false);
            processedCount++;
        }

        if (processedCount > 0)
        {
            Log.Message($"Flushed {processedCount} cached items");
            PikunikuAPMod.SaveDataHandler.SaveGame();
        }
    }

    private void ProcessItem(int index, ItemInfo item, bool save = true)
    {
        // Dedup gate (also here, not just HandleItem): FlushQueue replays cached items through
        // ProcessItem without the HandleItem early-out, so already-granted items must skip here too.
        var saveData = PikunikuAPMod.SaveDataHandler?.SaveData;
        if (saveData != null && index < saveData.ItemIndex)
        {
            Log.Debug($"Item {index} already processed (current: {saveData.ItemIndex}), skipping");
            return;
        }

        Log.Message($"Received item: {item.ItemName} (index {index}, ID {item.ItemId})");

        // Trophies are granted through TrophiesManager rather than the inventory popup.
        if (TryGetTrophy(item.ItemName, out Trophies trophyId))
        {
            lock (popupLock)
            {
                popupQueue.Enqueue(new PendingPopup { Index = index, IsTrophy = true, TrophyId = trophyId });
            }
            return;
        }

        // Coin filler items credit the coin counter directly (no inventory popup).
        if (item.ItemName == "5 Coins")
        {
            lock (popupLock)
            {
                popupQueue.Enqueue(new PendingPopup { Index = index, Credits = 5 });
            }
            return;
        }

        // Map the server item to its in-game asset by NAME (authoritative), not numeric ID.
        // Asset UniqueIDs were confirmed via the F7 dump (GameHandler.DumpIds).
        long internalId = 0;
        bool isHat = false;

        switch (item.ItemName)
        {
            case "Pencil Hat":    internalId = 1477252700; isHat = true; break;  // Hat_Pencil
            case "Water Hat":     internalId = 1042229131; isHat = true; break;  // Hat_Water
            case "Sunglasses":    internalId = 1070275662; isHat = true; break;  // Hat_Sunglasses
            case "X-Ray Glasses": internalId = 440003900;  isHat = true; break;  // Hat_XrayGlasses
            case "Flower Hat":    internalId = 1370948795; isHat = true; break;  // Hat_Flower
            case "Beast Mask":    internalId = 1008585758; isHat = true; break;  // Hat_BeastMask
            case "Some Arms":     internalId = 1531897703; isHat = true; break;  // Hat_Arms (it's a hat)
            case "Magnetic Card":                        internalId = 1533686085; break; // Obj_MetroCard
            case "The Cabin Key":                        internalId = 178453624;  break; // Obj_CabinKey
            case "A Detonator":                          internalId = 70335859;   break; // Obj_Detonator
            case "Apple":                                internalId = 1632897859; break; // Obj_Apple
            case "The Golden Tooth from the Silver Frog": internalId = 1363662089; break; // Obj_GoldenTooth
            case "A Video Game":                         internalId = 132530138;  break; // Obj_Cartridge
            case "A Scary Plush":                        internalId = 682565281;  break; // Obj_BeastPlush
            case "Forest Postcard":                      internalId = 471216775;  break; // Obj_Postcard_Forest
        }

        if (internalId == 0)
            Log.Warning($"Unhandled item: {item.ItemName} (ID {item.ItemId})");

        // Queue the pickup so Update() plays the popup and adds the item on the main thread.
        // Unmapped items (internalId 0) still queue, to keep the index advancing in order.
        lock (popupLock)
        {
            popupQueue.Enqueue(new PendingPopup { Index = index, InternalId = internalId, IsHat = isHat });
        }
    }

    private void Update()
    {
        if (isShowingPopups)
            return;

        bool hasWork;
        lock (popupLock)
            hasWork = popupQueue.Count > 0;

        if (hasWork)
            StartCoroutine(DrainPopupQueue());
    }

    private IEnumerator DrainPopupQueue()
    {
        isShowingPopups = true;
        try
        {
            while (true)
            {
                PendingPopup next;
                lock (popupLock)
                {
                    if (popupQueue.Count == 0)
                        break;
                    next = popupQueue.Dequeue();
                }

                yield return ShowPopup(next);
            }
            // Save once after the entire queue is drained, not after each individual item.
            PikunikuAPMod.SaveDataHandler?.SaveGame();
        }
        finally
        {
            isShowingPopups = false;
        }
    }

    private IEnumerator ShowPopup(PendingPopup pending)
    {
        // Trophies are granted through TrophiesManager, not the inventory popup.
        if (pending.IsTrophy)
        {
            GrantTrophy(pending.TrophyId);
            AdvanceIndex(pending.Index);
            yield break;
        }

        // Coins credit the counter directly. Wait until the coins manager exists.
        if (pending.Credits > 0)
        {
            while (InventoryManager.S == null || InventoryManager.S.Coins == null)
                yield return null;
            GrantCoins(pending.Credits);
            AdvanceIndex(pending.Index);
            yield break;
        }

        // Resolve the asset on the main thread.
        HatSO hat = null;
        Inventory_Object obj = null;
        if (pending.InternalId != 0)
        {
            if (pending.IsHat)
            {
                hat = Resources.FindObjectsOfTypeAll<HatSO>().FirstOrDefault(h => h.UniqueID == pending.InternalId);
                if (hat == null) Log.Warning($"Could not find HatSO with UniqueID {pending.InternalId}");
            }
            else
            {
                obj = Resources.FindObjectsOfTypeAll<Inventory_Object>().FirstOrDefault(o => o.UniqueID == pending.InternalId);
                if (obj == null) Log.Warning($"Could not find Inventory_Object with UniqueID {pending.InternalId}");
            }
        }

        if (hat != null || obj != null)
        {
            // Wait until the popup can play: inventory ready, not animating, not open.
            while (InventoryManager.S == null || InventoryManager.S.isAddingObject || InventoryManager.S.isOpen)
                yield return null;

            var inv = InventoryManager.S;

            // Flag this as an incoming item so the Add patches grant it rather than send a check.
            IsReceivingItem = true;
            try
            {
                if (hat != null)
                    yield return inv.StartCoroutine(inv._AddHatWithAnimation(hat));
                else
                    yield return inv.StartCoroutine(inv._AddObjectWithAnimation(obj, ""));
            }
            finally
            {
                IsReceivingItem = false;
            }
        }

        // Advance the processed-item index now that the popup (and add) is complete.
        AdvanceIndex(pending.Index);
    }

    // Advance the processed-item index after an item is applied; mid-apply close replays just that one.
    private void AdvanceIndex(int index)
    {
        try
        {
            var saveData = PikunikuAPMod.SaveDataHandler?.SaveData;
            if (saveData != null && index >= saveData.ItemIndex)
                saveData.ItemIndex = index + 1;
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to advance item index: {ex}");
        }
    }

    // Grant a server trophy. CatchTrophy only reads achievementID, so a throwaway Trophy is enough;
    // IsReceivingItem stops the CatchTrophy patch sending a spurious check back.
    private void GrantTrophy(Trophies trophyId)
    {
        try
        {
            var trophy = ScriptableObject.CreateInstance<Trophy>();
            trophy.achievementID = trophyId;

            IsReceivingItem = true;
            try
            {
                TrophiesManager.S.CatchTrophy(trophy);
            }
            finally
            {
                IsReceivingItem = false;
            }

            Destroy(trophy);
            Log.Message($"Granted trophy: {trophyId}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to grant trophy {trophyId}: {ex}");
        }
    }

    // Credit server coins. AddCredit(fromWorld: true) mirrors a world pickup.
    private void GrantCoins(int amount)
    {
        try
        {
            InventoryManager.S.Coins.AddCredit(amount, fromWorld: true);
            Log.Message($"Granted {amount} coins");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to grant {amount} coins: {ex}");
        }
    }

    // Map a server item name to its in-game trophy. Returns false for non-trophy items.
    private static bool TryGetTrophy(string itemName, out Trophies trophyId)
    {
        switch (itemName)
        {
            case "Walking Piku Trophy":         trophyId = Trophies.WALKING_PIKU; return true;
            case "The Hidden Rock Trophy":      trophyId = Trophies.HIDDEN_ROCK;  return true;
            case "Baskick Champion Trophy":     trophyId = Trophies.BASKICK;      return true;
            case "Sam the Slime Trophy":        trophyId = Trophies.SAM_SLIME;    return true;
            case "The Resistance Trophy":       trophyId = Trophies.RESISTANCE;   return true;
            case "A Giant Robot Trophy":        trophyId = Trophies.GIANT_ROBOT;  return true;
            case "The Demonic Toast Trophy":    trophyId = Trophies.GIANT_TOAST;  return true;
            case "PikDug Trophy":               trophyId = Trophies.PIKDUG;       return true;
            case "Piku at the Beach Trophy":    trophyId = Trophies.BEACH;        return true;
            case "The Worms Trophy":            trophyId = Trophies.WORMS;        return true;
            case "Ernie the Worm Trophy":       trophyId = Trophies.ERNIE;        return true;
            case "Sunshine Inc. Robot Trophy":  trophyId = Trophies.ROBOT;        return true;
            case "Mr. Sunshine Trophy":         trophyId = Trophies.MR_SUNSHINE;  return true;
            case "Piku & Niku I Trophy":   trophyId = Trophies.PIKU_NIKU_1; return true;
            case "Piku & Niku II Trophy":  trophyId = Trophies.PIKU_NIKU_2; return true;
            case "Piku & Niku III Trophy": trophyId = Trophies.PIKU_NIKU_3; return true;
            case "Piku & Niku IV Trophy":  trophyId = Trophies.PIKU_NIKU_4; return true;
            case "Piku & Niku V Trophy":   trophyId = Trophies.PIKU_NIKU_5; return true;
            default: trophyId = default(Trophies); return false;
        }
    }
}
