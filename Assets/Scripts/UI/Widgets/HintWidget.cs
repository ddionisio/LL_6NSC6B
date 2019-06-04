using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintWidget : MonoBehaviour {
    public const string sceneVarToolTipShown = "hintToolTipShown";

    [Header("Data")]
    [M8.TagSelector]
    public string tagHintRoot = "Hint";

    [Header("Display")]
    public GameObject displayGO;
    public Button button;

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

    void OnDisable() {
        if(PlayController.isInstantiated)
            PlayController.instance.modeChangedCallback -= OnChangeMode;

    }

    void OnEnable() {
        //refresh display
        OnChangeMode(PlayController.instance.curMode);

        PlayController.instance.modeChangedCallback += OnChangeMode;
    }

    void Awake() {
        button.onClick.AddListener(OnClick);

        mHintRootGO = GameObject.FindGameObjectWithTag(tagHintRoot);
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

        if(mHintRootGO)
            mHintRootGO.SetActive(false);
    }

    void OnClick() {
        if(mHintRootGO)
            mHintRootGO.SetActive(!mHintRootGO.activeSelf);
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
