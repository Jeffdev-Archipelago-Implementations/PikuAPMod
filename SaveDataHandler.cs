using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PikunikuAPMod;

public class CustomSaveData
{
    public int ItemIndex;
    // TODO: Add fields for everything the randomizer needs to persist
    // between sessions (received upgrades, checked flags, etc.)
    public List<long> HintedLocationIds = new();
}

public class SaveDataHandler
{
    private string folderName;
    private string fileName;
    public CustomSaveData SaveData;

    public void GetSaveGame(string seed, string slot)
    {
        if (SaveData != null)
            return;
        PikunikuAPMod.ArchipelagoHandler.OnDisconnected += () =>
        {
            SaveGame();
            SaveData = null;
        };
        folderName = Application.persistentDataPath + "/ArchipelagoSaves";
        fileName = folderName + $"/{slot}{seed}.json";
        if (File.Exists(fileName))
            LoadGame();
        else
            CreateNewGame();

        PikunikuAPMod.ItemHandler.FlushQueue();
    }

    private void LoadGame()
    {
        Log.Debug("Loading game...");
        try
        {
            using FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new StreamReader(fs);
            var json = reader.ReadToEnd();
            SaveData = JsonUtility.FromJson<CustomSaveData>(json);
        }
        catch (Exception ex)
        {
            Log.Debug($"Save file is corrupted or locked: {ex.Message}. Starting new game...");
            CreateNewGame();
            return;
        }
        Log.Debug("Game loaded!");
    }

    private void CreateNewGame()
    {
        Log.Debug("Creating new game...");
        SaveData = new CustomSaveData
        {
            ItemIndex = 0
        };
        // TODO: Initialize starting state from SlotData here.
        SaveGame();
    }

    public void SaveGame()
    {
        if (SaveData == null)
            return;
        Log.Debug("Saving game...");
        Directory.CreateDirectory(folderName);
        using var text = File.CreateText(fileName);
        text.Write(JsonUtility.ToJson(SaveData));
        text.Close();
    }
}
