using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintItemWidget : MonoBehaviour {
    public Image icon;
    public Text coordText;
    public GameObject activeGO;

    [M8.SoundPlaylist]
    public string sfxCorrect;

    public LevelEntityHint hint { get; private set; }

    public void Setup(LevelEntityHint aHint) {
        hint = aHint;

        icon.sprite = aHint.GetIconSprite();
        //icon.SetNativeSize();

        var iconRot = icon.transform.localEulerAngles;
        iconRot.z = hint.GetIconRotation();
        icon.transform.localEulerAngles = iconRot;

        hint.RefreshCellIndex();

        var levelGrid = PlayController.instance.levelGrid;

        var cellPos = hint.cellIndex;
        cellPos.col -= levelGrid.originCol;
        cellPos.row -= levelGrid.originRow;

        coordText.text = string.Format("({0}, {1})", cellPos.col, cellPos.row);

        activeGO.SetActive(false);
    }

    public void Refresh() {
        if(!hint) return;

        icon.SetNativeSize();

        bool isActive = false;

        var ents = PlayController.instance.levelGrid.GetEntities(hint.cellIndex);
        if(ents != null) {
            for(int i = 0; i < ents.Count; i++) {
                if(ents[i].name == hint.itemNameFromType) {
                    isActive = true;
                    break;
                }
            }
        }

        if(activeGO.activeSelf != isActive) {
            activeGO.SetActive(isActive);

            if(isActive) {
                if(!string.IsNullOrEmpty(sfxCorrect))
                    M8.SoundPlaylist.instance.Play(sfxCorrect, false);
            }
        }
    }
}
