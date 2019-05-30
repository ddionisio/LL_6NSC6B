using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelTile))]
public class LevelTileInspector : Editor {
    public override void OnInspectorGUI() {
        var dat = target as LevelTile;

        base.OnInspectorGUI();

        M8.EditorExt.Utility.DrawSeparator();

        EditorGUILayout.LabelField("Grid Cell", string.Format("{0}, {1}", dat.col, dat.row));
    }
}
