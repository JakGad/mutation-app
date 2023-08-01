FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["mutation-seeker-orchestrator.csproj", "mutation-app/"]
RUN dotnet nuget add source "http://nexus.jbox.local/repository/nuget-hosted/"
RUN dotnet restore "mutation-app/mutation-seeker-orchestrator.csproj"
COPY ["src", "mutation-app/"]
COPY ["Program.cs", "mutation-app/"]
WORKDIR "/src/mutation-app"
RUN dotnet build mutation-seeker-orchestrator.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "mutation-seeker-orchestrator.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV QUEUE_ADDRESS="localhost"
ENV QUEUE_LOGIN="myuser"
ENV QUEUE_PASSWORD="mypassword"
ENV QUEUE_PORT="5672"
ENV ASPNETCORE_URLS="http://localhost:5208"
ENV MONGO_CONNECTION_STRING="mongodb://root:example@localhost:27017"
ENV DATABASE_NAME="mutation"
ENV COLLECTION_NAME="mutation"
ENV OPTL_COLLECTOR="http://collector:4317"
ENTRYPOINT ["dotnet", "mutation-seeker-orchestrator.dll"]
