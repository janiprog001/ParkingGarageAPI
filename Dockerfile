FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# A projekt fájl másolása és a függőségek telepítése
COPY *.csproj ./
RUN dotnet restore

# A teljes kód másolása és az alkalmazás buildelése
COPY . ./
RUN dotnet publish -c Release -o /app

# Futtatási fázis
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./

# Környezeti változók beállítása - ezeket majd a Render felületen is meg kell adni
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV RENDER=true

# A port kiajánlása
EXPOSE 8080

# Az alkalmazás indítása
ENTRYPOINT ["dotnet", "ParkingGarageAPI.dll"] 