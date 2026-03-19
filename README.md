# ?? AI Log Analyzer

A professional, enterprise-grade log analysis tool powered by **Google Gemini AI** and **ASP.NET Core**. This application helps developers quickly identify errors, warnings, root causes, and provides AI-generated fixes for application logs.

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## ?? Features

### Core Functionality
- ? **AI-Powered Analysis** - Uses Google Gemini Pro for intelligent log analysis
- ? **Multi-Log Type Support** - Supports .NET, Node.js, Python, Docker, Kubernetes, and more
- ? **Pattern Detection** - Pre-analyzes logs for common issues (NullReference, Database, Auth, Timeouts)
- ? **Structured Results** - Clean JSON responses with errors, warnings, and info categorized
- ? **Severity Levels** - Automatic severity classification (Critical, High, Medium, Low, Info)
- ? **Root Cause Analysis** - AI identifies the underlying cause of issues
- ? **Suggested Fixes** - Step-by-step solutions with code examples
- ? **Professional UI** - Modern, responsive web interface
- ? **Download Reports** - Export analysis results as text files
- ? **Sample Logs** - Built-in examples for testing

### Technical Features
- ?? **Secure API Key Management** - Configuration-based API key storage
- ?? **Swagger/OpenAPI** - Full API documentation
- ?? **Fast Response** - Optimized async processing
- ?? **Beautiful UI** - Gradient design with smooth animations
- ?? **Responsive Design** - Works on desktop, tablet, and mobile
- ? **Error Handling** - Comprehensive error management
- ?? **Detailed Logging** - Built-in logging for debugging

## ??? Architecture

```
???????????????????
?   Frontend      ?
?  (HTML/CSS/JS)  ?
???????????????????
         ?
         ?
???????????????????????????
?  ASP.NET Core Web API   ?
?  ????????????????????   ?
?  ?   Controllers    ?   ?
?  ????????????????????   ?
?           ?             ?
?  ????????????????????   ?
?  ?    Services      ?   ?
?  ?  (AI Integration)?   ?
?  ????????????????????   ?
???????????????????????????
            ?
            ?
  ????????????????????
  ?   Gemini API     ?
  ?  (Google AI)     ?
  ????????????????????
```

## ?? Quick Start

### Prerequisites
- **.NET 10.0 SDK** or later
- **Google Gemini API Key** (Get one at [Google AI Studio](https://makersuite.google.com/app/apikey))
- A code editor (Visual Studio, VS Code, or Rider)

### Installation

1. **Clone or navigate to the project directory:**
   ```bash
   cd "E:\Log Analyzer AI"
   ```

2. **Configure the API Key:**
   
   Open `appsettings.json` and add your Gemini API key:
   ```json
   {
     "Gemini": {
       "ApiKey": "YOUR_API_KEY_HERE"
     }
   }
   ```

3. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

4. **Build the project:**
   ```bash
   dotnet build
   ```

5. **Run the application:**
   ```bash
   dotnet run
   ```

6. **Open your browser:**
   - Main App: `https://localhost:5001` or `http://localhost:5000`
   - Swagger UI: `https://localhost:5001/swagger`

## ?? Usage

### Web Interface

1. **Open the application** in your browser
2. **Paste your logs** into the text area (up to 50,000 characters)
3. **(Optional) Select log type** for better analysis
4. **Click "Analyze Logs"** and wait a few seconds
5. **Review the results:**
   - Summary and severity level
   - Root cause analysis
   - Suggested fixes
   - Categorized errors, warnings, and info
6. **Download the report** if needed

### API Endpoints

#### Analyze Logs
```http
POST /api/LogAnalyzer/analyze
Content-Type: application/json

{
  "logs": "your log content here",
  "logType": "dotnet"  // optional
}
```

**Response:**
```json
{
  "summary": "Analysis overview",
  "errors": [...],
  "warnings": [...],
  "info": [...],
  "rootCause": "Detailed explanation",
  "suggestedFix": "Step-by-step solution",
  "severity": "High",
  "analyzedAt": "2024-03-18T10:30:45Z"
}
```

#### Health Check
```http
GET /api/LogAnalyzer/health
```

#### Supported Log Types
```http
GET /api/LogAnalyzer/supported-log-types
```

## ?? Supported Log Types

| Type | Description |
|------|-------------|
| `.NET` | ASP.NET Core, .NET Framework logs |
| `Node.js` | Express, Node.js application logs |
| `Python` | Django, Flask, Python logs |
| `Docker` | Container runtime logs |
| `Kubernetes` | K8s pod and cluster logs |
| `Nginx` | Web server logs |
| `Apache` | Apache HTTP server logs |
| `Database` | SQL Server, PostgreSQL, MySQL |
| `General` | Any application logs |

## ?? What It Detects

### Error Patterns
- ? **NullReferenceException** - Null pointer issues
- ??? **Database Errors** - Connection failures, timeouts, deadlocks
- ?? **Authentication Issues** - Auth failures, 401/403 errors
- ?? **Timeouts** - Network and operation timeouts
- ?? **Network Errors** - API failures, connection issues
- ?? **Memory Issues** - High memory usage, leaks

### Analysis Output
- **Errors** - Critical issues that need immediate attention
- **Warnings** - Potential problems or deprecated code
- **Info** - Important context and informational messages
- **Root Cause** - AI-identified underlying issue
- **Suggested Fix** - Actionable solutions with code examples
- **Severity Level** - Critical, High, Medium, Low, Info

## ?? Project Structure

```
Log Analyzer AI/
├── Controllers/
│   └── LogAnalyzerController.cs      # API endpoints
├── Models/
│   ├── LogAnalysisRequest.cs         # Request model
│   ├── LogAnalysisResponse.cs        # Response model
│   ├── GeminiRequest.cs              # Gemini API request
│   └── GeminiResponse.cs             # Gemini API response
├── Services/
│   ├── ILogAnalyzerService.cs        # Service interface
│   └── GeminiLogAnalyzerService.cs   # AI integration logic
├── docs/                             # Project documentation and history
│   ├── CHANGELOG.md
│   ├── CONFIGURATION.md
│   └── ...                           # Other project guides
├── wwwroot/
│   ├── index.html                    # Main UI
│   ├── styles.css                    # Styling
│   └── app.js                        # Frontend logic
├── Program.cs                         # App configuration
├── appsettings.json                   # Configuration (Template)
└── LogAnalyzer.API.csproj            # Project file
```

## ?? UI Features

### Design Highlights
- **Modern Gradient Design** - Purple-to-blue gradient background
- **Card-Based Layout** - Clean, organized result cards
- **Color-Coded Severity** - Visual severity indicators
  - ?? Critical - Red
  - ?? High - Orange
  - ?? Medium - Yellow
  - ?? Low - Green
  - ?? Info - Blue
- **Smooth Animations** - Hover effects and transitions
- **Responsive Design** - Mobile, tablet, and desktop support
- **Character Counter** - Real-time character count
- **Loading Indicator** - Animated spinner during analysis

## ?? Configuration

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Gemini": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

### Environment Variables (Alternative)
For production, use environment variables:
```bash
export Gemini__ApiKey="your-api-key"
```

## ?? Deployment

### Local Development
```bash
dotnet run
```

### Production Build
```bash
dotnet publish -c Release -o ./publish
```

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "LogAnalyzer.API.dll"]
```

### Azure Deployment
1. Create an Azure App Service
2. Configure the Gemini API key in Application Settings
3. Deploy using Azure DevOps or GitHub Actions

## ?? Performance

- **Response Time:** ~2-5 seconds (depends on Gemini API)
- **Max Log Size:** 50,000 characters
- **Concurrent Requests:** Supports multiple simultaneous analyses
- **API Rate Limit:** Based on your Gemini API quota

## ??? Security

- ? API key stored in configuration (not hardcoded)
- ? Input validation and sanitization
- ? HTTPS enforced in production
- ? CORS configured for security
- ? Error messages don't expose sensitive data

## ?? Contributing

This is a professional portfolio project. Feel free to fork and customize for your needs.

## ?? License

MIT License - Feel free to use this project for learning and portfolio purposes.

## ?? Author

Built with ?? using ASP.NET Core and Google Gemini AI

## ?? Acknowledgments

- **Google Gemini AI** - For the powerful AI capabilities
- **ASP.NET Core Team** - For the excellent framework
- **Open Source Community** - For inspiration and support

## ?? Support

For issues or questions:
1. Check the Swagger documentation at `/swagger`
2. Review the console logs for errors
3. Verify your Gemini API key is valid

## ?? Future Enhancements

- [ ] Multiple log file upload
- [ ] Real-time log streaming
- [ ] Historical analysis storage
- [ ] User authentication
- [ ] Team collaboration features
- [ ] Custom AI prompts
- [ ] Export to PDF/JSON
- [ ] Integration with CI/CD pipelines
- [ ] Slack/Teams notifications

## ?? Why This Project Stands Out

### For Technical Leadership:
? **Solves Real Problems** - Reduces debugging time significantly  
? **AI Integration** - Shows modern tech stack proficiency  
? **Clean Architecture** - Well-structured, maintainable code  
? **Professional UI** - Production-ready design  
? **Comprehensive Testing** - Easy to demonstrate and test  
? **Scalable** - Can handle enterprise-level logs  
? **Well Documented** - Clear README and API docs  

### Technical Highlights:
- **Async/Await** - Proper async programming
- **Dependency Injection** - Following SOLID principles
- **REST API** - Clean API design
- **Error Handling** - Comprehensive exception management
- **Logging** - Built-in diagnostic logging
- **Configuration** - Proper settings management

---

**Built for excellence. Designed for impact. Ready to impress.** ??
