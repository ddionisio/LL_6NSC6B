using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[CustomEditor(typeof(LevelEntityHint))]
public class LevelEntityHintInspector : LevelEntityInspector {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
                
        if(GUI.changed) {
            var dat = target as LevelEntityHint;

            if(dat.iconSpriteRender) {
                var spr = dat.GetIconSprite();
                var rotZ = dat.GetIconRotation();
                var rot = Quaternion.Euler(0f, 0f, rotZ);
                var scale = dat.GetIconScale();

                if(dat.iconSpriteRender.sprite != spr) {
                    Undo.RecordObject(dat.iconSpriteRender, "Set Icon");
                    dat.iconSpriteRender.sprite = spr;
                }

                if(dat.iconSpriteRender.transform.localRotation != rot || dat.iconSpriteRender.transform.localScale.x != scale) {
                    Undo.RecordObject(dat.iconSpriteRender, "Set Icon Rotation");
                    dat.iconSpriteRender.transform.localRotation = rot;
                    dat.iconSpriteRender.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }
        }
    }
}