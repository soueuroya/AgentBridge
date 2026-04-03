import http from 'http';
import fs from 'fs';
import path from 'path';
import { WebSocket } from 'ws';

/**
 * BridgeLinkNode (Node.js)
 * High-performance real-time bridge between Unity and Antigravity.
 * 
 * This script:
 * 1. Discovers the running Antigravity instance via CDP.
 * 2. Injects prompts from Unity directly into the Antigravity session.
 * 3. Clicks the "Submit" button automatically.
 * 4. Waits for the AI response to appear in ActiveResponse.json.
 */

const PORT = 11500;
const CDP_PORTS = [11411, 9001, 9002, 9003, 9004, 9005, 9006, 9007, 9008, 9009, 9010, 9011, 9012, 9013, 9014, 9015, 9222, 9223, 9224, 9225, 9226, 9227, 9228, 9229, 9230, 9231, 9000];
const BRIDGE_DIR = '.agentbridge_bridge';
const RESPONSE_FILE = path.join(BRIDGE_DIR, 'ActiveResponse.json');

// --- CDP Discovery ---

async function getJson(url) {
    return new Promise((resolve, reject) => {
        const req = http.get(url, (res) => {
            let data = '';
            res.on('data', chunk => data += chunk);
            res.on('end', () => {
                try { resolve(JSON.parse(data)); } catch (e) { reject(e); }
            });
        });
        req.on('error', reject);
        req.setTimeout(2000, () => { req.destroy(); reject(new Error('timeout')); });
    });
}

function isAntigravity(t) {
    const url = (t.url || '').toLowerCase();
    const title = (t.title || '').toLowerCase();
    const type = (t.type || '').toLowerCase();
    
    // Only target top-level pages
    if (type !== 'page') return false;

    // Explicitly ignore management pages that don't have chat inputs
    if (title.includes('manage mcp') || title.includes('settings')) return false;

    return url.includes('workbench') || 
           url.includes('jetski') || 
           url.includes('antigravity') ||
           url.includes('vscode-file') || // Electron target
           title.includes('antigravity') ||
           title.includes('launchpad') || // Your specific session title
           title.includes('agentic ide') ||
           url.includes('localhost:3000') ||
           url.includes('google.com');
}

async function findAllAntigravityTargets() {
    console.log("[Bridge] Starting CDP Scan across ports: " + CDP_PORTS.join(', '));
    let candidates = [];

    for (const port of CDP_PORTS) {
        try {
            const url = `http://127.0.0.1:${port}/json/list`;
            const list = await getJson(url);
            
            if (!Array.isArray(list)) continue;

            for (const t of list) {
                if (t.type !== 'page') continue;
                
                // Prioritize Launchpad and Antigravity-titled pages
                if (isAntigravity(t)) {
                    if (t.title && t.title.toLowerCase().includes('launchpad')) {
                        candidates.unshift(t.webSocketDebuggerUrl); // High priority
                    } else {
                        candidates.push(t.webSocketDebuggerUrl);
                    }
                }
            }
        } catch (e) {}
    }
    return candidates;
}

// --- Interaction ---

async function injectPrompt(wsUrl, prompt) {
    let ws;
    try {
        ws = new WebSocket(wsUrl);
        await new Promise((resolve, reject) => {
            ws.on('open', resolve);
            ws.on('error', reject);
            setTimeout(() => reject(new Error('WS Timeout')), 5000);
        });

        const id = Math.floor(Math.random() * 1000000);
        const safeText = JSON.stringify(prompt);

        const EXPRESSION = `(async () => {
            function findEditor(root) {
                const selectors = [
                    'div[data-lexical-editor="true"][contenteditable="true"]',
                    'textarea[placeholder*="What can I help you with"]',
                    'div.editor-container div[contenteditable="true"]',
                    'div.monaco-editor div[contenteditable="true"]',
                    '#cascade [data-lexical-editor="true"][contenteditable="true"]',
                    '[contenteditable="true"][role="textbox"]',
                    '[contenteditable="true"]',
                    'textarea'
                ];
                
                for (const sel of selectors) {
                    const el = [...root.querySelectorAll(sel)].filter(e => e.offsetParent !== null || e.tagName === "TEXTAREA").at(-1);
                    if (el) return el;
                }

                const iframes = root.querySelectorAll('iframe');
                for (const f of iframes) {
                    try {
                        const el = findEditor(f.contentDocument || f.contentWindow.document);
                        if (el) return el;
                    } catch(e) {}
                }
                return null;
            }

            let editor = null;
            for (let i = 0; i < 5; i++) {
                editor = findEditor(document);
                if (editor) break;
                await new Promise(r => setTimeout(r, 200));
            }

            if (!editor) return { ok:false, reason:"editor_not_found", context: document.title, html: document.body.innerText.substring(0, 100) };

            editor.focus();
            if (editor.tagName === 'TEXTAREA') {
                editor.value = \${safeText};
                editor.dispatchEvent(new Event('input', { bubbles: true }));
                editor.dispatchEvent(new Event('change', { bubbles: true }));
            } else {
                document.execCommand?.("selectAll", false, null);
                document.execCommand?.("delete", false, null);
                let inserted = false;
                try { inserted = !!document.execCommand?.("insertText", false, \${safeText}); } catch {}
                if (!inserted) {
                    editor.innerText = \${safeText};
                    editor.dispatchEvent(new InputEvent("input", { bubbles:true, inputType:"insertText", data:\${safeText} }));
                }
            }

            await new Promise(r => setTimeout(r, 100));

            function findSubmit(root) {
                const selectors = [
                    'svg.lucide-arrow-right',
                    'svg.lucide-arrow-up',
                    'button.flex.items-center.justify-center.rounded-full.bg-primary',
                    'button[aria-label*="Send"]',
                    'button[aria-label*="Submit"]',
                    'button.submit-button',
                    'button[type="submit"]'
                ];

                for (const sel of selectors) {
                    const el = root.querySelector(sel)?.closest("button");
                    if (el && !el.disabled && (el.offsetParent !== null || el.type === "submit")) return el;
                }

                const iframes = root.querySelectorAll('iframe');
                for (const f of iframes) {
                    try {
                        const el = findSubmit(f.contentDocument || f.contentWindow.document);
                        if (el) return el;
                    } catch(e) {}
                }
                return null;
            }

            const submit = findSubmit(document);
            if (submit) {
                submit.click();
                return { ok:true, method:"click_submit" };
            }
            
            editor.dispatchEvent(new KeyboardEvent("keydown", { bubbles:true, key:"Enter", code:"Enter", keyCode: 13 }));
            editor.dispatchEvent(new KeyboardEvent("keyup", { bubbles:true, key:"Enter", code:"Enter", keyCode: 13 }));
            return { ok:true, method:"enter_keypress" };
        })()`;

        const payload = {
            id,
            method: 'Runtime.evaluate',
            params: {
                expression: EXPRESSION,
                returnByValue: true,
                awaitPromise: true
            }
        };

        ws.send(JSON.stringify(payload));

        const result = await new Promise((resolve, reject) => {
            const handler = (data) => {
                const msg = JSON.parse(data.toString());
                if (msg.id === id) {
                    ws.off('message', handler);
                    resolve(msg.result);
                }
            };
            ws.on('message', handler);
            setTimeout(() => reject(new Error('Eval Timeout')), 5000);
        });

        ws.close();
        return result;
    } catch (e) {
        if (ws) ws.close();
        return { result: { value: { ok: false, reason: "websocket_error", message: e.message } } };
    }
}

// --- Main Server ---

const server = http.createServer(async (req, res) => {
    if (req.method === 'GET' && req.url === '/ping') {
        res.writeHead(200);
        res.end("pong");
        return;
    }

    if (req.method === 'POST') {
        let body = '';
        req.on('data', chunk => body += chunk);
        req.on('end', async () => {
            try {
                console.log(`[Bridge] Received prompt from Unity: \${body.substring(0, 50)}...`);

                // 1. Discover Candidates
                const candidates = await findAllAntigravityTargets();
                if (candidates.length === 0) {
                    res.writeHead(503);
                    res.end(JSON.stringify({ error: "Antigravity session not found. Ensure Antigravity is open in a browser." }));
                    return;
                }

                // 2. Try candidates until one has an editor
                let lastResult = null;
                let success = false;
                for (const wsUrl of candidates) {
                    console.log(`[Bridge] Attempting injection into target: \${wsUrl}`);
                    const injectResult = await injectPrompt(wsUrl, body);
                    const val = injectResult?.value || injectResult?.result?.value;
                    
                    if (val && val.ok) {
                        console.log(`[Bridge] Injection SUCCESS on target: \${wsUrl}`);
                        success = true;
                        break;
                    } else {
                        console.log(`[Bridge] Injection failed on target: \${val?.reason || "unknown"}`);
                        lastResult = val;
                    }
                }

                if (!success) {
                    res.writeHead(500);
                    res.end(JSON.stringify({ error: "Could not find an active chat input in any Antigravity window.", detail: lastResult }));
                    return;
                }

                // 3. Wait for Response File
                console.log(`[Bridge] Waiting for AI response in \${RESPONSE_FILE}...`);
                const startTime = Date.now();
                const timeout = 600000; // 10 minutes (matching Unity timeout)
                
                while (Date.now() - startTime < timeout) {
                    if (fs.existsSync(RESPONSE_FILE)) {
                        const content = fs.readFileSync(RESPONSE_FILE, 'utf8');
                        fs.unlinkSync(RESPONSE_FILE); // Cleanup
                        res.writeHead(200, { 'Content-Type': 'application/json' });
                        res.end(content);
                        console.log(`[Bridge] Response returned to Unity.`);
                        return;
                    }
                    await new Promise(r => setTimeout(r, 500));
                }

                res.writeHead(504);
                res.end(JSON.stringify({ error: "Gateway Timeout: Agent did not respond in time." }));

            } catch (err) {
                console.error(`[Bridge] Error: ${err.message}`);
                res.writeHead(500);
                res.end(JSON.stringify({ error: err.message }));
            }
        });
    } else {
        res.writeHead(404);
        res.end();
    }
});

if (!fs.existsSync(BRIDGE_DIR)) fs.mkdirSync(BRIDGE_DIR);

server.listen(PORT, () => {
    console.log(`\x1b[32m[BridgeLinkNode] Real-Time Link active on http://localhost:${PORT}\x1b[0m`);
    console.log(`[Bridge] Automatically discovers Antigravity via CDP.`);
    console.log(`[Bridge] Forwarding prompts to AI window and waiting for ActiveResponse.json...`);
});
