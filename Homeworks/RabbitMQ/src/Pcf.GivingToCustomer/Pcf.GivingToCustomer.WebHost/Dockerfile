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
COPY ["Pcf.GivingToCustomer/Pcf.GivingToCustomer.WebHost/Pcf.GivingToCustomer.WebHost.csproj", "Pcf.GivingToCustomer/Pcf.GivingToCustomer.WebHost/"]
COPY ["Pcf.GivingToCustomer/Pcf.GivingToCustomer.Core/Pcf.GivingToCustomer.Core.csproj", "Pcf.GivingToCustomer/Pcf.GivingToCustomer.Core/"]
COPY ["Pcf.GivingToCustomer/Pcf.GivingToCustomer.DataAccess/Pcf.GivingToCustomer.DataAccess.csproj", "Pcf.GivingToCustomer/Pcf.GivingToCustomer.DataAccess/"]
COPY ["Pcf.GivingToCustomer/Pcf.GivingToCustomer.Integration/Pcf.GivingToCustomer.Integration.csproj", "Pcf.GivingToCustomer/Pcf.GivingToCustomer.Integration/"]
RUN dotnet restore "./Pcf.GivingToCustomer/Pcf.GivingToCustomer.WebHost/Pcf.GivingToCustomer.WebHost.csproj"
COPY . .
WORKDIR "/src/Pcf.GivingToCustomer/Pcf.GivingToCustomer.WebHost"
RUN dotnet build "./Pcf.GivingToCustomer.WebHost.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Pcf.GivingToCustomer.WebHost.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pcf.GivingToCustomer.WebHost.dll"]