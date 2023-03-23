using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Main_Menu.Notifications {
    
    [CreateAssetMenu(menuName = "NotificationData")]
    public class NotificationData : ScriptableObject {

        public List<NotificationDesign> designs = new();

        public NotificationDesign GetDesign(NotificationType type) {
            return designs.Find(x => x.type == type);
        }
    }

    [Serializable]
    public class NotificationDesign {
        public NotificationType type;
        public Color outline;
        public Color main;
        public Color text;
        public Color symbol;
    }
}