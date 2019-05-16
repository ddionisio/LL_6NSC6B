using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityMover : LevelEntity {
    public enum State {
        None,
        Idle,
        Moving,
        Jumping,
        Victory,
        Dead,

        Warp
    }

    [Header("Data")]
    public MoveDir startDir = MoveDir.Down;
    public float moveSpeed = 5f;
    public float moveChangeDirDelay = 0.15f;
    public float jumpHeight = 2f;

    [Header("Display")]
    public Transform displayRoot;
    public SpriteRenderer displaySpriteRender; //note: default facing right

    [Header("Animation")]
    public M8.Animator.Animate animator;

    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeIdleUp;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeIdleDown;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeIdleSide;

    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeMoveUp;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeMoveDown;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeMoveSide;

    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeJumpUp;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeJumpDown;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeJumpSide;

    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeVictory;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeDead;

    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeWarpOut;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeWarpIn;

    public State state {
        get { return mCurState; }
        set {
            if(mCurState != value) {
                var prevState = mCurState;
                mCurState = value;
                ApplyCurState(prevState);
            }
        }
    }

    public MoveDir dir {
        get { return mCurDir; }
        set {
            if(mCurDir != value) {
                mCurDir = value;
                ApplyCurDir();
            }
        }
    }

    /// <summary>
    /// Grab next cell index based on dir
    /// </summary>
    public CellIndex nextCellIndex {
        get {
            var toCellIndex = cellIndex;

            switch(mCurDir) {
                case MoveDir.Up:
                    toCellIndex.row--;
                    break;
                case MoveDir.Down:
                    toCellIndex.row++;
                    break;
                case MoveDir.Left:
                    toCellIndex.col--;
                    break;
                case MoveDir.Right:
                    toCellIndex.col++;
                    break;
            }

            return toCellIndex;
        }
    }

    private State mCurState = State.None;
    private MoveDir mCurDir;

    private CellIndex mDefaultCellIndex;
    private CellIndex mWarpToCellIndex;

    private Coroutine mRout;

    /// <summary>
    /// Return true if evaluation is finished (ends further evaluate)
    /// </summary>
    protected virtual bool EvaluateEntity(LevelEntity ent) {
        return false;
    }

    /// <summary>
    /// Return true if evaluation is finished (ends further evaluate)
    /// </summary>
    protected virtual bool EvaluateTile(LevelTile tile) {
        return false;
    }

    void OnDisable() {
        if(!Application.isPlaying)
            return;

        if(levelGrid)
            levelGrid.RemoveEntity(this);

        state = State.None;
    }

    void OnEnable() {
        if(!Application.isPlaying)
            return;

        if(levelGrid) {
            _row = mDefaultCellIndex.row;
            _col = mDefaultCellIndex.col;

            levelGrid.AddEntity(this);
        }

        state = State.Idle;
    }

    void OnDestroy() {
        if(!Application.isPlaying)
            return;

        if(PlayController.isInstantiated)
            PlayController.instance.modeChangedCallback -= OnModeChanged;
    }

    void Awake() {
        if(!Application.isPlaying)
            return;

        mDefaultCellIndex = cellIndex;

        PlayController.instance.modeChangedCallback += OnModeChanged;
    }

    void OnModeChanged(PlayController.Mode mode) {
        switch(mode) {
            case PlayController.Mode.Editing:
                //check if we are already on the original spot
                if(cellIndex == mDefaultCellIndex) {
                    state = State.Idle;
                    dir = startDir;
                }
                else {
                    //warp back to original position
                    switch(state) {
                        case State.Warp:
                            //if we are warping, change it to warp to default
                            if(mWarpToCellIndex != mDefaultCellIndex) {
                                mWarpToCellIndex = mDefaultCellIndex;
                                ApplyCurState(State.Warp);
                            }
                            break;

                        default:
                            mWarpToCellIndex = mDefaultCellIndex;
                            state = State.Warp;
                            break;
                    }
                }
                break;

            case PlayController.Mode.None:
            case PlayController.Mode.Pause:
                //stop moving, return to idle
                switch(state) {
                    case State.Moving:
                        state = State.Idle;
                        break;
                }
                break;

            case PlayController.Mode.Running:
                //move
                switch(state) {
                    case State.Warp:
                    case State.Idle:
                        state = State.Moving;
                        break;
                }
                break;
        }
    }

    IEnumerator DoMove() {
        var changeDirWait = moveChangeDirDelay > 0f ? new WaitForSeconds(moveChangeDirDelay) : null;

        while(mCurState == State.Moving) {
            //ensure current dir is the same
            var toDir = GetNextDir();
            if(dir != toDir) {
                //check next dir
                dir = toDir;
                yield return changeDirWait;
                continue;
            }

            var startPos = levelGrid.GetCellPosition(cellIndex);
            var endPos = levelGrid.GetCellPosition(nextCellIndex);
                        
            float dist = (endPos - startPos).magnitude;
            if(dist > 0f) {                
                var delay = dist / moveSpeed;
                var curTime = 0f;
                while(curTime < delay) {
                    yield return null;

                    curTime += Time.deltaTime;

                    var t = Mathf.Clamp01(curTime / delay);

                    var prevCell = cellIndex;

                    position = Vector2.Lerp(startPos, endPos, t);

                    //check if cell has changed
                    var _cellIndex = cellIndex;
                    if(_cellIndex != prevCell) {
                        var isEvalDone = false;

                        //evaluate
                        var ents = levelGrid.GetEntities(_cellIndex);
                        if(ents != null) {
                            for(int i = 0; i < ents.Count; i++) {
                                if(ents[i]) {
                                    isEvalDone = EvaluateEntity(ents[i]);
                                    if(isEvalDone)
                                        break;
                                }
                            }
                        }

                        if(!isEvalDone) {
                            var tile = levelGrid.GetTile(_cellIndex);
                            if(tile)
                                EvaluateTile(tile);
                        }
                    }
                }
            }
            else
                yield return null;
        }
    }

    private void ApplyCurState(State prevState) {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        SnapPosition();


        switch(prevState) {
            case State.Warp:
            case State.Jumping:
                if(displayRoot)
                    displayRoot.localPosition = Vector3.zero;
                break;
        }


        switch(mCurState) {
            case State.Moving:
                mRout = StartCoroutine(DoMove());
                break;

            default:
                break;
        }

        ApplyCurDir();
    }

    private MoveDir GetNextDir() {
        if(!levelGrid)
            return dir; //fail-safe

        var toDir = dir;

        //check for level entity move dir
        var ents = levelGrid.GetEntities(cellIndex);
        if(ents != null) {
            for(int i = 0; i < ents.Count; i++) {
                var entMoveDir = ents[i] as LevelEntityMoveDir;
                if(entMoveDir) {
                    toDir = entMoveDir.dirType;
                    break;
                }
            }
        }

        //check wall on current tile
        var tile = levelGrid.GetTile(cellIndex);
        if(!tile)
            return toDir; //fail-safe

        switch(toDir) {
            case MoveDir.Up:
                if(tile.isWallN)
                    toDir = MoveDir.Right;
                break;
            case MoveDir.Down:
                if(tile.isWallS)
                    toDir = MoveDir.Left;
                break;
            case MoveDir.Left:
                if(tile.isWallW)
                    toDir = MoveDir.Up;
                break;
            case MoveDir.Right:
                if(tile.isWallE)
                    toDir = MoveDir.Down;
                break;
        }
                
        //check wall on next tile, or if it's empty/outside
        var nextTile = levelGrid.GetTile(nextCellIndex);
        if(nextTile) {
            switch(toDir) {
                case MoveDir.Up:
                    if(nextTile.isWallS)
                        toDir = MoveDir.Right;
                    break;
                case MoveDir.Down:
                    if(nextTile.isWallN)
                        toDir = MoveDir.Left;
                    break;
                case MoveDir.Left:
                    if(nextTile.isWallE)
                        toDir = MoveDir.Up;
                    break;
                case MoveDir.Right:
                    if(nextTile.isWallW)
                        toDir = MoveDir.Down;
                    break;
            }
        }
        else {
            switch(toDir) {
                case MoveDir.Up:
                    toDir = MoveDir.Right;
                    break;
                case MoveDir.Down:
                    toDir = MoveDir.Left;
                    break;
                case MoveDir.Left:
                    toDir = MoveDir.Up;
                    break;
                case MoveDir.Right:
                    toDir = MoveDir.Down;
                    break;
            }
        }

        return toDir;
    }

    private void ApplyCurDir() {
        bool isHFlip = false;
        string take = null;

        switch(mCurState) {
            case State.Idle:
            case State.None:
                switch(mCurDir) {
                    case MoveDir.Up:
                        take = takeIdleUp;
                        break;
                    case MoveDir.Down:
                        take = takeIdleDown;
                        break;
                    case MoveDir.Left:
                        take = takeIdleSide;
                        isHFlip = true;
                        break;
                    case MoveDir.Right:
                        take = takeIdleSide;
                        break;
                }
                break;

            case State.Moving:
                switch(mCurDir) {
                    case MoveDir.Up:
                        take = takeMoveUp;
                        break;
                    case MoveDir.Down:
                        take = takeMoveDown;
                        break;
                    case MoveDir.Left:
                        take = takeMoveSide;
                        isHFlip = true;
                        break;
                    case MoveDir.Right:
                        take = takeMoveSide;
                        break;
                }
                break;

            case State.Jumping:
                switch(mCurDir) {
                    case MoveDir.Up:
                        take = takeJumpUp;
                        break;
                    case MoveDir.Down:
                        take = takeJumpDown;
                        break;
                    case MoveDir.Left:
                        take = takeJumpSide;
                        isHFlip = true;
                        break;
                    case MoveDir.Right:
                        take = takeJumpSide;
                        break;
                }
                break;

            case State.Victory:
                take = takeVictory;
                break;

            case State.Dead:
                take = takeDead;
                break;
        }

        if(displaySpriteRender)
            displaySpriteRender.flipX = isHFlip;

        if(animator && !string.IsNullOrEmpty(take))
            animator.Play(take);
    }
}
