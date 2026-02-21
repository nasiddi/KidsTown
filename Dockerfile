# Stage 1: Build React frontend with Vite
FROM node:20-alpine AS frontend-build
WORKDIR /frontend
COPY src/Application/ClientApp/kidstown/package.json ./
RUN npm install
COPY src/Application/ClientApp/kidstown/ ./
RUN npm run build

# Stage 2: Build .NET application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY KidsTown.sln Directory.Build.props Directory.Packages.props ./
COPY src/ src/
COPY tests/ tests/
RUN dotnet restore
RUN dotnet publish src/Application/Application.csproj -c Release -o /app/publish --no-restore

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=backend-build /app/publish .
COPY --from=frontend-build /frontend/dist ./wwwroot/
EXPOSE 5000
ENTRYPOINT ["dotnet", "Application.dll"]
