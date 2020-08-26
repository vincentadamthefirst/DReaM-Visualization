using System;
using System.Collections;
using UI.Main_Menu_Rework.Utils;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UI.Main_Menu_Rework.Elements {
    public abstract class CustomUiElement : MonoBehaviour {

        [Tooltip("Design used for this element")]
        public ApplicationDesign applicationDesign;

        private void Start() {
            UpdateUiElement();
            try {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
            } catch (NullReferenceException e) { }
        }

        private void OnValidate() {
            UpdateUiElement();
            try {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
            } catch (NullReferenceException e) { }
        }

        public abstract void UpdateUiElement();

        public void EnsureCoroutineStopped(ref Coroutine coroutine) {
            if (coroutine == null) return;
            StopCoroutine(coroutine);
            coroutine = null;
        }

        public Coroutine CreateAnimationRoutine(float duration, float initialProgress, Action<float> changeFunction,
            Action onComplete) {
            return StartCoroutine(GenericAnimationRoutine(duration, initialProgress, changeFunction, onComplete));
        }

        private static IEnumerator GenericAnimationRoutine(float duration, float initialProgress,
            Action<float> changeFunction, Action onComplete) {

            var elapsedTime = 0f;
            var progress = initialProgress;
            while (progress <= 1f) {
                changeFunction(progress);
                elapsedTime += Time.deltaTime;
                progress = elapsedTime / duration;
                yield return null;
            }

            changeFunction(1);
            onComplete?.Invoke();
        }
    }
}