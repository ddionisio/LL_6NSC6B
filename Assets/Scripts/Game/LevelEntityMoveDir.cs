using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

/// <summary>
/// Changes moving entity's direction
/// </summary>
public class LevelEntityMoveDir : LevelEntityPlaceable {    
    public const string parmType = "dirType";

    public enum Type {
        Up,
        Down,
        Left,
        Right
    }

    [Header("Display Dir")]
    public Transform displayDirRoot;
    public Transform ghostDirRoot;

    public Type dirType { get; private set; }

    protected override void Spawned(GenericParams parms) {
        if(parms != null) {
            if(parms.ContainsKey(parmType))
                dirType = parms.GetValue<Type>(parmType);
        }

        ApplyDirDisplay();
    }
    
    private void ApplyDirDisplay() {
        Vector3 angles;

        switch(dirType) {
            case Type.Down:
                angles = new Vector3(0f, 0f, 180f);
                break;
            case Type.Left:
                angles = new Vector3(0f, 0f, 90f);
                break;
            case Type.Right:
                angles = new Vector3(0f, 0f, -90f);
                break;
            default:
                angles = Vector3.zero;
                break;
        }

        displayDirRoot.localEulerAngles = angles;
        ghostDirRoot.localEulerAngles = angles;
    }
}
