// This script checks if SignalR is properly loaded and provides a fallback if needed
(function() {
    // Check if SignalR is already loaded
    if (typeof signalR !== 'undefined') {
        console.log('SignalR is already loaded');
        return;
    }
    
    // If not, try to load it from CDN
    console.log('Loading SignalR from CDN...');
    
    const script = document.createElement('script');
    script.src = 'https://cdn.jsdelivr.net/npm/@microsoft/signalr@latest/dist/browser/signalr.min.js';
    script.integrity = 'sha384-HwlJ7oCZnSDq9+LKSvtqHQHMsZR5CX0thkUBgP7E1fDvjQIrfxVbD0mZpGQIHOUJ'; // Optional but recommended for security
    script.crossOrigin = 'anonymous';
    
    script.onload = function() {
        console.log('SignalR loaded successfully from CDN');
        // Trigger an event to notify that SignalR is now loaded
        document.dispatchEvent(new Event('signalRLoaded'));
    };
    
    script.onerror = function() {
        console.error('Failed to load SignalR from CDN');
        // Possibly try another CDN or show an error message to the user
        showSignalRError();
    };
    
    document.head.appendChild(script);
    
    function showSignalRError() {
        // Create a notification for the user
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
