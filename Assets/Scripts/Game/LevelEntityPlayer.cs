using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityPlayer : LevelEntityMover {
    [Header("Data Player")]
    [M8.TagSelector]
    public string tagEnemy = "Enemy";
    public float deathToEditDelay = 1.5f;
    public bool isDeathToEdit = true;

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

    protected override State EvaluateObstacle(LevelEntityObstacle obstacle) {
        //check hole
        if(obstacle.mode == LevelEntityObstacle.Mode.Hole) {
            //die if it's not filled
            if(!mEntDeadMover)
                return State.Dead;
        }

        return base.EvaluateObstacle(obstacle);
    }

    protected override void OnMoveCurrentTile() {
        //do brightness on current tile
        var tile = levelGrid.GetTile(cellIndex);
        if(tile)
            tile.BrightnessFade(tileBrightOfs, tileBrightDelay);
    }

    protected override void OnDeadPost() {
        if(isDeathToEdit)
            mRout = StartCoroutine(DoChangeToEditMode());
    }

    IEnumerator DoChangeToEditMode() {
        yield return new WaitForSeconds(deathToEditDelay);

        mRout = null;

        PlayController.instance.curMode = PlayController.Mode.Editing;
    }
}
