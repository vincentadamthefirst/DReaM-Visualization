using UnityEngine;

namespace UI.Main_Menu.Notifications {
    public class NotificationManager : MonoBehaviour {
        public Notification notificationPrefab;

        public void ShowNotification(NotificationType type, string message) {
            var notification = Instantiate(notificationPrefab, transform);
            notification.Show(type, message);
        }
    }
}
