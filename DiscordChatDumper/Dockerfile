FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DiscordChatDumper/DiscordChatDumper.csproj", "DiscordChatDumper/"]
RUN dotnet restore "DiscordChatDumper/DiscordChatDumper.csproj"
COPY . .
WORKDIR "/src/DiscordChatDumper"
RUN dotnet build "DiscordChatDumper.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiscordChatDumper.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["sh", "-c", "dotnet DiscordChatDumper.dll dump guild"]
