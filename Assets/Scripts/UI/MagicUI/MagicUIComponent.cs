using System;
using System.Collections;
using UnityEngine;

namespace UI.MagicUI {
    
    [ExecuteInEditMode]
    public class MagicUiComponent : MonoBehaviour {

        public MagicUiData magicUiData;

        protected virtual void OnSkinUI() {
            
        }

        public virtual void Awake() {
            OnSkinUI();
        }

        public virtual void Update() {
            if (Application.isEditor) OnSkinUI();
        }

        public void EnsureCoroutineStopped(ref Coroutine coroutine) {
            if (coroutine == null) return;
            StopCoroutine(coroutine);
            coroutine = null;
        }

        public Coroutine CreateAnimationRoutine(float duration, Action<float> changeFunction, Action onComplete) {
            return StartCoroutine(GenericAnimationRoutine(duration, changeFunction, onComplete));
        }

        private static IEnumerator GenericAnimationRoutine(float duration, Action<float> changeFunction,
            Action onComplete) {

            var elapsedTime = 0f;
            var progress = 0f;
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
