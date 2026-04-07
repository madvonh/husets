#!/bin/sh
set -eu

BACKEND_URL="${API_HTTP:-${services__api__http__0:-}}"
echo "Resolved BACKEND_URL=${BACKEND_URL}"

if [ -z "${BACKEND_URL}" ]; then
  echo "Missing backend URL env (API_HTTP/services__api__http__0)"
  exit 1
fi

sed "s|__API_HTTP__|${BACKEND_URL}|g" /opt/nginx/default.conf.template > /etc/nginx/conf.d/default.conf
exec nginx -g 'daemon off;'
