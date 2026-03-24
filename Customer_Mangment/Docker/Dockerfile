# stage 1
From mcr.microsoft.com/dotnet/sdk:9.0 AS build
workdir /src

# dependency
COPY *.csproj .
RUN dotnet restore

# Publish
COPY . .
RUN dotnet publish -c Release -o /app


# stage 2

From mcr.microsoft.com/dotnet/aspnet:9.0
workdir /app
copy --from=build /app .

entrypoint ["dotnet", "Customer_Mangment.dll"]