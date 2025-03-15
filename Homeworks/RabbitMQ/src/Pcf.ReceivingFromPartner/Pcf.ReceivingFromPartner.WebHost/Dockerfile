# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.WebHost/Pcf.ReceivingFromPartner.WebHost.csproj", "Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.WebHost/"]
COPY ["Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.Core/Pcf.ReceivingFromPartner.Core.csproj", "Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.Core/"]
COPY ["Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.DataAccess/Pcf.ReceivingFromPartner.DataAccess.csproj", "Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.DataAccess/"]
COPY ["Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.Integration/Pcf.ReceivingFromPartner.Integration.csproj", "Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.Integration/"]
RUN dotnet restore "./Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.WebHost/Pcf.ReceivingFromPartner.WebHost.csproj"
COPY . .
WORKDIR "/src/Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.WebHost"
RUN dotnet build "./Pcf.ReceivingFromPartner.WebHost.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Pcf.ReceivingFromPartner.WebHost.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pcf.ReceivingFromPartner.WebHost.dll"]