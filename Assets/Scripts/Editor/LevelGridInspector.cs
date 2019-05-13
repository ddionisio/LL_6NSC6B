using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGrid))]
public class LevelGridInspector : Editor {

    BoxCollider2D mBoxColl;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(GUI.changed) {
            var dat = target as LevelGrid;

            //update box collider
            if(!mBoxColl)
                mBoxColl = dat.GetComponent<BoxCollider2D>();
            if(mBoxColl) {
                mBoxColl.offset = Vector2.zero;
                mBoxColl.size = new Vector2(dat.cellSize.x * dat.numCol, dat.cellSize.y * dat.numRow);
            }
        }
    }
}
