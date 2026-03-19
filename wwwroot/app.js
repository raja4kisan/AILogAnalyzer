// API Configuration
const API_BASE_URL = window.location.origin + '/api';

// State Management
let analysisHistory = [];
let currentTheme = 'light';

// DOM Elements
const logInput = document.getElementById('logInput');
const logType = document.getElementById('logType');
const analyzeBtn = document.getElementById('analyzeBtn');
const clearBtn = document.getElementById('clearBtn');
const loadSampleBtn = document.getElementById('loadSampleBtn');
const downloadBtn = document.getElementById('downloadBtn');
const charCount = document.getElementById('charCount');
const resultsSection = document.getElementById('resultsSection');
const loadingIndicator = document.getElementById('loadingIndicator');
const analysisResults = document.getElementById('analysisResults');

// Event Listeners
logInput.addEventListener('input', updateCharCount);
analyzeBtn.addEventListener('click', analyzeLogs);
clearBtn.addEventListener('click', clearAll);
loadSampleBtn.addEventListener('click', loadSampleLogs);
if (downloadBtn) downloadBtn.addEventListener('click', downloadReport);

// Initialize theme from localStorage
document.addEventListener('DOMContentLoaded', () => {
    const savedTheme = localStorage.getItem('theme') || 'light';
    setTheme(savedTheme);
    updateCharCount();
    loadHistoryFromLocalStorage();
});



// ================== THEME TOGGLE ==================
function toggleTheme() {
    const newTheme = currentTheme === 'light' ? 'dark' : 'light';
    setTheme(newTheme);
}

function setTheme(theme) {
    currentTheme = theme;
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
    
    // Update theme toggle icon
    const themeToggle = document.querySelector('.theme-toggle');
    if (themeToggle) {
        if (theme === 'dark') {
            themeToggle.innerHTML = `
                <svg class="moon-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                    <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path>
                </svg>
            `;
        } else {
            themeToggle.innerHTML = `
                <svg class="sun-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                    <circle cx="12" cy="12" r="5"></circle>
                    <line x1="12" y1="1" x2="12" y2="3"></line>
                    <line x1="12" y1="21" x2="12" y2="23"></line>
                    <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line>
                    <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line>
                    <line x1="1" y1="12" x2="3" y2="12"></line>
                    <line x1="21" y1="12" x2="23" y2="12"></line>
                    <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line>
                    <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line>
                </svg>
            `;
        }
    }
}

// ================== NAVIGATION ==================
function showAnalyzer() {
    // Update nav active state
    updateNavActive('analyzer');
    
    // Show/hide sections
    const mainContent = document.querySelector('.main-content');
    const historyModal = document.getElementById('historyModal');
    const statsModal = document.getElementById('statsModal');
    
    if (mainContent) mainContent.style.display = 'block';
    if (historyModal) historyModal.style.display = 'none';
    if (statsModal) statsModal.style.display = 'none';
}

function showHistory() {
    updateNavActive('history');
    displayHistoryModal();
}

function showStats() {
    updateNavActive('stats');
    displayStatsModal();
}

function updateNavActive(active) {
    const navItems = document.querySelectorAll('.nav-item');
    navItems.forEach(item => {
        item.classList.remove('active');
        const text = item.textContent.trim().toLowerCase();
        if (text.includes(active)) {
            item.classList.add('active');
        }
    });
}

// ================== HISTORY MODAL ==================
function displayHistoryModal() {
    // Remove existing modal if any
    const existingModal = document.getElementById('historyModal');
    if (existingModal) existingModal.remove();
    
    const modal = document.createElement('div');
    modal.id = 'historyModal';
    modal.className = 'modal';
    modal.innerHTML = `
        <div class="modal-backdrop" onclick="closeHistoryModal()"></div>
        <div class="modal-content card">
            <div class="modal-header">
                <h2 class="modal-title">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <circle cx="12" cy="12" r="10"></circle>
                        <polyline points="12 6 12 12 16 14"></polyline>
                    </svg>
                    Analysis History
                </h2>
                <button class="btn-icon-only" onclick="closeHistoryModal()">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <line x1="18" y1="6" x2="6" y2="18"></line>
                        <line x1="6" y1="6" x2="18" y2="18"></line>
                    </svg>
                </button>
            </div>
            <div class="modal-body">
                ${analysisHistory.length === 0 ? `
                    <div class="empty-state">
                        <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                            <circle cx="12" cy="12" r="10"></circle>
                            <line x1="12" y1="8" x2="12" y2="12"></line>
                            <line x1="12" y1="16" x2="12.01" y2="16"></line>
                        </svg>
                        <h3>No History Yet</h3>
                        <p>Your analysis history will appear here once you analyze some logs.</p>
                    </div>
                ` : `
                    <div class="history-list">
                        ${analysisHistory.slice().reverse().map((item, index) => `
                            <div class="history-item">
                                <div class="history-header">
                                    <span class="severity-badge severity-${item.severity.toLowerCase()}">${item.severity}</span>
                                    <span class="history-date">${new Date(item.analyzedAt).toLocaleString()}</span>
                                </div>
                                <div class="history-summary">${escapeHtml(item.summary.substring(0, 150))}${item.summary.length > 150 ? '...' : ''}</div>
                                <div class="history-stats">
                                    <span class="stat-badge error">? ${item.errorCount} Errors</span>
                                    <span class="stat-badge warning">?? ${item.warningCount} Warnings</span>
                                </div>
                            </div>
                        `).join('')}
                    </div>
                `}
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    setTimeout(() => modal.classList.add('show'), 10);
}

function closeHistoryModal() {
    const modal = document.getElementById('historyModal');
    if (modal) {
        modal.classList.remove('show');
        setTimeout(() => modal.remove(), 300);
    }
    updateNavActive('analyzer');
}

// ================== STATS MODAL ==================
async function displayStatsModal() {
    // Remove existing modal if any
    const existingModal = document.getElementById('statsModal');
    if (existingModal) existingModal.remove();
    
    // Fetch stats from API
    let stats = null;
    try {
        const response = await fetch(`${API_BASE_URL}/LogAnalyzer/stats`);
        if (response.ok) {
            stats = await response.json();
        }
    } catch (error) {
        console.error('Failed to fetch stats:', error);
    }
    
    // Calculate local stats
    const localStats = calculateLocalStats();
    
    const modal = document.createElement('div');
    modal.id = 'statsModal';
    modal.className = 'modal';
    modal.innerHTML = `
        <div class="modal-backdrop" onclick="closeStatsModal()"></div>
        <div class="modal-content card">
            <div class="modal-header">
                <h2 class="modal-title">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path d="M18 20V10M12 20V4M6 20v-6"></path>
                    </svg>
                    System Statistics
                </h2>
                <button class="btn-icon-only" onclick="closeStatsModal()">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <line x1="18" y1="6" x2="6" y2="18"></line>
                        <line x1="6" y1="6" x2="18" y2="18"></line>
                    </svg>
                </button>
            </div>
            <div class="modal-body">
                <div class="stats-grid">
                    <div class="stat-card">
                        <div class="stat-icon primary">??</div>
                        <div class="stat-value">${localStats.totalAnalyses}</div>
                        <div class="stat-label">Total Analyses</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-icon error">?</div>
                        <div class="stat-value">${localStats.totalErrors}</div>
                        <div class="stat-label">Errors Found</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-icon warning">??</div>
                        <div class="stat-value">${localStats.totalWarnings}</div>
                        <div class="stat-label">Warnings Found</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-icon success">?</div>
                        <div class="stat-value">${stats?.supportedLogTypes || 15}+</div>
                        <div class="stat-label">Supported Types</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-icon info">?</div>
                        <div class="stat-value">~25s</div>
                        <div class="stat-label">Avg Analysis Time</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-icon success">??</div>
                        <div class="stat-value">v2.0</div>
                        <div class="stat-label">API Version</div>
                    </div>
                </div>
                
                <div class="severity-breakdown">
                    <h3>Severity Distribution</h3>
                    <div class="severity-chart">
                        ${Object.entries(localStats.severityBreakdown).map(([severity, count]) => `
                            <div class="severity-bar">
                                <div class="severity-label">${severity}</div>
                                <div class="severity-progress">
                                    <div class="severity-fill severity-${severity.toLowerCase()}" 
                                         style="width: ${(count / Math.max(localStats.totalAnalyses, 1)) * 100}%">
                                        ${count}
                                    </div>
                                </div>
                            </div>
                        `).join('')}
                    </div>
                </div>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    setTimeout(() => modal.classList.add('show'), 10);
}

function closeStatsModal() {
    const modal = document.getElementById('statsModal');
    if (modal) {
        modal.classList.remove('show');
        setTimeout(() => modal.remove(), 300);
    }
    updateNavActive('analyzer');
}

// ================== HELPER FUNCTIONS ==================
function calculateLocalStats() {
    return {
        totalAnalyses: analysisHistory.length,
        totalErrors: analysisHistory.reduce((sum, item) => sum + item.errorCount, 0),
        totalWarnings: analysisHistory.reduce((sum, item) => sum + item.warningCount, 0),
        severityBreakdown: analysisHistory.reduce((acc, item) => {
            acc[item.severity] = (acc[item.severity] || 0) + 1;
            return acc;
        }, {})
    };
}

function saveToHistory(result) {
    const historyItem = {
        severity: result.severity || 'Unknown',
        summary: result.summary || 'No summary',
        analyzedAt: result.analyzedAt || new Date().toISOString(),
        errorCount: result.errors?.length || 0,
        warningCount: result.warnings?.length || 0,
        rootCause: result.rootCause || '',
        suggestedFix: result.suggestedFix || ''
    };
    
    analysisHistory.push(historyItem);
    
    // Keep only last 50 analyses
    if (analysisHistory.length > 50) {
        analysisHistory = analysisHistory.slice(-50);
    }
    
    // Save to localStorage
    localStorage.setItem('analysisHistory', JSON.stringify(analysisHistory));
}

function loadHistoryFromLocalStorage() {
    try {
        const stored = localStorage.getItem('analysisHistory');
        if (stored) {
            analysisHistory = JSON.parse(stored);
        }
    } catch (error) {
        console.error('Failed to load history:', error);
        analysisHistory = [];
    }
}

// Update character count
function updateCharCount() {
    const count = logInput.value.length;
    const percentage = (count / 50000) * 100;
    
    charCount.textContent = `${count.toLocaleString()} / 50,000`;
    
    // Update progress bar
    const charProgress = document.getElementById('charProgress');
    if (charProgress) {
        charProgress.style.width = `${percentage}%`;
        
        // Change color based on usage
        if (percentage > 90) {
            charProgress.style.background = 'var(--gradient-error)';
        } else if (percentage > 70) {
            charProgress.style.background = 'var(--gradient-warning)';
        } else {
            charProgress.style.background = 'var(--gradient-primary)';
        }
    }
    
    if (count > 50000) {
        charCount.style.color = 'var(--error-500)';
        analyzeBtn.disabled = true;
    } else {
        charCount.style.color = 'var(--gray-600)';
        analyzeBtn.disabled = false;
    }
}

// Clear all
function clearAll() {
    logInput.value = '';
    updateCharCount();
    resultsSection.style.display = 'none';
}

// Load sample logs
function loadSampleLogs() {
    const selectedLogType = logType.value;
    
    let sampleLog = '';
    
    // Android/Mobile samples
    if (selectedLogType === 'android' || selectedLogType === 'kotlin' || 
        selectedLogType === 'java' || selectedLogType === 'ndk') {
        sampleLog = `> Task :app:compileDebugKotlin FAILED
e: file:///app/src/main/java/com/example/app/MainActivity.kt:45:25 Unresolved reference: viewModel
e: file:///app/src/main/java/com/example/app/ui/HomeFragment.kt:78:17 Type mismatch: inferred type is String? but String was expected

FAILURE: Build failed with an exception.

* What went wrong:
Execution failed for task ':app:compileDebugKotlin'.
> Compilation error. See log for more details

> Task :app:processDebugResources FAILED
AAPT2 error: check logs for details
ERROR:/app/src/main/res/values/strings.xml:15: error: unescaped apostrophe in string

> Task :app:kaptDebugKotlin FAILED
error: [Dagger/MissingBinding] com.example.repository.UserRepository cannot be provided without an @Inject constructor

* What went wrong:
Execution failed for task ':app:mergeDebugNativeLibs'.
> Could not resolve all files for configuration ':app:debugRuntimeClasspath'.
  > Could not find com.android.support:appcompat-v7:28.0.0.

error: undefined reference to 'Java_com_example_app_NativeLib_nativeMethod'
CMake Error: Cannot find source file: native-lib.cpp

BUILD FAILED in 12s`;
    } else if (selectedLogType === 'reactnative') {
        sampleLog = `error: Error: Unable to resolve module @react-navigation/native from App.js
error: bundling failed: Error: Unable to resolve module react-native-gesture-handler
error: Invariant Violation: "main" has not been registered

Metro Bundler error: jest-haste-map: Haste module naming collision
error: SyntaxError: /app/src/components/Home.tsx: Unexpected token (45:12)

React Native version mismatch: 
  - Expected: 0.72.0
  - Found: 0.71.0

BUILD FAILED with 5 errors`;
    } else {
        // Default .NET sample
        sampleLog = `2024-03-18 10:30:45.123 [ERROR] Application startup exception in MyApp.Services.UserService
System.NullReferenceException: Object reference not set to an instance of an object.
   at MyApp.Services.UserService.GetUser(Int32 id) in UserService.cs:line 45
   at MyApp.Controllers.UserController.GetUserById(Int32 id) in UserController.cs:line 23

2024-03-18 10:30:46.456 [ERROR] Database connection failed
System.Data.SqlClient.SqlException: Connection Timeout Expired. The timeout period elapsed while attempting to consume the pre-login handshake acknowledgement.
   at System.Data.SqlClient.SqlConnection.OnError(SqlException exception)
   at MyApp.Data.DatabaseContext.Initialize() in DatabaseContext.cs:line 67

2024-03-18 10:30:47.789 [WARN] Failed to authenticate user: Invalid credentials
Authentication failed for user 'admin@example.com'
Retry attempt 1 of 3

2024-03-18 10:30:48.012 [ERROR] API request to external service timed out
System.Net.Http.HttpRequestException: The request timed out after 30 seconds
   at MyApp.Services.ExternalApiService.CallApi() in ExternalApiService.cs:line 89

2024-03-18 10:30:49.345 [WARN] Memory usage is high: 87% of available memory in use

2024-03-18 10:30:50.678 [INFO] Application attempting recovery...
Initiating database connection pool reset

2024-03-18 10:30:51.901 [ERROR] Recovery failed: Unable to establish database connection
Maximum retry attempts exceeded`;
    }

    logInput.value = sampleLog;
    updateCharCount();
}

// Analyze logs
async function analyzeLogs() {
    const logs = logInput.value.trim();
    
    if (!logs) {
        showToast('Please paste some logs to analyze.', 'warning');
        return;
    }

    if (logs.length > 50000) {
        showToast('Logs are too large. Maximum 50,000 characters allowed.', 'error');
        return;
    }

    // Update loading steps
    updateLoadingSteps(1);

    // Show loading
    resultsSection.style.display = 'block';
    loadingIndicator.style.display = 'block';
    analysisResults.style.display = 'none';
    analyzeBtn.disabled = true;
    analyzeBtn.innerHTML = `
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" class="spinning">
            <circle cx="12" cy="12" r="10"></circle>
            <path d="M12 6v6l4 2"></path>
        </svg>
        Analyzing...
    `;

    try {
        setTimeout(() => updateLoadingSteps(2), 1000);
        
        const response = await fetch(`${API_BASE_URL}/LogAnalyzer/analyze`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                logs: logs,
                logType: logType.value || null
            })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error || 'Analysis failed');
        }

        updateLoadingSteps(3);
        const result = await response.json();
        
        // Handle extended response format
        const analysis = result.analysis || result;
        
        // Save to history
        saveToHistory(analysis);
        
        displayResults(analysis);
        showToast('Analysis completed successfully!', 'success');
    } catch (error) {
        console.error('Error:', error);
        showToast(`Error analyzing logs: ${error.message}`, 'error');
        resultsSection.style.display = 'none';
    } finally {
        loadingIndicator.style.display = 'none';
        analyzeBtn.disabled = false;
        analyzeBtn.innerHTML = `
            <svg class="btn-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <circle cx="11" cy="11" r="8"></circle>
                <path d="m21 21-4.35-4.35"></path>
            </svg>
            Analyze with AI
        `;
        resetLoadingSteps();
    }
}

function updateLoadingSteps(activeStep) {
    const steps = document.querySelectorAll('.step');
    steps.forEach((step, index) => {
        if (index < activeStep) {
            step.classList.add('active');
            const icon = step.querySelector('.step-icon');
            if (icon) icon.textContent = '?';
        }
    });
}

function resetLoadingSteps() {
    const steps = document.querySelectorAll('.step');
    steps.forEach((step, index) => {
        step.classList.remove('active');
        const icon = step.querySelector('.step-icon');
        if (icon) icon.textContent = '?';
    });
    if (steps[0]) {
        const firstIcon = steps[0].querySelector('.step-icon');
        if (firstIcon) firstIcon.textContent = '?';
    }
}

// Display results
function displayResults(result) {
    try {
        // Summary
        const summaryContent = document.getElementById('summaryContent');
        if (summaryContent) {
            summaryContent.innerHTML = formatText(result.summary);
        }
        
        // Severity badge
        const severityBadge = document.getElementById('severityBadge');
        if (severityBadge) {
            severityBadge.textContent = result.severity || 'Unknown';
            const severityClass = String(result.severity || 'Info').toLowerCase();
            severityBadge.className = `severity-badge severity-${severityClass}`;
        }

        // Root cause
        const rootCauseContent = document.getElementById('rootCauseContent');
        if (rootCauseContent) {
            rootCauseContent.innerHTML = formatText(result.rootCause);
        }

        // Suggested fix
        const suggestedFixContent = document.getElementById('suggestedFixContent');
        if (suggestedFixContent) {
            suggestedFixContent.innerHTML = formatText(result.suggestedFix);
        }

        // Errors
        displayLogEntries('errors', result.errors, 'error');

        // Warnings
        displayLogEntries('warnings', result.warnings, 'warning');

        // Info
        displayLogEntries('info', result.info, 'info');

        // Timestamp
        const analyzedAtElement = document.getElementById('analyzedAt');
        if (analyzedAtElement) {
            analyzedAtElement.textContent = new Date(result.analyzedAt).toLocaleString();
        }

        // Show results with animation
        analysisResults.style.display = 'block';
        
        // Scroll to results
        setTimeout(() => {
            resultsSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 100);
    } catch (error) {
        console.error('Error displaying results:', error);
        showToast('Error displaying analysis results. Please check the console.', 'error');
    }
}

// Display log entries
function displayLogEntries(type, entries, cssClass) {
    const card = document.getElementById(`${type}Card`);
    const count = document.getElementById(`${type}Count`);
    const content = document.getElementById(`${type}Content`);

    if (!card || !count || !content) {
        console.warn(`Missing elements for ${type}`);
        return;
    }

    if (entries && entries.length > 0) {
        card.style.display = 'block';
        count.textContent = entries.length;
        
        content.innerHTML = entries.map(entry => `
            <div class="log-entry ${cssClass}">
                <div class="log-entry-meta">
                    ${entry.timestamp ? `<span>?? ${escapeHtml(entry.timestamp)}</span>` : ''}
                    ${entry.lineNumber ? `<span>?? ${escapeHtml(entry.lineNumber)}</span>` : ''}
                    ${entry.category ? `<span class="log-category">${escapeHtml(entry.category)}</span>` : ''}
                </div>
                <div class="log-entry-message">${escapeHtml(entry.message)}</div>
            </div>
        `).join('');
    } else {
        card.style.display = 'none';
    }
}

// Format text with basic markdown-like formatting
function formatText(text) {
    if (!text) return '<p style="color: var(--gray-500);">No information available</p>';
    
    let formatted = text;
    
    // Handle code blocks
    formatted = formatted.replace(/```(\w*)\n?([\s\S]*?)```/g, (match, lang, code) => {
        return `<pre><code>${escapeHtml(code.trim())}</code></pre>`;
    });
    
    const sections = formatted.split('\n\n');
    
    formatted = sections.map(section => {
        if (section.includes('<pre><code>')) {
            return section;
        }
        
        // Numbered list
        if (section.match(/^\d+[\.)]/m)) {
            const items = section.split('\n').map(line => {
                const match = line.match(/^\d+[\.)]\s*(.+)/);
                if (match) {
                    return `<li>${escapeHtml(match[1])}</li>`;
                }
                if (line.trim() && !line.match(/^\d+[\.)]/)) {
                    return escapeHtml(line);
                }
                return '';
            }).filter(Boolean).join('\n');
            return `<ol>${items}</ol>`;
        }
        
        // Bulleted list
        if (section.match(/^[\-\*•]/m)) {
            const items = section.split('\n').map(line => {
                const match = line.match(/^[\-\*•]\s*(.+)/);
                if (match) {
                    return `<li>${escapeHtml(match[1])}</li>`;
                }
                if (line.trim() && !line.match(/^[\-\*•]/)) {
                    return escapeHtml(line);
                }
                return '';
            }).filter(Boolean).join('\n');
            return `<ul>${items}</ul>`;
        }
        
        // Regular paragraph
        const lines = section.split('\n').map(line => escapeHtml(line.trim())).filter(Boolean);
        return `<p>${lines.join('<br>')}</p>`;
    }).join('');
    
    // Bold text
    formatted = formatted.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
    
    // Inline code
    formatted = formatted.replace(/`([^`]+)`/g, '<code>$1</code>');
    
    return formatted;
}

// Escape HTML
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Download report
function downloadReport() {
    const summary = document.getElementById('summaryContent')?.innerText || '';
    const severity = document.getElementById('severityBadge')?.textContent || '';
    const rootCause = document.getElementById('rootCauseContent')?.innerText || '';
    const suggestedFix = document.getElementById('suggestedFixContent')?.innerText || '';
    const timestamp = document.getElementById('analyzedAt')?.textContent || '';

    const report = `
==============================================
AI LOG ANALYSIS REPORT
==============================================
Generated: ${timestamp}
Severity: ${severity}

SUMMARY
----------------------------------------------
${summary}

ROOT CAUSE
----------------------------------------------
${rootCause}

SUGGESTED FIX
----------------------------------------------
${suggestedFix}

==============================================
Generated by AI Log Analyzer v2.0
==============================================
    `.trim();

    const blob = new Blob([report], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `log-analysis-${Date.now()}.txt`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    
    showToast('Report downloaded successfully!', 'success');
}

// ================== TOAST NOTIFICATIONS ==================
function showToast(message, type = 'info') {
    // Remove existing toast
    const existingToast = document.querySelector('.toast');
    if (existingToast) existingToast.remove();
    
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    
    const icon = {
        success: '?',
        error: '?',
        warning: '??',
        info: '??'
    }[type] || '??';
    
    toast.innerHTML = `
        <div class="toast-content">
            <span class="toast-icon">${icon}</span>
            <span class="toast-message">${escapeHtml(message)}</span>
        </div>
    `;
    
    document.body.appendChild(toast);
    
    setTimeout(() => toast.classList.add('show'), 10);
    
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

// Add CSS for spinning animation
const style = document.createElement('style');
style.textContent = `
    .spinning {
        animation: spin 1s linear infinite;
    }
`;
document.head.appendChild(style);
