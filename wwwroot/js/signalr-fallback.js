(function() {
    if (typeof signalR !== 'undefined') {
        console.log('SignalR is already loaded');
        return;
    }
    
    console.log('Loading SignalR from CDN...');
    
    const script = document.createElement('script');
    script.src = 'https://cdn.jsdelivr.net/npm/@microsoft/signalr@latest/dist/browser/signalr.min.js';
    script.integrity = 'sha384-HwlJ7oCZnSDq9+LKSvtqHQHMsZR5CX0thkUBgP7E1fDvjQIrfxVbD0mZpGQIHOUJ';
    script.crossOrigin = 'anonymous';
    
    script.onload = function() {
        console.log('SignalR loaded successfully from CDN');
        document.dispatchEvent(new Event('signalRLoaded'));
    };
    
    script.onerror = function() {
        console.error('Failed to load SignalR from CDN');
        showSignalRError();
    };
    
    document.head.appendChild(script);
    
    function showSignalRError() {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger m-3 position-fixed top-0 end-0';
        errorDiv.innerHTML = `
            <strong>Chat functionality is unavailable.</strong><br>
            Failed to load required libraries. Please try refreshing the page or contact support.
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;
        document.body.appendChild(errorDiv);
    }
})();
