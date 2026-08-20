FROM node:24-alpine AS build
WORKDIR /app

COPY src/ChangeMe.Frontend/package*.json ./
RUN npm ci

COPY src/ChangeMe.Frontend/ ./
RUN npm run build

FROM nginx:1.29-alpine AS final
WORKDIR /usr/share/nginx/html

RUN apk add --no-cache nginx-module-njs \
  && sed -i '1iload_module modules/ngx_http_js_module.so;' /etc/nginx/nginx.conf

COPY src/ChangeMe.Frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY src/ChangeMe.Frontend/nginx/00-csp-nonce.conf /etc/nginx/conf.d/00-csp-nonce.conf
COPY src/ChangeMe.Frontend/nginx/security-headers.conf /etc/nginx/snippets/security-headers.conf.template
COPY src/ChangeMe.Frontend/nginx/csp-nonce.js /etc/nginx/njs/csp-nonce.js
COPY src/ChangeMe.Frontend/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh
COPY --from=build /app/dist/ChangeMe.Frontend/browser/ ./

EXPOSE 80
ENTRYPOINT ["/docker-entrypoint.sh"]
