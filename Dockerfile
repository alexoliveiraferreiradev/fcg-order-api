FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release


WORKDIR /src

COPY ["src/Fcg.Orders.API/Fcg.Orders.API.csproj", "Fcg.Orders.API/"]
COPY ["src/Fcg.Orders.Application/Fcg.Orders.Application.csproj", "Fcg.Orders.Application/"]
COPY ["src/Fcg.Orders.Domain/Fcg.Orders.Domain.csproj", "Fcg.Orders.Domain/"]
COPY ["src/Fcg.Orders.Infrastructure/Fcg.Orders.Infrastructure.csproj", "Fcg.Orders.Infrastructure/"]

RUN dotnet restore "./Fcg.Orders.API/Fcg.Orders.API.csproj"

COPY src/ .
WORKDIR "/src/Fcg.Orders.API"
RUN dotnet build "./Fcg.Orders.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet ef migrations bundle --self-contained -r linux-x64 -o /app/build/efbundle

FROM build AS publish
RUN dotnet publish "./Fcg.Orders.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root

COPY --from=build /app/build/efbundle ./efbundle
RUN chmod +x ./efbundle
USER app

ENTRYPOINT []
CMD ["dotnet", "Fcg.Orders.API.dll"]