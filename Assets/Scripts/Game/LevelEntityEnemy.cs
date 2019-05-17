using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityEnemy : LevelEntityMover {

    private LevelEntityMover mEntDeadMover; //current dead mover on cell

    protected override void EvaluateBegin() {
        mEntDeadMover = null;
    }

    protected override State EvaluateEntity(LevelEntity ent) {
        if(ent is LevelEntityMover) {
            var entMover = (LevelEntityMover)ent;            
            if(entMover.state == State.Dead)
                mEntDeadMover = entMover;
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
