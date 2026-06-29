# Root Dockerfile for Render
# This builds the backend service located in the backend/ folder.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore project dependencies for the backend project
COPY backend/PaymentReport.Api.csproj ./backend/
WORKDIR /src/backend
RUN dotnet restore PaymentReport.Api.csproj

# Copy the backend source and publish
COPY backend/. ./
RUN dotnet publish PaymentReport.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ENTRYPOINT ["dotnet", "PaymentReport.Api.dll"]
