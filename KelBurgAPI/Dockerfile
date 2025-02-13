FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["KelBurgAPI.csproj", "./"]
RUN dotnet restore "KelBurgAPI.csproj"

COPY . .
RUN dotnet publish "KelBurgAPI.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
WORKDIR /app

COPY --from=build /app/publish .

RUN chown -R appuser:appgroup /app && chmod -R 755 /app
USER appuser

ENTRYPOINT ["dotnet", "KelBurgAPI.dll"]
