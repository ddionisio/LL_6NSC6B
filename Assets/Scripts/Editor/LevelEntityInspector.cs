using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelEntity), true)]
public class LevelEntityInspector : Editor {
    public override void OnInspectorGUI() {
        var dat = target as LevelEntity;

        base.OnInspectorGUI();

        M8.EditorExt.Utility.DrawSeparator();

        EditorGUILayout.LabelField("Grid Cell", string.Format("{0}, {1}", dat.col, dat.row));
    }
}