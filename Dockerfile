FROM node:22-alpine AS frontend-build
WORKDIR /src
COPY package.json package-lock.json ./
RUN npm ci
COPY tailwind.config.js ./
COPY Views ./Views
COPY Areas ./Areas
COPY wwwroot/css/input.css ./wwwroot/css/input.css
RUN npm run css:build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ASPNET Ecommerce.csproj", "./"]
RUN dotnet restore "ASPNET Ecommerce.csproj"
COPY . .
COPY --from=frontend-build /src/wwwroot/css/output.css ./wwwroot/css/output.css
RUN dotnet publish "ASPNET Ecommerce.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ASPNET Ecommerce.dll"]
