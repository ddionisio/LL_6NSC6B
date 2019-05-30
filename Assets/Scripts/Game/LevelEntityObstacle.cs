using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityObstacle : LevelEntity {
    public enum Mode {
        Hole
    }

    public Mode mode = Mode.Hole;
}
