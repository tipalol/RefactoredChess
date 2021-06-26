# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY Chess.Core/*.csproj ./Chess.Core/
COPY Chess.Core.Tests/*.csproj ./Chess.Core.Tests/
COPY Chess.Infrastructure/*.csproj ./Chess.Infrastucture/
COPY Chess.Web/*.csproj ./Chess.Web/

RUN dotnet nuget locals --clear all
RUN dotnet restore -f --no-cache --ignore-failed-sources
RUN pwd

# copy everything else and build app
COPY Chess.Core/. ./Chess.Core/
COPY Chess.Core.Tests/. ./Chess.Core.Tests/
COPY Chess.Infrastructure/. ./Chess.Infrastucture/
COPY Chess.Web/. ./Chess.Web/
WORKDIR /source/Chess.Web
RUN dotnet publish -c release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app ./

COPY sf_13/. /app/sf_13/.

CMD echo "file contents" > findme1.txt

ENTRYPOINT ["dotnet", "Chess.Web.dll"]