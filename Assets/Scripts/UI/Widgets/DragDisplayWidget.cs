using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragDisplayWidget : MonoBehaviour {
    [Header("Display")]
    public Image iconImage;

    public M8.UI.Graphics.ColorGroup colorGroup;
    public Color colorInvalid = Color.red;

    public Transform cellHighlightRoot { get; set; }

    public void SetActive(bool aActive) {
        gameObject.SetActive(aActive);

        if(!aActive) {
            if(cellHighlightRoot) cellHighlightRoot.gameObject.SetActive(false);
        }
    }

    public void Setup(Sprite iconSprite, float iconRotate) {
        if(iconImage) {
            iconImage.sprite = iconSprite;
            iconImage.SetNativeSize();
            iconImage.transform.localEulerAngles = new Vector3(0f, 0f, iconRotate);
        }
    }

    public void SetValid(bool isValid) {
        if(colorGroup) {
            if(isValid)
                colorGroup.Revert();
            else
                colorGroup.ApplyColor(colorInvalid);
        }

        if(cellHighlightRoot)
            cellHighlightRoot.gameObject.SetActive(isValid);
    }

    public void UpdateCellHighlightPos(Vector2 pos) {
        if(cellHighlightRoot)
            cellHighlightRoot.position = pos;
    }
}
