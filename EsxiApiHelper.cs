using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

public class EsxiApiHelper
{
    public static async Task<bool> VerifyEsxiCredentialsAsync(string esxiHost, string username, string password)
    {
        // Ersetze diesen Pfad mit dem tatsächlichen API-Endpunkt, der zur Verifizierung der Anmeldedaten verwendet werden soll
        string apiEndpoint = esxiHost + "/rest/appliance/system/version"; // Beispiel-Endpunkt

        using (var client = new HttpClient())
        {
            // Setze den Authorization-Header für Basic Auth
            var authToken = Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

            try
            {
                // Sende eine GET-Anfrage an den API-Endpunkt
                HttpResponseMessage response = await client.GetAsync(apiEndpoint);
                // Überprüfe den Statuscode der Antwort
                if (response.IsSuccessStatusCode)
                {
                    // Die Anmeldedaten sind korrekt
                    return true;
                }
                else
                {
                    // Die Anmeldedaten sind falsch oder es gab einen Fehler
                    return false;
                }
            }
            catch
            {
                // Bei einer Ausnahme wird false zurückgegeben
                return false;
            }
        }
    }
}
