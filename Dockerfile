
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["src/Marketplace.Web/Marketplace.Web.csproj", "Marketplace.Web/"]
COPY ["src/Marketplace.Application/Marketplace.Application.csproj", "Marketplace.Application/"]
COPY ["src/Marketplace.Domain/Marketplace.Domain.csproj", "Marketplace.Domain/"]
COPY ["src/Marketplace.Infrastructure/Marketplace.Infrastructure.csproj", "Marketplace.Infrastructure/"]


RUN dotnet restore "Marketplace.Web/Marketplace.Web.csproj" \
    --disable-parallel false \
    --verbosity minimal


COPY src/ .
WORKDIR "/src/Marketplace.Web"

RUN dotnet publish "Marketplace.Web.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --verbosity minimal


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80
ENTRYPOINT ["dotnet", "Marketplace.Web.dll"]