# ----- STAGE 1: Construcción -----
FROM node:20-alpine AS builder
WORKDIR /app

# Copiamos archivos de dependencias e instalamos todo
COPY package*.json ./
RUN npm ci

# Copiamos código y compilamos
COPY . .
RUN npm run build

# ----- STAGE 2: Producción -----
FROM node:20-alpine
WORKDIR /app

# Instalamos solo dependencias ligeras para producción
COPY package*.json ./
RUN npm ci --only=production

# Traemos el código ya compilado
COPY --from=builder /app/dist ./dist

# Puerto y comando de arranque
EXPOSE 3000
CMD ["node", "dist/main.js"]