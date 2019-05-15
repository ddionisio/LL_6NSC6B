using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Changes moving entity's direction
/// </summary>
public class LevelEntityMoveDir : LevelEntity, M8.IPoolSpawn, M8.IPoolDespawn, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {    
    public const string parmType = "dirType";

    public enum Type {
        Up,
        Down,
        Left,
        Right
    }

    [Header("Move")]
    public DG.Tweening.Ease moveEase = DG.Tweening.Ease.InOutSine;
    public float moveDelay = 0.3f;

    [Header("Display")]
    public Transform displayRoot;
    public Transform displayDirRoot;
    public M8.SpriteColorGroup displayColorGroup;
    public Color displayColorDragging = Color.gray;

    [Header("Display Ghost")]
    public Transform ghostRoot;
    public Transform ghostDirRoot;
    public M8.SpriteColorGroup ghostColorGroup;
    public Color ghostInvalidColor = Color.red;

    [Header("Display Misc.")]
    public GameObject highlightGO;

    public SpriteRenderer deleteFillSpriteRender; //fill will modify height, use grid or slice mode

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeSpawn;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeDelete;

    public Type dirType { get; private set; }

    public bool isInteractible {
        get {
            if(mRout != null)
                return false;

            //check play mode

            return true;
        }
    }

    private bool mIsDragging;
    private bool mIsDraggingPlaceable;
    private CellIndex mDraggingCellIndex;

    private float mDeleteFillDefaultHeight;

    private Coroutine mRout;

    private LevelGrid mLevelGrid;

    public static bool CheckPlaceable(LevelGrid grid, CellIndex cellIndex) {
        var ents = grid.GetEntities(cellIndex);

        //check if there's an entity, if it's only one and it's another dir, then it is fine.
        if(ents != null) {
            return ents.Count == 1 && ents[0] is LevelEntityMoveDir;
        }

        return true;
    }

    public override void Delete() {
        ApplyDeleteFill(0f);

        //despawn
        StopCurrentRout();

        mRout = StartCoroutine(DoDelete());
    }

    public void ApplyDeleteFill(float t) {
        if(deleteFillSpriteRender) {
            var s = deleteFillSpriteRender.size;
            s.y = mDeleteFillDefaultHeight * t;
            deleteFillSpriteRender.size = s;
        }
    }

    void OnApplicationFocus(bool focus) {
        if(!focus)
            DragInvalidate();
    }

    void Awake() {
        if(deleteFillSpriteRender) {
            mDeleteFillDefaultHeight = deleteFillSpriteRender.size.y;
        }
    }

    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {

        if(parms != null) {
            if(parms.ContainsKey(parmType))
                dirType = parms.GetValue<Type>(parmType);
        }

        displayRoot.localPosition = Vector3.zero;

        if(highlightGO) highlightGO.SetActive(false);

        ApplyDeleteFill(0f);

        ApplyDirDisplay();
        SnapPosition();

        if(animator && !string.IsNullOrEmpty(takeSpawn))
            animator.Play(takeSpawn);
    }

    void M8.IPoolDespawn.OnDespawned() {
        StopCurrentRout();

        DragInvalidate();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if(highlightGO) highlightGO.SetActive(isInteractible);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if(highlightGO) highlightGO.SetActive(false);
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

        //determine placement

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
    }

    private void DragInvalidate() {
        mIsDragging = false;

        displayColorGroup.Revert();
        ghostColorGroup.Revert();

        ghostRoot.gameObject.SetActive(false);
    }

    private void DragBegin() {
        mIsDragging = true;

        if(highlightGO) highlightGO.SetActive(false);

        ghostRoot.gameObject.SetActive(true);
    }

    private void DragUpdate(PointerEventData eventData) {
        if(eventData.pointerCurrentRaycast.isValid) {
            //update drag display position
            Vector2 pos = eventData.pointerCurrentRaycast.worldPosition;
            if(levelGrid) {
                mDraggingCellIndex = levelGrid.GetCellIndex(pos);
                ghostRoot.position = levelGrid.GetCellPosition(mDraggingCellIndex);

                mIsDraggingPlaceable = CheckPlaceable(levelGrid, mDraggingCellIndex);
            }
            else {
                mDraggingCellIndex.Invalidate();
                mIsDraggingPlaceable = false;
            }
        }
        else {
            mDraggingCellIndex.Invalidate();
            mIsDraggingPlaceable = false;
        }

        //check tile if placeable
        if(mIsDraggingPlaceable)
            ghostColorGroup.Revert();
        else
            ghostColorGroup.ApplyColor(ghostInvalidColor);
    }

    private void DragEnd() {
        //check tile if placeable
        if(mIsDraggingPlaceable) {
            var prevCellIndex = cellIndex;

            //check if we need to swap
            var ents = levelGrid.GetEntities(mDraggingCellIndex);
            if(ents != null && ents.Count == 1) {
                var entMoveDir = ents[0] as LevelEntityMoveDir;
                if(entMoveDir) {
                    entMoveDir.MoveTo(position);
                }
            }

            var toPos = levelGrid.GetCellPosition(mDraggingCellIndex);
            MoveTo(toPos);
        }

        DragInvalidate();
    }
        
    private void ApplyDirDisplay() {
        Vector3 angles;

        switch(dirType) {
            case Type.Down:
                angles = new Vector3(0f, 0f, 180f);
                break;
            case Type.Left:
                angles = new Vector3(0f, 0f, 90f);
                break;
            case Type.Right:
                angles = new Vector3(0f, 0f, -90f);
                break;
            default:
                angles = Vector3.zero;
                break;
        }

        displayDirRoot.localEulerAngles = angles;
        ghostDirRoot.localEulerAngles = angles;
    }

    private void MoveTo(Vector2 toPos) {
        StopCurrentRout();
        mRout = StartCoroutine(DoMove(toPos));
    }

    private void StopCurrentRout() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }
}
