using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using static VirtuSphere.apiService;
using static VirtuSphere.FMmain;

namespace VirtuSphere
{

    public class apiService
    {
        private readonly HttpClient _httpClient;
        internal readonly string apiUrl;  // Ändern auf internal oder public
        internal readonly string apiToken;  // Ändern auf internal oder public
        private readonly bool useTls;

        public string globalusername;
        public DateTime TokenExpiryTime { get; private set; }

        public apiService(HttpClient httpClient, string apiUrl, string apiToken, bool useTls)
        {
            _httpClient = httpClient;
            this.apiUrl = apiUrl;
            this.apiToken = apiToken;
            this.useTls = useTls;
        }


        public class ApiResponse
        {
            public bool success { get; set; }

        }
        private static bool callbackSet = false; // Statischer Flag zur Überwachung, ob der Callback gesetzt wurde

        public async Task<string> IsValidLogin(string username, string password, string hostname, bool useTls)
        {
            globalusername = username;

            if (useTls && !callbackSet)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                // Setze den ServerCertificateValidationCallback, nur wenn er noch nicht gesetzt wurde
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    Console.WriteLine("Zertifikatsvalidierungs-Callback ausgelöst");

                    var cert = (X509Certificate2)certificate;

                    if (sslPolicyErrors != SslPolicyErrors.None)
                    {
                        string certDetails = $"Aussteller: {cert.Issuer}\n" +
                                             $"Betreff: {cert.Subject}\n" +
                                             $"Gültig ab: {cert.NotBefore}\n" +
                                             $"Gültig bis: {cert.NotAfter}\n" +
                                             $"Fingerabdruck: {cert.Thumbprint}";

                        Console.WriteLine("Zertifikat ist nicht vertrauenswürdig");
                        Console.WriteLine(certDetails);

                        DialogResult result = MessageBox.Show($"Zertifikat ist nicht vertrauenswürdig\n\n{certDetails}\n\nMöchten Sie trotzdem fortfahren?", "Zertifikatfehler", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                        if (result == DialogResult.Yes)
                        {
                            return true; // Zertifikat wird für diese Sitzung ignoriert
                        }

                        return false; // Zertifikat ist nicht vertrauenswürdig und Benutzer möchte nicht fortfahren
                    }

                    return true; // Zertifikat ist vertrauenswürdig
                };

                callbackSet = true; // Setze den Flag, um zu markieren, dass der Callback nun gesetzt ist
            }

            string scheme = useTls ? "https" : "http";
            string requestUri = $"{scheme}://{hostname}/api/login.php";

            try
            {
                var headRequest = new HttpRequestMessage(HttpMethod.Head, requestUri);
                var headResponse = await _httpClient.SendAsync(headRequest);
                headResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Fehler bei der Verbindung: {e.Message}");
                return null; // Kann als Indikator für nicht erreichbare Adresse oder Verbindungsfehler dienen
            }

            var loginData = new Dictionary<string, string>
    {
        { "username", username },
        { "password", password }
    };
            var content = new FormUrlEncodedContent(loginData);
            var response = await _httpClient.PostAsync(requestUri, content);

            Console.WriteLine($"Verbindung zu {hostname} {(useTls ? "über TLS" : "ohne TLS")} hergestellt");
            Console.WriteLine(response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    dynamic result = JsonConvert.DeserializeObject(responseContent);
                    if (result != null && result != "Access Forbidden")
                    {
                        Console.WriteLine("Token: " + result);
                        return result;
                    }
                }
                else
                {
                    Console.WriteLine($"Anfrage an URL fehlgeschlagen: {response.RequestMessage.RequestUri}");
                    Console.WriteLine($"Statuscode: {response.StatusCode}");
                    Console.WriteLine($"Antwortinhalt: {responseContent}");
                    MessageBox.Show("Ein Fehler ist aufgetreten. Siehe Konsolenausgabe für Details.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception abgefangen: {ex.Message}");
                Console.WriteLine($"Anfrage: {response.RequestMessage}");
                Console.WriteLine($"Antwort: {responseContent}");
                MessageBox.Show("Ein kritischer Fehler ist aufgetreten. Siehe Konsolenausgabe für Details.", "Kritischer Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null; // Bei Fehlschlag
        }


        private void SaveIgnoredCertificate(string certCachePath, string thumbprint)
        {
            File.AppendAllText(certCachePath, thumbprint + Environment.NewLine);
        }

        private HashSet<string> LoadIgnoredCertificates(string certCachePath)
        {
            if (!File.Exists(certCachePath))
            {
                return new HashSet<string>();
            }

            var lines = File.ReadAllLines(certCachePath);
            return new HashSet<string>(lines);
        }




        public async Task<List<Package>> GetPackages()
        {
            string scheme = useTls ? "https" : "http";
            string requestUri = $"{scheme}://{apiUrl}/access.php?action=getPackages&token={apiToken}";
            var response = await _httpClient.GetAsync(requestUri);

            // wenn responsecode 418 ist, dann gib 418 zurück
            if ((int)response.StatusCode == 418)
            {
                Console.WriteLine("Token abgelaufen");
                MessageBox.Show("Token abgelaufen");
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var packageList = JsonConvert.DeserializeObject<List<Package>>(responseContent);
                return packageList;
            }
            return null; // Bei Fehlschlag oder "Access Forbidden"
        }

        public async Task<List<MissionItem>> GetMissions()
        {
            string scheme = useTls ? "https" : "http";
            string requestUri = $"{scheme}://{apiUrl}/access.php?action=getMissions&token={apiToken}";
            var response = await _httpClient.GetAsync(requestUri);

            // useTLs in console ausgeben
            Console.WriteLine($"useTls: {useTls}");
            Console.WriteLine($"RequestUri: {requestUri}");
            Console.WriteLine($"Request: {response}");
            Console.WriteLine($"Status Code:{response.StatusCode}");

            // wenn responsecode 418 ist, dann gib 418 zurück
            if ((int)response.StatusCode == 418)
            {
                Console.WriteLine("Token abgelaufen");
                MessageBox.Show("Token abgelaufen");
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var missionsList = JsonConvert.DeserializeObject<List<MissionItem>>(responseContent);
                    Console.WriteLine("Response Content: " + responseContent);
                    Console.WriteLine("----------------------");
                    return missionsList;
                }
                catch (JsonException)
                {
                    // Offne LogForm und gib die Fehlermeldung aus

                    ErrorForm logForm = new ErrorForm();
                    logForm.txtLog.Text = requestUri;
                    logForm.txtLog.Text += "\n" + responseContent;
                    logForm.ShowDialog();

                    MessageBox.Show("Ungültiges JSON: " + responseContent);
                    return null;
                }
            }
            return null; // Bei Fehlschlag oder "Access Forbidden"
        }


        public async Task<List<OSItem>> GetOS()
        {
            string scheme = useTls ? "https" : "http";
            string requestUri = $"{scheme}://{apiUrl}/access.php?action=getOS&token={apiToken}";
            var response = await _httpClient.GetAsync(requestUri);

            // wenn responsecode 418 ist, dann gib 418 zurück
            if ((int)response.StatusCode == 418)
            {
                Console.WriteLine("Token abgelaufen");
                MessageBox.Show("Token abgelaufen");
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var osList = JsonConvert.DeserializeObject<List<OSItem>>(responseContent);
                    return osList;
                }
                catch (JsonException)
                {
                    // Offne LogForm und gib die Fehlermeldung aus
                    ErrorForm logForm = new ErrorForm();
                    logForm.txtLog.Text = requestUri;
                    logForm.txtLog.Text += "\n" + responseContent;
                    logForm.ShowDialog();

                    MessageBox.Show("Ungültiges JSON: " + responseContent);
                    return null;
                }
            }
            return null; // Bei Fehlschlag oder "Access Forbidden"
        }


        // CreateOS(comboOS_Name.Text, comboOS_Status.Text);
        public async Task<bool> CreateOS(string osName, string osStatus)
        {
            if (apiUrl == null || apiToken == null || _httpClient == null)
            {
                Console.WriteLine("Hostname, Token, OSName oder HttpClient sind nicht verfügbar");
                return false;
            }
            else
            {
                string scheme = useTls ? "https" : "http";
                string requestUri = $"{scheme}://{apiUrl}/access.php?action=createOS&token={apiToken}&osName={osName}&osStatus={osStatus}";
                var response = await _httpClient.PostAsync(requestUri, null);

                // ausgabe response content
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                Console.WriteLine($"RequestUri: {requestUri}");
                Console.WriteLine($"Request: {response}");
                Console.WriteLine($"Status Code:{response.StatusCode}");

                // wenn responsecode 418 ist, dann gib 418 zurück
                if ((int)response.StatusCode == 418)
                {
                    Console.WriteLine("Token abgelaufen");
                    MessageBox.Show("Token abgelaufen");
                    return false;
                }
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(responseContent);
                    Console.WriteLine("OS erstellt");
                    // Wenn im Response-Code 200 steht, dann gib true zurück
                    return true;
                }
                else
                {
                    // Offne LogForm und gib die Fehlermeldung aus
                    ErrorForm logForm = new ErrorForm();
                    logForm.txtLog.Text = requestUri;
                    logForm.txtLog.Text += "\n" + responseContent;
                    logForm.ShowDialog();

                    MessageBox.Show("Fehler beim Erstellen des OS: " + responseContent);
                }
                return false;
            }
        }

        public async Task<bool> RemoveOS(int osId)
        {
            if (apiUrl == null || apiToken == null || _httpClient == null)
            {
                Console.WriteLine("Hostname, Token, OSId oder HttpClient sind nicht verfügbar");
                return false;
            }
            else
            {
                string scheme = useTls ? "https" : "http";
                string requestUri = $"{scheme}://{apiUrl}/access.php?action=deleteOS&token={apiToken}&osId={osId}";
                var response = await _httpClient.DeleteAsync(requestUri);

                // ausgabe response content
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                Console.WriteLine($"RequestUri: {requestUri}");
                Console.WriteLine($"Request: {response}");
                Console.WriteLine($"Status Code:{response.StatusCode}");

                // wenn responsecode 418 ist, dann gib 418 zurück
                if ((int)response.StatusCode == 418)
                {
                    Console.WriteLine("Token abgelaufen");
                    MessageBox.Show("Token abgelaufen");
                    return false;
                }
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("OS gelöscht");
                    // Wenn im Response-Code 200 steht, dann gib true zurück
                    return true;
                }
                else
                {
                    // Offne LogForm und gib die Fehlermeldung aus
                    ErrorForm logForm = new ErrorForm();
                    logForm.txtLog.Text = requestUri;
                    logForm.txtLog.Text += "\n" + responseContent;
                    logForm.ShowDialog();

                    MessageBox.Show("Fehler beim Löschen des OS: " + responseContent);
                }
                return false;
            }
        }

        // UpdateOS(comboOS_Name.Text, comboOS_Status.Text);
        public async Task<bool> UpdateOS(int osId, string osName, string osStatus)
        {
            if (apiUrl == null || apiToken == null || _httpClient == null)
            {
                Console.WriteLine("Hostname, Token, OSId oder HttpClient sind nicht verfügbar");
                return false;
            }
            else
            {
                string scheme = useTls ? "https" : "http";
                string requestUri = $"{scheme}://{apiUrl}/access.php?action=updateOS&token={apiToken}&osId={osId}&osName={osName}&osStatus={osStatus}";
                var response = await _httpClient.PutAsync(requestUri, null);

                // ausgabe response content
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                Console.WriteLine($"RequestUri: {requestUri}");
                Console.WriteLine($"Request: {response}");
                Console.WriteLine($"Status Code:{response.StatusCode}");

                // wenn responsecode 418 ist, dann gib 418 zurück
                if ((int)response.StatusCode == 418)
                {
                    Console.WriteLine("Token abgelaufen");
                    MessageBox.Show("Token abgelaufen");
                    return false;
                }
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("OS aktualisiert");
                    Console.WriteLine(responseContent);
                    // Wenn im Response-Code 200 steht, dann gib true zurück
                    return true;
                }
                else
                {
                    // Offne LogForm und gib die Fehlermeldung aus
                    ErrorForm logForm = new ErrorForm();
                    logForm.txtLog.Text = requestUri;
                    logForm.txtLog.Text += "\n" + responseContent;
                    logForm.ShowDialog();

                    MessageBox.Show("Fehler beim Aktualisieren des OS: " + responseContent);
                }
                return false;
            }
        }

        public async Task<List<VLANItem>> GetVLANs()
        {
            string scheme = useTls ? "https" : "http";
            string requestUri = $"{scheme}://{apiUrl}/access.php?action=getVLANs&token={apiToken}";
            var response = await _httpClient.GetAsync(requestUri);

            // wenn responsecode 418 ist, dann gib 418 zurück
            if ((int)response.StatusCode == 418)
            {
                Console.WriteLine("Token abgelaufen");
                MessageBox.Show("Token abgelaufen");
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    Console.WriteLine(responseContent);
                    try
                    {
                        var vlanList = JsonConvert.DeserializeObject<List<VLANItem>>(responseContent);
                        return vlanList;
                    }
                    catch (JsonException)
                    {
                        // Offne LogForm und gib die Fehlermeldung aus
                        ErrorForm logForm = new ErrorForm();
                        logForm.txtLog.Text = requestUri;
                        logForm.txtLog.Text += "\n" + responseContent;
                        logForm.ShowDialog();

                        MessageBox.Show("Ungültiges JSON: " + responseContent);
                        return null;
                    }
                }
            }

            return null; // Bei Fehlschlag oder "Access Forbidden"
        }

        public async Task<bool> DeleteMission(int missionId)
        {
            if (apiUrl == null || apiToken == null || _httpClient == null)
            {
                Console.WriteLine("Hostname, Token, MissionId oder HttpClient sind nicht verfügbar");
                return false;
            }
            else
            {
                Console.WriteLine("DeleteMission aufgerufen: Mission: " + missionId.ToString());

                // Die Mission ID ist in der klammer hinter dem Namen, also trenne sie

                Console.WriteLine("DeleteMission aufgerufen: Mission ID: " + missionId.ToString());

                string scheme = useTls ? "https" : "http";
                string requestUri = $"{scheme}://{apiUrl}/access.php?action=deleteMission&token={apiToken}&missionId={missionId}";
                var response = await _httpClient.DeleteAsync(requestUri);

                // ausgabe response content
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                // wenn responsecode 418 ist, dann gib 418 zurück
                if ((int)response.StatusCode == 418)
                {
                    Console.WriteLine("Token abgelaufen");
                    MessageBox.Show("Token abgelaufen");
                    return false;
                }
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Mission gelöscht");
                    // Wenn im Response-Code 200 steht, dann gib true zurück
                    return true;
                }
                else
                {
                    // Offne LogForm und gib die Fehlermeldung aus
                    ErrorForm logForm = new ErrorForm();
                    logForm.txtLog.Text = requestUri;
                    logForm.txtLog.Text += "\n" + responseContent;
                    logForm.ShowDialog();

                    MessageBox.Show("Fehler beim Löschen der Mission: " + responseContent);
                }
                return false;
            }
        }


        public async Task<bool> ExpandToken()
        {
            if (apiUrl == null || apiToken == null || _httpClient == null)
            {
                Console.WriteLine("Hostname, Token or HttpClient is not available");
                return false;
            }
            else
            {
                Console.WriteLine("ExpandToken called");

                // URL aufbauen, um die Token-Erweiterung anzufordern
                string scheme = useTls ? "https" : "http";
                string requestUri = $"{scheme}://{apiUrl}/access.php?action=expandToken&token={apiToken}";
                var response = await _httpClient.GetAsync(requestUri);

                Console.WriteLine($"RequestUri: {requestUri}");
                Console.WriteLine($"Request: {response}");
                Console.WriteLine($"Status Code: {response.StatusCode}");

                // Wenn der Statuscode 418 ist, bedeutet dies, dass das Token abgelaufen ist
                if ((int)response.StatusCode == 418)
                {
                    Console.WriteLine("Token abgelaufen");
                    MessageBox.Show("Token abgelaufen", "Token Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Überprüfen, ob die Anfrage erfolgreich war
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Response Content: " + responseContent);

                    try
                    {
                        // Verarbeitung der JSON-Antwort
                        bool result = JsonConvert.DeserializeObject<bool>(responseContent);
                        return result;
                    }
                    catch (JsonException)
                    {
                        // Offne LogForm und gib die Fehlermeldung aus
                        ErrorForm logForm = new ErrorForm();
                        logForm.txtLog.Text = requestUri;
                        logForm.txtLog.Text += "\n" + responseContent;
                        logForm.ShowDialog();

                        MessageBox.Show("Ungültiges JSON: " + responseContent);
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Error: " + response.StatusCode);
                    return false;
                }
            }
        }


        public async Task<bool> CreateMission(string missionName)
        {
            if (apiUrl == null || apiToken == null || missionName == null || _httpClient == null)
            {
                Console.WriteLine("Hostname, Token, MissionName or HttpClient is not available");
                return false;
            }
            else
            {
                Console.WriteLine("CreateMission called: MissionName: " + missionName);

                //missionName leerzeichen url encoden
                missionName = WebUtility.UrlEncode(missionName);

                string scheme = useTls ? "https" : "http";
                string requestUri = $"{scheme}://{apiUrl}/access.php?action=createMission&token={apiToken}&missionName={missionName}";
                var response = await _httpClient.PostAsync(requestUri, null);

                Console.WriteLine($"RequestUri: {requestUri}");
                Console.WriteLine($"Request: {response}");
                Console.WriteLine($"Status Code:{response.StatusCode}");

                // wenn responsecode 418 ist, dann gib 418 zurück
                if ((int)response.StatusCode == 418)
                {
                    Console.WriteLine("Token abgelaufen");
                    MessageBox.Show("Token abgelaufen");
                    return false;
                }

                if (!response.IsSuccessStatusCode)
                {
                    // Offne LogForm und gib die Fehlermeldung aus
                    var responseContent = await response.Content.ReadAsStringAsync();
                    ErrorForm logForm = new ErrorForm();
                    logForm.txtLog.Text = requestUri;
                    logForm.txtLog.Text += "\n" + responseContent;
                    logForm.ShowDialog();

                    MessageBox.Show("Fehler beim Erstellen der Mission: " + responseContent);
                }

                return response.IsSuccessStatusCode;
            }
        }

        public async Task<List<VM>> GetVMs(int missionId)
        {
            Console.WriteLine("----------------------");
            Console.WriteLine("Aktion GetVMs für VM-Liste wird durchgeführt");
            string scheme = useTls ? "https" : "http";
            string requestUri = $"{scheme}://{apiUrl}/access.php?action=getVMs&token={apiToken}&missionId={missionId}";
            var response = await _httpClient.GetAsync(requestUri);

            Console.WriteLine($"RequestUri: {requestUri}");
            Console.WriteLine($"Request: {response}");
            Console.WriteLine($"Status Code:{response.StatusCode}");

            if ((int)response.StatusCode == 418)
            {
                Console.WriteLine("Token abgelaufen");
                MessageBox.Show("Token abgelaufen");
                Console.WriteLine("----------------------");
                return null; // Beachte, dass du vielleicht eine leere Liste zurückgeben möchtest statt null, um NullReferenceExceptions zu vermeiden.
            }

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                // Direkt deserialisieren zu List<VM> statt zu List<string>
                try
                {
                    var vmList = JsonConvert.DeserializeObject<List<VM>>(responseContent);
                    Console.WriteLine("Response Content: " + responseContent);
                    Console.WriteLine("----------------------");
                    return vmList; // Diese Liste enthält VM-Objekte, nicht nur VM-Namen
                }
                catch (JsonException)
                {
                    // Offne LogForm und gib die Fehlermeldung aus
                    ErrorForm logForm = new ErrorForm();
                    logForm.txtLog.Text = requestUri;
                    logForm.txtLog.Text += "\n" + responseContent;
                    logForm.ShowDialog();

                    MessageBox.Show("Ungültiges JSON: " + responseContent);
                    // responseContent in console
                    Console.WriteLine("Response Content: " + responseContent);
                    Console.WriteLine("----------------------");
                    return null;
                }
            }
            Console.WriteLine("----------------------");
            return null; // Bei Fehlschlag oder "Access Forbidden"
        }


        public async Task<bool> UpdateMission(MissionItem updatedMission)
        {
            if (updatedMission == null)
            {
                Console.WriteLine("Aktualisierte Mission ist null");
                return false;
            }

            // Stellen Sie sicher, dass der Endpunkt Ihrer API korrekt ist.
            string scheme = useTls ? "https" : "http";
            string requestUri = $"{scheme}://{apiUrl}/access.php?action=updateMission&token={apiToken}&missionId={updatedMission.Id}";

            // Konfigurieren des Request Body als JSON
            var json = JsonConvert.SerializeObject(updatedMission);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Setzen des Authorization Headers, falls Ihre API dies benötigt
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiToken);

            // Senden der PUT-Anfrage an den Server
            var response = await _httpClient.PutAsync(requestUri, content);

            if ((int)response.StatusCode == 418)
            {
                Console.WriteLine("Token abgelaufen");
                MessageBox.Show("Token abgelaufen");
                return false;
            }

            Console.WriteLine("RequestUri: " + requestUri);
            Console.WriteLine("Request: " + json);
            Console.WriteLine($"Status Code: {response.StatusCode}");
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Content: {responseContent}");

            // zeige ergebnis in ErrorForm  
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                ErrorForm logForm = new ErrorForm();
                logForm.txtLog.Text = requestUri;
                logForm.txtLog.Text += "\n" + json;
                logForm.txtLog.Text += "\n" + responseContent;
                logForm.ShowDialog();
            }

            if (responseContent == "true")
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public async Task<bool> VmListToWebAPI(string action, int missionId, List<VM> vmList)
        {
            if (vmList == null)
            {
                Console.WriteLine("VM-Liste ist null");
                return false;
            }
            Console.WriteLine("----------------------");
            Console.WriteLine($"Aktion {action} für VM-Liste wird durchgeführt");
            foreach (var vm in vmList)
            {
                Console.WriteLine($"VM: {vm.vm_name}");
            }

            // Abbruch wenn hostname oder token leer sind
            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiToken))
            {
                Console.WriteLine("Hostname oder Token ist leer");
                return false;
            }

            string scheme = useTls ? "https" : "http";
            string requestUri = $"{scheme}://{apiUrl}/access.php?action={action}&token={apiToken}&missionId={missionId}";
            var json = JsonConvert.SerializeObject(vmList);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(requestUri, content);

            if ((int)response.StatusCode == 418)
            {
                Console.WriteLine("Token abgelaufen");
                MessageBox.Show("Token abgelaufen");
                return false;
            }

            Console.WriteLine("RequestUri: " + requestUri);
            Console.WriteLine("Request: " + json);
            Console.WriteLine($"Status Code: {response.StatusCode}");
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Content: {responseContent}");

            ApiResponse data = null;
            try
            {
                data = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
            }
            catch (JsonException)
            {
                // Offne LogForm und gib die Fehlermeldung aus
                ErrorForm logForm = new ErrorForm();
                logForm.txtLog.Text = requestUri;
                logForm.txtLog.Text += "\n" + json;
                logForm.txtLog.Text += "\n" + responseContent;
                logForm.ShowDialog();

                MessageBox.Show("Ungültiges JSON: " + responseContent);
                return false;
            }

            Console.WriteLine("----------------------");

            if (data != null && data.success)
            {
                return response.IsSuccessStatusCode;
            }
            else
            {
                MessageBox.Show("Operation fehlgeschlagen.");
                return false;
            }
        }


        public async Task<bool> RemoveVLAN(int vlanId)
        {
            if (apiUrl == null || apiToken == null || _httpClient == null)
            {
                Console.WriteLine("Hostname, Token, VLANId oder HttpClient sind nicht verfügbar");
                return false;
            }
            else
            {
                string scheme = useTls ? "https" : "http";
                string requestUri = $"{scheme}://{apiUrl}/access.php?action=deleteVLAN&token={apiToken}&vlanId={vlanId}";
                var response = await _httpClient.DeleteAsync(requestUri);

                // ausgabe response content
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                Console.WriteLine($"RequestUri: {requestUri}");
                Console.WriteLine($"Request: {response}");
                Console.WriteLine($"Status Code:{response.StatusCode}");

                // wenn responsecode 418 ist, dann gib 418 zurück
                if ((int)response.StatusCode == 418)
                {
                    Console.WriteLine("Token abgelaufen");
                    MessageBox.Show("Token abgelaufen");
                    return false;
                }
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("VLAN gelöscht");
                    // Wenn im Response-Code 200 steht, dann gib true zurück
                    return true;
                }
                else
                {
                    // Offne LogForm und gib die Fehlermeldung aus
                    ErrorForm logForm = new ErrorForm();
                    logForm.txtLog.Text = requestUri;
                    logForm.txtLog.Text += "\n" + responseContent;
                    logForm.ShowDialog();

                    MessageBox.Show("Fehler beim Löschen des VLAN: " + responseContent);
                }
                return false;
            }
        }


        public async Task<bool> UpdateVLAN(int vlanId, string vlanName)
        {
            if (apiUrl == null || apiToken == null || _httpClient == null)
            {
                Console.WriteLine("Hostname, Token, VLANId oder HttpClient sind nicht verfügbar");
                return false;
            }
            else
            {
                string scheme = useTls ? "https" : "http";
                string requestUri = $"{scheme}://{apiUrl}/access.php?action=updateVLAN&token={apiToken}&vlanId={vlanId}&vlanName={vlanName}";
                var response = await _httpClient.PutAsync(requestUri, null);

                // ausgabe response content
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                Console.WriteLine($"RequestUri: {requestUri}");
                Console.WriteLine($"Request: {response}");
                Console.WriteLine($"Status Code:{response.StatusCode}");

                // wenn responsecode 418 ist, dann gib 418 zurück
                if ((int)response.StatusCode == 418)
                {
                    Console.WriteLine("Token abgelaufen");
                    MessageBox.Show("Token abgelaufen");
                    return false;
                }
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("VLAN aktualisiert");
                    // Wenn im Response-Code 200 steht, dann gib true zurück
                    return true;
                }
                else
                {
                    // Offne LogForm und gib die Fehlermeldung aus
                    ErrorForm logForm = new ErrorForm();
                    logForm.txtLog.Text = requestUri;
                    logForm.txtLog.Text += "\n" + responseContent;
                    logForm.ShowDialog();

                    MessageBox.Show("Fehler beim Aktualisieren des VLAN: " + responseContent);
                }
                return false;
            }
        }


        public async Task<bool> CreateVLAN(string vlanName)
        {
            if (apiUrl == null || apiToken == null || _httpClient == null)
            {
                Console.WriteLine("Hostname, Token, VLANName oder HttpClient sind nicht verfügbar");
                return false;
            }
            else
            {
                string scheme = useTls ? "https" : "http";
                string requestUri = $"{scheme}://{apiUrl}/access.php?action=createVLAN&token={apiToken}&vlanName={vlanName}";
                var response = await _httpClient.PostAsync(requestUri, null);

                // ausgabe response content
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                Console.WriteLine($"RequestUri: {requestUri}");
                Console.WriteLine($"Request: {response}");
                Console.WriteLine($"Status Code:{response.StatusCode}");

                // wenn responsecode 418 ist, dann gib 418 zurück
                if ((int)response.StatusCode == 418)
                {
                    Console.WriteLine("Token abgelaufen");
                    MessageBox.Show("Token abgelaufen");
                    return false;
                }
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("VLAN erstellt");
                    // Wenn im Response-Code 200 steht, dann gib true zurück
                    return true;
                }
                else
                {
                    // Offne LogForm und gib die Fehlermeldung aus
                    ErrorForm logForm = new ErrorForm();
                    logForm.txtLog.Text = requestUri;
                    logForm.txtLog.Text += "\n" + responseContent;
                    logForm.ShowDialog();

                    MessageBox.Show("Fehler beim Erstellen des VLAN: " + responseContent);
                }
                return false;
            }
        }


        public class Missions
        {
            public string id { get; set; }
            public string mission_name { get; set; }
            public string mission_status { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public string mission_notes { get; set; }
            public string wds_vlan { get; set; }
            public string hypervisor_datastorage { get; set; }
            public string hypervisor_datacenter { get; set; }

        }




        public class VLAN
        {
            public string id { get; set; }
            public string vlan_name { get; set; }

        }



    }
}
