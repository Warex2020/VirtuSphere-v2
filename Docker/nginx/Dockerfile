FROM nginx:latest
COPY ./default.conf /etc/nginx/conf.d/default.conf

# Erstellen eines Verzeichnisses für SSL-Zertifikate
RUN mkdir -p /etc/nginx/ssl

# Kopieren des Initialisierungsskripts
COPY init.sh /init.sh
RUN chmod +x /init.sh

# Setzen des Entrypoints
ENTRYPOINT ["/init.sh"]

EXPOSE 80 443