FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Backend.Api/Backend.Api.csproj", "src/Backend.Api/"]
RUN dotnet restore "src/Backend.Api/Backend.Api.csproj"
COPY . .
WORKDIR "/src/src/Backend.Api"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Backend.Api.dll"]
