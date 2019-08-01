using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintWidget : MonoBehaviour {
    public const string sceneVarToolTipShown = "hintToolTipShown";

    [Header("Data")]
    [M8.TagSelector]
    public string tagHintRoot = "Hint";
    [M8.TagSelector]
    public string tagItemSelect = "ItemSelector";
    public bool alwaysShowHint; //if true, hints will be shown during edit mode regardless of button click
    public bool isPanelMode = true; //if true, show hints via panel

    [Header("Display")]
    public GameObject displayGO;
    public Button button;
    public HintContainerWidget hintPanelContainer;

    [Header("Tooltip")]
    public float tooltipDelay = 4f;
    public GameObject tooltipGO;
    public M8.Animator.Animate tooltipAnimator;
    [M8.Animator.TakeSelector(animatorField = "tooltipAnimator")]
    public string tooltipTakeEnter;
    [M8.Animator.TakeSelector(animatorField = "tooltipAnimator")]
    public string tooltipTakeExit;

    private bool mFirstPlayTimeIsApplied;
    private float mFirstPlayTime; //time since the first play has been hit


    private GameObject mHintRootGO;

    private LevelEntityHint[] mHintItems;

    private LevelEntityItemGroupWidget mItemSelectUI;

    private Coroutine mDragGuideRout;

    void OnDisable() {
        if(PlayController.isInstantiated)
            PlayController.instance.modeChangedCallback -= OnChangeMode;

        mDragGuideRout = null;
    }

    void OnEnable() {
        //refresh display
        OnChangeMode(PlayController.instance.curMode);

        PlayController.instance.modeChangedCallback += OnChangeMode;
    }

    void Awake() {
        button.onClick.AddListener(OnClick);

        if(alwaysShowHint)
            button.gameObject.SetActive(false);

        mHintRootGO = GameObject.FindGameObjectWithTag(tagHintRoot);

        if(mHintRootGO) {
            mHintItems = mHintRootGO.GetComponentsInChildren<LevelEntityHint>(true);
        }

        if(hintPanelContainer) {
            hintPanelContainer.Setup(mHintItems);
            hintPanelContainer.gameObject.SetActive(false);
        }
    }

    void OnChangeMode(PlayController.Mode mode) {
        if(mode == PlayController.Mode.Running) {
            if(!mFirstPlayTimeIsApplied) {
                mFirstPlayTime = Time.time;
                mFirstPlayTimeIsApplied = true;
            }
        }

        bool isActive = PlayWidget.editCounter >= GameData.instance.hintEditCount || (mFirstPlayTimeIsApplied && Time.time - mFirstPlayTime >= GameData.instance.hintEditDelay);
        bool isInteract = mode == PlayController.Mode.Editing;

        if(isActive) {
            if(displayGO) displayGO.SetActive(true);
            if(button) button.interactable = isInteract;

            //show tooltip?
            var isShowToolTip = M8.SceneState.isInstantiated ? M8.SceneState.instance.global.GetValue(sceneVarToolTipShown) == 0 : true;
            if(mode == PlayController.Mode.Editing && isShowToolTip) {
                StartCoroutine(DoShowHint());

                if(M8.SceneState.isInstantiated)
                    M8.SceneState.instance.global.SetValue(sceneVarToolTipShown, 1, false);
            }
            else {
                if(tooltipGO) tooltipGO.SetActive(false);

                StopAllCoroutines();
            }
        }
        else {
            if(displayGO) displayGO.SetActive(false);
        }
                
        if(!isPanelMode && alwaysShowHint && mode == PlayController.Mode.Editing) {
            if(mHintRootGO)
                mHintRootGO.SetActive(true);

            if(mDragGuideRout != null)
                StopCoroutine(mDragGuideRout);

            mDragGuideRout = StartCoroutine(DoShowDragGuide());
        }
        else {
            if(mHintRootGO)
                mHintRootGO.SetActive(false);

            if(mDragGuideRout != null) {
                StopCoroutine(mDragGuideRout);
                mDragGuideRout = null;
            }

            if(mItemSelectUI)
                mItemSelectUI.DragGuideHide();
        }

        if(hintPanelContainer) hintPanelContainer.gameObject.SetActive(false);
    }

    void OnClick() {
        if(isPanelMode) {
            if(hintPanelContainer && !hintPanelContainer.gameObject.activeSelf) {
                hintPanelContainer.gameObject.SetActive(true);

                if(mDragGuideRout != null)
                    StopCoroutine(mDragGuideRout);
                mDragGuideRout = StartCoroutine(DoPanelRefresh());
            }
        }
        else {
            if(mHintRootGO) {
                mHintRootGO.SetActive(!mHintRootGO.activeSelf);

                if(mHintRootGO.activeSelf) {
                    if(mDragGuideRout != null)
                        StopCoroutine(mDragGuideRout);
                    mDragGuideRout = StartCoroutine(DoShowDragGuide());
                }
            }
        }
    }

    IEnumerator DoPanelRefresh() {        
        var wait = new WaitForSeconds(0.5f);

        while(hintPanelContainer.gameObject.activeSelf) {
            hintPanelContainer.Refresh();
            yield return wait;
        }

        mDragGuideRout = null;
    }

    IEnumerator DoShowDragGuide() {
        var wait = new WaitForSeconds(0.3f);

        if(!mItemSelectUI) {
            var itemSelectGO = GameObject.FindGameObjectWithTag(tagItemSelect);
            mItemSelectUI = itemSelectGO.GetComponent<LevelEntityItemGroupWidget>();
        }

        if(mItemSelectUI) {
            var levelGrid = PlayController.instance.levelGrid;

            while(mHintRootGO.activeSelf) {
                for(int i = 0; i < mHintItems.Length; i++) {
                    var hintItm = mHintItems[i];

                    if(!CheckCellIndex(hintItm.itemNameFromType, hintItm.cellIndex)) {
                        mItemSelectUI.DragGuideShow(hintItm.itemNameFromType, hintItm.cellIndex, EvalItem);

                        while(!CheckCellIndex(hintItm.itemNameFromType, hintItm.cellIndex))
                            yield return wait;
                    }
                }

                mItemSelectUI.DragGuideHide();

                yield return wait;
            }
        }

        mDragGuideRout = null;
    }

    private bool EvalItem(LevelEntityPlaceable itm) {
        //check if this is already properly placed over a hint item
        for(int i = 0; i < mHintItems.Length; i++) {
            var hintItm = mHintItems[i];
            if(hintItm.itemNameFromType == itm.name && itm.cellIndex == hintItm.cellIndex)
                return false;
        }

        return true;
    }

    private bool CheckCellIndex(string entName, CellIndex cellIndex) {
        var ents = PlayController.instance.levelGrid.GetEntities(cellIndex);
        if(ents != null) {
            for(int i = 0; i < ents.Count; i++) {
                if(ents[i].name == entName) {
                    return true;
                }
            }
        }

        return false;
    }

    IEnumerator DoShowHint() {
        if(tooltipGO) tooltipGO.SetActive(true);

        if(tooltipAnimator && !string.IsNullOrEmpty(tooltipTakeEnter))
            yield return tooltipAnimator.PlayWait(tooltipTakeEnter);

        yield return new WaitForSeconds(tooltipDelay);

        if(tooltipAnimator && !string.IsNullOrEmpty(tooltipTakeExit))
            yield return tooltipAnimator.PlayWait(tooltipTakeExit);

        if(tooltipGO) tooltipGO.SetActive(false);
    }
}
