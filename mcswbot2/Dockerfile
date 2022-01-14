#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
RUN apt-get update && apt-get install -y libgdiplus
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["mcswbot2/mcswbot2.csproj", "mcswbot2/"]
RUN dotnet restore "mcswbot2/mcswbot2.csproj"
COPY . .
WORKDIR "/src/mcswbot2"
RUN dotnet build "mcswbot2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "mcswbot2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mcswbot2.dll"]
