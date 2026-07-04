# Frontend'i derler (React/Vite statik çıktısı)
FROM node:20-alpine AS frontend-build
WORKDIR /web
COPY web/package.json web/package-lock.json ./
RUN npm ci
COPY web/ ./
RUN npm run build

# Backend'i derler (yalnızca src/ altındaki proje referans grafiği gerekli)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY src/ ./src/
RUN dotnet publish src/PriceFinderAI.Api/PriceFinderAI.Api.csproj -c Release -o /app/publish

# Çalışma zamanı — tek imaj, API + statik frontend dosyaları birlikte
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=backend-build /app/publish .
COPY --from=frontend-build /web/dist ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PriceFinderAI.Api.dll"]
