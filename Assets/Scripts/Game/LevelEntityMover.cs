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
    public float jumpDelay = 0.5f;
    public int jumpDisplaySortOrder = 100;

    [Header("Display")]
    public Transform displayRoot;
    public SpriteRenderer displaySpriteRender; //note: default facing right
    public Transform displayShadowRoot;
    public ParticleSystem warpFX;
    public bool hideShadowOnDeath;
    public GameObject displayDeadGO;

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

    [Header("Audio")]
    [M8.SoundPlaylist]
    public string sfxDead;
    [M8.SoundPlaylist]
    public string sfxWarp;
    [M8.SoundPlaylist]
    public string sfxJump;

    [Header("Signal Listen")]
    public M8.Signal signalListenReset; //revert to starting position much like going to edit mode

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
    public CellIndex warpToCellIndex { get { return mWarpToCellIndex; } }

    public event System.Action moveUpdateCallback;

    protected Coroutine mRout;

    private State mCurState = State.None;
    private MoveDir mCurDir;

    private CellIndex mDefaultCellIndex;
    private CellIndex mWarpToCellIndex;

    private int mDefaultDisplaySortOrder;
    private Vector2 mDefaultDisplaySpriteLPos;
        
    public void SetDefaultCell(CellIndex cellIndex) {
        mDefaultCellIndex = cellIndex;
    }

    public void WarpTo(int col, int row) {
        WarpTo(new CellIndex(row, col));
    }

    public void WarpTo(CellIndex dest) {
        switch(state) {
            case State.Warp:
                //restart warping
                if(mWarpToCellIndex != dest) {
                    mWarpToCellIndex = dest;
                    ApplyCurState(State.Warp);
                }
                break;

            default:
                mWarpToCellIndex = dest;
                state = State.Warp;
                break;
        }
    }

    public void JumpTo(int col, int row) {
        JumpTo(new CellIndex(row, col));
    }

    public void JumpTo(CellIndex dest) {
        switch(state) {
            case State.Jumping:
                //restart jumping
                if(mWarpToCellIndex != dest) {
                    mWarpToCellIndex = dest;
                    ApplyCurState(State.Jumping);
                }
                break;

            default:
                mWarpToCellIndex = dest;
                state = State.Jumping;
                break;
        }
    }

    protected virtual void EvaluateBegin() { }

    /// <summary>
    /// Return state to switch to, if none, then evaluate further
    /// </summary>
    protected virtual State EvaluateEntity(LevelEntity ent) {
        var toState = State.None;

        //do our specific things
        if(ent is LevelEntityMoveDir) {
            //change dir
            dir = ((LevelEntityMoveDir)ent).dirType;
        }
        else if(ent is LevelEntityReflect) {
            var reflectEnt = (LevelEntityReflect)ent;

            //apply reflect relative to origin
            mWarpToCellIndex = cellIndex;

            if(reflectEnt.reflectX) {
                mWarpToCellIndex.col = levelGrid.originCol - (mWarpToCellIndex.col - levelGrid.originCol);
            }

            if(reflectEnt.reflectY) {
                mWarpToCellIndex.row = levelGrid.originRow - (mWarpToCellIndex.row - levelGrid.originRow);
            }

            toState = State.Jumping;
        }
        else if(ent is LevelEntityAbsolute) {
            var absEnt = (LevelEntityAbsolute)ent;
            if(absEnt.axisType != AxisType.None) {
                //apply abs. relative to origin
                mWarpToCellIndex = cellIndex;

                switch(absEnt.axisType) {
                    case AxisType.X:
                        mWarpToCellIndex.col = levelGrid.originCol + Mathf.Abs(mWarpToCellIndex.col - levelGrid.originCol);
                        break;
                    case AxisType.Y:
                        mWarpToCellIndex.row = levelGrid.originRow + Mathf.Abs(mWarpToCellIndex.row - levelGrid.originRow);
                        break;
                }

                toState = State.Jumping;
            }
        }

        return toState;
    }

    /// <summary>
    /// Return state to switch to, if none, then evaluate further
    /// </summary>
    protected virtual State EvaluateObstacle(LevelEntityObstacle obstacle) {
        return State.None;
    }

    protected virtual void OnMoveCurrentTile() { }

    protected virtual void OnDeadPost() { }

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
            mCellIndex = mDefaultCellIndex;
            SnapPosition();

            levelGrid.AddEntity(this);
        }

        state = State.Idle;
    }

    protected virtual void OnDestroy() {
        if(!Application.isPlaying)
            return;

        if(PlayController.isInstantiated)
            PlayController.instance.modeChangedCallback -= OnModeChanged;

        if(signalListenReset) signalListenReset.callback -= OnSignalReset;
    }

    protected virtual void Awake() {
        if(!Application.isPlaying)
            return;

        RefreshCellIndex();
        mDefaultCellIndex = cellIndex;

        if(displaySpriteRender) {
            mDefaultDisplaySortOrder = displaySpriteRender.sortingOrder;

            mDefaultDisplaySpriteLPos = displaySpriteRender.transform.localPosition;
        }

        if(displayDeadGO) displayDeadGO.SetActive(false);

        PlayController.instance.modeChangedCallback += OnModeChanged;

        if(signalListenReset) signalListenReset.callback += OnSignalReset;
    }

    protected virtual void OnModeChanged(PlayController.Mode mode) {
        switch(mode) {
            case PlayController.Mode.Editing:
                OnSignalReset();
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

    void OnSignalReset() {
        dir = startDir;

        //check if we are already on the original spot
        if(cellIndex == mDefaultCellIndex) {
            state = State.Idle;
        }
        else {
            //warp back to original position
            WarpTo(mDefaultCellIndex);
        }
    }

    private void MoveUpdate() {
        OnMoveCurrentTile();

        if(moveUpdateCallback != null)
            moveUpdateCallback();
    }

    IEnumerator DoMove() {
        var changeDirWait = moveChangeDirDelay > 0f ? new WaitForSeconds(moveChangeDirDelay) : null;

        yield return null;

        while(mCurState == State.Moving) {
            //ensure current dir is the same
            var toDir = EvalDir(dir);
            if(dir != toDir) {
                //check next dir
                dir = toDir;
                yield return changeDirWait;
                continue;
            }

            MoveUpdate();

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
                            MoveUpdate(); //allow one last move update before changing state
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
        if(!string.IsNullOrEmpty(sfxWarp))
            M8.SoundPlaylist.instance.Play(sfxWarp, false);

        if(animator && !string.IsNullOrEmpty(takeWarpOut))
            yield return animator.PlayWait(takeWarpOut);

        //apply warp position
        cellIndex = mWarpToCellIndex;
        SnapPosition();

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
        if(!string.IsNullOrEmpty(sfxJump))
            M8.SoundPlaylist.instance.Play(sfxJump, false);

        var startPos = position;
        var endPos = levelGrid.GetCellPosition(mWarpToCellIndex);

        //jump motion
        var curTime = 0f;
        while(curTime < jumpDelay) {
            yield return null;

            curTime += Time.deltaTime;

            var t = Mathf.Clamp01(curTime / jumpDelay);

            var toPos = Vector2.Lerp(startPos, endPos, t);
            var jumpPos = new Vector2(toPos.x, toPos.y + Mathf.Sin(Mathf.PI * t) * jumpHeight);

            if(displayRoot) displayRoot.position = jumpPos;

            if(displayShadowRoot) displayShadowRoot.position = toPos;
        }

        //apply position
        cellIndex = mWarpToCellIndex;
        SnapPosition();

        mRout = null;

        //change state depending on mode
        switch(PlayController.instance.curMode) {
            case PlayController.Mode.Running:
                var toState = EvaluateCurrentTile();
                if(toState != State.None) {
                    if(toState == State.Jumping) {
                        //jump again? check if it's to a different cell
                        if(mWarpToCellIndex != cellIndex)
                            ApplyCurState(mCurState);
                        else
                            state = State.Moving; //ignore and move along
                    }
                    else
                        state = toState;
                }
                else
                    state = State.Moving;
                break;

            default:
                state = State.Idle;
                break;
        }
    }

    IEnumerator DoDead() {
        if(!string.IsNullOrEmpty(sfxDead))
            M8.SoundPlaylist.instance.Play(sfxDead, false);

        if(animator && !string.IsNullOrEmpty(takeDead))
            yield return animator.PlayWait(takeDead);

        if(displayDeadGO) displayDeadGO.SetActive(true);

        mRout = null;

        OnDeadPost();
    }

    private State EvaluateCurrentTile() {
        var toState = State.None;

        EvaluateBegin();

        //check if tile is empty, should be dead
        var tile = levelGrid.GetTile(cellIndex);
        if(!tile)
            return State.Dead;

        //evaluate
        var ents = levelGrid.GetEntities(cellIndex);
        if(ents != null) {
            for(int i = 0; i < ents.Count; i++) {
                var ent = ents[i];

                if(ent && ent != this) {
                    toState = EvaluateEntity(ents[i]);
                    if(toState != State.None)
                        break;
                }
            }
        }

        if(toState == State.None) {
            if(levelGrid.tileObstacles != null) {
                var obstacle = levelGrid.tileObstacles[row, col];
                if(obstacle)
                    toState = EvaluateObstacle(obstacle);
            }
        }

        return toState;
    }

    private void ApplyCurState(State prevState) {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        SnapPosition();

        if(displaySpriteRender) {
            var t = displaySpriteRender.transform;
            t.localPosition = mDefaultDisplaySpriteLPos;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        prevCellIndex = cellIndex;

        int displaySortOrder = mDefaultDisplaySortOrder;

        switch(prevState) {
            case State.Warp:
                if(warpFX) {
                    var fxDat = warpFX.main;
                    fxDat.loop = false;
                }
                break;

            case State.Jumping:
                if(displayRoot)
                    displayRoot.localPosition = Vector3.zero;
                if(displayShadowRoot)
                    displayShadowRoot.localPosition = Vector3.zero;
                break;

            case State.Dead:
                if(animator && !string.IsNullOrEmpty(takeDead))
                    animator.ResetTake(takeDead);

                if(hideShadowOnDeath && displayShadowRoot)
                    displayShadowRoot.gameObject.SetActive(true);

                if(displayDeadGO) displayDeadGO.SetActive(false);
                break;
        }

        switch(mCurState) {
            case State.Moving:
                mRout = StartCoroutine(DoMove());
                break;

            case State.Warp:
                if(warpFX) {
                    var fxDat = warpFX.main;
                    fxDat.loop = true;
                    warpFX.Play();
                }

                mRout = StartCoroutine(DoWarp());
                break;

            case State.Jumping:
                displaySortOrder = jumpDisplaySortOrder;

                mRout = StartCoroutine(DoJump());
                break;

            case State.Victory:
                if(animator && !string.IsNullOrEmpty(takeVictory))
                    animator.Play(takeVictory);
                break;

            case State.Dead:
                if(hideShadowOnDeath && displayShadowRoot)
                    displayShadowRoot.gameObject.SetActive(false);

                mRout = StartCoroutine(DoDead());
                break;

            default:
                break;
        }

        if(displaySpriteRender)
            displaySpriteRender.sortingOrder = displaySortOrder;

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
