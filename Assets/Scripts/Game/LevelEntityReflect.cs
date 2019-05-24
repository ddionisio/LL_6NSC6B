using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

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
