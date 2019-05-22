using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityItemGroupWidget : MonoBehaviour {
    public enum State {
        Hidden,
        Shown
    }

    [Header("Data")]
    public string poolGroup = "levelEntityPlacer";

    [Header("Display")]
    public GameObject panelGO;

    public Transform itemsRoot; //placement for items

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeExit;

    public State curState {
        get { return mCurState; }
        private set {
            if(mCurState != value) {
                mCurState = value;

                if(mRout != null)
                    StopCoroutine(mRout);

                switch(mCurState) {
                    case State.Hidden:
                        //cancel item dragging
                        for(int i = 0; i < mItems.Count; i++) {
                            if(mItems[i] && mItems[i].isDragging)
                                mItems[i].DragInvalidate();
                        }

                        mRout = StartCoroutine(DoHide());
                        break;

                    case State.Shown:
                        mRout = StartCoroutine(DoShow());
                        break;
                }
            }
        }
    }

    private State mCurState;

    private M8.PoolController mPool;

    private const int itemsCapacity = 8;
    private M8.CacheList<LevelEntityItemWidget> mItems = new M8.CacheList<LevelEntityItemWidget>(itemsCapacity);

    private Coroutine mRout;

    public void Clear() {
        for(int i = 0; i < mItems.Count; i++) {
            var itm = mItems[i];
            if(itm && itm.gameObject.activeSelf) {
                itm.ReleaseAll();
                itm.RefreshDisplay();
            }
        }
    }
        
    public void Init(LevelItemData[] itemConfigs) {
        //init pool
        if(!mPool)
            mPool = M8.PoolController.CreatePool(poolGroup);

        //hide current items
        for(int i = 0; i < mItems.Count; i++) {
            if(mItems[i])
                mItems[i].gameObject.SetActive(false);
        }

        for(int i = 0; i < itemConfigs.Length; i++) {
            var itemConfig = itemConfigs[i];
            LevelEntityItemWidget item = null;

            //check if it already exists via name match
            for(int j = 0; j < mItems.Count; j++) {
                if(mItems[j] && mItems[j].name == itemConfig.name) {
                    item = mItems[j];                    
                    break;
                }
            }

            //instantiate item
            if(!item) {
                item = Instantiate(itemConfig.template, itemsRoot);
                item.name = itemConfig.name;

                mItems.Add(item);
            }

            item.Init(mPool, itemConfig.count);

            item.gameObject.SetActive(true);
            item.transform.SetSiblingIndex(i);
        }
    }

    void OnDisable() {
        mRout = null;
    }

    void OnDestroy() {
        if(PlayController.isInstantiated)
            PlayController.instance.modeChangedCallback -= OnModeChanged;
    }

    void Awake() {
        //init to hidden
        mCurState = State.Hidden;

        if(panelGO) panelGO.SetActive(false);

        if(animator && !string.IsNullOrEmpty(takeEnter)) animator.ResetTake(takeEnter);

        PlayController.instance.modeChangedCallback += OnModeChanged;
    }

    void OnModeChanged(PlayController.Mode mode) {
        switch(mode) {
            case PlayController.Mode.Editing:
                curState = State.Shown;
                break;

            case PlayController.Mode.None:
            case PlayController.Mode.Pause:
            case PlayController.Mode.Running:
                curState = State.Hidden;
                break;
        }
    }

    IEnumerator DoShow() {
        if(panelGO) panelGO.SetActive(true);

        if(animator && !string.IsNullOrEmpty(takeEnter))
            yield return animator.PlayWait(takeEnter);

        mRout = null;
    }

    IEnumerator DoHide() {
        if(animator && !string.IsNullOrEmpty(takeExit))
            yield return animator.PlayWait(takeExit);

        if(panelGO) panelGO.SetActive(false);

        mRout = null;
    }
}
