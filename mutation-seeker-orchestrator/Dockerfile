FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["mutation-seeker-orchestrator/mutation-seeker-orchestrator.csproj", "mutation-seeker-orchestrator/"]
RUN dotnet restore "mutation-seeker-orchestrator/mutation-seeker-orchestrator.csproj"
COPY . .
WORKDIR "/src/mutation-seeker-orchestrator"
RUN dotnet build "mutation-seeker-orchestrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "mutation-seeker-orchestrator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mutation-seeker-orchestrator.dll"]
