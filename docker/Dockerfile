FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/RScore.Consumer.Match.Domain/RScore.Consumer.Match.Domain.csproj", "src/RScore.Consumer.Match.Domain/"]
COPY ["src/RScore.Consumer.Match.Application/RScore.Consumer.Match.Application.csproj", "src/RScore.Consumer.Match.Application/"]
COPY ["src/RScore.Consumer.Match.Infrastructure/RScore.Consumer.Match.Infrastructure.csproj", "src/RScore.Consumer.Match.Infrastructure/"]
COPY ["src/RScore.Consumer.Match.Presentation/RScore.Consumer.Match.Presentation.csproj", "src/RScore.Consumer.Match.Presentation/"]

RUN dotnet restore "src/RScore.Consumer.Match.Presentation/RScore.Consumer.Match.Presentation.csproj"

COPY src/ .

WORKDIR "/src/RScore.Consumer.Match.Presentation"

RUN dotnet build "RScore.Consumer.Match.Presentation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RScore.Consumer.Match.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RScore.Consumer.Match.Presentation.dll"]