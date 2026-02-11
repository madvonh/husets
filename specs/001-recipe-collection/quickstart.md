# Quickstart: Recipe Collection (Local Development)

**Feature**: `001-recipe-collection`  
**Date**: 2026-02-10

This guide covers running the Recipe Collection app locally for development.

## Prerequisites

### Tools

- **.NET SDK** 8.0 or higher
- **Node.js** 18+ and **npm** 9+
- **Docker Desktop** (for Cosmos DB emulator)
- **Azure CLI** (optional, for Blob Storage emulator or real Azure resources)
- **Git** (to clone the repo)

### Azure Resources (Choose One)

#### Option A: Local Emulators (Recommended for Dev)

- **Cosmos DB**: Use the [Azure Cosmos DB Docker emulator](https://learn.microsoft.com/azure/cosmos-db/docker-emulator-linux)
- **Blob Storage**: Use [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) (npm package or Docker)
- **Azure AI Vision (OCR)**: Requires a real Azure resource (free tier available)

#### Option B: Real Azure Resources

- Create a Cosmos DB account (free tier: 1000 RU/s, 25 GB)
- Create a Blob Storage account (LRS, hot tier)
- Create an Azure AI Vision resource (free tier: 20 calls/min)

---

## Step 1: Start Local Azure Services

### Cosmos DB Emulator (Docker)

```powershell
# Pull and run Cosmos DB Linux emulator
docker run -d `
  -p 8081:8081 `
  -p 10350-10355:10350-10355 `
  --name cosmosdb-emulator `
  mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest

# Wait ~60 seconds for startup, then check health
Start-Sleep -Seconds 60
Invoke-WebRequest -Uri "https://localhost:8081/_explorer/index.html" -SkipCertificateCheck
```

**Connection String** (emulator default):
```
AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==
```

### Azurite (Blob Storage Emulator)

```powershell
# Install Azurite globally (if not already installed)
npm install -g azurite

# Start Azurite (blob service only, on default port 10000)
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

**Connection String** (Azurite default):
```
DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;
```

### Azure AI Vision (OCR)

You'll need a real Azure resource for this. Create one via Azure Portal or CLI:

```powershell
# Create resource group (if needed)
az group create --name rg-recipe-dev --location eastus

# Create Computer Vision resource (free tier)
az cognitiveservices account create `
  --name cv-recipe-dev `
  --resource-group rg-recipe-dev `
  --kind ComputerVision `
  --sku F0 `
  --location eastus `
  --yes

# Get endpoint and key
az cognitiveservices account show `
  --name cv-recipe-dev `
  --resource-group rg-recipe-dev `
  --query properties.endpoint -o tsv

az cognitiveservices account keys list `
  --name cv-recipe-dev `
  --resource-group rg-recipe-dev `
  --query key1 -o tsv
```

---

## Step 2: Configure Backend Environment Variables

**Note**: The backend includes in-memory fallback implementations for testing and local development. If Cosmos DB or Blob Storage connection strings are not configured, the app will use in-memory storage (data is not persisted). Azure AI Vision OCR requires a real endpoint and API key to function; otherwise, placeholder text will be returned.

### Required Environment Variables

- **CosmosDb:ConnectionString**: Azure Cosmos DB connection string or emulator connection string
- **CosmosDb:DatabaseName**: Database name (default: `RecipeCollection`)
- **CosmosDb:ContainerName**: Container name (default: `RecipeData`)
- **BlobStorage:ConnectionString**: Azure Blob Storage or Azurite connection string
- **BlobStorage:ContainerName**: Blob container name (default: `recipe-images`)
- **AzureVision:Endpoint**: Azure AI Vision endpoint URL
- **AzureVision:ApiKey**: Azure AI Vision API key
- **AllowedOrigins**: Array of allowed CORS origins (e.g., `["http://localhost:5173"]`)

### Configuration File

Create `backend/src/RecipeApi/appsettings.Development.json` (or use environment variables):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
    "DatabaseName": "RecipeCollection",
    "ContainerName": "RecipeData"
  },
  "BlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;",
    "ContainerName": "recipe-images"
  },
  "AzureVision": {
    "Endpoint": "https://cv-recipe-dev.cognitiveservices.azure.com/",
    "ApiKey": "<YOUR_VISION_API_KEY>"
  },
  "AllowedOrigins": ["http://localhost:5173"]
}
```

**Security Note**: Never commit real API keys. Use User Secrets for local dev:

```powershell
cd backend/src/RecipeApi
dotnet user-secrets set "AzureVision:ApiKey" "<YOUR_KEY>"
```

---

## Step 3: Run Backend

```powershell
cd backend/src/RecipeApi

# Restore dependencies
dotnet restore

# Run tests (after implementation)
cd ../../tests/RecipeApi.Tests
dotnet test

# Run API
cd ../../src/RecipeApi
dotnet run
```

**API should be running at**: `https://localhost:5001` (or `http://localhost:5000`)

Test health endpoint:
```powershell
Invoke-RestMethod -Uri "https://localhost:5001/health" -SkipCertificateCheck
```

---

## Step 4: Configure Frontend Environment Variables

Create `frontend/.env.local`:

```env
VITE_API_BASE_URL=https://localhost:5001
```

---

## Step 5: Run Frontend

```powershell
cd frontend

# Install dependencies
npm install

# Run dev server
npm run dev
```

**Frontend should be running at**: `http://localhost:5173`

Open the browser and verify:
- Home page loads
- Search UI is visible
- Mobile viewport is usable (responsive)

---

## Step 6: Initialize Cosmos DB (First Run)

On first run, the backend should auto-create:
- Database: `RecipeCollection`
- Container: `RecipeData` (partition key: `/pk`)

If not auto-created, run this script manually (Cosmos SDK or Data Explorer):

```csharp
// In Program.cs or a /setup endpoint (dev only)
var cosmosClient = new CosmosClient(connectionString);
var database = await cosmosClient.CreateDatabaseIfNotExistsAsync("RecipeCollection");
await database.Database.CreateContainerIfNotExistsAsync(
    new ContainerProperties("RecipeData", "/pk"));
```

---

## Step 7: Initialize Blob Storage Container

Azurite won't auto-create containers. Use Azure Storage Explorer or CLI:

```powershell
# Using Azure CLI (with Azurite connection string)
$conn = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"

az storage container create --name recipe-images --connection-string $conn
```

Or let the backend auto-create on first upload (add logic in BlobService).

---

## Testing the End-to-End Flow

1. **Upload a recipe photo**:
   - Open frontend (`http://localhost:5173`)
   - Navigate to "Add Recipe" page
   - Upload a recipe image (JPEG/PNG)
   - Verify OCR text is displayed and editable

2. **Save the recipe**:
   - Edit the OCR text if needed
   - Enter a title
   - Add optional tags (e.g., "dessert")
   - Save
   - Verify redirect to recipe detail page

3. **Search for the recipe**:
   - Return to home page
   - Search by keyword (e.g., "chocolate")
   - Verify recipe appears in results
   - Filter by tag (e.g., "dessert")
   - Verify recipe appears

4. **Manage tags**:
   - Open recipe detail page
   - Add a new tag (e.g., "cookies")
   - Verify tag appears
   - Remove a tag
   - Verify tag is removed

---

## Troubleshooting

### Cosmos DB Emulator Won't Start

- Ensure Docker is running
- Check ports 8081, 10350-10355 are not in use
- Review Docker logs: `docker logs cosmosdb-emulator`
- On Windows, you may need to use the Windows emulator instead: [Cosmos DB Windows Emulator](https://learn.microsoft.com/azure/cosmos-db/emulator)

### Azurite Connection Refused

- Ensure Azurite is running: `Get-Process azurite`
- Check port 10000 is not in use
- Verify connection string in `appsettings.Development.json`

### OCR Returns Empty Text

- Check Azure Vision endpoint and key are correct
- Ensure the uploaded image contains readable text
- Check API logs for OCR errors (correlation ID helps tracing)
- Verify Azure Vision quota isn't exhausted (free tier: 20/min)

### CORS Errors

- Ensure `AllowedOrigins` in backend config includes the frontend URL (`http://localhost:5173`)
- Check browser console for specific CORS errors
- Verify backend is running on HTTPS (or allow HTTP in CORS policy for dev)

### Recipe Not Appearing in Search

- Check Cosmos Data Explorer to verify the recipe document was created
- Verify `searchText` and `normalizedTags` fields are populated
- Check search query parameters in browser network tab
- Review backend logs for query errors

---

## Next Steps

- Implement unit tests for ingredient parsing (`backend/tests/`)
- Implement frontend tests (Vitest + React Testing Library)
- Add Dockerfile for backend (containerized deployment)
- Configure CI/CD pipeline (GitHub Actions)
- Deploy to Azure (Static Web App + App Service + real Cosmos DB)

---

## Useful Commands

```powershell
# Backend
dotnet build                  # Build
dotnet test                   # Run tests
dotnet run                    # Run API

# Frontend
npm install                   # Install dependencies
npm run dev                   # Dev server
npm run build                 # Production build
npm run preview               # Preview production build
npm run lint                  # Lint
npm run type-check            # TypeScript check

# Docker
docker ps                     # List running containers
docker logs <container>       # View logs
docker stop <container>       # Stop container
docker start <container>      # Start container
docker rm <container>         # Remove container

# Azurite
azurite --silent --location <path>        # Start
Ctrl+C                                     # Stop
```

---

## References

- [Azure Cosmos DB Emulator (Docker)](https://learn.microsoft.com/azure/cosmos-db/docker-emulator-linux)
- [Azurite (Blob Storage Emulator)](https://learn.microsoft.com/azure/storage/common/storage-use-azurite)
- [Azure AI Vision (Read OCR)](https://learn.microsoft.com/azure/ai-services/computer-vision/overview-ocr)
- [.NET Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [Vite + React + TypeScript](https://vitejs.dev/guide/)
- [Tailwind CSS](https://tailwindcss.com/docs)
