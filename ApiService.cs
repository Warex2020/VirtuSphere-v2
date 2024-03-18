using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static VirtuSphere.apiService;
using static VirtuSphere.FMmain;

namespace VirtuSphere
{

    public class apiService
    {
        private HttpClient _httpClient;
        internal string apiToken;
        internal string apiUrl;

        public string globalusername;
        public DateTime TokenExpiryTime { get; private set; }

        public apiService(string apiToken, string apiUrl)
        {
            _httpClient = new HttpClient();
            this.apiToken = apiToken;
            this.apiUrl = apiUrl;
        }

        public class ApiResponse
        {
            public bool success { get; set; }

        }

        public async Task<string> IsValidLogin(string username, string password, string hostname, bool useTls)
        {
            globalusername = username;
            // Setze die SecurityProtocol nur, wenn useTls wahr ist
            if (useTls)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            }

            string scheme = useTls ? "https" : "http";
            string requestUri = $"{scheme}://{hostname}/api/login.php";

            // Prüfung, ob die Adresse erreichbar ist und die Verbindung möglich ist
            try
            {
                // Versuche, eine Kopfzeilenanfrage zu senden, um zu sehen, ob der Host erreichbar ist
                var headRequest = new HttpRequestMessage(HttpMethod.Head, requestUri);
                var headResponse = await _httpClient.SendAsync(headRequest);
                headResponse.EnsureSuccessStatusCode(); // Löst eine Ausnahme aus, wenn der Statuscode außerhalb von 2xx liegt

                // Hier könnte man zusätzlich prüfen, ob eine Verbindung ohne TLS möglich ist,
                // indem man eine Anfrage über HTTP sendet und die Antwort prüft.
                // Da dies jedoch ein Sicherheitsrisiko darstellen kann, überspringen wir diesen Schritt.

                // Für die Prüfung des Zertifikats, siehe unten
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Fehler bei der Verbindung: {e.Message}");
                return null; // Kann als Indikator für nicht erreichbare Adresse oder Verbindungsfehler dienen
            }

            // Führe die Anmeldeanfrage aus
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
                    Console.WriteLine($"Request to URL failed: {response.RequestMessage.RequestUri}");
                    Console.WriteLine($"Status Code: {response.StatusCode}");
                    Console.WriteLine($"Response Content: {responseContent}");
                    MessageBox.Show("Ein Fehler ist aufgetreten. Siehe Konsolenausgabe für Details.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught: {ex.Message}");
                Console.WriteLine($"Request: {response.RequestMessage}");
                Console.WriteLine($"Response: {responseContent}");
                MessageBox.Show("Ein kritischer Fehler ist aufgetreten. Siehe Konsolenausgabe für Details.", "Kritischer Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            return null; // Bei Fehlschlag
        }


        public async Task<List<Package>> GetPackages()
        {
            string requestUri = $"http://{apiUrl}/access.php?action=getPackages&token={apiToken}";
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
            string requestUri = $"http://{apiUrl}/access.php?action=getMissions&token={apiToken}";
            var response = await _httpClient.GetAsync(requestUri);

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
        public async Task<List<Missions>> LoadMissions(string hostname, string token)
        {
            string requestUri = $"http://{hostname}/access.php?action=getMissions&token={token}";
            var response = await _httpClient.GetAsync(requestUri);

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
                    // Direktes Deserialisieren in eine Liste von Missions-Objekten
                    var missionsList = JsonConvert.DeserializeObject<List<Missions>>(responseContent);
                    return missionsList;
                }
                catch (JsonException ex)
                {
                    // Offne LogForm und gib die Fehlermeldung aus

                    ErrorForm logForm = new ErrorForm();
                    logForm.txtLog.Text = requestUri;
                    logForm.txtLog.Text += "\n" + responseContent;
                    logForm.ShowDialog();

                    MessageBox.Show("Ungültiges JSON: " + ex.Message);
                    return null;
                }
            }
            return null; // Bei Fehlschlag oder "Access Forbidden"
        }


        public async Task<List<OSItem>> GetOS()
        {
            string requestUri = $"http://{apiUrl}/access.php?action=getOS&token={apiToken}";
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
                var osList = JsonConvert.DeserializeObject<List<OSItem>>(responseContent);
                return osList;
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
                string requestUri = $"http://{apiUrl}/access.php?action=createOS&token={apiToken}&osName={osName}&osStatus={osStatus}";
                var response = await _httpClient.PostAsync(requestUri, null);

                // ausgabe response content
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);

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
                    Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                    Console.WriteLine("OS erstellt");
                    // Wenn im Response-Code 200 steht, dann gib true zurück
                    return true;
                }
                return false;
            }
        }
        // RemoveOS
        public async Task<bool> RemoveOS(int osId)
        {
            if (apiUrl == null || apiToken == null || _httpClient == null)
            {
                Console.WriteLine("Hostname, Token, OSId oder HttpClient sind nicht verfügbar");
                return false;
            }
            else
            {
                string requestUri = $"http://{apiUrl}/access.php?action=deleteOS&token={apiToken}&osId={osId}";
                var response = await _httpClient.DeleteAsync(requestUri);

                // ausgabe response content
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);

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
                string requestUri = $"http://{apiUrl}/access.php?action=updateOS&token={apiToken}&osId={osId}&osName={osName}&osStatus={osStatus}";
                var response = await _httpClient.PutAsync(requestUri, null);

                // ausgabe response content
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);

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
                    Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                    // Wenn im Response-Code 200 steht, dann gib true zurück
                    return true;
                }
                return false;
            }
        }
        public async Task<List<VLANItem>> GetVLANs()
        {

            string requestUri = $"http://{apiUrl}/access.php?action=getVLANs&token={apiToken}";
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


                string requestUri = $"http://{apiUrl}/access.php?action=deleteMission&token={apiToken}&missionId={missionId}";
                var response = await _httpClient.DeleteAsync(requestUri);

                // ausgabe response content
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);

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
                    return response.IsSuccessStatusCode;
                }
                return false;

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



                string requestUri = $"http://{apiUrl}/access.php?action=createMission&token={apiToken}&missionName={missionName}";
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

                Console.WriteLine(response.StatusCode);

                return response.IsSuccessStatusCode;
            }
        }
        public async Task<List<VM>> GetVMs(int missionId)
        {
            Console.WriteLine("----------------------");
            Console.WriteLine("Aktion GetVMs für VM-Liste wird durchgeführt");
            string requestUri = $"http://{apiUrl}/access.php?action=getVMs&token="+apiToken+"&missionId="+missionId;
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
            string requestUri = $"http://{apiUrl}/access.php?action=updateMission&token={apiToken}&missionId={updatedMission.Id}";

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

            if(responseContent == "true")
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


            string requestUri = $"http://{apiUrl}/access.php?action={action}&token={apiToken}&missionId={missionId}";
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


        // methode removeVLAN
        public async Task<bool> RemoveVLAN(int vlanId)
        {
            if (apiUrl == null || apiToken == null || _httpClient == null)
            {
                Console.WriteLine("Hostname, Token, VLANId oder HttpClient sind nicht verfügbar");
                return false;
            }
            else
            {
                string requestUri = $"http://{apiUrl}/access.php?action=deleteVLAN&token={apiToken}&vlanId={vlanId}";
                var response = await _httpClient.DeleteAsync(requestUri);

                // ausgabe response content
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);

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
                string requestUri = $"http://{apiUrl}/access.php?action=updateVLAN&token={apiToken}&vlanId={vlanId}&vlanName={vlanName}";
                var response = await _httpClient.PutAsync(requestUri, null);

                // ausgabe response content
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);

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
                return false;
            }
        }

        // CreateVLAN(comboPortgruppe_Name.Text)
        public async Task<bool> CreateVLAN(string vlanName)
        {
            if (apiUrl == null || apiToken == null || _httpClient == null)
            {
                Console.WriteLine("Hostname, Token, VLANName oder HttpClient sind nicht verfügbar");
                return false;
            }
            else
            {
                string requestUri = $"http://{apiUrl}/access.php?action=createVLAN&token={apiToken}&vlanName={vlanName}";
                var response = await _httpClient.PostAsync(requestUri, null);

                // ausgabe response content
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);

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
