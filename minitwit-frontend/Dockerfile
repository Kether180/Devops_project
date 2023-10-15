FROM node:alpine
WORKDIR /app
COPY package.json ./
COPY package-lock.json ./
COPY ./ ./
RUN npm i
# Expose port 3000
EXPOSE 3000
ENV HOST=0.0.0.0
CMD ["npm", "run", "start"]