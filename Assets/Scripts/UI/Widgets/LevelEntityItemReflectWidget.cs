using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityItemReflectWidget : LevelEntityItemWidget {
    [Header("Reflect Data")]
    public bool reflectX;
    public bool reflectY;

    protected override void ApplySpawnParms(M8.GenericParams parms) {
        parms[LevelEntityReflect.parmReflectX] = reflectX;
        parms[LevelEntityReflect.parmReflectY] = reflectY;
    }
}
