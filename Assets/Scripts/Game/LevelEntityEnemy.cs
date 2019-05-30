using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityEnemy : LevelEntityMover {
    [Header("Data Enemy")]
    [M8.TagSelector]
    public string tagEntityKill = "Player";

    private LevelEntityMover mEntDeadMover; //current dead mover on cell

    protected override void EvaluateBegin() {
        mEntDeadMover = null;
    }

    protected override State EvaluateEntity(LevelEntity ent) {
        if(ent is LevelEntityMover) {
            var entMover = (LevelEntityMover)ent;
            if(ent.CompareTag(tagEntityKill)) {
                if(entMover.state == State.Moving)
                    entMover.state = State.Dead;
            }
            else if(entMover.state == State.Dead)
                mEntDeadMover = entMover;
        }

        return base.EvaluateEntity(ent);
    }

    protected override State EvaluateObstacle(LevelEntityObstacle obstacle) {
        //check hole
        if(obstacle.mode == LevelEntityObstacle.Mode.Hole) {
            //die if it's not filled
            if(!mEntDeadMover)
                return State.Dead;
        }

        return base.EvaluateObstacle(obstacle);
    }
}
