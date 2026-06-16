using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace PikunikuAPMod
{
    // How DeathLink is decided: forced On, forced Off, or follow the slot's yaml option.
    public enum DeathLinkMode { On, Off, YamlSetting }

    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class PikunikuAPMod : BaseUnityPlugin
    {
        private const string PluginGuid = "PikunikuAPMod";
        private const string PluginName = "Pikuniku Archipelago Mod";
        private const string PluginVersion = "0.2.0";
        private const string PluginAuthor = "Jeffdev";
        public static string PluginDir;
        private readonly Harmony harmony = new(PluginGuid);
        public static ConfigEntry<bool> FilterLog;
        public static ConfigEntry<DeathLinkMode> DeathLink;
        public static SlotData SlotData;
        public static ArchipelagoHandler ArchipelagoHandler { get; private set; }
        public static GameHandler GameHandler { get; private set; }
        public static ItemHandler ItemHandler { get; private set; }
        public static ClientView ClientView { get; private set; }
        public static SaveDataHandler SaveDataHandler { get; private set; }

        // Caches assemblies we force-load from our plugin folder so the resolver
        // returns the same instance every time it's asked.
        private static readonly Dictionary<string, Assembly> _resolvedDeps = new();

        public void Awake()
        {
            PluginDir = Path.GetDirectoryName(Info.Location);
            Log.Init(Logger);
            
            SetupDependencyResolver();

            Application.runInBackground = true;
            harmony.PatchAll();
            
            gameObject.AddComponent<FileWriter>();
            ArchipelagoHandler = gameObject.AddComponent<ArchipelagoHandler>();
            GameHandler = gameObject.AddComponent<GameHandler>();
            ItemHandler = gameObject.AddComponent<ItemHandler>();
            // ClientView subscribes to ArchipelagoHandler events in its Awake, so add it last.
            ClientView = gameObject.AddComponent<ClientView>();
            SaveDataHandler = new SaveDataHandler();

            ArchipelagoHandler.OnConnected += () =>
            {
                Log.Message("Connected to Archipelago - loading items");
            };

            ArchipelagoHandler.OnDisconnected += () =>
            {
                Log.Message("Disconnected from Archipelago");
            };

            FilterLog = Config.Bind(
                "Logging",
                "FilterLog",
                false,
                "Filter the archipelago log to only show messages relevant to you."
            );

            DeathLink = Config.Bind(
                "Archipelago",
                "DeathLink",
                DeathLinkMode.YamlSetting,
                "DeathLink (share deaths with the multiworld): On = always on, Off = always off, "
                + "YamlSetting = follow the slot's yaml option."
            );
        }

        // Names we always want to satisfy from our own plugin folder rather than
        // letting the runtime bind them to an older copy already loaded by the game.
        private static readonly string[] ForcedDeps =
        {
            "Newtonsoft.Json",
            "websocket-sharp",
            "Archipelago.MultiClient.Net",
        };

        private void SetupDependencyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveBundledDependency;

            // Proactively load our versions so the correct ones are present in the
            // domain when MultiClient.Net's strongly-versioned references resolve.
            foreach (var dep in ForcedDeps)
                TryLoadFromPluginDir(dep);
        }

        private static Assembly ResolveBundledDependency(object sender, ResolveEventArgs args)
        {
            // args.Name is a full display name, e.g. "Newtonsoft.Json, Version=11.0.0.0, ..."
            string simpleName = new AssemblyName(args.Name).Name;

            foreach (var dep in ForcedDeps)
            {
                if (string.Equals(simpleName, dep, StringComparison.OrdinalIgnoreCase))
                    return TryLoadFromPluginDir(dep);
            }
            return null;
        }

        private static Assembly TryLoadFromPluginDir(string simpleName)
        {
            if (_resolvedDeps.TryGetValue(simpleName, out var cached))
                return cached;

            try
            {
                string path = Path.Combine(PluginDir, simpleName + ".dll");
                if (!File.Exists(path))
                    return null;

                var asm = Assembly.LoadFrom(path);
                _resolvedDeps[simpleName] = asm;
                Log.Info($"Loaded bundled dependency {simpleName} v{asm.GetName().Version} from plugin folder");
                return asm;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load bundled dependency {simpleName}: {ex.Message}");
                return null;
            }
        }

        public void OnDestroy()
        {
            ArchipelagoHandler?.Disconnect();
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveBundledDependency;
            harmony?.UnpatchSelf();
        }
    }
}
