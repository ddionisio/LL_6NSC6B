using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPlayerTrailCell : MonoBehaviour {
    public GameObject upGO;
    public GameObject downGO;
    public GameObject leftGO;
    public GameObject rightGO;
    public GameObject endGO;

    public void ApplyDisplay(LevelPlayerTrail.TrailFlag flags) {
        if(upGO) upGO.SetActive((flags & LevelPlayerTrail.TrailFlag.Up) != LevelPlayerTrail.TrailFlag.None);
        if(downGO) downGO.SetActive((flags & LevelPlayerTrail.TrailFlag.Down) != LevelPlayerTrail.TrailFlag.None);
        if(leftGO) leftGO.SetActive((flags & LevelPlayerTrail.TrailFlag.Left) != LevelPlayerTrail.TrailFlag.None);
        if(rightGO) rightGO.SetActive((flags & LevelPlayerTrail.TrailFlag.Right) != LevelPlayerTrail.TrailFlag.None);
        if(endGO) endGO.SetActive((flags & LevelPlayerTrail.TrailFlag.End) != LevelPlayerTrail.TrailFlag.None);
    }
}
