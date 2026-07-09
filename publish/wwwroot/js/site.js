// SignalR connection for real-time notifications
let notificationConnection = null;

function initializeSignalR(userId) {
    if (!userId) return;

    notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl('/notificationHub')
        .withAutomaticReconnect()
        .build();

    notificationConnection.on('ReceiveNotification', function (notification) {
        showToast(notification.title, notification.message);
        loadNotificationCount();
        loadNotifications();
    });

    notificationConnection.start()
        .then(function () {
            notificationConnection.invoke('JoinUserGroup', userId);
        })
        .catch(function (err) {
            console.error('SignalR Error: ', err);
        });
}

function loadNotificationCount() {
    $.get('/Notification/GetUnreadCount', function (data) {
        const count = data.count;
        const badge = $('#notificationCount');
        if (count > 0) {
            badge.text(count > 99 ? '99+' : count).show();
        } else {
            badge.hide();
        }
    });
}

function loadNotifications() {
    $.get('/Notification/GetRecentNotifications', function (data) {
        const list = $('#notificationList');
        list.empty();

        if (data.length === 0) {
            list.html('<div class="text-center p-3 text-muted"><small>لا توجد إشعارات</small></div>');
            return;
        }

        data.forEach(function (notification) {
            const item = `
                <a href="${notification.linkUrl || '#'}" class="dropdown-item py-2 ${notification.isRead ? '' : 'bg-light fw-bold'}" data-id="${notification.id}">
                    <div class="d-flex justify-content-between">
                        <strong class="small">${notification.title}</strong>
                        <small class="text-muted" style="font-size: 0.7rem;">${notification.timeAgo}</small>
                    </div>
                    <div class="small text-muted text-truncate">${notification.message}</div>
                </a>
            `;
            list.append(item);
        });
    });
}

function showToast(title, message) {
    const toastHtml = `
        <div class="toast align-items-center text-white bg-primary border-0 fade show" role="alert" aria-live="assertive" aria-atomic="true" style="position: fixed; top: 20px; left: 20px; z-index: 9999;">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${title}</strong><br>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    const $toast = $(toastHtml).appendTo('body');
    setTimeout(function () {
        $toast.toast('hide').on('hidden.bs.toast', function () {
            $toast.remove();
        });
    }, 5000);
}

$(document).ready(function () {
    // Initialize notifications if user is logged in
    const userId = $('#currentUserId').val();
    if (userId) {
        loadNotificationCount();
        loadNotifications();
        initializeSignalR(userId);
    }

    // Mark all notifications as read from dropdown
    $('#markAllRead').on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $.post('/Notification/MarkAllAsRead', function () {
            loadNotificationCount();
            loadNotifications();
        });
    });

    // Load notifications when dropdown opens
    $('#notificationDropdown').on('show.bs.dropdown', function () {
        loadNotifications();
    });
});
