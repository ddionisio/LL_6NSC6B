using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityPlayer : LevelEntityMover {
    [Header("Data Player")]
    [M8.TagSelector]
    public string tagEnemy = "Enemy";

    [Header("Tile Brightness Info")]
    public float tileBrightOfs = 0.2f;
    public float tileBrightDelay = 0.3f;

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
                    //entMover.state = State.Victory;

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

    protected override void OnMoveCurrentTile() {
        //do brightness on current tile
        var tile = levelGrid.GetTile(cellIndex);
        if(tile)
            tile.BrightnessFade(tileBrightOfs, tileBrightDelay);
    }
}
