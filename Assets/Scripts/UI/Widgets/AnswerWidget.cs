using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerWidget : MonoBehaviour {
    [Header("Data")]
    public int index;

    [Header("Display")]
    public Button button;
    public GameObject wrongGO;
    public GameObject correctGO;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector]
    public int takeError = -1;
	[M8.Animator.TakeSelector]
	public int takeCorrect = -1;

    [Header("SFX")]
    [M8.SoundPlaylist]
    public string sfxCorrect;
	[M8.SoundPlaylist]
	public string sfxError;

    public bool active { get { return gameObject.activeSelf; } set { gameObject.SetActive(value); } }
    public bool interactable { get { return button ? button.interactable : false; } set { if(button) button.interactable = value; } }

    public event System.Action<AnswerWidget> clickCallback;

    public void Error() {
        if(takeError != -1)
            animator.Play(takeError);

        if(!string.IsNullOrEmpty(sfxError))
            M8.SoundPlaylist.instance.Play(sfxError, false);

		if(wrongGO) 
            wrongGO.SetActive(true);

		if(correctGO)
			correctGO.SetActive(false);

		interactable = false;
	}

    public void Correct() {
		if(takeCorrect != -1)
			animator.Play(takeCorrect);

		if(!string.IsNullOrEmpty(sfxCorrect))
			M8.SoundPlaylist.instance.Play(sfxCorrect, false);

		if(correctGO)
			correctGO.SetActive(true);

		if(wrongGO)
			wrongGO.SetActive(false);

		interactable = false;
	}

	void OnEnable() {
        if(button)
            button.interactable = true;

        if(wrongGO) 
            wrongGO.SetActive(false);

		if(correctGO) 
            correctGO.SetActive(false);
	}

	void Awake() {
        if(button)
            button.onClick.AddListener(OnClick);
	}

    void OnClick() {
        clickCallback?.Invoke(this);
	}
}
