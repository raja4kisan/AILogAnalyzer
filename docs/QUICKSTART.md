# ?? Quick Start Guide - AI Log Analyzer

## Step-by-Step Setup (5 Minutes)

### 1?? Verify Prerequisites
```bash
# Check if .NET is installed
dotnet --version
# Should show 10.0 or higher
```

### 2?? Navigate to Project Directory
```bash
cd "E:\Log Analyzer AI"
```

### 3?? Verify API Key Configuration
Open `appsettings.json` and ensure your Gemini API key is set:
```json
{
  "Gemini": {
    "ApiKey": "AIzaSyB8SVcXltUbRjDPKRXRskGqV7-U0dKzDYo"
  }
}
```
? **API Key is already configured!**

### 4?? Run the Application
```bash
dotnet run
```

You should see output like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

### 5?? Open in Browser
**Main Application:**
- ?? https://localhost:5001
- ?? http://localhost:5000

**Swagger API Documentation:**
- ?? https://localhost:5001/swagger

### 6?? Test the Application

#### Option A: Use Sample Logs (Recommended for First Test)
1. Click the "Load Sample" button
2. Click "Analyze Logs"
3. Wait 2-5 seconds
4. Review the AI-generated analysis!

#### Option B: Paste Your Own Logs
1. Copy logs from your application
2. Paste into the text area
3. Select log type (optional)
4. Click "Analyze Logs"

---

## ?? What to Expect

### Analysis Results Include:
- ? **Summary** - Quick overview with severity badge
- ? **Root Cause** - AI-identified underlying issue
- ? **Suggested Fix** - Step-by-step solutions
- ? **Errors** - All detected errors with line numbers
- ? **Warnings** - Potential issues
- ? **Important Info** - Context and helpful information

### Visual Indicators:
- ?? **Critical** - Immediate attention required
- ?? **High** - Important issues
- ?? **Medium** - Should be addressed
- ?? **Low** - Minor issues
- ?? **Info** - Informational only

---

## ??? Troubleshooting

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Port Already in Use
If port 5000/5001 is already in use, edit `Properties/launchSettings.json`:
```json
"applicationUrl": "https://localhost:7001;http://localhost:7000"
```

### API Key Issues
If you get "API key not configured" error:
1. Check `appsettings.json` has the `Gemini` section
2. Verify the API key is not empty
3. Restart the application

### Gemini API Errors
- **401 Unauthorized**: Check if API key is valid
- **429 Too Many Requests**: Wait a moment and try again
- **503 Service Unavailable**: Gemini API might be down, try later

---

## ?? Testing Checklist

? Application starts without errors  
? Main page loads at https://localhost:5001  
? Swagger UI accessible at /swagger  
? Sample logs load correctly  
? Analysis completes successfully  
? Results display with proper formatting  
? Download report works  
? Clear button resets the form  

---

## ?? Sample Log for Testing

If you need a quick test, paste this:
```
2024-03-18 10:30:45 [ERROR] NullReferenceException at UserService.cs:45
2024-03-18 10:30:46 [ERROR] Database connection timeout
2024-03-18 10:30:47 [WARN] Authentication failed for user admin
```

---

## ?? Production Deployment

### Build for Production
```bash
dotnet publish -c Release -o ./publish
```

### Run Published Version
```bash
cd publish
dotnet LogAnalyzer.API.dll
```

### Environment Variables (Recommended for Production)
Instead of `appsettings.json`, use environment variables:
```bash
export Gemini__ApiKey="your-api-key"
export ASPNETCORE_ENVIRONMENT="Production"
```

---

## ?? Need Help?

1. **Check Swagger docs**: https://localhost:5001/swagger
2. **Review README.md**: Comprehensive documentation
3. **Check console output**: Look for error messages
4. **Verify API key**: Test at https://aistudio.google.com

---

## ?? Demo Script (For Presentations)

### 1. Show the UI
"This is an AI-powered log analyzer built with ASP.NET Core and Google Gemini."

### 2. Load Sample Logs
"Let me demonstrate with sample application logs that contain errors and warnings."

### 3. Analyze
"The AI analyzes the logs in real-time using Gemini Pro..."

### 4. Show Results
"As you can see, it identified:
- The specific errors with line numbers
- The root cause (database connection issues)
- Step-by-step fixes with code examples
- Severity level (High/Critical)"

### 5. Highlight Features
"Key features include:
- Support for multiple log types (.NET, Node.js, Python, Docker, etc.)
- Pattern detection for common issues
- Downloadable reports
- Professional, responsive UI
- Full REST API with Swagger documentation"

### 6. Show API Docs
"Here's the Swagger documentation showing all available endpoints..."

---

## ? Tips for Best Results

1. **Include timestamps** - Helps AI understand the sequence of events
2. **Include stack traces** - Provides context for errors
3. **Include 20-100 lines** - Enough context for good analysis
4. **Select log type** - Improves AI's understanding
5. **Include warnings** - Not just errors

---

**You're all set! Start analyzing logs with AI! ??**
