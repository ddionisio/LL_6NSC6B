using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LevelItemData {
    public LevelEntityItemWidget template; //ensure template name is unique
    public int count;

    public string name {
        get { return template ? template.name : ""; }
    }
}