// نظام إشعارات المحادثات للموقع

document.addEventListener('DOMContentLoaded', function() {
    // تحقق من وجود عنصر إشعارات المحادثات
    if (!document.getElementById('chatNotificationsDropdown')) return;
    
    // إنشاء اتصال SignalR
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/chathub')
        .withAutomaticReconnect()
        .build();
    
    // بدء الاتصال
    connection.start()
        .then(() => {
            console.log("Chat Notifications Connected");
            loadChatNotifications();
        })
        .catch(err => console.error("Error connecting to chat hub:", err));
    
    // تحميل الإشعارات
    async function loadChatNotifications() {
        try {
            let conversations;
            
            // التحقق مما إذا كان المستخدم مسؤولاً أم مستخدمًا عاديًا
            if (document.body.classList.contains('role-admin')) {
                conversations = await connection.invoke("GetAdminConversations");
            } else {
                conversations = await connection.invoke("GetUserConversations");
            }
            
            // عرض الإشعارات
            renderNotifications(conversations);
            
            // تحديث عدد الإشعارات غير المقروءة
            updateUnreadCount(conversations);
        } catch (err) {
            console.error("Error loading chat notifications:", err);
            document.getElementById('chatNotificationsList').innerHTML = `
                <div class="dropdown-item py-3 text-center text-danger">
                    <i class="fas fa-exclamation-circle me-2"></i>حدث خطأ أثناء تحميل الإشعارات
                </div>
            `;
        }
    }
    
    // عرض الإشعارات
    function renderNotifications(conversations) {
        const container = document.getElementById('chatNotificationsList');
        
        // فقط العناصر التي تحتوي على رسائل غير مقروءة
        const unreadConversations = conversations.filter(c => c.unreadCount > 0);
        
        if (unreadConversations.length === 0) {
            container.innerHTML = `
                <div class="dropdown-item py-3 text-center text-muted">
                    <i class="fas fa-check-circle me-2"></i>لا توجد رسائل جديدة
                </div>
            `;
            return;
        }
        
        // ترتيب المحادثات حسب آخر رسالة (الأحدث أولاً)
        unreadConversations.sort((a, b) => new Date(b.lastMessageAt) - new Date(a.lastMessageAt));
        
        // إنشاء قائمة الإشعارات
        container.innerHTML = '';
        
        for (let i = 0; i < Math.min(unreadConversations.length, 5); i++) {
            const conv = unreadConversations[i];
            const isAdmin = document.body.classList.contains('role-admin');
            
            // الاسم المعروض يختلف باختلاف نوع المستخدم
            const displayName = isAdmin ? conv.userName : conv.adminName;
            
            // تحويل التاريخ إلى نص مناسب
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
                    ${escapeHtml(conv.lastMessage || 'محادثة جديدة')}
                </div>
                <div class="text-end">
                    <small class="text-muted">${timeAgo}</small>
                </div>
            `;
            
            container.appendChild(item);
        }
        
        // إضافة رابط "عرض الكل" إذا كان هناك أكثر من 5 محادثات
        if (unreadConversations.length > 5) {
            const viewAllLink = document.createElement('div');
            viewAllLink.className = 'dropdown-item text-center py-2 text-primary';
            
            viewAllLink.innerHTML = `
                <i class="fas fa-arrow-circle-right me-1"></i>
                عرض كل الرسائل (${unreadConversations.length})
            `;
            
            viewAllLink.addEventListener('click', () => {
                window.location.href = isAdmin ? '/AdminChat/Index' : '/Support/UserChat';
            });
            
            container.appendChild(viewAllLink);
        }
    }
    
    // تحديث عدد الإشعارات غير المقروءة
    function updateUnreadCount(conversations) {
        const badge = document.querySelector('.chat-badge');
        
        // إجمالي عدد الرسائل غير المقروءة
        const totalUnread = conversations.reduce((total, conv) => total + conv.unreadCount, 0);
        
        if (totalUnread > 0) {
            badge.textContent = totalUnread > 99 ? '99+' : totalUnread;
            badge.classList.remove('d-none');
        } else {
            badge.classList.add('d-none');
        }
    }
    
    // استقبال الإشعارات الجديدة
    connection.on("ReceiveAdminMessage", function(data) {
        // تحديث الإشعارات فقط إذا كان المستخدم مسؤولاً
        if (document.body.classList.contains('role-admin')) {
            loadChatNotifications();
        }
    });
    
    connection.on("ReceiveReply", function(data) {
        // تحديث الإشعارات للمستخدم العادي
        if (!document.body.classList.contains('role-admin')) {
            loadChatNotifications();
        }
    });
    
    // تحديث الإشعارات عند النقر على زر التحديث
    document.querySelector('.btn-refresh-notifications').addEventListener('click', function(e) {
        e.preventDefault();
        
        // تغيير أيقونة التحديث إلى أيقونة دوارة
        this.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
        
        // تحميل الإشعارات
        loadChatNotifications()
            .finally(() => {
                // إعادة أيقونة التحديث
                this.innerHTML = '<i class="fas fa-sync-alt"></i>';
            });
    });
    
    // تحديث الإشعارات كل دقيقة
    setInterval(loadChatNotifications, 60000);
    
    // دوال مساعدة
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
            return `منذ ${diffDay} ${diffDay === 1 ? 'يوم' : 'أيام'}`;
        } else if (diffHour > 0) {
            return `منذ ${diffHour} ${diffHour === 1 ? 'ساعة' : 'ساعات'}`;
        } else if (diffMin > 0) {
            return `منذ ${diffMin} ${diffMin === 1 ? 'دقيقة' : 'دقائق'}`;
        } else {
            return 'الآن';
        }
    }
});
