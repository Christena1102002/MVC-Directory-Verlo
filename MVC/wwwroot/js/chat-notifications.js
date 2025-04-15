// Chat notifications system for the website

document.addEventListener('DOMContentLoaded', function() {
    const initChat = () => {
        if (typeof signalR === 'undefined') {
            console.error('SignalR is not loaded yet');
            
            document.addEventListener('signalRLoaded', initChat);
            return;
        }
        
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/chathub")
            .withAutomaticReconnect([0, 1000, 5000, 10000, 30000])
            .build();
            
        connection.onreconnecting(error => {
            console.warn('Connection lost. Reconnecting...', error);
            showConnectionStatus('Reconnecting...');
        });
        
        connection.onreconnected(connectionId => {
            console.log('Connection reestablished. ID:', connectionId);
            hideConnectionStatus();
            loadChatNotifications();
        });
        
        connection.onclose(error => {
            console.error('Connection closed', error);
            showConnectionStatus('Connection lost, attempting to reconnect...');
        });

        connection.start()
            .then(() => {
                console.log("Chat Notifications Connected");
                hideConnectionStatus();
                loadChatNotifications();
            })
            .catch(err => {
                console.error("Error connecting to chat hub:", err);
              
            });
        
        function showConnectionStatus(message) {
            let statusEl = document.getElementById('chat-connection-status');
            
            if (!statusEl) {
                statusEl = document.createElement('div');
                statusEl.id = 'chat-connection-status';
                statusEl.className = 'alert alert-warning position-fixed top-0 start-50 translate-middle-x m-3';
                statusEl.style.zIndex = '9999';
                document.body.appendChild(statusEl);
            }
            
            statusEl.innerHTML = `
                <i class="fas fa-exclamation-triangle me-2"></i>${message}
            `;
            
            statusEl.style.display = 'block';
        }
        
        function hideConnectionStatus() {
            const statusEl = document.getElementById('chat-connection-status');
            if (statusEl) {
                statusEl.style.display = 'none';
            }
        }
        
        async function loadChatNotifications() {
            try {
                let conversations;
                
                if (document.body.classList.contains('role-admin')) {
                    conversations = await connection.invoke("GetAdminConversations");
                } else {
                    conversations = await connection.invoke("GetUserConversations");
                }
                
                renderNotifications(conversations);
                
                updateUnreadCount(conversations);
            } catch (err) {
                console.error("Error loading chat notifications:", err);
                document.getElementById('chatNotificationsList').innerHTML = `
                    <div class="dropdown-item py-3 text-center text-danger">
                        <i class="fas fa-exclamation-circle me-2"></i>Error loading notifications
                    </div>
                `;
            }
        }
        
        function renderNotifications(conversations) {
            const container = document.getElementById('chatNotificationsList');
            if (!container) return;
            
            const unreadConversations = conversations.filter(c => c.unreadCount > 0);
            
            if (unreadConversations.length === 0) {
                container.innerHTML = `
                    <div class="dropdown-item py-3 text-center text-muted">
                        <i class="fas fa-check-circle me-2"></i>No new messages
                    </div>
                `;
                return;
            }
            
            unreadConversations.sort((a, b) => new Date(b.lastMessageAt) - new Date(a.lastMessageAt));
            
            container.innerHTML = '';
            
            for (let i = 0; i < Math.min(unreadConversations.length, 5); i++) {
                const conv = unreadConversations[i];
                const isAdmin = document.body.classList.contains('role-admin');
                
                const displayName = isAdmin ? conv.userName : 'Support Team';
                
                const timeAgo = getTimeAgo(new Date(conv.lastMessageAt));
                
                const item = document.createElement('a');
                item.href = isAdmin ? `/AdminChat/Index?id=${conv.id}` : `/Support/UserChat?id=${conv.id}`;
                item.className = 'dropdown-item p-3 border-bottom';
                
                item.innerHTML = `
                    <div class="d-flex justify-content-between align-items-center mb-1">
                        <strong class="text-primary">${escapeHtml(displayName)}</strong>
                        <span class="badge bg-danger rounded-pill">${conv.unreadCount}</span>
                    </div>
                    <div class="text-truncate text-muted small">
                        ${escapeHtml(conv.lastMessage || 'New conversation')}
                    </div>
                    <div class="text-end">
                        <small class="text-muted">${timeAgo}</small>
                    </div>
                `;
                
                container.appendChild(item);
            }
            
            if (unreadConversations.length > 5) {
                const viewAllLink = document.createElement('div');
                viewAllLink.className = 'dropdown-item text-center py-2 text-primary';
                const isAdmin = document.body.classList.contains('role-admin');
                
                viewAllLink.innerHTML = `
                    <i class="fas fa-arrow-circle-right me-1"></i>
                    View all messages (${unreadConversations.length})
                `;
                
                viewAllLink.addEventListener('click', () => {
                    window.location.href = isAdmin ? '/AdminChat/Index' : '/Support/UserChat';
                });
                
                container.appendChild(viewAllLink);
            }
        }
        
        function updateUnreadCount(conversations) {
            const badge = document.querySelector('.chat-badge');
            if (!badge) return;
            
            const totalUnread = conversations.reduce((total, conv) => total + conv.unreadCount, 0);
            
            if (totalUnread > 0) {
                badge.textContent = totalUnread > 99 ? '99+' : totalUnread;
                badge.classList.remove('d-none');
            } else {
                badge.classList.add('d-none');
            }
            
            updatePageTitle(totalUnread);
        }
        
        function updatePageTitle(unreadCount) {
            const originalTitle = document.title.replace(/^\(\d+\)\s/, '');
            
            if (unreadCount > 0) {
                document.title = `(${unreadCount}) ${originalTitle}`;
            } else {
                document.title = originalTitle;
            }
        }
        
        connection.on("ReceiveAdminMessage", function(data) {
            if (document.body.classList.contains('role-admin')) {
                loadChatNotifications();
                showToastNotification(`New message from ${data.senderName}`, data.message);
            }
        });
        
        connection.on("ReceiveReply", function(data) {
            if (!document.body.classList.contains('role-admin')) {
                loadChatNotifications();
                showToastNotification('New message from Support Team', data.message);
            }
        });
        
        function showToastNotification(title, message) {
            if (!("Notification" in window)) {
                console.log("This browser does not support desktop notifications");
                return;
            }
            
            if (Notification.permission === "granted") {
                const notification = new Notification(title, { 
                    body: message,
                    icon: '/images/logo-icon.png'
                });
                
                notification.onclick = function() {
                    window.focus();
                    const chatPath = document.body.classList.contains('role-admin') ? 
                        '/AdminChat/Index' : '/Support/UserChat';
                    window.location.href = chatPath;
                };
            } else if (Notification.permission !== "denied") {
                Notification.requestPermission().then(function(permission) {
                    if (permission === "granted") {
                        const notification = new Notification(title, { 
                            body: message,
                            icon: '/images/logo-icon.png'
                        });
                        
                        notification.onclick = function() {
                            window.focus();
                            const chatPath = document.body.classList.contains('role-admin') ? 
                                '/AdminChat/Index' : '/Support/UserChat';
                            window.location.href = chatPath;
                        };
                    }
                });
            }
        }
        
        const refreshBtn = document.querySelector('.btn-refresh-notifications');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', function(e) {
                e.preventDefault();
                
                this.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
                
                loadChatNotifications()
                    .finally(() => {
                        this.innerHTML = '<i class="fas fa-sync-alt"></i>';
                    });
            });
        }
        
        setInterval(loadChatNotifications, 60000);
        
        function escapeHtml(text) {
            if (!text) return '';
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }
        
        function getTimeAgo(date) {
            const now = new Date();
            const diffMs = now - date;
            const diffSec = Math.floor(diffMs / 1000);
            const diffMin = Math.floor(diffSec / 60);
            const diffHour = Math.floor(diffMin / 60);
            const diffDay = Math.floor(diffHour / 24);
            
            if (diffDay > 0) {
                return `${diffDay} ${diffDay === 1 ? 'day' : 'days'} ago`;
            } else if (diffHour > 0) {
                return `${diffHour} ${diffHour === 1 ? 'hour' : 'hours'} ago`;
            } else if (diffMin > 0) {
                return `${diffMin} ${diffMin === 1 ? 'minute' : 'minutes'} ago`;
            } else {
                return 'Just now';
            }
        }
    };

    if ("Notification" in window && Notification.permission === "default") {
        document.addEventListener('click', function requestNotificationPermission() {
            Notification.requestPermission();
            document.removeEventListener('click', requestNotificationPermission);
        }, {once: true});
    }

    initChat();
});
