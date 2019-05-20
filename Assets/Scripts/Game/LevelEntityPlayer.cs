using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityPlayer : LevelEntityMover {
    [Header("Data Player")]
    [M8.TagSelector]
    public string tagEnemy = "Enemy";

    private LevelEntityMover mEntDeadMover; //current dead mover on cell

    protected override void EvaluateBegin() {
        mEntDeadMover = null;
    }

    protected override State EvaluateEntity(LevelEntity ent) {
        if(ent is LevelEntityMover) {
            var entMover = (LevelEntityMover)ent;
            if(entMover.state == State.Dead)
                mEntDeadMover = entMover;
            else {
                //check if it's an enemy, die
                if(entMover.CompareTag(tagEnemy)) {
                    //let enemy celebrate
                    entMover.state = State.Victory;

                    return State.Dead;
                }
            }
        }
        else if(ent is LevelEntityGoal) {
            var entGoal = (LevelEntityGoal)ent;
            entGoal.state = LevelEntityGoal.State.Collected;
        }

        return base.EvaluateEntity(ent);
    }

    protected override State EvaluateTile(LevelTile tile) {
        //check hole
        if(tile.isPit) {
            //die if it's not filled
            if(!mEntDeadMover)
                return State.Dead;
        }

        return base.EvaluateTile(tile);
    }
}
