import requests
import json

def read_file_and_send_data(file_path, api_url):
    # Datei lesen
    with open(file_path, 'r') as file:
        data = json.load(file)  # JSON-Daten direkt einlesen

    # Jedes Element in 'data' ist bereits ein passendes Dictionary,
    # also können wir die Daten direkt konvertieren
    json_data = json.dumps(data)
    
    # Daten an die WebAPI senden
    headers = {'Content-Type': 'application/json'}
    response = requests.post(api_url, data=json_data, headers=headers)
    
    # Antwort ausgeben
    print(f"Status Code: {response.status_code}")
    print(f"Response Body: {response.text}")

# Pfad zur Datei
file_path = '/tmp/vm_mac_list.csv'
# URL der WebAPI
api_url = 'http://{{apiUrl}}/db_importMAC.php?action=updateInterface'

# Funktion ausführen
read_file_and_send_data(file_path, api_url)