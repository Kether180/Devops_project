FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /App

# Copy everything
COPY *.csproj ./
# Restore as distinct layers

RUN dotnet restore MiniTwit.csproj
COPY . ./
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build-env /App/out .

EXPOSE 5050

ENTRYPOINT ["dotnet", "MiniTwit.dll"]
