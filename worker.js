/**
 * AI Log Analyzer - Cloudflare Worker
 * Handles log analysis requests using OpenRouter API
 */

export default {
  async fetch(request, env, ctx) {
    const url = new URL(request.url);

    // CORS Headers
    const corsHeaders = {
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'GET, POST, OPTIONS',
      'Access-Control-Allow-Headers': 'Content-Type',
    };

    // Handle OPTIONS request (CORS preflight)
    if (request.method === 'OPTIONS') {
      return new Response(null, { headers: corsHeaders });
    }

    // Route: /api/LogAnalyzer/analyze
    if (url.pathname === '/api/LogAnalyzer/analyze' && request.method === 'POST') {
      try {
        const { logs, logType } = await request.json();

        if (!logs) {
          return new Response(JSON.stringify({ error: 'No logs provided' }), {
            status: 400,
            headers: { ...corsHeaders, 'Content-Type': 'application/json' },
          });
        }

        // Perform analysis
        const analysis = await analyzeLogs(logs, logType, env);

        return new Response(JSON.stringify(analysis), {
          headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        });
      } catch (error) {
        return new Response(JSON.stringify({ error: error.message }), {
          status: 500,
          headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        });
      }
    }

    // Default route: Health check or info
    return new Response('AI Log Analyzer API is running. Point your frontend to /api/LogAnalyzer/analyze', {
      headers: { 'Content-Type': 'text/plain' },
    });
  },
};

/**
 * Core analysis logic (Ported from C#)
 */
async function analyzeLogs(logs, logType, env) {
  const apiKey = env.OPENROUTER_API_KEY;
  const model = env.OPENROUTER_MODEL || 'openai/gpt-3.5-turbo';
  const baseUrl = 'https://openrouter.ai/api/v1/chat/completions';

  if (!apiKey) {
    throw new Error('OPENROUTER_API_KEY environment variable is not set');
  }

  // Quick Pattern Detection
  const quickAnalysis = {
    errorCount: (logs.match(/\berror\b|\bexception\b|\bfailed\b/gi) || []).length,
    warningCount: (logs.match(/\bwarn(ing)?\b/gi) || []).length,
    nullCount: (logs.match(/null\s*reference|nullpointer/gi) || []).length,
    dbCount: (logs.match(/database|sql|connection/gi) || []).length,
    authCount: (logs.match(/auth|unauthorized|forbidden/gi) || []).length,
    timeoutCount: (logs.match(/timeout|timed\s*out/gi) || []).length,
  };

  const logTypeHint = logType ? `Log Type: ${logType}\n` : '';
  const prompt = `You are a senior software engineer and debugging expert. Analyze the following application logs and provide a detailed, structured analysis.

${logTypeHint}
DETECTED PATTERNS:
- Errors found: ${quickAnalysis.errorCount}
- Warnings found: ${quickAnalysis.warningCount}
- Null reference patterns: ${quickAnalysis.nullCount}
- Database issues: ${quickAnalysis.dbCount}
- Authentication issues: ${quickAnalysis.authCount}
- Timeout issues: ${quickAnalysis.timeoutCount}

LOGS TO ANALYZE:
${logs.substring(0, 40000)}

CRITICAL: You MUST respond with ONLY a valid JSON object. Do not include any markdown formatting, code blocks, or explanatory text.

Your response must be a valid JSON object with this exact structure:
{
  "summary": "Comprehensive overview: What happened? Is it server-side or client-side? Overall health status.",
  "errors": [
    {
      "message": "Detailed error description",
      "lineNumber": "line number if visible",
      "timestamp": "timestamp if visible",
      "category": "error category"
    }
  ],
  "warnings": [
    {
      "message": "Warning description",
      "lineNumber": "line number",
      "category": "warning category"
    }
  ],
  "info": [
    {
      "message": "Contextual info",
      "timestamp": "timestamp"
    }
  ],
  "rootCause": "In-depth technical analysis of the primary issue.",
  "suggestedFix": "Step-by-step solution with code examples.",
  "severity": "Info|Low|Medium|High|Critical"
}

Output ONLY the JSON object.`;

  const response = await fetch(baseUrl, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${apiKey}`,
      'Content-Type': 'application/json',
      'HTTP-Referer': 'https://github.com/raja4kisan/AILogAnalyzer',
      'X-Title': 'AI Log Analyzer',
    },
    body: JSON.stringify({
      model: model,
      messages: [
        { role: 'system', content: 'You are a professional log analysis assistant. Provide analysis in pure JSON format.' },
        { role: 'user', content: prompt }
      ],
      response_format: { type: 'json_object' }
    }),
  });

  if (!response.ok) {
    const errorBody = await response.text();
    throw new Error(`OpenRouter API error: ${response.status} - ${errorBody}`);
  }

  const result = await response.json();
  let content = result.choices[0].message.content.trim();

  // Clean Markdown blocks
  content = content.replace(/^```json/i, '').replace(/^```/i, '').replace(/```$/i, '').trim();

  const finalAnalysis = JSON.parse(content);
  finalAnalysis.analyzedAt = new Date().toISOString();
  
  return finalAnalysis;
}
