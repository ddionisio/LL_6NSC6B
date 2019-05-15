using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class LevelEntityItemWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public const string parmPool = "pool";
    public const string parmLevelGrid = "levelGrid";
    public const string parmCellHighlight = "cellHighlight";
    public const string parmCount = "count";

    [Header("Template")]
    public GameObject template;
    public int templateCapacity = 16;

    [Header("Display")]
    public GameObject highlightGO;

    public M8.UI.Graphics.ColorGroup colorGroup;
    public Color colorDisabled = Color.gray;
        
    public Text countText;

    [Header("Display Drag")]
    public Transform dragRoot;
    public M8.UI.Graphics.ColorGroup dragColorGroup;
    public Color dragColorInvalid = Color.red;

    public int count { get { return mActiveEntities != null ? mActiveEntities.Capacity - mActiveEntities.Count : 0; } }

    public int activeCount { get { return mActiveEntities != null ? mActiveEntities.Count : 0; } }

    public bool isInteractible { get { return count > 0; } }

    public bool isDragging { get; private set; }

    public LevelGrid levelGrid { get; private set; }

    private M8.PoolController mPool;
    private Transform mCellHighlightRoot;

    private M8.CacheList<LevelEntity> mActiveEntities;

    public virtual void Init(M8.PoolController aPool, LevelGrid aLevelGrid, Transform aCellHighlight, int aCount) {
        mPool = aPool;
        levelGrid = aLevelGrid;
        mCellHighlightRoot = aCellHighlight;

        if(mActiveEntities == null || mActiveEntities.Count < aCount)
            mActiveEntities = new M8.CacheList<LevelEntity>(aCount);

        if(mPool)
            mPool.AddType(template, templateCapacity, templateCapacity);

        if(highlightGO) highlightGO.SetActive(false);


        RefreshDisplay();
    }

    public virtual void Deinit() {
        //clear out some data
        ReleaseAll();
        DragInvalidate();
    }

    public void DragInvalidate() {
        isDragging = false;

        if(dragRoot) dragRoot.gameObject.SetActive(false);

        if(mCellHighlightRoot) mCellHighlightRoot.gameObject.SetActive(false);

        RefreshDisplay();
    }

    public void ReleaseAll() {
        if(mActiveEntities == null)
            return;

        for(int i = 0; i < mActiveEntities.Count; i++) {
            if(mActiveEntities[i] && mActiveEntities[i].poolData) {
                mActiveEntities[i].poolData.despawnCallback -= OnEntityDespawn;
                mActiveEntities[i].poolData.Release();
            }
        }

        mActiveEntities.Clear();
    }

    /// <summary>
    /// Force release given entity
    /// </summary>
    public void Release(LevelEntity ent) {
        if(!ent)
            return;

        if(mActiveEntities == null)
            return;

        if(ent.poolData)
            ent.poolData.despawnCallback -= OnEntityDespawn;

        mActiveEntities.Remove(ent);
    }

    protected abstract bool IsPlaceable(CellIndex cellIndex);

    protected virtual M8.GenericParams GetSpawnParms() { return null; }
    
    void OnApplicationFocus(bool focus) {
        if(!focus)
            DragInvalidate();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if(highlightGO) highlightGO.SetActive(isInteractible);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if(highlightGO) highlightGO.SetActive(false);
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if(!isInteractible)
            return;

        isDragging = true;

        if(highlightGO) highlightGO.SetActive(false);

        if(dragRoot) dragRoot.gameObject.SetActive(true);
                
        RefreshDisplay();

        DragUpdate(eventData);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!isDragging)
            return;

        DragUpdate(eventData);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(!isDragging)
            return;

        //check spawnable
        if(levelGrid && eventData.pointerCurrentRaycast.isValid) {
            var cellIndex = levelGrid.GetCellIndex(eventData.pointerCurrentRaycast.worldPosition);
            if(cellIndex.isValid && IsPlaceable(cellIndex)) {
                //check if there exists another, delete them if deletable
                var ents = levelGrid.GetEntities(cellIndex);
                if(ents != null) {
                    for(int i = 0; i < ents.Count; i++) {
                        if(ents[i])
                            ents[i].Delete();
                    }
                }

                //spawn entity
                var toPos = levelGrid.GetCellPosition(cellIndex);

                var spawnParms = GetSpawnParms();

                var ent = mPool.Spawn<LevelEntity>(name + count.ToString(), levelGrid.entitiesRoot, toPos, spawnParms);

                ent.poolData.despawnCallback += OnEntityDespawn;

                mActiveEntities.Add(ent);
            }
        }

        DragInvalidate();
    }

    void DragUpdate(PointerEventData eventData) {
        bool dragValid = false;
        Vector2 dragCellPos = Vector2.zero;

        if(eventData.pointerCurrentRaycast.isValid) {
            if(mCellHighlightRoot) {
                if(levelGrid) {
                    //check if cell is valid
                    var cellIndex = levelGrid.GetCellIndex(eventData.pointerCurrentRaycast.worldPosition);
                    if(cellIndex.isValid && IsPlaceable(cellIndex)) {
                        dragValid = true;
                        dragCellPos = levelGrid.GetCellPosition(cellIndex);
                    }
                }
            }
        }

        if(mCellHighlightRoot) {
            mCellHighlightRoot.gameObject.SetActive(dragValid);
            if(dragValid)
                mCellHighlightRoot.position = dragCellPos;
        }

        if(dragColorGroup) {
            if(dragValid)
                dragColorGroup.Revert();
            else
                dragColorGroup.ApplyColor(dragColorInvalid);
        }

        dragRoot.position = eventData.position;
    }

    void OnEntityDespawn(M8.PoolDataController pdc) {
        pdc.despawnCallback -= OnEntityDespawn;

        //remove from actives
        if(mActiveEntities != null) {
            for(int i = 0; i < mActiveEntities.Count; i++) {
                if(mActiveEntities[i].poolData == pdc) {
                    mActiveEntities.RemoveAt(i);
                    break;
                }
            }
        }

        RefreshDisplay();
    }

    private void RefreshDisplay() {
        if(colorGroup) {
            if(isInteractible && !isDragging)
                colorGroup.Revert();
            else
                colorGroup.ApplyColor(colorDisabled);
        }

        if(countText)
            countText.text = count.ToString();
    }
}
