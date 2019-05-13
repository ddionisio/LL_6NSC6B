using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Changes moving entity's direction
/// </summary>
public class LevelEntityDir : LevelEntity {
    public enum Dir {
        Up,
        Down,
        Left,
        Right
    }

    [Header("Display")]
    public Transform dirDisplayRoot;

    public Transform ghostDisplayRoot; //displayed during dragging
    public Color ghostValidColor = Color.white;
    public Color ghostInvalidColor = Color.white;

    public SpriteRenderer fillSpriteRender; //fill will modify height, use grid or slice mode

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeSpawn;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeDelete;
}
