/**
 * Handles offline states
 */
(function() {
    // Check if Service Worker is supported
    if ('serviceWorker' in navigator) {
        // Register Service Worker
        navigator.serviceWorker.register('/service-worker.js')
            .then(registration => {
                console.log('Service Worker registered with scope:', registration.scope);
            })
            .catch(error => {
                console.error('Service Worker registration failed:', error);
            });
    }
    
    let isOnline = navigator.onLine;
    const offlineNotification = document.createElement('div');
    
    // Initialize notification
    function initOfflineNotification() {
        offlineNotification.className = 'offline-notification';
        offlineNotification.innerHTML = `
            <div class="offline-message">
                <i class="fas fa-exclamation-triangle me-2"></i>
                You are offline. Some features may not work.
                <button class="offline-close" title="Close">&times;</button>
            </div>
        `;
        
        document.body.appendChild(offlineNotification);
        
        // Add CSS style
        const style = document.createElement('style');
        style.textContent = `
            .offline-notification {
                position: fixed;
                bottom: 0;
                left: 0;
                right: 0;
                z-index: 9999;
                padding: 10px;
                transform: translateY(100%);
                transition: transform 0.3s ease;
            }
            
            .offline-notification.show {
                transform: translateY(0);
            }
            
            .offline-message {
                background-color: #f8d7da;
                color: #721c24;
                border: 1px solid #f5c6cb;
                border-radius: 5px;
                padding: 12px;
                display: flex;
                align-items: center;
                justify-content: space-between;
                font-size: 14px;
                box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.1);
            }
            
            .offline-close {
                background: none;
                border: none;
                color: #721c24;
                font-size: 20px;
                cursor: pointer;
                padding: 0;
                margin-left: 10px;
            }
        `;
        
        document.head.appendChild(style);
        
        // Close event handler
        offlineNotification.querySelector('.offline-close').addEventListener('click', function() {
            offlineNotification.classList.remove('show');
        });
    }
    
    // Show offline notification
    function showOfflineNotification() {
        if (!document.body.contains(offlineNotification)) {
            initOfflineNotification();
        }
        
        offlineNotification.classList.add('show');
    }
    
    // Hide offline notification
    function hideOfflineNotification() {
        if (document.body.contains(offlineNotification)) {
            offlineNotification.classList.remove('show');
        }
    }
    
    // Monitor connection state
    window.addEventListener('online', function() {
        isOnline = true;
        hideOfflineNotification();
        
        // Reload deferred data
        document.dispatchEvent(new Event('connectivity-restored'));
    });
    
    window.addEventListener('offline', function() {
        isOnline = false;
        showOfflineNotification();
    });
    
    // Check connection status at startup
    document.addEventListener('DOMContentLoaded', function() {
        if (!isOnline) {
            showOfflineNotification();
        }
    });
    
    // API to check connection status
    window.networkStatus = {
        isOnline: () => isOnline,
        checkConnection: () => {
            return new Promise((resolve) => {
                if (isOnline) {
                    // Test real server connection
                    fetch('/health-check', { 
                        method: 'HEAD',
                        cache: 'no-store',
                        mode: 'no-cors'
                    })
                    .then(() => {
                        resolve(true);
                    })
                    .catch(() => {
                        isOnline = false;
                        showOfflineNotification();
                        resolve(false);
                    });
                } else {
                    resolve(false);
                }
            });
        }
    };
})();
