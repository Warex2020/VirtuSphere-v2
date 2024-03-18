import json
import csv

input_file_path = '/tmp/vm_infos.json'
output_file_path = '/tmp/vm_network_details.csv'
# URL der WebAPI
api_url = 'http://{{apiUrl}}/db_importMAC.php?action=updateInterface'


with open(input_file_path, 'r') as json_file:
    vm_infos = json.load(json_file)

with open(output_file_path, 'w', newline='') as csv_file:
    csv_writer = csv.writer(csv_file)
    csv_writer.writerow(['VM Name', 'Interface', 'MAC Address'])

    for vm_info in vm_infos:
        if 'instance' in vm_info and 'guest' in vm_info['instance'] and 'net' in vm_info['instance']['guest']:
            for net_info in vm_info['instance']['guest']['net']:
                csv_writer.writerow([vm_info['item']['vm_name'], net_info.get('network'), net_info.get('macAddress')])


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



# Funktion ausführen
read_file_and_send_data(output_file_path, api_url)