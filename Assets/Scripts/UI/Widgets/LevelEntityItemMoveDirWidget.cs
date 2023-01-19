using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityItemMoveDirWidget : LevelEntityItemWidget {
    [Header("Move Dir Data")]
    public MoveDir dir;

    public override float iconRotation {
        get {
            switch(dir) {
                case MoveDir.Down:
                    return 180f;
                case MoveDir.Left:
                    return 90f;
                case MoveDir.Right:
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
