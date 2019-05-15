using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityItemMoveDirWidget : LevelEntityItemWidget {
    protected override bool IsPlaceable(CellIndex cellIndex) {
        return true;
    }

    protected override M8.GenericParams GetSpawnParms() {
        return null;
    }
}
