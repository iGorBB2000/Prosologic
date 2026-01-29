FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Prosologic.Runtime/Prosologic.Runtime.csproj", "Prosologic.Runtime/"]
RUN dotnet restore "Prosologic.Runtime/Prosologic.Runtime.csproj"
COPY . .
WORKDIR "/src/Prosologic.Runtime"
RUN dotnet build "./Prosologic.Runtime.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Prosologic.Runtime.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Prosologic.Runtime.dll"]
