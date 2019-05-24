using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelEntityItemAbsoluteWidget : LevelEntityItemWidget {
    [Header("Absolute Data")]
    public AxisType axis;

    protected override void ApplySpawnParms(GenericParams parms) {
        parms[LevelEntityAbsolute.parmAxisType] = axis;
    }

    protected override void DragUpdated(PointerEventData eventData) {
        LevelEntityAbsolute.ApplyCellDestination(eventData, axis);
    }

    protected override void DragInvalidated() {
        var levelGrid = PlayController.instance.levelGrid;

        if(levelGrid) {
            if(levelGrid.cellDestGO)
                levelGrid.cellDestGO.SetActive(false);
        }
    }
}
