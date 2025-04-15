// Notifications System

document.addEventListener('DOMContentLoaded', function() {
    // Only run if the notifications element exists
    if (!document.getElementById('notificationDropdown')) return;

    // SignalR connection for real-time notifications
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/notificationHub')
        .build();
        
    // Start the connection
    connection.start().catch(err => console.error('Error connecting to SignalR:', err));
    
    // Handle incoming reviews
    connection.on('NewNotification', function() {
        checkNotifications();
    });
    
    // Check for notifications when the page loads
    checkNotifications();
    
    // Mark all notifications as read
    document.getElementById('markAllRead').addEventListener('click', function(e) {
        e.preventDefault();
        
        fetch('/Review/MarkAllAsRead', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                document.querySelectorAll('.notification-item').forEach(item => {
                    item.classList.remove('unread');
                });
                
                document.getElementById('notificationBadge').style.display = 'none';
                showToast('success', 'All notifications marked as read');
            }
        })
        .catch(error => console.error('Error marking notifications as read:', error));
    });
    
    // Function to check for notifications
    function checkNotifications() {
        // Get user notifications
        fetch('/Review/GetUserBusinessReviewNotifications')
            .then(response => response.json())
            .then(data => {
                updateNotificationsList(data);
                
                // Update notification count
                fetch('/Review/GetUnreadReviewsCountForUserBusinesses')
                    .then(response => response.json())
                    .then(count => {
                        const badge = document.getElementById('notificationBadge');
                        
                        if (count > 0) {
                            badge.textContent = count > 99 ? '99+' : count;
                            badge.style.display = 'block';
                        } else {
                            badge.style.display = 'none';
                        }
                    })
                    .catch(error => console.error('Error getting notification count:', error));
            })
            .catch(error => console.error('Error getting notifications:', error));
    }
    
    // Function to update notifications list
    function updateNotificationsList(notifications) {
        const notificationsList = document.getElementById('notificationsList');
        const emptyNotification = document.querySelector('.notification-empty');
        
        // Remove existing notifications except headers and footers
        document.querySelectorAll('.notification-item').forEach(item => {
            item.remove();
        });
        
        if (notifications && notifications.length > 0) {
            // Hide empty notification message
            emptyNotification.style.display = 'none';
            
            // Get the divider after the header
            const headerDivider = notificationsList.querySelector('hr.dropdown-divider');
            
            // Add notifications before the last divider
            notifications.forEach(notification => {
                const listItem = document.createElement('li');
                listItem.className = 'notification-item' + (notification.isRead ? '' : ' unread');
                listItem.dataset.id = notification.id;
                listItem.dataset.businessId = notification.businessId;
                
                listItem.innerHTML = `
                    <a href="/Business/GetBusinessById?id=${notification.businessId}" class="dropdown-item">
                        <div class="notification-content">
                            <strong>${notification.email}</strong> left a ${notification.rating}-star review on <strong>${notification.businessName}</strong>
                        </div>
                        <div class="notification-time">${notification.timeAgo}</div>
                    </a>
                `;
                
                // Add click event to mark as read
                listItem.addEventListener('click', function() {
                    if (!notification.isRead) {
                        fetch('/Review/MarkAsRead', {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify({ id: notification.id })
                        })
                        .then(response => response.json())
                        .then(data => {
                            if (data.success) {
                                listItem.classList.remove('unread');
                                checkNotifications(); // Update badge count
                            }
                        })
                        .catch(error => console.error('Error marking notification as read:', error));
                    }
                });
                
                // Insert after the header divider
                headerDivider.after(listItem);
            });
        } else {
            // Show empty notification message
            emptyNotification.style.display = 'block';
        }
    }
    
    // Check for new notifications every minute
    setInterval(checkNotifications, 60000);
    
    // Toast function for notifications
    window.showToast = function(type, message) {
        const toastContainer = document.createElement('div');
        toastContainer.className = 'position-fixed top-0 end-0 p-3';
        toastContainer.style.zIndex = 1080;
        
        const bgClass = type === 'success' ? 'bg-success' : 'bg-danger';
        
        toastContainer.innerHTML = `
            <div class="toast align-items-center text-white ${bgClass} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;
        
        document.body.appendChild(toastContainer);
        
        const toastElement = toastContainer.querySelector('.toast');
        const toast = new bootstrap.Toast(toastElement, { delay: 5000 });
        toast.show();
        
        toastElement.addEventListener('hidden.bs.toast', function() {
            document.body.removeChild(toastContainer);
        });
    };
});
