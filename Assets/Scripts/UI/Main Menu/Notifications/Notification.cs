using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu.Notifications {
    public class Notification : MonoBehaviour {
        public Image background;
        public Image outline;
        public Image symbol;
        public TextMeshProUGUI text;
        public NotificationData notificationData;
        
        public void Show(NotificationType type, string message, float aliveTime = 5f, float fadeInTime = .5f, float fadeOutTime = .3f) {
            name = type + " Notification";
            
            background.color = notificationData.GetDesign(type).main;
            outline.color = notificationData.GetDesign(type).outline;
            text.color = notificationData.GetDesign(type).text;
            symbol.color = notificationData.GetDesign(type).symbol;
            
            text.text = message;
            StartCoroutine(FadeIn(fadeInTime, ChangeOpacity));
            StartCoroutine(SelfDestruct(aliveTime, fadeOutTime));
        }

        private void ChangeOpacity(float opacity) {
            var tmpColBackground = background.color;
            tmpColBackground.a = opacity / 3f;
            background.color = tmpColBackground;

            var tmpColOutline = outline.color;
            tmpColOutline.a = opacity;
            outline.color = tmpColOutline;

            var tmpColSymbol = symbol.color;
            tmpColSymbol.a = opacity;
            symbol.color = tmpColSymbol;

            var tmpColText = text.color;
            tmpColText.a = opacity;
            text.color = tmpColText;
        }

        private IEnumerator FadeIn(float duration, Action<float> changeFunction) {
            var elapsedTime = 0f;
            var progress = 0f;
            while (progress <= 1f) {
                changeFunction(progress);
                elapsedTime += Time.deltaTime;
                progress = elapsedTime / duration;
                yield return null;
            }

            changeFunction(1);
        }
        
        private IEnumerator FadeOut(float duration, Action<float> changeFunction) {
            var elapsedTime = 0f;
            var progress = 0f;
            while (progress <= 1f) {
                changeFunction(1 - progress);
                elapsedTime += Time.deltaTime;
                progress = elapsedTime / duration;
                yield return null;
            }

            changeFunction(0);
        }

        private IEnumerator SelfDestruct(float aliveTime, float fadeOutTime) {
            yield return new WaitForSeconds(aliveTime - fadeOutTime);
            StartCoroutine(FadeOut(fadeOutTime, ChangeOpacity));
            yield return new WaitForSeconds(fadeOutTime);
            Destroy(gameObject);
        }
    }
}
