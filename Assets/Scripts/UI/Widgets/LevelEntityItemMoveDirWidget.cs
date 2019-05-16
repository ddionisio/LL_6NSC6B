using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityItemMoveDirWidget : LevelEntityItemWidget {
    [Header("Move Dir Data")]
    public LevelEntityMoveDir.Type dir;

    public override float iconRotation {
        get {
            switch(dir) {
                case LevelEntityMoveDir.Type.Down:
                    return 180f;
                case LevelEntityMoveDir.Type.Left:
                    return 90f;
                case LevelEntityMoveDir.Type.Right:
                    return -90f;
                default:
                    return 0f;
            }
        }
    }

    protected override void ApplySpawnParms(M8.GenericParams parms) {
        parms[LevelEntityMoveDir.parmType] = dir;
    }
}
