using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityItemMoveDirWidget : LevelEntityItemWidget {
    [Header("Move Dir Data")]
    public LevelEntityMoveDir.Type dir;

    [Header("Move Dir Display")]
    public Transform dirDisplayRoot; //default to up

    private M8.GenericParams mSpawnParms;

    protected override bool IsPlaceable(CellIndex cellIndex) {
        return LevelEntityMoveDir.CheckPlaceable(levelGrid, cellIndex);
    }

    protected override M8.GenericParams GetSpawnParms() {
        return mSpawnParms;
    }

    void Awake() {
        mSpawnParms = new M8.GenericParams();
        mSpawnParms[LevelEntityMoveDir.parmType] = dir;

        Vector3 rot;

        switch(dir) {
            case LevelEntityMoveDir.Type.Down:
                rot = new Vector3(0f, 0f, 180f);
                break;
            case LevelEntityMoveDir.Type.Left:
                rot = new Vector3(0f, 0f, 90f);
                break;
            case LevelEntityMoveDir.Type.Right:
                rot = new Vector3(0f, 0f, -90f);
                break;
            default:
                rot = Vector3.zero;
                break;
        }

        dirDisplayRoot.localEulerAngles = rot;
    }
}
