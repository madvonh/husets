# OCR Service Configuration

The Recipe API supports two OCR implementations for extracting text from recipe images:

## 1. Azure Computer Vision (Production)

For production use with Azure's cloud OCR service:

### Configuration
Add to `appsettings.json` or `appsettings.local.json`:

```json
{
  "AzureVision": {
    "Endpoint": "https://your-resource.cognitiveservices.azure.com/",
    "ApiKey": "your-api-key-here"
  }
}
```

### Features
- ✓ High accuracy
- ✓ Fast processing
- ✓ Supports multiple languages
- ✗ Requires Azure subscription
- ✗ Internet connection required

## 2. Tesseract OCR (Local Development)

For local development without Azure dependencies:

### Setup (Already Done!)
The project automatically uses Tesseract when Azure Vision credentials are not configured.

### Prerequisites
- ✓ Tesseract NuGet package (installed)
- ✓ tessdata language files (downloaded)
- ✓ English language data at `tessdata/eng.traineddata`

### Language (Optional)
By default, the API uses English (`eng`). To enable Swedish, set:

```json
{
  "Tesseract": {
    "Language": "swe"
  }
}
```

You can also load multiple languages (for example Swedish + English):

```json
{
  "Tesseract": {
    "Language": "eng+swe"
  }
}
```

### Configuration (Optional)
To customize the tessdata path, add to `appsettings.json`:

```json
{
  "Tesseract": {
    "DataPath": "path/to/custom/tessdata"
  }
}
```

### Features
- ✓ Runs completely locally
- ✓ No cloud dependencies
- ✓ No API costs
- ✓ Works offline
- ✗ Lower accuracy than Azure
- ✗ English only (by default)

## Adding More Languages

To support additional languages with Tesseract:

1. Download language files from: https://github.com/tesseract-ocr/tessdata_fast
2. Save `.traineddata` files to `backend/src/RecipeApi/tessdata/`
3. Set `Tesseract:Language` in configuration to the language code

Common codes:
- English: `eng` (file: `eng.traineddata`)
- Swedish: `swe` (file: `swe.traineddata`)

## Automatic Fallback

The application automatically selects the OCR implementation:

1. **Azure Vision** is used if `AzureVision:Endpoint` and `AzureVision:ApiKey` are configured
2. **Tesseract** is used as fallback when Azure credentials are missing
3. **Placeholder text** is returned if neither service is properly configured

Check the logs for which service is being used:
- `"Azure Vision not configured..."` → Using Tesseract
- `"Tesseract data not available..."` → Using placeholder

## Testing

Test the OCR endpoint:

```http
POST http://localhost:5137/recipes/ocr
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary
X-Correlation-Id: test-123

------WebKitFormBoundary
Content-Disposition: form-data; name="file"; filename="recipe.jpg"
Content-Type: image/jpeg

[binary image data]
------WebKitFormBoundary--
```

## Debugging

To see which OCR service is active, check the startup logs or test logs:
- Azure Vision initialization creates a client at startup
- Tesseract checks for the tessdata directory path
- Both log warnings if not properly configured
