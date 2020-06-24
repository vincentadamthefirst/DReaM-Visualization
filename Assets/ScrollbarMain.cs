using UnityEngine;
using UnityEngine.UI;

public class ScrollbarMain : MonoBehaviour {

    public Transform content;
    public GameObject entryPrefab;

    public RectTransform self;
    
    private void Update() {
        if (Input.GetKeyUp(KeyCode.O)) {
            Instantiate(entryPrefab, content, false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(self);
        }
    }
}