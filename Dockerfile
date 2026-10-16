FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src


RUN dotnet tool install --global dotnet-ef --version 9.0.18
ENV PATH="$PATH:/root/.dotnet/tools"

COPY ["src/Fcg.Orders.API/Fcg.Orders.API.csproj", "Fcg.Orders.API/"]
COPY ["src/Fcg.Orders.Application/Fcg.Orders.Application.csproj", "Fcg.Orders.Application/"]
COPY ["src/Fcg.Orders.Domain/Fcg.Orders.Domain.csproj", "Fcg.Orders.Domain/"]
COPY ["src/Fcg.Orders.Infrastructure/Fcg.Orders.Infrastructure.csproj", "Fcg.Orders.Infrastructure/"]

RUN dotnet restore "./Fcg.Orders.API/Fcg.Orders.API.csproj"

COPY src/ .
WORKDIR "/src/Fcg.Orders.API"

RUN dotnet publish "./Fcg.Orders.API.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false
RUN dotnet ef migrations bundle --no-build --configuration $BUILD_CONFIGURATION --self-contained -r linux-x64 -o /app/efbundle

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /app/efbundle ./efbundle

ENTRYPOINT []
CMD ["dotnet", "Fcg.Orders.API.dll"]
