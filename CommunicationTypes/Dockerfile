FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["mutation-app.csproj", "CommunicationTypes"]
COPY ["src", "mutation-app/"]
COPY ["Program.cs", "mutation-app/"]
WORKDIR "/src/mutation-app"
RUN dotnet build mutation-app.csproj -c Release -o /app/build
