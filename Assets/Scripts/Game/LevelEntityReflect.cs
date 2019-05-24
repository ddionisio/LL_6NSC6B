using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelEntityReflect : LevelEntityPlaceable {
    public const string parmReflectX = "reflectX";
    public const string parmReflectY = "reflectY";

    [Header("Sprite Refs")]
    public Sprite spriteReflectXLeft; //to left
    public Sprite spriteReflectXRight; //to right
    public Sprite spriteReflectYUp; //to up
    public Sprite spriteReflectYDown; //to down
    public Sprite spriteReflectXYUL; //to upper left
    public Sprite spriteReflectXYUR; //to upper right
    public Sprite spriteReflectXYLL; //to lower left
    public Sprite spriteReflectXYLR; //to lower right

    [Header("Sprite Drag Refs")]
    public Sprite spriteDragXReflect;
    public Sprite spriteDragYReflect;
    public Sprite spriteDragXYReflect;

    [Header("Display")]
    public SpriteRenderer iconSpriteRender;

    public bool reflectX { get; private set; }
    public bool reflectY { get; private set; }

    public override Sprite dragIcon {
        get {
            if(reflectX && reflectY)
                return spriteDragXYReflect;
            else if(reflectX)
                return spriteDragXReflect;
            else if(reflectY)
                return spriteDragYReflect;

            return null;
        }
    }

    public static void ApplyCellDestination(PointerEventData pointerEventData, bool isReflectX, bool isReflectY) {
        var drag = PlayController.instance.levelGridPointer;

        var pointerCellIndex = drag ? drag.GetCellIndex(pointerEventData) : new CellIndex(-1, -1);
        ApplyCellDestination(pointerCellIndex, isReflectX, isReflectY);
    }

    public static void ApplyCellDestination(CellIndex pointerCellIndex, bool isReflectX, bool isReflectY) {
        var levelGrid = PlayController.instance.levelGrid;

        if(levelGrid) {
            bool isDestActive = false;

            if(pointerCellIndex.isValid) {
                if(levelGrid.cellDestGO) levelGrid.cellDestGO.SetActive(true);

                var srcPos = levelGrid.GetCellPosition(pointerCellIndex);

                var destCellIndex = new CellIndex(
                    isReflectY ? levelGrid.originRow - (pointerCellIndex.row - levelGrid.originRow) : pointerCellIndex.row,
                    isReflectX ? levelGrid.originCol - (pointerCellIndex.col - levelGrid.originCol) : pointerCellIndex.col);

                if(pointerCellIndex != destCellIndex) {
                    var destPos = levelGrid.GetCellPosition(destCellIndex);

                    var dpos = destPos - srcPos;
                    var len = dpos.magnitude;
                    var dir = dpos / len;

                    isDestActive = true;

                    if(levelGrid.cellDestReticleRoot) levelGrid.cellDestReticleRoot.position = destPos;

                    if(levelGrid.cellDestSpriteRender) {
                        var t = levelGrid.cellDestSpriteRender.transform;

                        t.position = srcPos;
                        t.up = dir;

                        levelGrid.cellDestSpriteRender.size = new Vector2(levelGrid.cellDestSpriteRender.size.x, len);
                    }
                }
            }

            if(levelGrid.cellDestGO) levelGrid.cellDestGO.SetActive(isDestActive);
        }
    }

    protected override void Spawned(GenericParams parms) {
        reflectX = reflectY = false;

        if(parms != null) {
            if(parms.ContainsKey(parmReflectX))
                reflectX = parms.GetValue<bool>(parmReflectX);

            if(parms.ContainsKey(parmReflectY))
                reflectY = parms.GetValue<bool>(parmReflectY);
        }

        //determine display
        ApplySprite();
    }

    protected override void MoveFinish() {
        ApplySprite();
    }

    protected override void OnDragUpdate(PointerEventData ptrData) {
        ApplyCellDestination(ptrData, reflectX, reflectY);
    }
    protected override void OnDragInvalidate() {
        if(PlayController.instance.levelGrid.cellDestGO)
            PlayController.instance.levelGrid.cellDestGO.SetActive(false);
    }
    protected override void OnPointerEnter(PointerEventData eventData) {
        ApplyCellDestination(cellIndex, reflectX, reflectY);
    }
    protected override void OnPointerExit(PointerEventData eventData) {
        var levelGrid = PlayController.instance.levelGrid;
        if(levelGrid && levelGrid.cellDestGO)
            levelGrid.cellDestGO.SetActive(false);
    }

    private void ApplySprite() {
        if(iconSpriteRender) {
            var quadrant = levelGrid.GetQuadrant(cellIndex);

            if(reflectX && reflectY) {
                //diagonal
                switch(quadrant) {
                    case QuadrantType.Quadrant1:
                        iconSpriteRender.sprite = spriteReflectXYLL;
                        break;
                    case QuadrantType.Quadrant2:
                        iconSpriteRender.sprite = spriteReflectXYLR;
                        break;
                    case QuadrantType.Quadrant3:
                        iconSpriteRender.sprite = spriteReflectXYUR;
                        break;
                    case QuadrantType.Quadrant4:
                        iconSpriteRender.sprite = spriteReflectXYUL;
                        break;
                    case QuadrantType.AxisX:
                        iconSpriteRender.sprite = cellIndex.col - levelGrid.originCol > 0 ? spriteReflectXLeft : spriteReflectXRight;
                        break;
                    case QuadrantType.AxisY:
                        iconSpriteRender.sprite = cellIndex.row - levelGrid.originRow > 0 ? spriteReflectYDown : spriteReflectYUp;
                        break;
                    default:
                        iconSpriteRender.sprite = spriteReflectXYUR;
                        break;
                }
            }
            else if(reflectX) {
                //vertical
                switch(quadrant) {
                    case QuadrantType.Quadrant1:
                    case QuadrantType.Quadrant4:
                        iconSpriteRender.sprite = spriteReflectXLeft;
                        break;
                    case QuadrantType.Quadrant2:
                    case QuadrantType.Quadrant3:
                        iconSpriteRender.sprite = spriteReflectXRight;
                        break;
                    case QuadrantType.AxisX:
                        iconSpriteRender.sprite = cellIndex.col - levelGrid.originCol > 0 ? spriteReflectXLeft : spriteReflectXRight;
                        break;
                    default:
                        iconSpriteRender.sprite = spriteReflectXLeft;
                        break;
                }
            }
            else if(reflectY) {
                //vertical
                switch(quadrant) {
                    case QuadrantType.Quadrant1:
                    case QuadrantType.Quadrant2:
                        iconSpriteRender.sprite = spriteReflectYDown;
                        break;
                    case QuadrantType.Quadrant3:
                    case QuadrantType.Quadrant4:
                        iconSpriteRender.sprite = spriteReflectYUp;
                        break;
                    case QuadrantType.AxisY:
                        iconSpriteRender.sprite = cellIndex.row - levelGrid.originRow > 0 ? spriteReflectYDown : spriteReflectYUp;
                        break;
                    default:
                        iconSpriteRender.sprite = spriteReflectYUp;
                        break;
                }
            }
        }
    }
}
