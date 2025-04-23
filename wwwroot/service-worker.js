/**
 * Service Worker to provide basic offline experience
 */

// Cache version (remember to update when changing static files)
const CACHE_VERSION = 'vyrlo-v1';

// Files to cache
const CACHED_FILES = [
    '/',
    '/offline.html',
    '/css/site.css',
    '/js/site.js',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
    '/favicon.ico',
    'https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css',
    'https://cdn.jsdelivr.net/npm/@popperjs/core@2.11.6/dist/umd/popper.min.js',
    'https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js',
    'https://code.jquery.com/jquery-3.6.0.min.js',
    'https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css',
    'https://cdn.jsdelivr.net/npm/@microsoft/signalr@6.0.10/dist/browser/signalr.min.js'
];

// When Service Worker is installed
self.addEventListener('install', event => {
    console.log('[Service Worker] Installing...');
    
    // Immediate install, skip waiting
    self.skipWaiting();
    
    event.waitUntil(
        caches.open(CACHE_VERSION)
            .then(cache => {
                console.log('[Service Worker] Caching app shell and content...');
                // Cache core files
                return cache.addAll(CACHED_FILES);
            })
    );
});

// When Service Worker is activated
self.addEventListener('activate', event => {
    console.log('[Service Worker] Activating...');
    
    // Update cache and remove old versions
    event.waitUntil(
        caches.keys()
            .then(cacheNames => {
                return Promise.all(
                    cacheNames
                        .filter(cacheName => cacheName !== CACHE_VERSION)
                        .map(cacheName => {
                            console.log('[Service Worker] Clearing old cache:', cacheName);
                            return caches.delete(cacheName);
                        })
                );
            })
            .then(() => {
                // Take control of pages without waiting for refresh
                return self.clients.claim();
            })
    );
});

// Handle network requests
self.addEventListener('fetch', event => {
    // Ignore API paths and content management
    if (event.request.url.includes('/api/') || 
        event.request.url.includes('/signalr') || 
        event.request.url.includes('/admin')) {
        return;
    }
    
    event.respondWith(
        // Try new network request
        fetch(event.request)
            .then(response => {
                // Check for valid response (status code 2xx)
                if (!response || response.status !== 200 || response.type !== 'basic') {
                    return response;
                }
                
                // Store a copy of the response in cache
                const responseToCache = response.clone();
                
                // Update cache only for static requests (CSS, JS, images)
                if (isStaticRequest(event.request.url)) {
                    caches.open(CACHE_VERSION)
                        .then(cache => {
                            cache.put(event.request, responseToCache);
                        });
                }
                
                return response;
            })
            .catch(error => {
                console.log('[Service Worker] Fetch failed; returning offline page instead.', error);
                
                // If request fails, try to get resources from cache
                return caches.match(event.request)
                    .then(cachedResponse => {
                        if (cachedResponse) {
                            // If found in cache, use it
                            return cachedResponse;
                        }
                        
                        // If it's an HTML page request (navigation), show offline page
                        if (event.request.mode === 'navigate') {
                            return caches.match('/offline.html');
                        }
                        
                        // If not a page request and not in cache, return 404 error
                        return new Response('Content not available offline', {
                            status: 404,
                            headers: new Headers({
                                'Content-Type': 'text/plain'
                            })
                        });
                    });
            })
    );
});

// Check if it's a static request
function isStaticRequest(url) {
    const staticExtensions = ['.css', '.js', '.jpg', '.jpeg', '.png', '.gif', '.svg', '.ico', '.woff', '.woff2', '.ttf'];
    return staticExtensions.some(ext => url.toLowerCase().endsWith(ext));
}
