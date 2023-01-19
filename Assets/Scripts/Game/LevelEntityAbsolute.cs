using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;
using UnityEngine.EventSystems;

//only for siths
public class LevelEntityAbsolute : LevelEntityPlaceable {
    public const string parmAxisType = "axisType";

    [Header("Sprite Refs")]
    public Sprite spriteAxisX;
    public Sprite spriteAxisY;

    [Header("Sprite Drag Refs")]
    public Sprite spriteDragAxisX;
    public Sprite spriteDragAxisY;

    [Header("Display")]
    public SpriteRenderer iconSpriteRender;

    public AxisType axisType { get; private set; }

    public override Sprite dragIcon {
        get {
            switch(axisType) {
                case AxisType.X:
                    return spriteDragAxisX;
                case AxisType.Y:
                    return spriteDragAxisY;
            }

            return null;
        }
    }

    public static void ApplyCellDestination(PointerEventData pointerEventData, AxisType aAxisType) {
        var drag = PlayController.instance.levelGridPointer;

        var pointerCellIndex = drag ? drag.GetCellIndex(pointerEventData) : new CellIndex(-1, -1);
        ApplyCellDestination(pointerCellIndex, aAxisType);
    }

    public static void ApplyCellDestination(CellIndex pointerCellIndex, AxisType aAxisType) {
        var levelGrid = PlayController.instance.levelGrid;

        if(levelGrid) {
            bool isDestActive = false;

            if(pointerCellIndex.isValid) {
                if(levelGrid.cellDestGO) levelGrid.cellDestGO.SetActive(true);

                var srcPos = levelGrid.GetCellPosition(pointerCellIndex);

                CellIndex destCellIndex;
                switch(aAxisType) {
                    case AxisType.X:
                        destCellIndex = new CellIndex(
                            pointerCellIndex.row,
                            levelGrid.originCol + Mathf.Abs(pointerCellIndex.col - levelGrid.originCol));
                        break;
                    case AxisType.Y:
                        destCellIndex = new CellIndex(
                            levelGrid.originRow + Mathf.Abs(pointerCellIndex.row - levelGrid.originRow),
                            pointerCellIndex.col);
                        break;
                    default:
                        destCellIndex = new CellIndex(-1, -1);
                        break;
                }

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
        axisType = AxisType.None;

        if(parms != null) {
            if(parms.ContainsKey(parmAxisType))
                axisType = parms.GetValue<AxisType>(parmAxisType);
        }

        if(iconSpriteRender) {
            switch(axisType) {
                case AxisType.X:
                    iconSpriteRender.sprite = spriteAxisX;
                    break;
                case AxisType.Y:
                    iconSpriteRender.sprite = spriteAxisY;
                    break;
            }
        }
    }

    protected override void OnDragUpdate(PointerEventData ptrData) {
        ApplyCellDestination(ptrData, axisType);
    }
    protected override void OnDragInvalidate() {
        if(PlayController.instance.levelGrid.cellDestGO)
            PlayController.instance.levelGrid.cellDestGO.SetActive(false);
    }
    protected override void OnPointerEnter(PointerEventData eventData) {
        ApplyCellDestination(cellIndex, axisType);
    }
    protected override void OnPointerExit(PointerEventData eventData) {
        var levelGrid = PlayController.instance.levelGrid;
        if(levelGrid && levelGrid.cellDestGO)
            levelGrid.cellDestGO.SetActive(false);
    }
}
