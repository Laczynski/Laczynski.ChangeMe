#!/bin/sh
set -e

API_URL="${CHANGE_ME_API_URL:-/api/v1}"
CONFIG_PATH=/usr/share/nginx/html/runtime-config.js
HEADERS_PATH=/etc/nginx/snippets/security-headers.conf
HEADERS_TEMPLATE_PATH=/etc/nginx/snippets/security-headers.conf.template
escaped_url=$(printf '%s' "$API_URL" | sed 's/\\/\\\\/g; s/"/\\"/g')

cat > "$CONFIG_PATH" <<EOF
window.__CHANGE_ME_CONFIG__ = {
  apiUrl: "${escaped_url}"
};
EOF

csp_connect_src_extra=""
case "$API_URL" in
  http://*|https://*)
    api_origin=$(printf '%s' "$API_URL" | sed -E 's|^(https?://[^/]+).*|\1|')
    csp_connect_src_extra=" ${api_origin}"
    ;;
esac

cp "$HEADERS_TEMPLATE_PATH" "$HEADERS_PATH"
sed -i "s#__CSP_CONNECT_SRC_EXTRA__#${csp_connect_src_extra}#g" "$HEADERS_PATH"

exec nginx -g 'daemon off;'
