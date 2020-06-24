using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class ScrollbarExpanderTest : MonoBehaviour {
    public GameObject childPrefab;
    public Transform childrenContainer;

    private RectTransform _bigParent;

    public void Start() {
        _bigParent = (RectTransform) transform.parent;
    }

    public void ClickHandler() {
        var newChild = Instantiate(childPrefab, childrenContainer, false);
        newChild.transform.SetAsLastSibling();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(_bigParent);

        // var parent = transform.parent;
        // var parentParent = parent.parent;
        //
        // try {
        //     var next = parentParent.GetChild(parent.GetSiblingIndex() + 1);
        //     next.GetComponent<ScrollbarExpanderTest>().UpdateHandle();
        // } catch (UnityException) {
        //     // ignore
        // }

    }

    public void UpdateHandle() {
        var tmp = new GameObject();
        tmp.transform.parent = childrenContainer;
        Destroy(tmp);
    }
}
