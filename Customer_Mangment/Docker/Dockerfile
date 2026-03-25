# stage 1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# dependency
COPY *.csproj .
RUN dotnet restore

# Publish
COPY . .
RUN dotnet publish -c Release -o /app


# stage 2

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Customer_Mangment.dll"]