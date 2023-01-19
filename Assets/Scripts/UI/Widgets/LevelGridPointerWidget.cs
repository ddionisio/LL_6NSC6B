using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelGridPointerWidget : MonoBehaviour {
    public enum Mode {
        None,
        Drag,
        Pointer
    }

    [Header("Drag")]
    public string tagDragLevelEntityPlaceable = "LevelEntityPlaceable";

    public GameObject dragPointerGO;
    public GameObject dragDisplayGO;
    public Image dragIconImage;

    public M8.UI.Graphics.ColorGroup dragColorGroup;
    public Color dragColorInvalid = Color.red;

    [Header("Pointer")]
    public Transform pointerRoot;
    public GameObject pointerDisplayGO;
    public Text pointerDescText; //display: quadrant 1, 2, ...etc. or axis-X/Y, Origin
    public string pointerCoordStringFormat = "({0}, {1})";
    public bool isPointerDisabled;
    
    public Mode mode {
        get { return mMode; }
        set {
            if(mMode != value) {
                mMode = value;
                ApplyCurrentMode();
            }
        }
    }

    public bool isDragValid { get; private set; }

    public CellIndex pointerCellIndex { get; private set; } = new CellIndex(-1, -1);

    private Mode mMode = Mode.None;
    private bool mIsPointerCoordValid = false;

    private System.Text.StringBuilder mDescStringBuff = new System.Text.StringBuilder();

    public void SetupDrag(Sprite iconSprite, float iconRotate) {
        if(dragIconImage) {
            dragIconImage.sprite = iconSprite;
            dragIconImage.SetNativeSize();
            dragIconImage.transform.localEulerAngles = new Vector3(0f, 0f, iconRotate);
        }
    }

    public CellIndex GetCellIndex(PointerEventData ptrData) {
        var levelGrid = PlayController.instance.levelGrid;
        var _pointerCellIndex = new CellIndex(-1, -1);

        if(levelGrid && ptrData.pointerCurrentRaycast.isValid) {
            var go = ptrData.pointerCurrentRaycast.gameObject;
            if(go == levelGrid.gameObject || go.CompareTag(tagDragLevelEntityPlaceable)) {
                _pointerCellIndex = levelGrid.GetCellIndex(ptrData.pointerCurrentRaycast.worldPosition);
            }
        }

        if(_pointerCellIndex.isValid && !LevelEntityPlaceable.CheckPlaceable(levelGrid, _pointerCellIndex))
            _pointerCellIndex.row = _pointerCellIndex.col = -1;

        return _pointerCellIndex;
    }

    public void UpdatePointer(PointerEventData ptrData) {
        var lastPointerCellIndex = pointerCellIndex;
        pointerCellIndex = new CellIndex(-1, -1);
                
        var curMode = PlayController.instance.curMode;
        var levelGrid = PlayController.instance.levelGrid;

        //determine cell and validity
        pointerRoot.position = ptrData.position;

        if(levelGrid && ptrData.pointerCurrentRaycast.isValid) {
            var go = ptrData.pointerCurrentRaycast.gameObject;
            if(go == levelGrid.gameObject || go.CompareTag(tagDragLevelEntityPlaceable)) {
                pointerCellIndex = levelGrid.GetCellIndex(ptrData.pointerCurrentRaycast.worldPosition);
            }   
        }

        var isCellHighlightShow = pointerCellIndex.isValid && curMode == PlayController.Mode.Editing;

        if(mode == Mode.Drag) {
            var _isDragValid = pointerCellIndex.isValid && LevelEntityPlaceable.CheckPlaceable(levelGrid, pointerCellIndex);
            if(isDragValid != _isDragValid) {
                isDragValid = _isDragValid;
                if(isDragValid)
                    dragColorGroup.Revert();
                else
                    dragColorGroup.ApplyColor(dragColorInvalid);

                CellHighlightSetActive(isDragValid);
            }

            if(!isDragValid)
                isCellHighlightShow = false;
        }

        //show pointer info?
        bool isPointerRefresh = false;

        bool isPointerCoordValid;
        //if(levelGrid.GetTile(pointerCellIndex) == null) //only show if there's a tile
            //isPointerCoordValid = false;
        if(curMode != PlayController.Mode.Editing) //only show if we are in edit mode
            isPointerCoordValid = false;
        else
            isPointerCoordValid = pointerCellIndex.isValid;

        if(mIsPointerCoordValid != isPointerCoordValid) { 
            mIsPointerCoordValid = isPointerCoordValid;
            if(pointerDisplayGO) pointerDisplayGO.SetActive(mIsPointerCoordValid && !isPointerDisabled);

            isPointerRefresh = mIsPointerCoordValid;
        }
        else if(mIsPointerCoordValid && pointerCellIndex != lastPointerCellIndex)
            isPointerRefresh = true;

        //update pointer display
        if(isPointerRefresh) {
            if(pointerDescText) {
                mDescStringBuff.Clear();

                var textRef = GameData.instance.GetQuadrantTextRef(pointerCellIndex);

                mDescStringBuff.AppendLine(M8.Localize.Get(textRef));
                mDescStringBuff.AppendFormat(pointerCoordStringFormat, pointerCellIndex.col - levelGrid.originCol, pointerCellIndex.row - levelGrid.originRow);

                pointerDescText.text = mDescStringBuff.ToString();
            }
        }

        //cell highlight
        if(levelGrid.cellHighlightRoot) {
            if(isCellHighlightShow) {
                levelGrid.cellHighlightRoot.position = levelGrid.GetCellPosition(pointerCellIndex);
                CellHighlightSetActive(true);

            }
            else
                CellHighlightSetActive(false);
        }
    }

    void OnDestroy() {
        if(PlayController.isInstantiated)
            PlayController.instance.modeChangedCallback -= OnModeChanged;
    }

    void Awake() {
        ApplyCurrentMode();

        PlayController.instance.modeChangedCallback += OnModeChanged;
    }

    void Update() {
        if(mMode == Mode.Pointer) {
            var ptrData = GetLastPointerEventData();
            if(ptrData != null) {
                //check if we are on a level entity or grid
                UpdatePointer(ptrData);
            }
            else {
                mIsPointerCoordValid = false;
                if(pointerDisplayGO) pointerDisplayGO.SetActive(false);
            }
        }
    }

    void OnModeChanged(PlayController.Mode playMode) {
        switch(playMode) {
            case PlayController.Mode.None:
                mode = Mode.None;
                break;

            default:
                if(mode == Mode.None)
                    mode = Mode.Pointer;
                break;
        }
    }

    private void ApplyCurrentMode() {
        //if(dragPointerGO) dragPointerGO.SetActive(mMode == Mode.Drag);
        if(dragPointerGO) dragPointerGO.SetActive(false);
        if(dragDisplayGO) dragDisplayGO.SetActive(mMode == Mode.Drag);

        CellHighlightSetActive(mMode == Mode.Drag);

        if(pointerRoot)
            pointerRoot.gameObject.SetActive(mMode != Mode.None);

        mIsPointerCoordValid = false;
        if(pointerDisplayGO) pointerDisplayGO.SetActive(false);

        isDragValid = true;
        dragColorGroup.Revert();
    }

    private void CellHighlightSetActive(bool active) {
        if(!PlayController.isInstantiated || !PlayController.instance.levelGrid)
            return;

        var lvlGrid = PlayController.instance.levelGrid;

        bool isPointerActive = false;
        bool isDotXActive = false;
        bool isDotYActive = false;
        bool isLineXActive = false;
        bool isLineYActive = false;

        if(active && lvlGrid.cellHighlightRoot) {
            var cellInd = lvlGrid.GetCellIndex(lvlGrid.cellHighlightRoot.position);

            if(cellInd.col >= 0 && cellInd.col < lvlGrid.numCol && cellInd.row >= 0 && cellInd.row < lvlGrid.numRow) {
                isPointerActive = true;

                //not in origin?
                if(cellInd.col != lvlGrid.originCol || cellInd.row != lvlGrid.originRow) {
                    //along y-axis?
                    if(cellInd.row != lvlGrid.originRow) {
                        isDotYActive = true;
                        if(lvlGrid.cellHighlightSpriteRenderDotY) lvlGrid.cellHighlightSpriteRenderDotY.transform.position = lvlGrid.GetCellPosition(cellInd.col, lvlGrid.originRow);

                        isLineYActive = true;
                        if(lvlGrid.cellHighlightSpriteRenderLineY) {
                            var dRow = cellInd.row - lvlGrid.originRow;
                            lvlGrid.cellHighlightSpriteRenderLineY.transform.position = lvlGrid.GetCellPosition(cellInd.col, lvlGrid.originRow);
                            var s = lvlGrid.cellHighlightSpriteRenderLineY.size;
                            s.y = Mathf.Abs(dRow) * lvlGrid.cellSize.y;
                            lvlGrid.cellHighlightSpriteRenderLineY.size = s;
                            if(dRow > 0)
                                lvlGrid.cellHighlightSpriteRenderLineY.transform.localRotation = Quaternion.identity;
                            else
                                lvlGrid.cellHighlightSpriteRenderLineY.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
                        }
                    }

                    //along x-axis?
                    if(cellInd.col != lvlGrid.originCol) {
                        isDotXActive = true;
                        if(lvlGrid.cellHighlightSpriteRenderDotX) lvlGrid.cellHighlightSpriteRenderDotX.transform.position = lvlGrid.GetCellPosition(lvlGrid.originCol, cellInd.row);

                        isLineXActive = true;
                        if(lvlGrid.cellHighlightSpriteRenderLineX) {
                            var dCol = cellInd.col - lvlGrid.originCol;
                            lvlGrid.cellHighlightSpriteRenderLineX.transform.position = lvlGrid.GetCellPosition(lvlGrid.originCol, cellInd.row);
                            var s = lvlGrid.cellHighlightSpriteRenderLineX.size;
                            s.y = Mathf.Abs(dCol) * lvlGrid.cellSize.y;
                            lvlGrid.cellHighlightSpriteRenderLineX.size = s;
                            if(dCol > 0)
                                lvlGrid.cellHighlightSpriteRenderLineX.transform.localEulerAngles = new Vector3(0f, 0f, -90f);
                            else
                                lvlGrid.cellHighlightSpriteRenderLineX.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                        }
                    }
                }
            }
        }

        if(lvlGrid.cellHighlightRoot) lvlGrid.cellHighlightRoot.gameObject.SetActive(isPointerActive);
        if(lvlGrid.cellHighlightSpriteRenderDotX) lvlGrid.cellHighlightSpriteRenderDotX.gameObject.SetActive(isDotXActive);
        if(lvlGrid.cellHighlightSpriteRenderDotY) lvlGrid.cellHighlightSpriteRenderDotY.gameObject.SetActive(isDotYActive);
        if(lvlGrid.cellHighlightSpriteRenderLineX) lvlGrid.cellHighlightSpriteRenderLineX.gameObject.SetActive(isLineXActive);
        if(lvlGrid.cellHighlightSpriteRenderLineY) lvlGrid.cellHighlightSpriteRenderLineY.gameObject.SetActive(isLineYActive);
    }

    private PointerEventData GetLastPointerEventData() {
        var inputModule = M8.UI.InputModule.instance;
        if(inputModule) {
            var dat = inputModule.LastPointerEventData(PointerInputModule.kMouseLeftId);
            if(dat != null)
                return dat;

            //try right btn.
            dat = inputModule.LastPointerEventData(PointerInputModule.kMouseRightId);
            if(dat != null)
                return dat;

            //try mid btn.
            dat = inputModule.LastPointerEventData(PointerInputModule.kMouseMiddleId);
            if(dat != null)
                return dat;

            //try something
            dat = inputModule.LastPointerEventData(PointerInputModule.kFakeTouchesId);
            if(dat != null)
                return dat;
        }

        return null;
    }
}
