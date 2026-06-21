using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Pikuniku.Achievements;
using UnityEngine;
using Object = System.Object;

namespace PikunikuAPMod;

public class GameHandler : MonoBehaviour
{
    // The scarecrow face is kept fully vanilla: never a check, grant, or AP popup override.
    private const int ScarecrowFaceId = 1358097203;

    private void Awake()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

#if DEBUG
    private static bool _pendingBossWarp;
#endif

    private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene,
        UnityEngine.SceneManagement.LoadSceneMode _)
    {
        if (scene.name == Scenes.Prologue)
            PikunikuAPMod.ClientView?.QueueMessage("Tip: Press F2 to skip the intro, and press F1 to unstuck yourself.");

#if DEBUG
        if (_pendingBossWarp && scene.name == Scenes.MountainVillage)
        {
            _pendingBossWarp = false;
            if (MainStory_Manager.S != null)
            {
                MainStory_Manager.S.CurrentSegment = StorySegment_BeforeSecondBoss;
                Log.Message($"Boss warp: story segment set to {StorySegment_BeforeSecondBoss}");
            }
            PikunikuAPMod.ClientView?.QueueMessage($"[Debug] Warped to Mountain Village (story seg {StorySegment_BeforeSecondBoss}). Walk to the boss trigger to test.");
        }
#endif

        foreach (var buyable in UnityEngine.Object.FindObjectsOfType<BuyableObject>())
            HideBuyableIfChecked(buyable);
    }

    private static void HideBuyableIfChecked(BuyableObject b)
    {
        string locationName;
        if (b.buyType == BuyableObject.BuyType.HAT)
            locationName = b.hat != null ? HatLocationName(b.hat.UniqueID) : null;
        else
            locationName = b.obj != null ? ObjectLocationName(b.obj.UniqueID) : null;

        if (TryGetLocationId(locationName, out long id) && IsChecked(id))
            b.gameObject.SetActive(false);
    }

    private void Start() { }

    private void Update()
    {
        // F7: dump every loaded hat/item/trophy with its in-game ID.
        if (Input.GetKeyDown(KeyCode.F7))
            DumpIds();

        // F1: reload the current area back to its start.
        if (Input.GetKeyDown(KeyCode.F1))
            ForceRestartArea();
        
        if (Input.GetKeyDown((KeyCode.F2)))
            TrySkipCutscene();

#if DEBUG
        // F3–F6, F9–F11: warp to major areas. F8: second-robot story point (Water Hat gate testing).
        if (Input.GetKeyDown(KeyCode.F3))  WarpToScene(Scenes.MountainVillage);
        if (Input.GetKeyDown(KeyCode.F4))  WarpToScene(Scenes.ValleyRoad);
        if (Input.GetKeyDown(KeyCode.F5))  WarpToScene(Scenes.Forest);
        if (Input.GetKeyDown(KeyCode.F6))  WarpToScene(Scenes.Lake);
        if (Input.GetKeyDown(KeyCode.F8))  WarpToSecondBoss();
        if (Input.GetKeyDown(KeyCode.F9))  WarpToScene(Scenes.Mine);
        if (Input.GetKeyDown(KeyCode.F10)) WarpToScene(Scenes.HQ);
        if (Input.GetKeyDown(KeyCode.F11)) WarpToScene(Scenes.Beach);
#endif

        // Apply a received DeathLink on the main thread (flagged from the socket thread).
        if (_pendingDeathLinkKill)
        {
            _pendingDeathLinkKill = false;
            ApplyDeathLinkKill();
        }
    }

    private const int StorySegment_WakeUp = StorySegments.INTRO_WAKEUP;
    private const int StorySegment_WhereAmI = StorySegments.MOUNTAIN_WHEREAREYOU;
#if DEBUG
    private const int StorySegment_BeforeSecondBoss = StorySegments.MOUNTAIN_GETBACKMOUNTAIN;
#endif

    private static void TrySkipCutscene()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (scene == Scenes.Prologue)
        {
            if (MainStory_Manager.S != null) return;
            Log.Message("Skipping prologue cutscene -> Mountain Village");
            if (MainStory_Manager.S != null) MainStory_Manager.S.CurrentSegment = StorySegment_WhereAmI;
            UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.MountainVillage);
            return;
        }
    }

#if DEBUG
    private static void WarpToScene(string sceneName)
    {
        Log.Message($"Debug warp -> {sceneName}");
        PikunikuAPMod.ClientView?.QueueMessage($"[Debug] Warping to {sceneName}...");
        if (TransitionModule.S != null && !TransitionModule.S.isPlaying)
            TransitionModule.S.SceneTransition_Smooth(sceneName, ShouldSave: false);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    private static void WarpToSecondBoss()
    {
        Log.Message("Debug warp -> Mountain Village (second-robot story point)");
        // Set segment now in case MainStory_Manager persists across the load.
        if (MainStory_Manager.S != null)
            MainStory_Manager.S.CurrentSegment = StorySegment_BeforeSecondBoss;
        // Flag so OnSceneLoaded re-applies the segment if MainStory_Manager is recreated.
        _pendingBossWarp = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.MountainVillage);
    }
#endif

    private static void ForceRestartArea()
    {
        if (LevelSpecific.S == null || TransitionModule.S == null || TransitionModule.S.isPlaying)
            return;

        string current = GameManager.CurrentSceneName;
        string target = current == "04_MINE" ? "04.1_LAKE" : current;

        Log.Message($"Force restart: loading '{target}'");
        TransitionModule.S.SceneTransition_Smooth(target, ShouldSave: false);
    }

    private static void DumpIds()
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("===== Pikuniku AP — F7 ID Dump =====");

            var hats = Resources.FindObjectsOfTypeAll<HatSO>()
                .OrderBy(h => h.name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            sb.AppendLine($"--- Hats (HatSO) [{hats.Length}] ---");
            foreach (var h in hats)
                sb.AppendLine($"  {h.UniqueID,-12} {h.name}");

            var objects = Resources.FindObjectsOfTypeAll<Inventory_Object>()
                .OrderBy(o => o.name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            sb.AppendLine($"--- Key Items (Inventory_Object) [{objects.Length}] ---");
            foreach (var o in objects)
                sb.AppendLine($"  {o.UniqueID,-12} {o.name}");

            var trophies = Resources.FindObjectsOfTypeAll<Trophy>()
                .OrderBy(t => t.name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            sb.AppendLine($"--- Trophies (Trophy) [{trophies.Length}] ---");
            foreach (var t in trophies)
                sb.AppendLine($"  {t.achievementID,-16} {t.name}");

            // Coins are per-scene; press F7 in each area to harvest them all.
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var coins = Resources.FindObjectsOfTypeAll<Collectible>()
                .Where(c => c.gameObject.scene.IsValid()) // skip prefab assets, keep scene instances
                .OrderByDescending(c => c.transform.position.y)
                .ThenBy(c => c.transform.position.x)
                .ToArray();
            sb.AppendLine($"--- Coins (Collectible) in scene '{scene}' [{coins.Length}] ---");
            foreach (var c in coins)
                sb.AppendLine("  " + CoinLine(c));

            sb.Append("===== end dump =====");
            Log.Message(sb.ToString());
        }
        catch (Exception ex)
        {
            Log.Error($"F7 ID dump failed: {ex}");
        }
    }

    private static string CoinLine(Collectible c)
    {
        var p = c.transform.position;
        return $"{c.UniqueID} (Loc: {p.x:0.0},{p.y:0.0})";
    }
    
    private static bool _suppressCoinCredit;

    [HarmonyPatch(typeof(Collectible), "OnDone")]
    private class Collectible_OnDone_Patch
    {
        private static void Prefix()
        {
            if (!PikunikuAPMod.SlotData.Coinsanity) return;
            _suppressCoinCredit = true;
        }

        private static void Postfix(Collectible __instance)
        {
            if (!PikunikuAPMod.SlotData.Coinsanity) return;
            _suppressCoinCredit = false;

            string locName = CoinLocationName(__instance);
            if (TryGetLocationId(locName, out long locId))
            {
                Log.Message($"Coinsanity check: '{locName}' ({locId}) [{CoinLine(__instance)}]");
                PikunikuAPMod.ArchipelagoHandler?.CheckLocation(locId);
            }
            else
            {
                Log.Message($"Coin picked up (unmapped): {CoinLine(__instance)}");
            }

            __instance.Collected = false;
        }
    }
    
    [HarmonyPatch(typeof(CoinsManager), "AddCredit")]
    private class CoinsManager_AddCredit_Patch
    {
        private static bool Prefix()
        {
            if (!_suppressCoinCredit) return true;
            _suppressCoinCredit = false; // consume it; let nothing else be suppressed
            return false;  
        }
    }

    // ===== AP reskins: swap world pickups with the embedded AP art =====

    private static Texture2D _coinTex, _presentTex, _presentTopTex;
    private static Material _coinMat, _presentMat, _presentTopMat;
    private static Mesh _coinQuad;

    private static Material MakeUnlitMaterial(Texture2D tex)
        => new Material(Shader.Find("Sprites/Default")) { mainTexture = tex };

    [HarmonyPatch(typeof(Collectible), "Start")]
    private class Collectible_Start_Reskin_Patch
    {
        private static void Postfix(Collectible __instance)
        {
            var mr = __instance.meshRenderer;
            var mf = mr != null ? mr.GetComponent<MeshFilter>() : null;
            if (mf == null) return;

            if (_coinMat == null)
            {
                if (_coinTex == null)
                    _coinTex = LoadEmbeddedTexture("PikunikuAPMod.Assets.pikuniku_coin_ap_logo.png");
                if (_coinTex == null) return;
                _coinMat = MakeUnlitMaterial(_coinTex);
            }
            // Build the quad once, sized to the coin mesh to keep its footprint.
            if (_coinQuad == null && mf.sharedMesh != null)
                _coinQuad = CreateQuadMesh(mf.sharedMesh.bounds.size);
            if (_coinQuad == null) return;

            mf.sharedMesh = _coinQuad;     
            mr.sharedMaterial = _coinMat; 
        }
    }

    [HarmonyPatch(typeof(GiftBox), "SetupTrophy")]
    private class GiftBox_SetupTrophy_Reskin_Patch
    {
        private static void Postfix(GiftBox __instance)
        {
            if (__instance.transform_boxWrapper == null) return;

            var allMeshes = __instance.transform_boxWrapper
                .GetComponentsInChildren<MeshFilter>(true)
                .Where(f => f.sharedMesh != null)
                .ToList();
            if (allMeshes.Count == 0) return;
            
            var boxMeshes = allMeshes
                .OrderByDescending(f => MeshVolume(f.sharedMesh))
                .Take(2)
                .OrderByDescending(f => f.transform.position.y) // upper mesh (lid) first
                .ToList();

            foreach (var f in allMeshes)
            {
                if (boxMeshes.Contains(f)) continue;
                var rr = f.GetComponent<MeshRenderer>();
                if (rr != null) rr.enabled = false;
            }

            if (boxMeshes.Count == 1)
            {
                ApplyPresentSkin(boxMeshes[0], top: false);
                return;
            }
            
            var topF = boxMeshes[0];
            var bottomF = boxMeshes[1];
            var topTex = PresentTexture(top: true);
            var bottomTex = PresentTexture(top: false);
            if (topTex == null || bottomTex == null) return;

            var ts = topF.transform.lossyScale;
            var bs = bottomF.transform.lossyScale;

            // Shared present width (world units) from the lid's footprint.
            float w = topF.sharedMesh.bounds.size.x * Mathf.Abs(ts.x);
            float topH = w * (topTex.height / (float)topTex.width);
            float bottomH = w * (bottomTex.height / (float)bottomTex.width);

            // Quads are in local mesh space, so convert world size back through lossyScale.
            ApplyPresentSkinSized(topF, top: true,
                new Vector3(w / Mathf.Abs(ts.x), topH / Mathf.Abs(ts.y), 0f));
            ApplyPresentSkinSized(bottomF, top: false,
                new Vector3(w / Mathf.Abs(bs.x), bottomH / Mathf.Abs(bs.y), 0f));

            // Slide the base directly under the lid so the cards abut into one present.
            var top = topF.transform.position;
            var bp = bottomF.transform.position;
            bp.x = top.x;
            bp.z = top.z;
            bp.y = top.y - topH * 0.5f - bottomH * 0.5f;
            bottomF.transform.position = bp;
        }
    }

    private static float MeshVolume(Mesh m)
    {
        var s = m.bounds.size;
        return s.x * s.y * s.z;
    }

    // Fallback for a single-mesh box: skin it sized to its own bounds.
    private static void ApplyPresentSkin(MeshFilter mf, bool top)
    {
        if (mf.sharedMesh == null) return;
        ApplyPresentSkinSized(mf, top, mf.sharedMesh.bounds.size);
    }

    // Swap a gift-box mesh for a flat quad skinned with the present's top/bottom art.
    private static void ApplyPresentSkinSized(MeshFilter mf, bool top, Vector3 localSize)
    {
        var mat = PresentMaterial(top);
        if (mat == null) return;
        mf.sharedMesh = CreateQuadMesh(localSize);
        var r = mf.GetComponent<MeshRenderer>();
        if (r != null) r.sharedMaterial = mat;
    }

    private static Texture2D PresentTexture(bool top)
    {
        if (top)
        {
            if (_presentTopTex == null)
                _presentTopTex = LoadEmbeddedTexture("PikunikuAPMod.Assets.pikuniku_present_archipelago_top.png");
            return _presentTopTex;
        }
        if (_presentTex == null)
            _presentTex = LoadEmbeddedTexture("PikunikuAPMod.Assets.pikuniku_present_archipelago.png");
        return _presentTex;
    }

    private static Material PresentMaterial(bool top)
    {
        var tex = PresentTexture(top);
        if (tex == null) return null;
        if (top)
            return _presentTopMat != null ? _presentTopMat : (_presentTopMat = MakeUnlitMaterial(tex));
        return _presentMat != null ? _presentMat : (_presentMat = MakeUnlitMaterial(tex));
    }

    private static Texture2D _apLogoTex;
    private static Material _apLogoMat;

    [HarmonyPatch(typeof(InventoryManager), "SetupAnimObj")]
    private class InventoryManager_SetupAnimObj_Patch
    {
        private static void Postfix(InventoryManager __instance, Inventory_Object obj)
        {
            // Scarecrow face stays vanilla — keep its real popup.
            if (obj != null && obj.UniqueID == ScarecrowFaceId) return;
            ApplyApPopupOverride(__instance);
        }
    }

    [HarmonyPatch(typeof(InventoryManager), "SetupAnimHat")]
    private class InventoryManager_SetupAnimHat_Patch
    {
        private static void Postfix(InventoryManager __instance) => ApplyApPopupOverride(__instance);
    }

    private static void ApplyApPopupOverride(InventoryManager inv)
    {
        // Skip server-granted items — those are genuinely received and stay vanilla.
        if (PikunikuAPMod.ItemHandler != null && PikunikuAPMod.ItemHandler.IsReceivingItem)
            return;

        if (_apLogoMat == null)
        {
            if (_apLogoTex == null)
                _apLogoTex = LoadEmbeddedTexture("PikunikuAPMod.Assets.archipelago_logo.png");
            if (_apLogoTex != null) _apLogoMat = MakeUnlitMaterial(_apLogoTex);
        }

        if (_apLogoMat != null && inv.Anim_meshFilter != null && inv.Anim_meshRenderer != null)
        {
            var src = inv.Anim_meshFilter.sharedMesh;
            if (src != null)
            {
                // Clear rotation so the logo isn't skewed, but keep the scale the game already
                // set on Anim_ObjectRect — it sized the real item correctly and will do the same
                // for our logo quad.
                if (inv.Anim_ObjectRect != null)
                    inv.Anim_ObjectRect.localEulerAngles = Vector3.zero;

                inv.Anim_meshFilter.sharedMesh = CreateQuadMeshForTexture(src.bounds.size, _apLogoTex);
            }
            inv.Anim_meshRenderer.sharedMaterial = _apLogoMat;
        }

        var at = inv.Anim_AutoText;
        if (at != null && at.text != null)
        {
            at.TextID = "";
            at.text.SetText("archipelago item");
        }
    }

    // A flat quad fitted to the footprint while keeping the texture's aspect ratio.
    private static Mesh CreateQuadMeshForTexture(Vector3 footprint, Texture2D tex)
    {
        float w = footprint.x > 0f ? footprint.x : 1f;
        float h = footprint.y > 0f ? footprint.y : 1f;
        if (tex != null && tex.width > 0 && tex.height > 0)
        {
            float texAspect = (float)tex.width / tex.height;
            if (w / h > texAspect) w = h * texAspect;
            else                   h = w / texAspect;
        }
        return CreateQuadMesh(new Vector3(w, h, 0f));
    }

    // A flat quad (UVs 0..1) sized to the given bounds.
    private static Mesh CreateQuadMesh(Vector3 size)
    {
        float w = size.x > 0f ? size.x * 0.5f : 0.5f;
        float h = size.y > 0f ? size.y * 0.5f : 0.5f;
        var mesh = new Mesh
        {
            vertices = new[]
            {
                new Vector3(-w, -h, 0f), new Vector3(w, -h, 0f),
                new Vector3(-w,  h, 0f), new Vector3(w,  h, 0f),
            },
            uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) },
            triangles = new[] { 0, 2, 1, 2, 3, 1 },
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    // Load a PNG embedded in the mod DLL into a Texture2D.
    private static Texture2D LoadEmbeddedTexture(string resourceName)
    {
        try
        {
            var asm = Assembly.GetExecutingAssembly();
            using (Stream stream = asm.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Log.Error($"Embedded texture not found: {resourceName}");
                    return null;
                }

                var data = new byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);

                var tex = new Texture2D(2, 2);
                if (tex.LoadImage(data))
                {
                    // Clamp so edge filtering doesn't bleed a seam from the wrapped opposite edge.
                    tex.wrapMode = TextureWrapMode.Clamp;
                    Log.Info($"Loaded embedded texture {resourceName} ({tex.width}x{tex.height})");
                    return tex;
                }
                Log.Error($"Failed to decode embedded texture: {resourceName}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error loading embedded texture {resourceName}: {ex.Message}");
        }
        return null;
    }

    /// <summary>Called once a successful AP connection is established.</summary>
    public void InitOnConnect() { }

    /// <summary>
    /// Point Pikuniku's save system at a per-seed slot, so each AP seed gets a separate game save.
    /// A new seed maps to an empty slot (starts fresh); reconnecting to a known seed resumes it.
    /// </summary>
    public void SetupSeedSlot(string seed, string apSlot)
    {
        try
        {
            if (Pikuniku.Persistence.SlotManager.S == null)
            {
                Log.Warning("SlotManager not ready; cannot set per-seed save slot");
                return;
            }

            string slotName = MakeSlotName(apSlot, seed);
            Pikuniku.Persistence.Settings.S.CurrentSlotName = slotName;
            Pikuniku.Persistence.SlotManager.S.Slot = new Pikuniku.Persistence.Slot(slotName);
            Log.Info($"Using per-seed game save slot: {slotName}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to set per-seed save slot: {ex}");
        }
    }

    private static string MakeSlotName(string apSlot, string seed)
    {
        var sb = new StringBuilder("AP_");
        AppendSanitized(sb, apSlot);
        sb.Append('_');
        AppendSanitized(sb, seed);
        return sb.ToString();
    }

    private static void AppendSanitized(StringBuilder sb, string s)
    {
        if (string.IsNullOrEmpty(s)) return;
        foreach (char c in s)
            sb.Append(char.IsLetterOrDigit(c) ? c : '_');
    }
    
    // ===== Location lookups =====

    private static bool IsChecked(long locId)
        => PikunikuAPMod.ArchipelagoHandler != null
        && PikunikuAPMod.ArchipelagoHandler.IsLocationChecked(locId);

    private static bool TryGetLocationId(string locationName, out long locId)
    {
        locId = 0;
        return locationName != null && ArchipelagoConstants.Locations.TryGetValue(locationName, out locId);
    }

    private static string HatLocationName(int uniqueId) => uniqueId switch
    {
        1477252700 => "Pencil Hat",
        1042229131 => "Water Hat",
        1070275662 => "Forest Sunglasses Purchase",
        440003900  => "Forest X-Ray Goggles Purchase",
        1370948795 => "Flower Hat",
        1008585758 => "Beast Mask",
        1531897703 => "Some Arms",
        _ => null,
    };

    // Counts how many apple checks the player has sent this session (index into "Apple 1/2/3").
    // Not reset on scene load; synced up from AP's confirmed state lazily in NextAppleLocationName.
    private static int _appleCheckIndex = 0;

    private static string NextAppleLocationName()
    {
        if (TryGetLocationId("Apple 1", out long a1) && IsChecked(a1) && _appleCheckIndex < 1) _appleCheckIndex = 1;
        if (TryGetLocationId("Apple 2", out long a2) && IsChecked(a2) && _appleCheckIndex < 2) _appleCheckIndex = 2;
        if (TryGetLocationId("Apple 3", out long a3) && IsChecked(a3) && _appleCheckIndex < 3) _appleCheckIndex = 3;
        return _appleCheckIndex < 3 ? "Apple " + (_appleCheckIndex + 1) : null;
    }

    private static string ObjectLocationName(int uniqueId) => uniqueId switch
    {
        1632897859 => NextAppleLocationName(),
        132530138  => "A Video Game",
        1533686085 => "Magnetic Card",
        178453624  => "The Cabin Key",
        70335859   => "A Detonator",
        1363662089 => "Golden Tooth from the Silver Frog",
        682565281  => "Valley Plush Purchase",
        471216775  => "Forest Postcard Purchase",
        _ => null,
    };

    private static string TrophyLocationName(string achievementId) => achievementId switch
    {
        "WALKING_PIKU" => "Walking Piku Trophy",
        "HIDDEN_ROCK"  => "The Hidden Rock Trophy",
        "BASKICK"      => "Baskick Champion Trophy",
        "SAM_SLIME"    => "Sam The Slime Trophy",
        "RESISTANCE"   => "The Resistance Trophy",
        "GIANT_ROBOT"  => "A Giant Robot Trophy",
        "GIANT_TOAST"  => "Demonic Toast Trophy",
        "PIKDUG"       => "PikDug Trophy",
        "BEACH"        => "Piku at The Beach Trophy",
        "WORMS"        => "The Worms Trophy",
        "ERNIE"        => "Ernie the Worm Trophy",
        "ROBOT"        => "Sunshine Inc. Robot Trophy",
        "MR_SUNSHINE"  => "Mr. Sunshine Trophy",
        "PIKU_NIKU_1"  => "Piku & Niku I Trophy",
        "PIKU_NIKU_2"  => "Piku & Niku II Trophy",
        "PIKU_NIKU_3"  => "Piku & Niku III Trophy",
        "PIKU_NIKU_4"  => "Piku & Niku IV Trophy",
        "PIKU_NIKU_5"  => "Piku & Niku V Trophy",
        _ => null,
    };

    private static string CoinLocationName(Collectible c)
    {
        var p = c.transform.position;
        return ArchipelagoConstants.ResolveCoinLocation(c.UniqueID, p.x, p.y);
    }

    private static string InsectLocationName(Insect insectId) => insectId switch
    {
        Insect.INSECT_HQ       => "Sunshine HQ Dancing Bug",
        Insect.INSECT_FOREST   => "Forest Dancing Bug",
        Insect.INSECT_MINE     => "Cave Dancing Bug",
        Insect.INSECT_MOUNTAIN => "Valley Dancing Bug",  
        Insect.INSECT_SWAMP    => "Road to Forest Dancing Bug", 
        _ => null,
    };
    
    private static void SendCheck(string locationName)
    {
        if (TryGetLocationId(locationName, out long id))
        {
            Log.Info($"Sending AP Check for {locationName} ({id})");
            PikunikuAPMod.ArchipelagoHandler?.CheckLocation(id);
        }
    }
    
    // Received DeathLink: flag from the socket thread; the kill runs on the main thread in Update.
    private volatile bool _pendingDeathLinkKill;

    public void Kill() => _pendingDeathLinkKill = true;

    // In a boss/Game Over scene, fail the encounter (reload); elsewhere do the respawning spike kill.
    private static void ApplyDeathLinkKill()
    {
        if (LevelSpecific.S == null || LevelSpecific.S.Piku == null)
            return;

        if (IsGameOverScene())
        {
            if (TransitionModule.S != null && !TransitionModule.S.isPlaying)
            {
                Log.Message("DeathLink received — game over (restarting the encounter)");
                TransitionModule.S.SceneTransition_Smooth(GameManager.CurrentSceneName, ShouldSave: false);
            }
            return;
        }

        Log.Message("DeathLink received — spike kill");
        var piku = LevelSpecific.S.Piku;
        piku.Kill(piku.bodyRb.position);
    }

    // A scene has a Game Over screen when one of the boss managers is present.
    private static bool IsGameOverScene()
        => UnityEngine.Object.FindObjectOfType<Forest_BossManager>() != null
        || UnityEngine.Object.FindObjectOfType<Lake_Boss_Manager>() != null
        || UnityEngine.Object.FindObjectOfType<RunnerManager>() != null
        || UnityEngine.Object.FindObjectOfType<HQ_BossManager>() != null;

    // Outgoing DeathLink: only the no-respawn kill (hasRespawnPos:false) is a Game Over, so firing
    // on that case alone sends on Game Overs but not ordinary respawn deaths (which received ones use).
    [HarmonyPatch(typeof(Piku), "_KillCoroutine")]
    private class Piku_KillCoroutine_DeathLink_Patch
    {
        private static void Prefix(bool hasRespawnPos)
        {
            if (hasRespawnPos) return;
            PikunikuAPMod.ArchipelagoHandler?.OnPlayerGameOver();
        }
    } 

    [HarmonyPatch(typeof(Piku), nameof(Piku.Kill), [])]
    private class Piku_Kill_Patch
    {
        private static bool Prefix()
        {
            PikunikuAPMod.ArchipelagoHandler.SendDeath();
            return true;
        }
    }
    
    // When a shop item (hat or object) is bought, _AddHatWithAnimation/_AddObjectWithAnimation checks
    // whether you already own it and returns early if so — skipping Hat_Add/Object_Add entirely, which
    // means the AP check is never sent.  Intercept those coroutine factories when hasBeenBought=true
    // for shop items and call Hat_Add/Object_Add directly so our existing prefixes can fire.
    private static IEnumerator ShopPurchaseNoop() { yield break; }

    [HarmonyPatch(typeof(InventoryManager), "_AddHatWithAnimation")]
    private class InventoryManager_AddHatWithAnimation_Patch
    {
        private static bool Prefix(InventoryManager __instance, HatSO ObjectToAdd, bool hasBeenBought,
            ref IEnumerator __result)
        {
            if (!hasBeenBought) return true;
            if (ObjectToAdd == null || !TryGetLocationId(HatLocationName(ObjectToAdd.UniqueID), out _))
                return true;

            __instance.Hat_Add(ObjectToAdd); // existing Hat_Add prefix sends AP check and returns false
            __result = ShopPurchaseNoop();
            return false;
        }
    }

    [HarmonyPatch(typeof(InventoryManager), "_AddObjectWithAnimation")]
    private class InventoryManager_AddObjectWithAnimation_Patch
    {
        private static bool Prefix(InventoryManager __instance, Inventory_Object ObjectToAdd,
            string WorldName, bool hasBeenBought, ref IEnumerator __result)
        {
            if (!hasBeenBought) return true;
            if (ObjectToAdd == null || !TryGetLocationId(ObjectLocationName(ObjectToAdd.UniqueID), out _))
                return true;

            __instance.Object_Add(ObjectToAdd, WorldName); // existing Object_Add prefix sends AP check and returns false
            __result = ShopPurchaseNoop();
            return false;
        }
    }

    [HarmonyPatch(typeof(InventoryManager), "Hat_Add")]
    private class InventoryManager_Hat_Add_Patch
    {
        private static bool Prefix(HatSO ObjectToAdd)
        {
            if (ObjectToAdd == null) return true;

            // An item we're granting from the server: actually add it, no location check.
            if (PikunikuAPMod.ItemHandler != null && PikunikuAPMod.ItemHandler.IsReceivingItem)
                return true;

            Log.Info($"Caught Hat Receipt: {ObjectToAdd.name} (ID: {ObjectToAdd.UniqueID})");

            string locationName = HatLocationName(ObjectToAdd.UniqueID);
            if (TryGetLocationId(locationName, out long id))
            {
                Log.Info($"Sending AP Check for {locationName} ({id})");
                PikunikuAPMod.ArchipelagoHandler.CheckLocation(id);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Piku), "SetDefaultProperties")]
    private class Piku_SetDefaultProperties_Patch
    {
        private static void Postfix(Piku __instance)
        {
            if (!PikunikuAPMod.ArchipelagoHandler.IsConnected) return;
            var randomColor = Color.HSVToRGB(UnityEngine.Random.value, 0.75f, 0.9f);
            var slotColorData = PikunikuAPMod.SlotData.PikuColor;
        
            if (slotColorData == "0") return; // Off
        
            if (slotColorData == "1") // Random per screen
            {
                __instance.Set_BodyColor(randomColor);
                __instance.default_bodyColor = randomColor;
                return;
            }
        
            // Random per seed or hex code
            if (ColorUtility.TryParseHtmlString(slotColorData, out Color color))
            {
                __instance.Set_BodyColor(color);
                __instance.default_bodyColor = color;
            }
        }
    }

    [HarmonyPatch(typeof(InventoryManager), "Object_Add")]
    private class InventoryManager_Object_Add_Patch
    {
        private static bool Prefix(Inventory_Object ObjectToAdd)
        {
            if (ObjectToAdd == null) return true;

            // Scarecrow face is fully vanilla: add normally, no flag, no check.
            if (ObjectToAdd.UniqueID == ScarecrowFaceId) return true;

            // An item we're granting from the server: actually add it, no location check.
            if (PikunikuAPMod.ItemHandler != null && PikunikuAPMod.ItemHandler.IsReceivingItem)
                return true;

            Log.Info($"Caught Object Receipt: {ObjectToAdd.name} (ID: {ObjectToAdd.UniqueID})");

            string locationName = ObjectLocationName(ObjectToAdd.UniqueID);
            if (TryGetLocationId(locationName, out long id))
            {
                Log.Info($"Sending AP Check for {locationName} ({id})");
                PikunikuAPMod.ArchipelagoHandler.CheckLocation(id);
                if (ObjectToAdd.UniqueID == 1632897859)
                    _appleCheckIndex++;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(TrophiesManager), "CatchTrophy")]
    private class TrophiesManager_CatchTrophy_Patch
    {
        private static bool Prefix(Trophy trophy)
        {
            if (trophy == null) return true;

            // A trophy we're granting from the server: let it through, no location check.
            if (PikunikuAPMod.ItemHandler != null && PikunikuAPMod.ItemHandler.IsReceivingItem)
                return true;

            Log.Info($"Caught Trophy Receipt: {trophy.name} (ID: {trophy.achievementID})");

            string locationName = TrophyLocationName(trophy.achievementID.ToString());
            if (TryGetLocationId(locationName, out long id))
            {
                Log.Info($"Sending AP Check for {locationName} ({id})");
                PikunikuAPMod.ArchipelagoHandler.CheckLocation(id);
            }

            return false;
        }
    }

    // ===== Hide world pickups only once their check is confirmed, not when you own the
    //       item — otherwise an AP-granted item would hide a pickup you still need to catch. =====

    // Don't hide a world hat just because the hat is in your inventory.
    [HarmonyPatch(typeof(HatCatch), "CheckIfAlreadyCollected")]
    private class HatCatch_CheckIfAlreadyCollected_Patch
    {
        private static bool Prefix(ref bool __result) { __result = false; return false; }
    }

    [HarmonyPatch(typeof(HatCatch), "Start")]
    private class HatCatch_Start_Patch
    {
        private static void Postfix(HatCatch __instance)
        {
            if (__instance.hat != null
                && TryGetLocationId(HatLocationName(__instance.hat.UniqueID), out long id)
                && IsChecked(id))
            {
                __instance.DisableHat();
            }
        }
    }

    [HarmonyPatch(typeof(ObjectCatch), "Start")]
    private class ObjectCatch_Start_Patch
    {
        private static void Postfix(ObjectCatch __instance)
        {
            if (__instance.obj == null)
                return;

            // Handle apples: hide only when all 3 checks are done, otherwise keep catchable.
            if (__instance.obj.UniqueID == 1632897859)
            {
                if (NextAppleLocationName() == null)
                    __instance.gameObject.SetActive(false);
                else
                    __instance.Collected = false;
                return;
            }

            // Handle other objects normally
            if (!TryGetLocationId(ObjectLocationName(__instance.obj.UniqueID), out long id))
                return;

            if (IsChecked(id))
                __instance.gameObject.SetActive(false);
            else
                __instance.Collected = false; // stay catchable until the check is confirmed
        }
    }

    // Drive the cave computer-room worm off the location CHECK rather than item ownership: keep
    // offering the cartridge until the check is sent, then fall back to the "already played" dialogue.
    [HarmonyPatch(typeof(Mine_ComputerRoom), "Start")]
    private class Mine_ComputerRoom_Start_Patch
    {
        private static void Postfix(Mine_ComputerRoom __instance)
        {
            if (__instance.worm == null || !TryGetLocationId("A Video Game", out long id))
                return;

            if (IsChecked(id))
            {
                // Already sent - vanilla "you already played it" branch.
                __instance.worm.dialogueModule.customCoroutineDial = null;
                __instance.worm.SetDialogues(Mine_ComputerRoom.dial_checkPlayed);
            }
            else
            {
                // Offer the cartridge again so catching it sends the check.
                __instance.worm.dialogueModule.customCoroutineDial = __instance._GiveCartridge;
            }
            __instance.worm.canTalk = true;
        }
    }

    // Don't hide a gift box just because you own its trophy.
    [HarmonyPatch(typeof(GiftBox), "hasAlreadyBeenCollected")]
    private class GiftBox_hasAlreadyBeenCollected_Patch
    {
        private static bool Prefix(ref bool __result) { __result = false; return false; }
    }

    [HarmonyPatch(typeof(GiftBox), "Start")]
    private class GiftBox_Start_Patch
    {
        private static void Postfix(GiftBox __instance)
        {
            if (__instance.trophy != null
                && TryGetLocationId(TrophyLocationName(__instance.trophy.achievementID.ToString()), out long id)
                && IsChecked(id))
            {
                __instance.gameObject.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(Collectible), "Start")]
    private class Collectible_Start_Hide_Patch
    {
        private static void Postfix(Collectible __instance)
        {
            if (TryGetLocationId(CoinLocationName(__instance), out long id) && IsChecked(id))
                __instance.gameObject.SetActive(false);
        }
    }

    // Disable cave rock-fall blockades so those passages stay open.
    [HarmonyPatch(typeof(Mine_RockFall), "Awake")]
    private class Mine_RockFall_Awake_Disable_Patch
    {
        private static void Postfix(Mine_RockFall __instance) => __instance.gameObject.SetActive(false);
    }

    // Dancing with a hidden insect is an AP check; map the insect to its location and send it.
    [HarmonyPatch(typeof(InsectManager), "OnInsectSeen")]
    private class InsectManager_OnInsectSeen_Patch
    {
        private static void Postfix(Insect insectID)
        {
            string locName = InsectLocationName(insectID);
            if (locName != null)
            {
                Log.Message($"Insect danced: {insectID} -> '{locName}'");
                SendCheck(locName);
            }
            else
            {
                Log.Message($"Insect danced (unmapped): {insectID}");
            }
        }
    }

    // Robot-boss defeat checks, driven off the main-story segment reached on a win (sends are idempotent).
    [HarmonyPatch(typeof(MainStory_Manager), nameof(MainStory_Manager.CurrentSegment), MethodType.Setter)]
    private class MainStory_CurrentSegment_Setter_Patch
    {
        private static void Postfix(int value)
        {
            switch (value)
            {
                case StorySegments.FOREST_CELEBRATEVICTORY: SendCheck("Defeat the First Giant Robot"); break;
                case StorySegments.FOREST_CELEBRATEVICTORYMOUNTAINBOSS: SendCheck("Defeat the Second Giant Robot"); break;
                case StorySegments.HQ_OOPS: SendCheck("Defeat the Third Giant Robot"); break;
            }
        }
    }

    [HarmonyPatch(typeof(HQ_BossManager), "MayorKick")]
    private class HQ_BossManager_MayorKick_Patch
    {
        private static void Prefix()
        {
            if (PikunikuAPMod.ArchipelagoHandler?.IsConnected != true) return;
            Log.Message("Mr. Sunshine defeated - releasing Archipelago goal");
            PikunikuAPMod.ArchipelagoHandler.SetGoal();
        }
    }

    // Drawing a face on the forest tree is an AP check.
    [HarmonyPatch(typeof(PikuPaint), "CloseIt")]
    private class PikuPaint_CloseIt_Patch
    {
        private static void Postfix(Pikuniku.Drawing.DrawingArea drawArea)
        {
            if (drawArea == null || !drawArea.isForest || drawArea.IsEmpty())
                return;
            SendCheck("Draw on the Tree");
        }
    }

    // Create our AP connection panel once the title-screen menu initialises.
    [HarmonyPatch(typeof(UI_PauseMenu), "Launch")]
    private class UI_PauseMenu_Launch_Patch
    {
        private static void Postfix(UI_PauseMenu __instance)
        {
            if (!__instance.isTitleScreen) return;
            var aventurePanel = UnityEngine.Object.FindObjectOfType<UI_MenuPanel_Aventure>();
            if (aventurePanel == null) return;
            APConnectionPanel.CreateFor(__instance, aventurePanel);
        }
    }

    // Disconnect when backing out to title screen root via Back
    [HarmonyPatch(typeof(UI_PauseMenu), "SwitchToPanelFromBack")]
    private class UI_PauseMenu_Back_Disconnect_Patch
    {
        [HarmonyPrefix]
        private static void Prefix(UI_PauseMenu __instance, UI_MenuPanel sourcePanel)
        {
            // Only the title-screen Back disconnects; the in-game pause menu shouldn't.
            if (!__instance.isTitleScreen) return;

            Log.Info("Disconnecting from Archipelago, hit back on title screen");
            PikunikuAPMod.ArchipelagoHandler?.Disconnect();
        }
    }

    // Redirect Adventure navigation to our AP panel when not connected.
    [HarmonyPatch(typeof(UI_PauseMenu), "SwitchToPanel",
        new[] { typeof(UI_MenuPanel), typeof(int) })]
    private class UI_PauseMenu_SwitchToPanel_Patch
    {
        private static void Prefix(UI_PauseMenu __instance,
            ref UI_MenuPanel nextPanel, int direction)
        {
            if (nextPanel is not UI_MenuPanel_Aventure adventure) return;
            if (PikunikuAPMod.ArchipelagoHandler?.IsConnected == true) return;

            // Ensure the connection panel exists.
            var apPanel = APConnectionPanel.Instance;
            if (apPanel == null)
            {
                Log.Info("APConnectionPanel.Instance was null in SwitchToPanel prefix, attempting to create...");
                apPanel = APConnectionPanel.CreateFor(__instance, adventure);
            }

            if (apPanel == null)
            {
                Log.Error("Failed to ensure APConnectionPanel instance");
                return;
            }

            // Record where to return if the user presses Back from our panel.
            apPanel.panelOnBack = __instance.currentPanel;
            apPanel.panelOnBack_position = -direction;

            // Replace the destination — SwitchToPanel slides our panel in instead.
            nextPanel = apPanel;
        }
    }

    [HarmonyPatch(typeof(InventoryManager), "Start")]
    public class InventoryManager_Start_Patch
    {
        public static void Postfix()
        {
            if (PikunikuAPMod.ItemHandler != null)
            {
                Log.Debug("InventoryManager started, flushing item queue...");
                PikunikuAPMod.ItemHandler.FlushQueue();
            }
        }
    }

    // Block the second-robot boss event unless the player has the Water Hat.
    [HarmonyPatch(typeof(Event_MountainVillage_Boss), "EventsCoroutine")]
    private class Event_MountainVillage_Boss_EventsCoroutine_Patch
    {
        private static IEnumerator EmptyCoroutine() { yield break; }

        private static bool Prefix(ref IEnumerator __result)
        {
            var ap = PikunikuAPMod.ArchipelagoHandler;
            if (ap == null || !ap.IsConnected) return true;
            if (ap.HasReceivedItem("Water Hat")) return true;

            Log.Message("Blocked second robot boss: Water Hat not received from AP");
            PikunikuAPMod.ClientView?.QueueMessage("You need the Water Hat to fight the second robot!");
            __result = EmptyCoroutine();
            return false;
        }
    }

    // Block and hide a shop item if its AP location has already been checked.
    [HarmonyPatch(typeof(BuyableObject), "Buy")]
    private class BuyableObject_Buy_Patch
    {
        private static bool Prefix(BuyableObject __instance)
        {
            HideBuyableIfChecked(__instance);
            string locationName;
            if (__instance.buyType == BuyableObject.BuyType.HAT)
                locationName = __instance.hat != null ? HatLocationName(__instance.hat.UniqueID) : null;
            else
                locationName = __instance.obj != null ? ObjectLocationName(__instance.obj.UniqueID) : null;

            return !TryGetLocationId(locationName, out long id) || !IsChecked(id);
        }
    }

    // ===== Fix: shop becomes uninteractable when AP delivers the shop item to you =====
    // BuyableObject registers a CanBeTargetCheck delegate that gates the interaction via
    // Hat_AlreadyInInventory / Object_CheckForTypePossession.  When you own the item (received
    // from another world via AP), those return true → the shop is blocked before Buy() fires.
    // Solution: return false for shop-purchase AP locations so the gate always opens, then rely on
    // BuyableObject_Buy_Patch to block re-purchase once the location is confirmed checked.

    // Protects PutHatOnLoading's Hat_AlreadyInInventory call from the override below, so scene-load
    // hat re-equip still works for shop hats the player already owns.
    private static bool _suppressHatOwnershipCheck;

    [HarmonyPatch(typeof(InventoryManager), "PutHatOnLoading")]
    private class InventoryManager_PutHatOnLoading_Patch
    {
        private static void Prefix() => _suppressHatOwnershipCheck = true;
        private static void Postfix() => _suppressHatOwnershipCheck = false;
    }

    [HarmonyPatch(typeof(InventoryManager), "Hat_AlreadyInInventory")]
    private class InventoryManager_Hat_AlreadyInInventory_Patch
    {
        private static void Postfix(HatSO hatToCheck, ref bool __result)
        {
            if (!__result || _suppressHatOwnershipCheck || hatToCheck == null) return;
            var locName = HatLocationName(hatToCheck.UniqueID);
            if (locName != null && locName.IndexOf("Purchase", StringComparison.Ordinal) >= 0)
                __result = false;
        }
    }

    [HarmonyPatch(typeof(InventoryManager), "Object_CheckForTypePossession")]
    private class InventoryManager_Object_CheckForTypePossession_Patch
    {
        private static void Postfix(Inventory_Object ObjectToCheck, ref bool __result)
        {
            if (!__result || ObjectToCheck == null) return;
            var locName = ObjectLocationName(ObjectToCheck.UniqueID);
            if (locName != null && locName.IndexOf("Purchase", StringComparison.Ordinal) >= 0)
                __result = false;
        }
    }

}
