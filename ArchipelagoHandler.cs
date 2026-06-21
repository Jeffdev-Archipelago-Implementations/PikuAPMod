using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Colors;
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace PikunikuAPMod
{
    public class ArchipelagoHandler : MonoBehaviour
    {
        private ArchipelagoSession Session { get; set; }
        private string Server { get; set; }
        private int Port { get; set; }
        private string Slot { get; set; }
        private string Password { get; set; }
        private string seed;
        public bool IsConnected => Session != null && Session.Socket.Connected;
        public event Action OnConnected;
        public event Action<string> OnConnectionFailed;
        public event Action OnDisconnected;
        // Raised for every Archipelago log line (may fire on the socket thread).
        public event Action<string> OnLogMessage;
        // net35: no ConcurrentQueue, so use a plain Queue guarded by a lock
        private readonly Queue<long> locationsToCheck = new();
        private readonly object queueLock = new();
        private string lastDeath;
        private DateTime lastDeathLinkTime = DateTime.Now;
        private readonly Random random = new();

        private readonly string[] deathMessages =
        [
            "got kicked a little too hard.",
            "should not have trusted Mr. Sunshine.",
            "drank too much green liquid.",
            "got FREE MONEY!!!",
            "got chosen to go to the volcano.",
            "got kicked out of el bunko."
        ];

        private static string GetColorHex(PaletteColor? color)
        {
            return color switch
            {
                PaletteColor.Red => "#EE0000",
                PaletteColor.Green => "#00FF7F",
                PaletteColor.Yellow => "#FAFAD2",
                PaletteColor.Blue => "#6495ED",
                PaletteColor.Magenta => "#EE00EE",
                PaletteColor.Cyan => "#00EEEE",
                PaletteColor.Black => "#000000",
                PaletteColor.White => "#FFFFFF",
                PaletteColor.SlateBlue => "#6D8BE8",
                PaletteColor.Salmon => "#FA8072",
                PaletteColor.Plum => "#AF99EF",
                _ => "#FFFFFF" // Default to white
            };
        }

        public void CreateSession(string server, int port, string slot, string password)
        {
            Server = server;
            Port = port;
            Slot = slot;
            Password = password;
            Session = ArchipelagoSessionFactory.CreateSession(Server, Port);
            Session.MessageLog.OnMessageReceived += OnMessageReceived;
            Session.Socket.ErrorReceived += OnError;
            Session.Socket.SocketClosed += OnSocketClosed;
            Session.Socket.PacketReceived += PacketReceived;
            Session.Items.ItemReceived += ItemReceived;
        }

        private void OnDestroy()
        {
            if (Session == null)
                return;
            Session.MessageLog.OnMessageReceived -= OnMessageReceived;
            Session.Socket.ErrorReceived -= OnError;
            Session.Socket.SocketClosed -= OnSocketClosed;
            Session.Socket.PacketReceived -= PacketReceived;
            Session.Items.ItemReceived -= ItemReceived;
        }

        public void Connect()
        {
            Log.Message($"Logging in to {Server}:{Port} as {Slot}...");

            // net35 build of Archipelago.MultiClient.Net has no ConnectAsync/LoginAsync;
            // TryConnectAndLogin is the synchronous equivalent available on all targets.
            var result = Session.TryConnectAndLogin(
                ArchipelagoConstants.GameName,
                Slot,
                ItemsHandlingFlags.AllItems,
                new Version(0, 6, 7),
                [],
                null,
                Password
            );

            if (result.Successful)
            {
                Log.Message($"Success! Connected to {Server}:{Port}");
                var successResult = (LoginSuccessful)result;
                PikunikuAPMod.SlotData = new SlotData(successResult.SlotData);
                Log.Info(successResult.SlotData);
                Log.Info(PikunikuAPMod.SlotData.PikuColor);
                Log.Info(PikunikuAPMod.SlotData.DeathLinkAmnesty);
                Log.Info(PikunikuAPMod.SlotData.DeathLink);
                Log.Info(PikunikuAPMod.SlotData.Coinsanity);
                Log.Info(PikunikuAPMod.SlotData.CoopLevels);

                // SlotData is now set, so DeathLinkEnabled can resolve YamlSetting — push the tag.
                ApplyDeathLinkTag();

                seed = Session.RoomState.Seed;
                if (seed != null)
                {
                    PikunikuAPMod.SaveDataHandler!.GetSaveGame(seed, Slot);
                    // Give this seed its own Pikuniku save slot before the UI shows
                    // Continue/New Game (OnConnected switches to the adventure panel).
                    PikunikuAPMod.GameHandler.SetupSeedSlot(seed, Slot);
                }

                PikunikuAPMod.GameHandler.InitOnConnect();
                StartCoroutine(RunCheckQueue());
                OnConnected?.Invoke();
                return;
            }

            var failure = (LoginFailure)result;
            var errorMessage = $"Failed to Connect to {Server}:{Port} as {Slot}:";
            errorMessage = failure.Errors.Aggregate(errorMessage, (current, error) => current + $"\n    {error}");
            errorMessage = failure.ErrorCodes.Aggregate(errorMessage, (current, error) => current + $"\n    {error}");
            OnConnectionFailed?.Invoke(errorMessage);
            Log.Error(errorMessage);
        }

        public void Disconnect()
        {
            if (Session == null)
                return;
            StopAllCoroutines();
            Session.Socket.Disconnect();
            Session = null;
            Log.Message("Disconnected from Archipelago");
        }

        private void OnError(Exception ex, string message)
        {
            Log.Error($"Socket error: {message} - {ex.Message}");
        }

        private void OnSocketClosed(string reason)
        {
            StopAllCoroutines();
            Log.Warning($"Socket closed: {reason}");
            OnDisconnected?.Invoke();
        }

        private void ItemReceived(ReceivedItemsHelper helper)
        {
            try
            {
                var currentItemIndex = PikunikuAPMod.SaveDataHandler?.SaveData?.ItemIndex ?? 0;

                while (helper.Any())
                {
                    var item = helper.DequeueItem();
                    var itemIndex = helper.Index - 1;

                    if (itemIndex >= currentItemIndex)
                        PikunikuAPMod.ItemHandler.HandleItem(itemIndex, item);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"ItemReceived Error: {ex}");
                throw;
            }
        }

        public void SetGoal()
        {
            Session.SetGoalAchieved();
            Session.SetClientState(ArchipelagoClientState.ClientGoal);
        }

        public void CheckLocations(long[] ids)
        {
            lock (queueLock)
            {
                foreach (var id in ids)
                    locationsToCheck.Enqueue(id);
            }
        }

        public void CheckLocation(long id)
        {
            if (IsLocationChecked(id))
                return;
            lock (queueLock)
            {
                locationsToCheck.Enqueue(id);
            }
        }

        private IEnumerator RunCheckQueue()
        {
            while (true)
            {
                long locationId = 0;
                var hasLocation = false;
                lock (queueLock)
                {
                    if (locationsToCheck.Count > 0)
                    {
                        locationId = locationsToCheck.Dequeue();
                        hasLocation = true;
                    }
                }

                if (hasLocation)
                {
                    Session.Locations.CompleteLocationChecks(locationId);
                    Log.Message($"Sent location check: {locationId}");
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        public bool HasReceivedItem(string itemName)
        {
            if (Session == null || Session.Items == null) return false;
            return Session.Items.AllItemsReceived.Any(item => item.ItemName == itemName);
        }

        public bool IsLocationChecked(long id)
        {
            if (Session == null || Session.Locations == null)
                return false;
            return Session.Locations.AllLocationsChecked.Contains(id);
        }

        public int CountLocationsCheckedInRange(long start, long end)
        {
            if (Session == null || Session.Locations == null)
                return 0;
            return Session.Locations.AllLocationsChecked.Count(loc => loc >= start && loc < end);
        }

        public void UpdateTags(List<string> tags)
        {
            if (Session == null)
                return;
            var packet = new ConnectUpdatePacket
            {
                Tags = tags.ToArray(),
                ItemsHandling = ItemsHandlingFlags.AllItems
            };
            Session.Socket.SendPacket(packet);
        }

        // DeathLink active state: On/Off force it; YamlSetting follows the slot's yaml option.
        public bool DeathLinkEnabled =>
            PikunikuAPMod.DeathLink?.Value switch
            {
                DeathLinkMode.On  => true,
                DeathLinkMode.Off => false,
                _                 => PikunikuAPMod.SlotData?.DeathLink ?? false, // YamlSetting
            };

        // Push the DeathLink tag matching the resolved state. Called on connect once SlotData exists.
        public void ApplyDeathLinkTag()
        {
            UpdateTags(DeathLinkEnabled ? new List<string> { "DeathLink" } : new List<string>());
        }

        private void OnMessageReceived(LogMessage message)
        {
            string messageStr;
            if (message.Parts.Any(x => x.Type == MessagePartType.Player) &&
                PikunikuAPMod.FilterLog != null &&
                PikunikuAPMod.FilterLog.Value &&
                !message.Parts.Any(x => x.Text.Contains(Session!.Players.GetPlayerName(Session.ConnectionInfo.Slot))))
                return;
            if (message.Parts.Length == 1)
            {
                messageStr = message.Parts[0].Text;
            }
            else
            {
                var builder = new StringBuilder();
                foreach (var part in message.Parts)
                {
                    string hexColor = GetColorHex(part.PaletteColor);
                    builder.Append($"<color={hexColor}>{part.Text}</color>");
                }
                messageStr = builder.ToString();
            }
            AddMessageToGameLog(messageStr);
        }

        public void AddMessageToGameLog(string message)
        {
            Log.Message(message);
            OnLogMessage?.Invoke(message);
        }

        private void PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case BouncePacket bouncePacket:
                    BouncePacketReceived(bouncePacket);
                    break;
            }
        }

        // Player's own Game Overs since the last send, for honouring the slot's DeathLink amnesty.
        private int _gameOversSinceLastSend;

        // Sends a DeathLink on a Game Over once the amnesty has been used up.
        public void OnPlayerGameOver()
        {
            if (!DeathLinkEnabled)
                return;

            int amnesty = PikunikuAPMod.SlotData?.DeathLinkAmnesty ?? 0;
            _gameOversSinceLastSend++;
            if (_gameOversSinceLastSend <= amnesty)
            {
                Log.Message($"DeathLink amnesty {_gameOversSinceLastSend}/{amnesty} — not sending");
                return;
            }

            _gameOversSinceLastSend = 0;
            SendDeath();
        }

        public void SendDeath()
        {
            if (!DeathLinkEnabled)
                return;

            var packet = new BouncePacket();
            var now = DateTime.Now;

            if (now - lastDeathLinkTime < TimeSpan.FromSeconds(2))
                return;

            packet.Tags = ["DeathLink"];
            packet.Data = new Dictionary<string, JToken>
            {
                { "time", now.ToUnixTimeStamp() },
                { "source", Slot },
                { "cause", $"{Slot} {deathMessages[random.Next(deathMessages.Length)]}" }
            };

            lastDeathLinkTime = now;
            Session.Socket.SendPacket(packet);
        }

        private void BouncePacketReceived(BouncePacket packet)
        {
            if (!DeathLinkEnabled) return;

            ProcessBouncePacket(packet, "DeathLink", ref lastDeath, (source, data) => {
                var deathCause = data.TryGetValue("cause", out var value) ? value.ToString() : $"{source} has died.";
                HandleDeathLink(source, deathCause);
            });
        }

        private static void ProcessBouncePacket(BouncePacket packet, string tag, ref string lastTime,
            Action<string, Dictionary<string, JToken>> handler)
        {
            if (!packet.Tags.Contains(tag)) return;
            if (!packet.Data.TryGetValue("time", out var timeObj))
                return;
            if (lastTime == timeObj.ToString())
                return;
            lastTime = timeObj.ToString();
            if (!packet.Data.TryGetValue("source", out var sourceObj))
                return;
            var source = sourceObj?.ToString() ?? "Unknown";

            handler(source, packet.Data);
        }

        private void HandleDeathLink(string source, string cause)
        {
            AddMessageToGameLog(cause);
            if (source == Slot)
                return;
            
            PikunikuAPMod.GameHandler.Kill();
        }

        /// <summary>
        /// Scout a location. net35 has no Task-based scouting, so the result
        /// arrives via callback (potentially on a non-main thread).
        /// </summary>
        public void ScoutLocation(long locationId, Action<ScoutedItemInfo> callback, bool createHint = false)
        {
            Session.Locations.ScoutLocationsAsync(results =>
            {
                if (results != null && results.Count > 0)
                    callback?.Invoke(results.Values.First());
            }, createHint, locationId);
        }

        public string GetPlayerName(int player)
        {
            return Session.Players.GetPlayerAlias(player) ?? $"Player {player}";
        }

        public string GetLocationName(long locationId)
        {
            return Session.Locations.GetLocationNameFromId(locationId) ?? $"Location {locationId}";
        }
    }
}
