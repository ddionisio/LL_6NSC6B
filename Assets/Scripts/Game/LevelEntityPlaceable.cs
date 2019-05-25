using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelEntityPlaceable : LevelEntity, M8.IPoolSpawn, M8.IPoolDespawn, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public const string parmCellIndex = "cellIndex";

    [Header("Move")]
    public DG.Tweening.Ease moveEase = DG.Tweening.Ease.InOutSine;
    public float moveDelay = 0.15f;

    [Header("Display")]
    public Transform displayRoot;
    public M8.SpriteColorGroup displayColorGroup;
    public Color displayColorDragging = Color.gray;

    [Header("Display Misc.")]
    public GameObject highlightGO;

    public SpriteRenderer deleteFillSpriteRender; //fill will modify height, use grid or slice mode

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeSpawn;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeDelete;

    public bool isInteractible {
        get {
            if(mRout != null)
                return false;

            //check play mode
            return PlayController.instance.curMode == PlayController.Mode.Editing;
        }
    }

    public M8.PoolDataController poolData {
        get {
            if(!mPoolDat)
                mPoolDat = GetComponent<M8.PoolDataController>();
            return mPoolDat;
        }
    }

    public virtual Sprite dragIcon { get { return null; } }
    public virtual float dragIconRotate { get { return 0f; } }

    protected Coroutine mRout;

    private M8.PoolDataController mPoolDat;

    private bool mIsDragging;

    private float mDeleteFillDefaultHeight;

    public static bool CheckPlaceable(LevelGrid grid, CellIndex cellIndex) {
        if(!grid)
            return false;

        var tile = grid.GetTile(cellIndex);

        //Debug.Log(string.Format("cellIndex: {0}, {1}", cellIndex.col, cellIndex.row));

        //check tile, ensure it is blank
        if(tile == null || !tile.isPlaceable)
            return false;

        var ents = grid.GetEntities(cellIndex);

        //check if there's an entity, if it's only one and it's another placeable, then it is fine.
        if(ents != null && ents.Count > 0) {
            return ents.Count == 1 && ents[0] is LevelEntityPlaceable;
        }

        return true;
    }

    public void Delete() {
        ApplyDeleteFill(0f);

        //despawn
        StopCurrentRout();

        mRout = StartCoroutine(DoDelete());
    }

    public void ApplyDeleteFill(float t) {
        if(deleteFillSpriteRender) {
            if(t > 0f) {
                deleteFillSpriteRender.gameObject.SetActive(true);

                var s = deleteFillSpriteRender.size;
                s.y = mDeleteFillDefaultHeight * t;
                deleteFillSpriteRender.size = s;
            }
            else
                deleteFillSpriteRender.gameObject.SetActive(false);
        }
    }

    protected void MoveTo(Vector2 toPos) {
        StopCurrentRout();
        mRout = StartCoroutine(DoMove(toPos));
    }

    protected void StopCurrentRout() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    protected virtual void Awake() {
        if(deleteFillSpriteRender) {
            mDeleteFillDefaultHeight = deleteFillSpriteRender.size.y;
            deleteFillSpriteRender.gameObject.SetActive(false);
        }
    }

    protected virtual void Spawned(M8.GenericParams parms) { }
    protected virtual void Despawned() { }

    protected virtual void MoveFinish() { }

    protected virtual void OnDragBegin() { }
    protected virtual void OnDragUpdate(PointerEventData eventData) { }
    protected virtual void OnDragInvalidate() { }
    protected virtual void OnPointerEnter(PointerEventData eventData) { }
    protected virtual void OnPointerExit(PointerEventData eventData) { }

    void OnApplicationFocus(bool focus) {
        if(!Application.isPlaying)
            return;

        if(!focus)
            DragInvalidate();
    }
        
    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {

        var _cellIndex = new CellIndex(-1, -1);

        if(parms != null) {
            if(parms.ContainsKey(parmCellIndex)) {
                _cellIndex = parms.GetValue<CellIndex>(parmCellIndex);
            }
        }

        //setup cell
        if(_cellIndex.isValid) {
            mCellIndex = _cellIndex;

            if(levelGrid)
                levelGrid.AddEntity(this);

            SnapPosition();
        }

        displayRoot.localPosition = Vector3.zero;

        if(highlightGO) highlightGO.SetActive(false);

        ApplyDeleteFill(0f);
                
        DragInvalidate();

        if(animator && !string.IsNullOrEmpty(takeSpawn))
            animator.Play(takeSpawn);

        Spawned(parms);
    }

    void M8.IPoolDespawn.OnDespawned() {
        Despawned();

        StopCurrentRout();

        DragInvalidate();

        if(deleteFillSpriteRender)
            deleteFillSpriteRender.gameObject.SetActive(false);

        if(levelGrid)
            levelGrid.RemoveEntity(this);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if(highlightGO) highlightGO.SetActive(isInteractible);

        if(isInteractible)
            OnPointerEnter(eventData);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if(highlightGO) highlightGO.SetActive(false);

        OnPointerExit(eventData);
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if(isInteractible) {
            DragBegin();
            DragUpdate(eventData);
        }
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;

        DragUpdate(eventData);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;

        DragUpdate(eventData);

        DragEnd();
    }

    IEnumerator DoDelete() {
        if(animator && !string.IsNullOrEmpty(takeDelete))
            yield return animator.PlayWait(takeDelete);

        mRout = null;

        poolData.Release();
    }

    IEnumerator DoMove(Vector2 toPos) {
        Vector2 fromPos = position;

        position = toPos;

        displayRoot.position = fromPos;

        var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(moveEase);

        float curTime = 0f;
        while(curTime < moveDelay) {
            yield return null;

            curTime += Time.deltaTime;

            var t = easeFunc(curTime, moveDelay, 0f, 0f);

            displayRoot.position = Vector2.Lerp(fromPos, toPos, t);
        }

        displayRoot.localPosition = Vector3.zero;

        mRout = null;

        MoveFinish();
    }

    private void DragInvalidate() {
        if(mIsDragging) {
            if(PlayController.isInstantiated && PlayController.instance.levelGridPointer)
                PlayController.instance.levelGridPointer.mode = LevelGridPointerWidget.Mode.Pointer;
        }

        mIsDragging = false;

        if(displayColorGroup) displayColorGroup.Revert();

        if(levelGrid) {
            if(levelGrid.cellHighlightRoot) levelGrid.cellHighlightRoot.gameObject.SetActive(false);
            if(levelGrid.cellDestGO) levelGrid.cellDestGO.SetActive(false);
        }

        OnDragInvalidate();
    }

    private void DragBegin() {
        mIsDragging = true;

        if(displayColorGroup) displayColorGroup.ApplyColor(displayColorDragging);

        if(highlightGO) highlightGO.SetActive(false);

        var drag = PlayController.isInstantiated ? PlayController.instance.levelGridPointer : null;
        if(drag) {
            drag.SetupDrag(dragIcon, dragIconRotate);
            drag.mode = LevelGridPointerWidget.Mode.Drag;
        }

        OnDragBegin();
    }

    private void DragUpdate(PointerEventData eventData) {
        var drag = PlayController.isInstantiated ? PlayController.instance.levelGridPointer : null;
        if(drag)
            drag.UpdatePointer(eventData);

        OnDragUpdate(eventData);
    }

    private void DragEnd() {
        //check tile if placeable
        var drag = PlayController.isInstantiated ? PlayController.instance.levelGridPointer : null;
        if(drag && drag.isDragValid) {
            var cellIndex = drag.pointerCellIndex;

            //check if we need to swap
            bool isMoveValid = true;

            var ents = levelGrid.GetEntities(cellIndex);
            if(ents != null && ents.Count == 1) {
                var otherEnt = ents[0] as LevelEntityPlaceable;
                if(otherEnt) {
                    if(otherEnt != this) //don't swap with self
                        otherEnt.MoveTo(position);
                    else
                        isMoveValid = false;
                }
            }

            if(isMoveValid) {
                var toPos = levelGrid.GetCellPosition(cellIndex);
                MoveTo(toPos);
            }
        }

        DragInvalidate();
    }
}
