# Multi-stage Dockerfile for PaymentReport.Api
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore first for better caching
COPY *.sln . 2>/dev/null || true
COPY PaymentReport.Api.csproj ./
RUN dotnet restore PaymentReport.Api.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish PaymentReport.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Render (and many PaaS) provide PORT via env var
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "PaymentReport.Api.dll"]
