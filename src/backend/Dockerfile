# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY RecipeCollection.Api/RecipeCollection.Api.csproj RecipeCollection.Api/
COPY RecipeCollection.Data/RecipeCollection.Data.csproj RecipeCollection.Data/
COPY RecipeCollection.Domain/RecipeCollection.Domain.csproj RecipeCollection.Domain/
COPY RecipeCollection.ServiceDefaults/RecipeCollection.ServiceDefaults.csproj RecipeCollection.ServiceDefaults/
RUN dotnet restore RecipeCollection.Api/RecipeCollection.Api.csproj

COPY . .
RUN dotnet publish RecipeCollection.Api/RecipeCollection.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends tesseract-ocr libleptonica-dev libtesseract-dev \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RecipeCollection.Api.dll"]
