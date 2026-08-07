FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /App

COPY ../ ./
RUN dotnet restore
RUN dotnet publish -o out

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /App
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "RScore.Consumer.Match.dll"]