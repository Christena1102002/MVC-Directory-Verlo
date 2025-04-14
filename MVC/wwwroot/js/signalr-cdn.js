/**
 * This file provides a consistent way to load SignalR from CDN with fallback
 */
(function() {
    // First check if SignalR is already loaded
    if (typeof signalR !== 'undefined') {
        console.log('SignalR is already loaded');
        return;
    }
    
    console.log('Loading SignalR from CDN...');
    
    const loadSignalR = () => {
        // Load from CDN (version-specific for stability without integrity)
        const script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/@microsoft/signalr@6.0.10/dist/browser/signalr.min.js';
        script.crossOrigin = 'anonymous';
        
        script.onload = function() {
            console.log('SignalR loaded successfully from CDN');
            
            // Check if signalR is actually defined
            if (typeof signalR !== 'undefined') {
                document.dispatchEvent(new Event('signalRLoaded'));
            } else {
                console.error('SignalR object not found even though script loaded');
                loadFromAlternativeCDN();
            }
        };
        
        script.onerror = function() {
            console.error('Failed to load SignalR from CDN, trying alternative CDN...');
            loadFromAlternativeCDN();
        };
        
        document.head.appendChild(script);
    };
    
    const loadFromAlternativeCDN = () => {
        const script = document.createElement('script');
        // Try unpkg as alternative
        script.src = 'https://unpkg.com/@microsoft/signalr@6.0.10/dist/browser/signalr.min.js';
        
        script.onload = function() {
            console.log('SignalR loaded successfully from alternative CDN');
            if (typeof signalR !== 'undefined') {
                document.dispatchEvent(new Event('signalRLoaded'));
            } else {
                console.error('SignalR object not found from alternative CDN');
                loadInlineSignalR();
            }
        };
        
        script.onerror = function() {
            console.error('Failed to load SignalR from alternative CDN, trying minimal implementation...');
            loadInlineSignalR();
        };
        
        document.head.appendChild(script);
    };
    
    const loadInlineSignalR = () => {
        console.log('Loading minimal inline SignalR implementation...');
        
        // Create a minimal version of SignalR that implements just what we need
        window.signalR = {
            HubConnectionBuilder: function() {
                return {
                    withUrl: function() { 
                        return this;
                    },
                    withAutomaticReconnect: function() {
                        return this;
                    },
                    build: function() {
                        return {
                            start: function() {
                                return Promise.resolve();
                            },
                            on: function() {},
                            onreconnecting: function() {},
                            onreconnected: function() {},
                            onclose: function() {},
                            invoke: function() {
                                return Promise.resolve([]);
                            }
                        };
                    }
                };
            }
        };
        
        console.log('Added minimal SignalR implementation as fallback');
        document.dispatchEvent(new Event('signalRLoaded'));
        
        showSignalRError();
    };
    
    const showSignalRError = () => {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-warning m-3 position-fixed bottom-0 end-0';
        errorDiv.style.zIndex = '9999';
        errorDiv.innerHTML = `
            <strong>Limited functionality available</strong><br>
            Real-time features are disabled. Please try refreshing the page.
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;
        document.body.appendChild(errorDiv);
        
        // Hide after 10 seconds
        setTimeout(() => {
            if (document.body.contains(errorDiv)) {
                errorDiv.classList.add('fade');
                setTimeout(() => errorDiv.remove(), 500);
            }
        }, 10000);
    };
    
    // Start loading
    loadSignalR();
})();
