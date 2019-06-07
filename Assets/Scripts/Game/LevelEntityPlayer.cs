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

    [Header("Display Player")]
    public Transform editDirDisplayRoot;

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

    protected override void OnModeChanged(PlayController.Mode mode) {
        base.OnModeChanged(mode);

        switch(mode) {
            case PlayController.Mode.Editing:
                if(editDirDisplayRoot) {
                    switch(dir) {
                        case MoveDir.Up:
                            editDirDisplayRoot.localRotation = Quaternion.identity;
                            break;
                        case MoveDir.Down:
                            editDirDisplayRoot.up = Vector3.down;
                            break;
                        case MoveDir.Left:
                            editDirDisplayRoot.up = Vector3.left;
                            break;
                        case MoveDir.Right:
                            editDirDisplayRoot.up = Vector3.right;
                            break;
                    }

                    editDirDisplayRoot.gameObject.SetActive(true);
                }
                break;

            default:
                if(editDirDisplayRoot) editDirDisplayRoot.gameObject.SetActive(false);
                break;
        }
    }

    protected override void Awake() {
        base.Awake();

        if(editDirDisplayRoot) editDirDisplayRoot.gameObject.SetActive(false);
    }

    IEnumerator DoChangeToEditMode() {
        yield return new WaitForSeconds(deathToEditDelay);

        mRout = null;

        PlayController.instance.curMode = PlayController.Mode.Editing;
    }
}
