using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add this with LevelGrid for PlayController to configure level
/// </summary>
public class LevelEntityItemGroupConfig : MonoBehaviour {
    [System.Serializable]
    public struct Data {
        public LevelEntityItemWidget template; //ensure template name is unique
        public int count;

        public string name {
            get { return template ? template.name : ""; }
        }
    }

    public Data[] items;
}
