using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class SshConnector
{

    public string GenerateSSHKey(string privateKeyPath)
    {
        string publicKeyPath = privateKeyPath + ".pub"; // Standardpfad für den öffentlichen Schlüssel
        try
        {
            // Generiere das Schlüsselpaar mit ssh-keygen
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "ssh-keygen",
                Arguments = $"-t rsa -b 2048 -N \"\" -f \"{privateKeyPath}\"", // Verwende Arguments anstelle von ArgumentList
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            Process process = Process.Start(psi);
            process.WaitForExit();

            // Lese den öffentlichen Schlüssel aus der Datei
            string publicKey = File.ReadAllText(publicKeyPath);
            return publicKey;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ein Fehler ist aufgetreten: {ex.Message}");
            return null;
        }
    }

    public List<string> ExecuteCommands(string host, int port, string username, string password, List<string> commands)
    {
        var output = new List<string>();

        using (var client = new SshClient(host, port, username, password))
        {
            try
            {
                client.Connect();

                if (client.IsConnected)
                {
                    foreach (var command in commands)
                    {
                        var result = client.CreateCommand(command).Execute();
                        // Fügt jede Zeile der Ausgabe als eigenen Eintrag hinzu
                        //output.AddRange(result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None));
                        output.AddRange(result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));

                    }
                }
                else
                {
                    Console.WriteLine("Verbindung fehlgeschlagen.");
                    output.Add("Verbindung fehlgeschlagen.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ein Fehler ist aufgetreten: {ex.Message}");
                output.Add($"Ein Fehler ist aufgetreten: {ex.Message}");
            }
            finally
            {
                client.Disconnect();

            }
        }

        return output;
    }

    public static bool CheckSshConnection(string host, int port, string username, string password)
    {
        // Gib mir die Verbindungsinformation in der Console aus
        Console.WriteLine($"Verbindung zu {host}:{port} mit Benutzername {username} wird hergestellt...");

        try
        {
            using (var client = new SshClient(host, port, username, password))
            {
                client.Connect();
                if (client.IsConnected)
                {
                    client.Disconnect();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        catch (Exception)
        {
            // Bei jeglichen Fehlern, die während des Verbindungsversuchs auftreten, geben wir false zurück.
            return false;
        }
    }
}
