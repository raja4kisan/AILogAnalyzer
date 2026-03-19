# ?? Configuration Setup Instructions

## ?? IMPORTANT: API Keys Required

This application requires Azure OpenAI and/or Google Gemini API credentials to function.

### ?? Setup Steps:

1. **Copy the template configuration:**
   ```bash
   cp appsettings.json appsettings.Development.json
   ```

2. **Edit `appsettings.Development.json` with your actual API keys:**

   ```json
   {
     "AzureOpenAI": {
       "ApiKey": "your-actual-azure-openai-key",
       "Endpoint": "https://your-resource.openai.azure.com/",
       "Deployment": "your-deployment-name"
     },
     "Gemini": {
       "ApiKey": "your-actual-gemini-key"
     }
   }
   ```

3. **The application will automatically use Development settings in development mode**

### ?? Getting API Keys:

#### Azure OpenAI:
1. Go to [Azure Portal](https://portal.azure.com)
2. Create/Select an Azure OpenAI resource
3. Navigate to "Keys and Endpoint"
4. Copy your key and endpoint

#### Google Gemini (Optional):
1. Go to [Google AI Studio](https://makersuite.google.com/app/apikey)
2. Create API key
3. Copy the key

### ??? Security Notes:

- ? `appsettings.Development.json` is already in `.gitignore` - your keys are safe!
- ? Never commit files with real API keys
- ? Use environment variables for production deployments
- ? Rotate keys if accidentally exposed

### ?? Alternative: Environment Variables

You can also set keys as environment variables:

**Windows (PowerShell):**
```powershell
$env:AzureOpenAI__ApiKey = "your-key"
$env:AzureOpenAI__Endpoint = "your-endpoint"
$env:AzureOpenAI__Deployment = "your-deployment"
```

**Linux/Mac:**
```bash
export AzureOpenAI__ApiKey="your-key"
export AzureOpenAI__Endpoint="your-endpoint"
export AzureOpenAI__Deployment="your-deployment"
```

### ? Verification:

After configuration, run:
```bash
dotnet run
```

If configured correctly, you'll see:
```
Now listening on: http://localhost:5000
```

Navigate to http://localhost:5000 and test the analyzer!

---

**For detailed setup instructions, see [README.md](README.md)**
