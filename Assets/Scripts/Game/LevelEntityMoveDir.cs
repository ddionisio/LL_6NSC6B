using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

/// <summary>
/// Changes moving entity's direction
/// </summary>
public class LevelEntityMoveDir : LevelEntityPlaceable {    
    public const string parmType = "dirType";

    [Header("Display Dir")]
    public Sprite dragIconSprite;
    public Transform displayDirRoot;

    public MoveDir dirType { get; private set; }

    public override Sprite dragIcon { get { return dragIconSprite; } }
    public override float dragIconRotate {
        get {
            switch(dirType) {
                case MoveDir.Down:
                    return 180f;
                case MoveDir.Left:
                    return 90f;
                case MoveDir.Right:
                    return -90f;
                default:
                    return 0f;
            }
        }
    }

    protected override void Spawned(GenericParams parms) {
        if(parms != null) {
            if(parms.ContainsKey(parmType))
                dirType = parms.GetValue<MoveDir>(parmType);
        }

        ApplyDirDisplay();
    }
    
    private void ApplyDirDisplay() {
        Vector3 angles = new Vector3(0f, 0f, dragIconRotate);

        displayDirRoot.localEulerAngles = angles;
    }
}
