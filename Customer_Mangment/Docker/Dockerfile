# stage 1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY Customer_Mangment/*.csproj ./Customer_Mangment/
COPY Customer_Mangment.ServiceDefaults/*.csproj ./Customer_Mangment.ServiceDefaults/

RUN dotnet restore ./Customer_Mangment/Customer_Mangment.csproj

# Copy all source
COPY Customer_Mangment/. ./Customer_Mangment/
COPY Customer_Mangment.ServiceDefaults/. ./Customer_Mangment.ServiceDefaults/

RUN dotnet publish ./Customer_Mangment/Customer_Mangment.csproj -c Release -o /app

# stage 2
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Customer_Mangment.dll"]