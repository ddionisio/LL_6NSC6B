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
    public Image iconImage;

    public GameObject highlightGO;

    public M8.UI.Graphics.ColorGroup colorGroup;
    public Color colorDisabled = Color.gray;
        
    public Text countText;

    [Header("Audio")]
    [M8.SoundPlaylist]
    public string sfxDragBegin = "tap";

    public int count { get { return mActiveEntities != null ? mActiveEntities.Capacity - mActiveEntities.Count : 0; } }

    public int activeCount { get { return mActiveEntities != null ? mActiveEntities.Count : 0; } }

    public bool isInteractible { get { return count > 0; } }

    public bool isDragging { get; private set; }

    public virtual float iconRotation { get { return 0f; } }

    private M8.PoolController mPool;

    private M8.CacheList<LevelEntityPlaceable> mActiveEntities;

    private M8.GenericParams mSpawnParms = new M8.GenericParams();

    public virtual void Init(M8.PoolController aPool, int aCount) {
        if(iconImage) iconImage.transform.localEulerAngles = new Vector3(0f, 0f, iconRotation);

        mPool = aPool;

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
        if(isDragging) {
            if(PlayController.isInstantiated && PlayController.instance.levelGridPointer)
                PlayController.instance.levelGridPointer.mode = LevelGridPointerWidget.Mode.Pointer;
        }

        isDragging = false;

        RefreshDisplay();

        DragInvalidated();
    }

    public void RefreshDisplay() {
        if(colorGroup) {
            if(isInteractible && !isDragging)
                colorGroup.Revert();
            else
                colorGroup.ApplyColor(colorDisabled);
        }

        if(countText)
            countText.text = count.ToString();
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

    /// <summary>
    /// Get an active item (first element in active list)
    /// </summary>
    public LevelEntityPlaceable GetActiveItem() {
        return mActiveEntities.Count > 0 ? mActiveEntities[0] : null;
    }

    public LevelEntityPlaceable GetActiveItem(int index) {
        return index >= 0 && index < mActiveEntities.Count ? mActiveEntities[index] : null;
    }

    /// <summary>
    /// Get active item that matches given cell
    /// </summary>
    public LevelEntityPlaceable GetActiveItemFromCell(CellIndex cellIndex) {
        for(int i = 0; i < mActiveEntities.Count; i++) {
            if(mActiveEntities[i].cellIndex == cellIndex)
                return mActiveEntities[i];
        }

        return null;
    }

    protected virtual void ApplySpawnParms(M8.GenericParams parms) { }

    protected virtual void DragUpdated(PointerEventData eventData) { }
    protected virtual void DragInvalidated() { }
    
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

        var drag = PlayController.isInstantiated ? PlayController.instance.levelGridPointer : null;
        if(drag) {
            var iconSpr = iconImage ? iconImage.sprite : null;
            drag.SetupDrag(iconSpr, iconRotation);
            drag.mode = LevelGridPointerWidget.Mode.Drag;
        }
                
        RefreshDisplay();

        DragUpdate(eventData);

        if(!string.IsNullOrEmpty(sfxDragBegin))
            M8.SoundPlaylist.instance.Play(sfxDragBegin, false);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!isDragging)
            return;

        DragUpdate(eventData);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(!isDragging)
            return;

        DragUpdate(eventData);

        var drag = PlayController.isInstantiated ? PlayController.instance.levelGridPointer : null;

        //check spawnable
        if(drag && drag.isDragValid) {
            var levelGrid = PlayController.instance.levelGrid;
            var cellIndex = drag.pointerCellIndex;

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

            var ent = mPool.Spawn<LevelEntityPlaceable>(template.name, name, levelGrid.entitiesRoot, mSpawnParms);

            ent.poolData.despawnCallback += OnEntityDespawn;

            mActiveEntities.Add(ent);
        }

        DragInvalidate();
    }

    void DragUpdate(PointerEventData eventData) {
        var drag = PlayController.isInstantiated ? PlayController.instance.levelGridPointer : null;
        if(drag) {
            drag.UpdatePointer(eventData);

            DragUpdated(eventData);
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
}
