# ?? Testing Guide - AI Log Analyzer

## Complete Test Scenarios

### Test 1: Basic Application Startup ?

**Steps:**
1. Run `dotnet run`
2. Check console output for "Now listening on" messages
3. Open https://localhost:5001

**Expected Result:**
- Application starts without errors
- UI loads correctly
- No console errors

---

### Test 2: Sample Log Analysis ?

**Steps:**
1. Click "Load Sample" button
2. Review the loaded log content
3. Click "Analyze Logs"
4. Wait for analysis

**Expected Result:**
- Sample logs populate the textarea
- Loading spinner appears
- Results display after 2-5 seconds
- Analysis shows:
  - Summary with severity badge
  - Root cause analysis
  - Suggested fixes
  - Categorized errors and warnings

---

### Test 3: Custom Log Analysis ?

**Test Log 1 - .NET Application Error:**
```
2024-03-18 14:23:11 [ERROR] System.NullReferenceException: Object reference not set to an instance of an object.
   at MyApp.Services.PaymentService.ProcessPayment(PaymentRequest request) in PaymentService.cs:line 123
   at MyApp.Controllers.PaymentController.Post(PaymentRequest request) in PaymentController.cs:line 45
2024-03-18 14:23:12 [ERROR] Failed to process payment for user ID: 12345
```

**Expected Analysis:**
- Error: NullReferenceException detected
- Root Cause: Null object in PaymentService
- Suggested Fix: Add null checks before accessing objects
- Severity: High or Critical

---

**Test Log 2 - Database Connection:**
```
2024-03-18 15:45:33 [ERROR] Database connection failed
System.Data.SqlClient.SqlException: Connection Timeout Expired
Server: prod-db-01.database.windows.net
Database: ProductionDB
2024-03-18 15:45:35 [WARN] Retry attempt 1 of 3
2024-03-18 15:45:37 [ERROR] All retry attempts failed
```

**Expected Analysis:**
- Error: Database timeout detected
- Root Cause: Database connection issues
- Suggested Fix: Check connection string, increase timeout, verify network
- Severity: Critical

---

**Test Log 3 - Authentication Issue:**
```
2024-03-18 16:12:45 [WARN] Failed login attempt
User: admin@company.com
IP: 192.168.1.105
Reason: Invalid credentials
2024-03-18 16:12:50 [WARN] Failed login attempt
User: admin@company.com
IP: 192.168.1.105
Reason: Invalid credentials
2024-03-18 16:12:55 [ERROR] Account locked due to multiple failed attempts
```

**Expected Analysis:**
- Warnings: Multiple failed login attempts
- Error: Account lockout
- Root Cause: Authentication failures
- Suggested Fix: Reset password, implement CAPTCHA
- Severity: Medium to High

---

### Test 4: API Endpoint Testing ??

#### Test Health Endpoint
```bash
curl https://localhost:5001/api/LogAnalyzer/health
```

**Expected Response:**
```json
{
  "status": "healthy",
  "timestamp": "2024-03-18T10:30:45Z"
}
```

---

#### Test Supported Log Types
```bash
curl https://localhost:5001/api/LogAnalyzer/supported-log-types
```

**Expected Response:**
```json
[
  {
    "id": "dotnet",
    "name": ".NET Application Logs",
    "description": "ASP.NET Core, .NET Framework logs"
  },
  ...
]
```

---

#### Test Log Analysis (POST)
```bash
curl -X POST https://localhost:5001/api/LogAnalyzer/analyze \
  -H "Content-Type: application/json" \
  -d '{
    "logs": "2024-03-18 10:30:45 [ERROR] Test error",
    "logType": "dotnet"
  }'
```

**Expected Response:**
```json
{
  "summary": "...",
  "errors": [...],
  "warnings": [...],
  "rootCause": "...",
  "suggestedFix": "...",
  "severity": "High",
  "analyzedAt": "2024-03-18T10:30:45Z"
}
```

---

### Test 5: UI Features Testing ??

#### Character Counter
1. Type in textarea
2. Watch character count update
3. Exceed 50,000 characters

**Expected:**
- Counter updates in real-time
- Counter turns red when limit exceeded
- Analyze button disables when over limit

---

#### Log Type Selector
1. Click dropdown
2. Select different log types

**Expected:**
- All types listed correctly
- Selection changes properly

---

#### Clear Button
1. Load or type logs
2. Click "Clear"

**Expected:**
- Textarea empties
- Character count resets to 0
- Results hide

---

#### Download Report
1. Analyze logs successfully
2. Click "Download Report"

**Expected:**
- Text file downloads
- Contains analysis results
- Properly formatted

---

### Test 6: Error Handling ???

#### Empty Logs
1. Click "Analyze Logs" with empty textarea

**Expected:**
- Alert: "Please paste some logs to analyze."

---

#### Logs Too Large
1. Paste > 50,000 characters
2. Try to analyze

**Expected:**
- Alert: "Logs too large. Maximum 50,000 characters allowed."

---

#### Invalid API Key
1. Set invalid API key in appsettings.json
2. Try to analyze

**Expected:**
- Error message displayed
- Results show error state

---

#### Network Error
1. Disconnect internet
2. Try to analyze

**Expected:**
- Error alert shown
- Application doesn't crash

---

### Test 7: Swagger Documentation ??

**Steps:**
1. Navigate to https://localhost:5001/swagger
2. Expand endpoints
3. Try "Try it out" on health endpoint

**Expected:**
- Swagger UI loads correctly
- All endpoints documented
- Interactive testing works

---

### Test 8: Responsive Design ??

**Desktop (1920x1080):**
- Full layout visible
- Proper spacing
- No scrolling issues

**Tablet (768x1024):**
- Layout adapts
- Buttons stack vertically
- Readable text

**Mobile (375x667):**
- Single column layout
- Touch-friendly buttons
- Proper text wrapping

---

### Test 9: Performance Testing ?

#### Small Log (< 1000 chars)
**Expected Response Time:** 2-3 seconds

#### Medium Log (5000-10000 chars)
**Expected Response Time:** 3-5 seconds

#### Large Log (40000-50000 chars)
**Expected Response Time:** 5-8 seconds

---

### Test 10: Different Log Formats ??

#### .NET Logs
```
2024-03-18 10:30:45.123 [ERROR] Exception details
   at ClassName.MethodName() in FileName.cs:line 45
```

#### Node.js Logs
```
[2024-03-18T10:30:45.123Z] ERROR: Application error
    at processRequest (/app/server.js:123:15)
```

#### Python Logs
```
2024-03-18 10:30:45,123 ERROR [module.name] Error message
Traceback (most recent call last):
  File "app.py", line 123, in function_name
```

#### Docker Logs
```
2024-03-18T10:30:45.123Z container-name ERROR: Connection refused
```

**Expected:**
- AI correctly identifies log type
- Parses errors appropriately
- Provides relevant fixes

---

## Test Results Checklist ?

### Core Functionality
- [ ] Application starts successfully
- [ ] UI loads without errors
- [ ] Sample logs work correctly
- [ ] Custom log analysis works
- [ ] Results display properly
- [ ] Severity badges show correct colors
- [ ] Download report works

### API Testing
- [ ] Health endpoint responds
- [ ] Supported types endpoint works
- [ ] Analyze endpoint returns valid JSON
- [ ] Error responses are proper HTTP codes

### UI Features
- [ ] Character counter updates
- [ ] Log type selector works
- [ ] Clear button functions
- [ ] Loading indicator appears
- [ ] Results scroll smoothly

### Error Handling
- [ ] Empty input validation
- [ ] Size limit enforcement
- [ ] API error handling
- [ ] Network error handling

### Cross-Browser
- [ ] Works in Chrome
- [ ] Works in Edge
- [ ] Works in Firefox
- [ ] Works in Safari (Mac)

### Responsive Design
- [ ] Desktop layout correct
- [ ] Tablet layout adapts
- [ ] Mobile layout functional

---

## Performance Benchmarks ??

| Log Size | Expected Time | Status |
|----------|--------------|--------|
| 100 lines | 2-3 sec | ? |
| 500 lines | 3-4 sec | ? |
| 1000 lines | 4-6 sec | ? |
| 2000+ lines | 6-8 sec | ? |

---

## Known Limitations ??

1. **Max 50,000 characters** - Gemini API limits
2. **Response time varies** - Depends on Gemini API load
3. **Rate limits** - Based on your API quota
4. **Internet required** - Needs connection to Gemini API

---

## Troubleshooting Test Failures ??

### Analysis Takes Too Long
- Check internet connection
- Verify Gemini API status
- Reduce log size

### Parsing Errors in Results
- AI may return non-JSON format occasionally
- Fallback to pattern-based analysis
- This is expected behavior

### UI Not Loading
- Clear browser cache
- Check console for errors
- Verify static files are served

---

## Security Testing ??

### Test API Key Protection
1. Check network tab
2. Verify API key not exposed in responses
3. Confirm HTTPS usage in production

**Expected:**
- API key only sent to Gemini
- No key exposure in frontend
- Secure transmission

---

## Automated Testing (Optional) ??

### Unit Test Example (Future Enhancement)
```csharp
[Test]
public async Task AnalyzeLogs_ValidInput_ReturnsAnalysis()
{
    // Arrange
    var service = new GeminiLogAnalyzerService(...);
    var logs = "2024-03-18 [ERROR] Test error";
    
    // Act
    var result = await service.AnalyzeLogsAsync(logs);
    
    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Errors);
}
```

---

**All tests passed? You have a production-ready application! ??**
