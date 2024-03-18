import json
import csv

# Pfad zur JSON-Datei
file_path = '/tmp/vm_infos.json'


api_url = 'http://{{apiUrl}}/db_importMAC.php?action=updateInterface'

import requests
import json

def send_data_to_server(file_path, api_url):
    # Versuche, die Datei zu oeffnen und den Inhalt zu lesen
    try:
        with open(file_path, 'r') as file:
            data = json.load(file)
    except Exception as e:
        print(f"Fehler beim Lesen der Datei: {e}")
        return

    # Konvertiere die Daten in einen JSON-String
    json_data = json.dumps(data)

    # Setze die HTTP Headers für die POST-Anfrage
    headers = {'Content-Type': 'application/json'}

    # Versuche, die Daten an die WebAPI zu senden
    try:
        response = requests.post(api_url, data=json_data, headers=headers)
        
        # Überprüfe den Statuscode der Antwort
        if response.status_code == 200:
            print("Daten erfolgreich gesendet.")
        else:
            print(f"Fehler beim Senden der Daten: {response.status_code}")
        
        # Gib den Antworttext aus (optional)
        print(f"Antwort vom Server: {response.text}")
        
    except Exception as e:
        print(f"Fehler beim Senden der Daten: {e}")


# Funktion ausfuehren
send_data_to_server(file_path, api_url)
