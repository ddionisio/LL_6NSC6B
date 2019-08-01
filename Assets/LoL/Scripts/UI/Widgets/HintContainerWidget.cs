using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintContainerWidget : MonoBehaviour {
    [Header("UI")]
    public HintItemWidget itemTemplate;
    public Transform containerRoot;

    public HintItemWidget[] items { get; private set; }

    public void Setup(LevelEntityHint[] hints) {
        if(items != null) {
            Debug.LogWarning("Hints have already been applied.");
            return;
        }

        items = new HintItemWidget[hints.Length];
        for(int i = 0; i < hints.Length; i++) {
            var itm = Instantiate(itemTemplate);

            itm.transform.SetParent(containerRoot, false);
            itm.Setup(hints[i]);
            itm.gameObject.SetActive(true);

            items[i] = itm;
        }
    }

    public void Refresh() {
        if(items != null) {
            for(int i = 0; i < items.Length; i++)
                items[i].Refresh();
        }
    }

    void Awake() {
        itemTemplate.gameObject.SetActive(false);
    }
}
