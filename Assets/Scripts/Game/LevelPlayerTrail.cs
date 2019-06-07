using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPlayerTrail : MonoBehaviour {
    [System.Flags]
    public enum TrailFlag {
        None = 0x0,
        Up = 0x1,
        Down = 0x2,
        Left = 0x4,
        Right = 0x8,
        End = 0x10
    }

    [Header("Data")]
    public LevelPlayerTrailCell template;

    private TrailFlag[,] mTrailGrid; //[row, col]

    private M8.CacheList<LevelPlayerTrailCell> mCellActives;
    private M8.CacheList<LevelPlayerTrailCell> mCellCache;

    private CellIndex mPrevCell;
    private int mMoveCount;

    void OnDestroy() {
        if(PlayController.isInstantiated) {
            PlayController.instance.modeChangedCallback -= OnModeChanged;

            if(PlayController.instance.player)
                PlayController.instance.player.moveUpdateCallback -= OnPlayerMoveUpdate;
        }
    }

    void Awake() {
        var playCtrl = PlayController.instance;
        var lvlGrid = playCtrl.levelGrid;

        //setup collections
        mTrailGrid = new TrailFlag[lvlGrid.numRow, lvlGrid.numCol];

        var capacity = lvlGrid.numRow * lvlGrid.numCol;
        mCellActives = new M8.CacheList<LevelPlayerTrailCell>(capacity);
        mCellCache = new M8.CacheList<LevelPlayerTrailCell>(capacity);

        playCtrl.modeChangedCallback += OnModeChanged;
        playCtrl.player.moveUpdateCallback += OnPlayerMoveUpdate;
    }

    void OnModeChanged(PlayController.Mode mode) {
        switch(mode) {
            case PlayController.Mode.Editing:
                if(mMoveCount != 0) {
                    OnPlayerMoveUpdate();
                    if(mPrevCell.row >= 0 && mPrevCell.row < mTrailGrid.GetLength(0) && mPrevCell.col >= 0 && mPrevCell.col < mTrailGrid.GetLength(1))
                        mTrailGrid[mPrevCell.row, mPrevCell.col] |= TrailFlag.End;
                }

                //generate trails
                GenerateDisplay();
                break;

            default:
                ClearGrid();
                ClearCells();

                mPrevCell = PlayController.instance.player.cellIndex;
                mMoveCount = 0;
                break;
        }
    }

    void OnPlayerMoveUpdate() {
        var player = PlayController.instance.player;
        var curCell = player.cellIndex;

        //tag end
        if(curCell == mPrevCell) {
            mTrailGrid[curCell.row, curCell.col] |= TrailFlag.End;
        }
        else if(curCell.row >= 0 && curCell.row < mTrailGrid.GetLength(0) && curCell.col >= 0 && curCell.col < mTrailGrid.GetLength(1)) {
            //set flag on current and previous

            //up
            if(mPrevCell.row + 1 == curCell.row && mPrevCell.col == curCell.col) {
                mTrailGrid[mPrevCell.row, mPrevCell.col] |= TrailFlag.Up;
                mTrailGrid[curCell.row, curCell.col] |= TrailFlag.Down;
            }
            //down
            else if(mPrevCell.row - 1 == curCell.row && mPrevCell.col == curCell.col) {
                mTrailGrid[mPrevCell.row, mPrevCell.col] |= TrailFlag.Down;
                mTrailGrid[curCell.row, curCell.col] |= TrailFlag.Up;
            }
            //right
            else if(mPrevCell.col + 1 == curCell.col && mPrevCell.row == curCell.row) {
                mTrailGrid[mPrevCell.row, mPrevCell.col] |= TrailFlag.Right;
                mTrailGrid[curCell.row, curCell.col] |= TrailFlag.Left;
            }
            //left
            else if(mPrevCell.col - 1 == curCell.col && mPrevCell.row == curCell.row) {
                mTrailGrid[mPrevCell.row, mPrevCell.col] |= TrailFlag.Left;
                mTrailGrid[curCell.row, curCell.col] |= TrailFlag.Right;
            }
            else
                mTrailGrid[mPrevCell.row, mPrevCell.col] |= TrailFlag.End;

            mPrevCell = curCell;
            mMoveCount++;
        }
    }

    private void GenerateDisplay() {
        for(int r = 0; r < mTrailGrid.GetLength(0); r++) {
            for(int c = 0; c < mTrailGrid.GetLength(1); c++) {
                var flag = mTrailGrid[r, c];
                if(flag != TrailFlag.None) {
                    var cellIndex = new CellIndex(r, c);

                    var cellDisplay = AllocateCellActive(cellIndex);
                    cellDisplay.ApplyDisplay(flag);
                }
            }
        }
    }

    private LevelPlayerTrailCell AllocateCellActive(CellIndex cellIndex) {
        LevelPlayerTrailCell ret = null;

        if(mCellCache.Count == 0) {
            ret = Instantiate(template, transform);
        }
        else {
            ret = mCellCache.RemoveLast();
            ret.gameObject.SetActive(true);
        }

        if(ret) {
            ret.transform.position = PlayController.instance.levelGrid.GetCellPosition(cellIndex);

            mCellActives.Add(ret);
        }

        return ret;
    }

    private void ClearGrid() {
        for(int r = 0; r < mTrailGrid.GetLength(0); r++) {
            for(int c = 0; c < mTrailGrid.GetLength(1); c++)
                mTrailGrid[r, c] = TrailFlag.None;
        }
    }

    private void ClearCells() {
        for(int i = 0; i < mCellActives.Count; i++) {
            if(mCellActives[i]) {
                mCellActives[i].gameObject.SetActive(false);
                mCellCache.Add(mCellActives[i]);
            }
        }

        mCellActives.Clear();
    }
}
