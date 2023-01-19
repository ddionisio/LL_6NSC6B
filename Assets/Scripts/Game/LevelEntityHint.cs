using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityHint : LevelEntity {
    public enum HintType {
        Up,
        Down,
        Left,
        Right,
        ReflectX,
        ReflectY,
        ReflectXY,
        AbsoluteX,
        AbsoluteY
    }

    [Header("Data")]
    public HintType type;

    [Header("Sprites")]
    public Sprite[] iconSprites; //relative to HintType

    [Header("Display")]
    public SpriteRenderer iconSpriteRender;
    public float iconReflectScale = 0.85f;

    public string itemNameFromType {
        get {
            switch(type) {
                case HintType.Up:
                    return "itemMoveDirUp";
                case HintType.Down:
                    return "itemMoveDirDown";
                case HintType.Left:
                    return "itemMoveDirLeft";
                case HintType.Right:
                    return "itemMoveDirRight";
                case HintType.ReflectX:
                    return "itemReflectX";
                case HintType.ReflectY:
                    return "itemReflectY";
                case HintType.ReflectXY:
                    return "itemReflectXY";
                case HintType.AbsoluteX:
                    return "itemAbsoluteX";
                case HintType.AbsoluteY:
                    return "itemAbsoluteY";
                default:
                    return "";
            }
        }
    }

    public float GetIconRotation() {
        switch(type) {
            case HintType.Down:
                return 180f;
            case HintType.Left:
                return 90f;
            case HintType.Right:
                return -90f;

            default:
                return 0f;
        }
    }

    public float GetIconScale() {
        switch(type) {
            case HintType.ReflectX:
            case HintType.ReflectY:
            case HintType.ReflectXY:
                return iconReflectScale;
            default:
                return 1f;
        }
    }

    public Sprite GetIconSprite() {
        int sprInd = (int)type;
        if(sprInd < iconSprites.Length)
            return iconSprites[sprInd];

        return iconSpriteRender ? iconSpriteRender.sprite : null;
    }

    void OnEnable() {
        RefreshCellIndex();
    }
}
