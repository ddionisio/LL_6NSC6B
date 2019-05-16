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

    [Header("Config")]
    public string tagLevelEntityPlaceable = "LevelEntityPlaceable";

    [Header("Template")]
    public GameObject template;
    public int templateCapacity = 16;

    [Header("Display")]
    public Image iconImage;

    public GameObject highlightGO;

    public M8.UI.Graphics.ColorGroup colorGroup;
    public Color colorDisabled = Color.gray;
        
    public Text countText;

    public int count { get { return mActiveEntities != null ? mActiveEntities.Capacity - mActiveEntities.Count : 0; } }

    public int activeCount { get { return mActiveEntities != null ? mActiveEntities.Count : 0; } }

    public bool isInteractible { get { return count > 0; } }

    public bool isDragging { get; private set; }

    public LevelGrid levelGrid { get; private set; }

    public virtual float iconRotation { get { return 0f; } }

    private M8.PoolController mPool;

    private DragDisplayWidget mDrag;

    private M8.CacheList<LevelEntityPlaceable> mActiveEntities;

    private M8.GenericParams mSpawnParms = new M8.GenericParams();

    public virtual void Init(M8.PoolController aPool, LevelGrid aLevelGrid, DragDisplayWidget aDrag, int aCount) {
        if(iconImage) iconImage.transform.localEulerAngles = new Vector3(0f, 0f, iconRotation);

        mPool = aPool;
        levelGrid = aLevelGrid;
        mDrag = aDrag;

        if(mActiveEntities == null || mActiveEntities.Count < aCount)
            mActiveEntities = new M8.CacheList<LevelEntityPlaceable>(aCount);

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

        if(mDrag)
            mDrag.SetActive(false);

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
    public void Release(LevelEntityPlaceable ent) {
        if(!ent)
            return;

        if(mActiveEntities == null)
            return;

        if(ent.poolData)
            ent.poolData.despawnCallback -= OnEntityDespawn;

        mActiveEntities.Remove(ent);
    }

    protected virtual void ApplySpawnParms(M8.GenericParams parms) { }
    
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

        if(mDrag) {
            var iconSpr = iconImage ? iconImage.sprite : null;
            mDrag.Setup(iconSpr, iconRotation);
            mDrag.SetActive(true);
        }
                
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
        if(CheckPointerEventValid(eventData)) {
            var cellIndex = levelGrid.GetCellIndex(eventData.pointerCurrentRaycast.worldPosition);
            if(cellIndex.isValid && LevelEntityPlaceable.CheckPlaceable(levelGrid, cellIndex)) {
                //check if there exists another, delete them if deletable
                var ents = levelGrid.GetEntities(cellIndex);
                if(ents != null) {
                    for(int i = 0; i < ents.Count; i++) {
                        var entPlaceable = ents[i] as LevelEntityPlaceable;
                        if(entPlaceable)
                            entPlaceable.Delete();
                    }
                }

                //spawn entity
                mSpawnParms[LevelEntityPlaceable.parmCellIndex] = cellIndex;

                ApplySpawnParms(mSpawnParms);

                var ent = mPool.Spawn<LevelEntityPlaceable>(name + count.ToString(), levelGrid.entitiesRoot, mSpawnParms);

                ent.poolData.despawnCallback += OnEntityDespawn;

                mActiveEntities.Add(ent);
            }
        }

        DragInvalidate();
    }

    void DragUpdate(PointerEventData eventData) {
        bool dragValid = false;
        Vector2 dragCellPos = Vector2.zero;

        if(CheckPointerEventValid(eventData)) {
            //check if cell is valid
            var cellIndex = levelGrid.GetCellIndex(eventData.pointerCurrentRaycast.worldPosition);
            if(cellIndex.isValid && LevelEntityPlaceable.CheckPlaceable(levelGrid, cellIndex)) {
                dragValid = true;
                dragCellPos = levelGrid.GetCellPosition(cellIndex);
            }
        }

        if(mDrag) {
            mDrag.SetValid(dragValid);
            mDrag.UpdateCellHighlightPos(dragCellPos);

            mDrag.transform.position = eventData.position;
        }
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

    private bool CheckPointerEventValid(PointerEventData eventData) {
        if(!eventData.pointerCurrentRaycast.isValid)
            return false;

        if(!levelGrid)
            return false;

        var go = eventData.pointerCurrentRaycast.gameObject;

        if(go != levelGrid.gameObject) {
            //check if it's a placeable entity
            if(!go.CompareTag(tagLevelEntityPlaceable))
                return false;
        }

        return true;
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
