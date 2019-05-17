using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

public class LevelEntityItemAbsoluteWidget : LevelEntityItemWidget {
    [Header("Absolute Data")]
    public AxisType axis;

    protected override void ApplySpawnParms(GenericParams parms) {
        parms[LevelEntityAbsolute.parmAxisType] = axis;
    }
}
