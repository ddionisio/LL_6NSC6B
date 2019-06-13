using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalQuadrantQuestion : M8.ModalController, M8.IModalPush, M8.IModalPop, M8.IModalActive {
    [Header("Answer Data")]
    public Button[] answerButtons;
    public string answerTakeEnter = "enter";
    public string answerTakeCorrect = "correct";
    public string answerTakeWrong = "wrong";
    public float answerShowDelay = 0.15f;

    [Header("Quadrant Data")]
    public GameObject quadrantGO;
    public Text quadrantText;
    [M8.Localize]
    public string[] quadrantTextRefs;

    [Header("Coord Data")]
    public Transform coordRoot;
    public Text coordText;
    public string coordStringFormat = "X, Y = ({0}, {1})";

    [Header("Displays")]
    public GameObject nextButtonGO;

    [Header("Audio")]
    [M8.Localize]
    public string questionTextRef;
    [M8.SoundPlaylist]
    public string sfxCorrect;
    [M8.SoundPlaylist]
    public string sfxWrong;

    private M8.Animator.Animate[] mAnswerAnimators;

    private int mAnswerIndex;

    private int mQuadrantIndex;

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {
            if(!string.IsNullOrEmpty(questionTextRef))
                LoLManager.instance.SpeakText(questionTextRef);

            StartCoroutine(DoShowAnswers());
        }
    }

    void M8.IModalPop.Pop() {
        //warp player back to start
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        //initialize answers
        for(int i = 0; i < answerButtons.Length; i++) {
            answerButtons[i].interactable = false;
            mAnswerAnimators[i].ResetTake(answerTakeEnter);
        }

        mAnswerIndex = -1;

        //initialize play
        var levelGrid = PlayController.instance.levelGrid;
        var player = PlayController.instance.player;
        var playerCellIndex = player.cellIndex;

        var quadType = levelGrid.GetQuadrant(playerCellIndex);
        switch(quadType) {
            case QuadrantType.Quadrant1:
                mQuadrantIndex = 0;
                break;
            case QuadrantType.Quadrant2:
                mQuadrantIndex = 1;
                break;
            case QuadrantType.Quadrant3:
                mQuadrantIndex = 2;
                break;
            case QuadrantType.Quadrant4:
                mQuadrantIndex = 3;
                break;
            default:
                mQuadrantIndex = -1;
                break;
        }

        //setup coord display
        var cam = Camera.main;
        var coordUIPos = cam.WorldToScreenPoint(player.position);

        coordRoot.position = coordUIPos;

        coordText.text = string.Format(coordStringFormat, player.col - levelGrid.originCol, player.row - levelGrid.originRow);
        //

        //move player
        player.position = new Vector2(-1000, -1000);

        //hide gameplay
        levelGrid.entitiesRoot.gameObject.SetActive(false);
        levelGrid.obstaclesRoot.gameObject.SetActive(false);
        levelGrid.wallRoot.gameObject.SetActive(false);

        //initialize quadrant
        if(mQuadrantIndex != -1)
            quadrantText.text = M8.Localize.Get(quadrantTextRefs[mQuadrantIndex]);

        //initialize displays
        if(quadrantGO) quadrantGO.SetActive(false);
        if(nextButtonGO) nextButtonGO.SetActive(false);
    }

    void Awake() {
        mAnswerAnimators = new M8.Animator.Animate[answerButtons.Length];
        for(int i = 0; i < answerButtons.Length; i++) {
            int index = i;
            answerButtons[i].onClick.AddListener(delegate () { AnswerClickIndex(index); });
            mAnswerAnimators[i] = answerButtons[i].GetComponent<M8.Animator.Animate>();
        }
    }

    private void AnswerClickIndex(int index) {
        if(mAnswerIndex == -1) {
            for(int i = 0; i < answerButtons.Length; i++)
                answerButtons[i].interactable = false;

            mAnswerIndex = index;

            //check correct
            bool isCorrect = mAnswerIndex == mQuadrantIndex;

            //button anim
            //sound fx
            if(isCorrect) {
                mAnswerAnimators[index].Play(answerTakeCorrect);

                if(!string.IsNullOrEmpty(sfxCorrect))
                    M8.SoundPlaylist.instance.Play(sfxCorrect, false);
            }
            else {
                mAnswerAnimators[index].Play(answerTakeWrong);

                if(!string.IsNullOrEmpty(sfxWrong))
                    M8.SoundPlaylist.instance.Play(sfxWrong, false);
            }

            //show quadrant
            if(quadrantGO) quadrantGO.SetActive(true);

            if(mQuadrantIndex != -1)
                LoLManager.instance.SpeakText(quadrantTextRefs[mQuadrantIndex]);

            var levelGrid = PlayController.instance.levelGrid;
            var player = PlayController.instance.player;

            //show gameplay
            levelGrid.entitiesRoot.gameObject.SetActive(true);
            levelGrid.obstaclesRoot.gameObject.SetActive(true);
            levelGrid.wallRoot.gameObject.SetActive(true);

            //warp player
            player.WarpTo(player.defaultCellIndex);

            //next ready
            if(nextButtonGO) nextButtonGO.SetActive(true);
        }
    }

    IEnumerator DoShowAnswers() {
        var wait = new WaitForSeconds(answerShowDelay);

        for(int i = 0; i < mAnswerAnimators.Length; i++) {
            mAnswerAnimators[i].Play(answerTakeEnter);
            answerButtons[i].interactable = true;
            yield return wait;
        }
    }
}
