FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["mutation-app/mutation-app.csproj", "mutation-app/"]
RUN dotnet restore "mutation-app/mutation-app.csproj"
COPY . .
WORKDIR "/src/mutation-app"
RUN dotnet build "mutation-app.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "mutation-app.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mutation-app.dll"]
