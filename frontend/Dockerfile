# Frontend de Gestión de Productos (React 19), servido con nginx.

# ---------- Compilación ----------
FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
# Vite escribe la dirección de la API dentro de los archivos compilados.
ARG VITE_API_URL=http://localhost:5142/api
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# ---------- Servidor ----------
# nginx sin privilegios de administrador; escucha en el puerto 8080.
FROM nginxinc/nginx-unprivileged:1.29-alpine
COPY nginx/default.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html

# Origen de la API que permite la política de seguridad de contenido (CSP).
ENV API_ORIGIN=http://localhost:5142
EXPOSE 8080
