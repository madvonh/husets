# syntax=docker/dockerfile:1

FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

RUN npm run build

FROM nginx:1.27-alpine AS runtime

COPY nginx.conf.template /opt/nginx/default.conf.template
COPY start-nginx.sh /opt/nginx/start-nginx.sh
RUN chmod +x /opt/nginx/start-nginx.sh
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["/bin/sh", "/opt/nginx/start-nginx.sh"]
