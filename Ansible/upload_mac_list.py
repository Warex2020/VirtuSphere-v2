import requests
import json

def read_file_and_send_data(file_path, api_url):
    # Datei lesen
    with open(file_path, 'r') as file:
        lines = file.readlines()
    
    # Datenstruktur fuer JSON vorbereiten
    data = []
    for line in lines:
        vm_name, interface, mac_address = line.strip().split(';')
        data.append({
            "vm_name": vm_name,
            "interface": interface,
            "mac_address": mac_address
        })
    
    # Daten als JSON konvertieren
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
api_url = 'http://{{WEBAPI}}/db_importMAC.php?action=updateInterface'

# Funktion ausführen
read_file_and_send_data(file_path, api_url)
