using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace PikunikuAPMod
{
    public class LastConnectionInfo
    {
        public string Host;
        public string Port;
        public string SlotName;
        public string Password;
    }

    public class FileWriter : MonoBehaviour
    {
        private const string LastConnectionFileName = "last_connection.txt";

        // Save the last used connection info to disk. Overwrites each time so it's the default on next run.
        public static void WriteLastConnection(string host, int port, string slotName, string password)
        {
            try
            {
                string path = Application.persistentDataPath + "/" + LastConnectionFileName;
                var lines = new List<string>
                {
                    host ?? "",
                    port.ToString(),
                    slotName ?? "",
                    password ?? ""
                };
                File.WriteAllLines(path, lines.ToArray());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to write last connection info: {ex.Message}");
            }
        }

        public static LastConnectionInfo ReadLastConnection()
        {
            try
            {
                string path = Application.persistentDataPath + "/" + LastConnectionFileName;
                if (!File.Exists(path))
                    return null;

                var lines = File.ReadAllLines(path);
                return new LastConnectionInfo
                {
                    Host = lines.Length > 0 ? lines[0] : null,
                    Port = lines.Length > 1 ? lines[1] : null,
                    SlotName = lines.Length > 2 ? lines[2] : null,
                    Password = lines.Length > 3 ? lines[3] : null
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to read last connection info: {ex.Message}");
                return null;
            }
        }
    }
}
