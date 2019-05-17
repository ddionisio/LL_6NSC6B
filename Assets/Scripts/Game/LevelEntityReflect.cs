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

    [Header("Display")]
    public SpriteRenderer iconSpriteRender;

    public bool reflectX { get; private set; }
    public bool reflectY { get; private set; }

    protected override void Spawned(GenericParams parms) {
        reflectX = reflectY = false;

        if(parms != null) {
            if(parms.ContainsKey(parmReflectX))
                reflectX = parms.GetValue<bool>(parmReflectX);

            if(parms.ContainsKey(parmReflectY))
                reflectY = parms.GetValue<bool>(parmReflectY);
        }

        //determine display
        if(iconSpriteRender) {
            var quadrant = levelGrid.GetQuadrant(cellIndex);
            
            if(reflectX && reflectY) {
                //diagonal
                switch(quadrant) {
                    case 1:
                        iconSpriteRender.sprite = spriteReflectXYLL;
                        break;
                    case 2:
                        iconSpriteRender.sprite = spriteReflectXYLR;
                        break;
                    case 3:
                        iconSpriteRender.sprite = spriteReflectXYUR;
                        break;
                    case 4:
                        iconSpriteRender.sprite = spriteReflectXYUL;
                        break;
                    case -1:
                        iconSpriteRender.sprite = cellIndex.col - levelGrid.originCol > 0 ? spriteReflectXLeft : spriteReflectXRight;
                        break;
                    case -2:
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
                    case 1:
                    case 4:
                        iconSpriteRender.sprite = spriteReflectXLeft;
                        break;
                    case 2:
                    case 3:
                        iconSpriteRender.sprite = spriteReflectXRight;
                        break;
                    case -1:
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
                    case 1:
                    case 2:
                        iconSpriteRender.sprite = spriteReflectYDown;
                        break;
                    case 3:
                    case 4:
                        iconSpriteRender.sprite = spriteReflectYUp;
                        break;
                    case -2:
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
