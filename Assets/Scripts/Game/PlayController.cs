using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayController : GameModeController<PlayController> {
    public enum Mode {
        None,
        Editing,
        Running,
        Pause //similar to run, but moving entities will stop
    }

    [Header("Look Up")]
    [M8.TagSelector]
    public string tagLevel = "Level";
    [M8.TagSelector]
    public string tagItemSelectorUI = "ItemSelector";

    [Header("Data")]
    public LevelItemData[] items;

    public Mode curMode {
        get { return mCurMode; }
        set {
            if(mCurMode != value) {
                mCurMode = value;

                modeChangedCallback?.Invoke(mCurMode);
            }
        }
    }

    public event System.Action<Mode> modeChangedCallback;

    private Mode mCurMode = Mode.None;

    private LevelGrid mLevelGrid;

    private LevelEntityItemGroupWidget mItemSelectorUI;

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        //grab level
        var levelGO = GameObject.FindGameObjectWithTag(tagLevel);
        if(levelGO)
            mLevelGrid = levelGO.GetComponent<LevelGrid>();

        //initialize item placement group
        var itemSelectorGO = GameObject.FindGameObjectWithTag(tagItemSelectorUI);
        if(itemSelectorGO) {
            mItemSelectorUI = itemSelectorGO.GetComponent<LevelEntityItemGroupWidget>();
            if(mItemSelectorUI) {
                mItemSelectorUI.Init(items, mLevelGrid);
            }
        }
    }

    protected override IEnumerator Start() {
        yield return base.Start();
    }
}
