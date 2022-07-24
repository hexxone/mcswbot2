#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
RUN apt-get update && apt-get install -y libgdiplus
# use Germany as default TimeZone
RUN ln -sf /usr/share/zoneinfo/Europe/Berlin /etc/localtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["./mcswbot2/McswBot2.csproj", "mcswbot2/"]
RUN dotnet restore "mcswbot2/McswBot2.csproj"
COPY . .
WORKDIR "/src/mcswbot2"
RUN dotnet build "McswBot2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "McswBot2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "McswBot2.dll"]
