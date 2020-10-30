using System;
using System.Collections;
using UI.Main_Menu_Rework.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu_Rework.Elements {
    public abstract class CustomUiElement : MonoBehaviour {
        // the central UI controller
        protected CentralUiController centralUiController;

        private void Start() {
            PerformUpdate();
        }

        private void OnValidate() {
            PerformUpdate();
        }

        public void PerformUpdate() {
            centralUiController = FindObjectOfType<CentralUiController>();
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