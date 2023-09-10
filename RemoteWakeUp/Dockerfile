FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 9000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RemoteWakeUp/RemoteWakeUp.csproj", "RemoteWakeUp/"]
RUN dotnet restore "RemoteWakeUp/RemoteWakeUp.csproj"
COPY . .
WORKDIR "/src/RemoteWakeUp"
RUN dotnet build "RemoteWakeUp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RemoteWakeUp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RemoteWakeUp.dll"]
