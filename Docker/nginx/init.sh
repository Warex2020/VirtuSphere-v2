#!/bin/bash

# Pfad zu den Zertifikaten
SSL_DIR="/etc/nginx/ssl"
SSL_CERT="$SSL_DIR/nginx-selfsigned.crt"
SSL_KEY="$SSL_DIR/nginx-selfsigned.key"

# Überprüfen, ob Zertifikat und Schlüssel existieren, andernfalls erstellen
if [ ! -f "$SSL_CERT" ] || [ ! -f "$SSL_KEY" ]; then
    echo "Generating self-signed SSL certificate..."
    mkdir -p "$SSL_DIR"
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "$SSL_KEY" -out "$SSL_CERT" -subj "/CN=${SSL_SUBJECT}"
else
    echo "SSL certificate already exists."
fi

# Starten von nginx
nginx -g 'daemon off;'
