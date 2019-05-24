using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelEntityItemReflectWidget : LevelEntityItemWidget {
    [Header("Reflect Data")]
    public bool reflectX;
    public bool reflectY;

    protected override void ApplySpawnParms(M8.GenericParams parms) {
        parms[LevelEntityReflect.parmReflectX] = reflectX;
        parms[LevelEntityReflect.parmReflectY] = reflectY;
    }

    protected override void DragUpdated(PointerEventData eventData) {
        LevelEntityReflect.ApplyCellDestination(eventData, reflectX, reflectY);
    }

    protected override void DragInvalidated() {
        var levelGrid = PlayController.instance.levelGrid;

        if(levelGrid) {
            if(levelGrid.cellDestGO)
                levelGrid.cellDestGO.SetActive(false);
        }
    }
}
