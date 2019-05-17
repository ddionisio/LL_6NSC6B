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
                    toCellIndex.row++;
                    break;
                case MoveDir.Down:
                    toCellIndex.row--;
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

    public CellIndex prevCellIndex { get; private set; }
        
    private State mCurState = State.None;
    private MoveDir mCurDir;

    private CellIndex mDefaultCellIndex;
    private CellIndex mWarpToCellIndex;

    private Coroutine mRout;

    /// <summary>
    /// Return state to switch to, if none, then evaluate further
    /// </summary>
    protected virtual State EvaluateEntity(LevelEntity ent) {
        if(ent is LevelEntityMoveDir) {
            //change dir
            dir = ((LevelEntityMoveDir)ent).dirType;
        }

        return State.None;
    }

    /// <summary>
    /// Return state to switch to, if none, then evaluate further
    /// </summary>
    protected virtual State EvaluateTile(LevelTile tile) {
        return State.None;
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
                dir = startDir;

                //check if we are already on the original spot
                if(cellIndex == mDefaultCellIndex) {
                    state = State.Idle;
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
            var toDir = EvalDir(dir);
            if(dir != toDir) {
                //check next dir
                dir = toDir;
                yield return changeDirWait;
                continue;
            }

            //check if we are heading to prev cell, try another route
            if(nextCellIndex == prevCellIndex) {
                var checkDir = GetNextDir(dir);
                var nextDir = EvalDir(checkDir);
                if(checkDir == nextDir)
                    dir = nextDir;
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
                    if(cellIndex != prevCell) {
                        prevCellIndex = prevCell;

                        var prevDir = dir;

                        var toState = EvaluateCurrentTile();
                        if(toState != State.None) {
                            state = toState;
                            yield break;
                        }
                        else {
                            //reset prevCellIndex if dir is changed
                            if(dir != prevDir)
                                prevCellIndex = cellIndex;
                        }
                    }
                }
            }
            else
                yield return null;
        }
    }

    IEnumerator DoWarp() {
        var prevPos = transform.position;

        //apply warp position
        cellIndex = mWarpToCellIndex;
        SnapPosition();

        if(displayRoot) displayRoot.position = prevPos;

        if(animator && !string.IsNullOrEmpty(takeWarpOut))
            yield return animator.PlayWait(takeWarpOut);

        if(displayRoot) displayRoot.localPosition = Vector3.zero;

        if(animator && !string.IsNullOrEmpty(takeWarpIn))
            yield return animator.PlayWait(takeWarpIn);

        mRout = null;

        //change state depending on mode
        switch(PlayController.instance.curMode) {
            case PlayController.Mode.Running:
                var toState = EvaluateCurrentTile();
                if(toState != State.None)
                    state = toState;
                else
                    state = State.Moving;
                break;

            default:
                state = State.Idle;
                break;
        }
    }

    IEnumerator DoJump() {
        Vector2 prevPos = transform.position;

        //apply warp position
        cellIndex = mWarpToCellIndex;
        SnapPosition();

        if(displayRoot) displayRoot.position = prevPos;

        //jump motion
        var dist = (position - prevPos).magnitude;
        if(dist > 0f) {
            var delay = dist / moveSpeed;
            var curTime = 0f;
            while(curTime < delay) {
                yield return null;

                curTime += Time.deltaTime;

                var t = Mathf.Clamp01(curTime / delay);

                var toPos = Vector2.Lerp(prevPos, position, t);

                toPos.y += Mathf.Sin(Mathf.PI * t) * jumpHeight;

                displayRoot.position = toPos;
            }
        }

        mRout = null;

        //change state depending on mode
        switch(PlayController.instance.curMode) {
            case PlayController.Mode.Running:
                var toState = EvaluateCurrentTile();
                if(toState != State.None)
                    state = toState;
                else
                    state = State.Moving;
                break;

            default:
                state = State.Idle;
                break;
        }
    }

    private State EvaluateCurrentTile() {
        var toState = State.None;

        //evaluate
        var ents = levelGrid.GetEntities(cellIndex);
        if(ents != null) {
            for(int i = 0; i < ents.Count; i++) {
                if(ents[i]) {
                    toState = EvaluateEntity(ents[i]);
                    if(toState != State.None)
                        break;
                }
            }
        }

        if(toState != State.None) {
            var tile = levelGrid.GetTile(cellIndex);
            if(tile)
                toState = EvaluateTile(tile);
        }

        return toState;
    }

    private void ApplyCurState(State prevState) {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        SnapPosition();

        prevCellIndex = cellIndex;

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

            case State.Warp:
                mRout = StartCoroutine(DoWarp());
                break;

            case State.Victory:
                if(animator && !string.IsNullOrEmpty(takeVictory))
                    animator.Play(takeVictory);
                break;

            case State.Dead:
                if(animator && !string.IsNullOrEmpty(takeDead))
                    animator.Play(takeDead);
                break;

            default:
                break;
        }

        ApplyCurDir();
    }

    private MoveDir GetNextDir(MoveDir aDir) {
        switch(aDir) {
            case MoveDir.Up:
                return MoveDir.Right;
            case MoveDir.Down:
                return MoveDir.Left;
            case MoveDir.Left:
                return MoveDir.Up;
            case MoveDir.Right:
                return MoveDir.Down;
        }

        return aDir;
    }

    private CellIndex GetNextTile(MoveDir aDir) {
        var toCellIndex = cellIndex;

        switch(aDir) {
            case MoveDir.Up:
                toCellIndex.row++;
                break;
            case MoveDir.Down:
                toCellIndex.row--;
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

    private MoveDir EvalDir(MoveDir toDir) {
        if(!levelGrid)
            return toDir; //fail-safe

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
        var toCell = GetNextTile(toDir);
        var nextTile = levelGrid.GetTile(toCell);
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

        //check if toDir tile is already travelled

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
        }

        if(displaySpriteRender)
            displaySpriteRender.flipX = isHFlip;

        if(animator && !string.IsNullOrEmpty(take))
            animator.Play(take);
    }
}
