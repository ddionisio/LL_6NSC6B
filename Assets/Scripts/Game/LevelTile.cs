using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LevelTile : MonoBehaviour {
    [System.Flags]
    public enum Flag {
        None = 0x0,
        WallN = 0x1,
        WallE = 0x2,
        WallS = 0x4,
        WallW = 0x8,
        Pit = 0x10,
        Goal = 0x20
    }

    public GameObject[] flagDisplays;

#if UNITY_EDITOR
    void Update() {
        if(!Application.isPlaying) {
            //snap to cell if parent is level grid
            var levelGrid = transform.parent ? transform.parent.GetComponent<LevelGrid>() : null;
            if(levelGrid) {
                int r, c;
                if(levelGrid.GetCellIndexLocal(transform.localPosition, out c, out r))
                    transform.position = levelGrid.GetCellPosition(c, r);
            }
        }
    }
#endif
}
