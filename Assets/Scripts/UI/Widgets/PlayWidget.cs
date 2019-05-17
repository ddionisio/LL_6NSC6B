using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayWidget : MonoBehaviour {
    [Header("Data")]
    public float clickBusyDelay = 0.4f;

    [Header("Display")]    
    public Button button;
    public GameObject displayGO;
    public GameObject playIconGO;
    public GameObject stopIconGO;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeExit;

    public bool isBusy { get { return mRout != null; } }
    public bool isHidden { get { return displayGO ? displayGO.activeSelf : false; } }

    private Coroutine mRout;

    void OnDisable() {
        mRout = null;

        if(PlayController.isInstantiated)
            PlayController.instance.modeChangedCallback -= OnChangeMode;
    }

    void OnEnable() {
        switch(PlayController.instance.curMode) {
            case PlayController.Mode.Editing:
            case PlayController.Mode.Running:
            case PlayController.Mode.Pause:
                if(isHidden)
                    mRout = StartCoroutine(DoShow());
                else
                    RefreshDisplay(PlayController.instance.curMode);
                break;

            default:
                RefreshDisplay(PlayController.instance.curMode);
                break;
        }

        PlayController.instance.modeChangedCallback += OnChangeMode;
    }

    void Awake() {
        if(button)
            button.onClick.AddListener(OnClick);
    }

    void OnChangeMode(PlayController.Mode mode) {
        switch(mode) {
            case PlayController.Mode.Editing:
            case PlayController.Mode.Running:
            case PlayController.Mode.Pause:
                if(isHidden) {
                    if(mRout != null)
                        StopCoroutine(mRout);

                    mRout = StartCoroutine(DoShow());
                }
                else
                    RefreshDisplay(mode);
                break;

            case PlayController.Mode.None:
                if(!isHidden) {
                    if(mRout != null)
                        StopCoroutine(mRout);

                    mRout = StartCoroutine(DoHide());
                }
                else
                    RefreshDisplay(mode);
                break;

            default:
                RefreshDisplay(mode);
                break;
        }
    }

    void OnClick() {
        if(mRout != null)
            StopCoroutine(mRout);

        mRout = StartCoroutine(DoBusy());

        switch(PlayController.instance.curMode) {
            case PlayController.Mode.Editing:
                PlayController.instance.curMode = PlayController.Mode.Running;
                break;
            case PlayController.Mode.Running:
                PlayController.instance.curMode = PlayController.Mode.Editing;
                break;
        }
    }

    private void RefreshDisplay(PlayController.Mode mode) {
        bool isVisible;
        bool isInteractible;
        bool isPlay;

        switch(mode) {
            case PlayController.Mode.Running:
                isInteractible = true;
                isPlay = true;
                isVisible = true;
                break;
            case PlayController.Mode.Editing:
                isInteractible = true;
                isPlay = false;
                isVisible = true;
                break;
            case PlayController.Mode.Pause:
                isInteractible = false;
                isPlay = true;
                isVisible = true;
                break;
            default:
                isInteractible = false;
                isPlay = false;
                isVisible = false;
                break;
        }

        if(button) button.interactable = isInteractible && !isBusy;

        if(isVisible) {
            if(displayGO) displayGO.SetActive(true);

            if(playIconGO) playIconGO.SetActive(!isPlay);
            if(stopIconGO) stopIconGO.SetActive(isPlay);
        }
        else {
            if(displayGO) displayGO.SetActive(false);
        }
    }

    IEnumerator DoBusy() {
        if(button) button.interactable = false;

        yield return new WaitForSeconds(clickBusyDelay);

        mRout = null;

        RefreshDisplay(PlayController.instance.curMode);
    }

    IEnumerator DoShow() {
        if(button) button.interactable = false;

        if(animator && !string.IsNullOrEmpty(takeEnter))
            yield return animator.PlayWait(takeEnter);

        mRout = null;

        RefreshDisplay(PlayController.instance.curMode);
    }

    IEnumerator DoHide() {
        if(button) button.interactable = false;

        if(animator && !string.IsNullOrEmpty(takeExit))
            yield return animator.PlayWait(takeExit);

        mRout = null;

        RefreshDisplay(PlayController.instance.curMode);
    }
}
