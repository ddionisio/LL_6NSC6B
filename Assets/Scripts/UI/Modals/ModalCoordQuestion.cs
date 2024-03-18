using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class ModalCoordQuestion : M8.ModalController, M8.IModalPush, M8.IModalPop, M8.IModalActive {
    [System.Serializable]
    public struct AnswerData {
        public GameObject rootGO;
        public Button button;
        public TMP_Text text;
        public M8.Animator.Animate animator;

        public void Init(int cellX, int cellY, QuadrantType quadrant, string takeReset, string stringFormat) {
            int ansX, ansY;

            switch(quadrant) {
                case QuadrantType.Quadrant1:
                    ansX = Mathf.Abs(cellX); ansY = Mathf.Abs(cellY);
                    break;
                case QuadrantType.Quadrant2:
                    ansX = -Mathf.Abs(cellX); ansY = Mathf.Abs(cellY);
                    break;
                case QuadrantType.Quadrant3:
                    ansX = -Mathf.Abs(cellX); ansY = -Mathf.Abs(cellY);
                    break;
                case QuadrantType.Quadrant4:
                    ansX = Mathf.Abs(cellX); ansY = -Mathf.Abs(cellY);
                    break;
                default:
                    ansX = cellX; ansY = cellY;
                    break;
            }

            text.text = string.Format(stringFormat, ansX, ansY);

            button.interactable = false;
            animator.ResetTake(takeReset);
            rootGO.SetActive(false);
        }

        public void Show(string enterTake) {
            rootGO.SetActive(true);
            animator.Play(enterTake);
            button.interactable = true;
        }
    }

    [Header("Answer Data")]
    public AnswerData[] answers;
    public string answerStringFormat = "({0}, {1})";
    public string answerTakeEnter = "enter";
    public string answerTakeCorrect = "correct";
    public string answerTakeWrong = "wrong";
    public float answerShowDelay = 0.15f;

    [Header("Quadrant Data")]
    public TMP_Text quadrantText;

    [Header("Coord Data")]
    public Transform coordRoot;
    public TMP_Text coordText;
    public string coordStringFormat = "(x = {0},  y = {1})";

    [Header("Displays")]
    public GameObject nextButtonGO;

    [Header("Audio")]
    [M8.Localize]
    public string questionTextRef;
    [M8.SoundPlaylist]
    public string sfxCorrect;
    [M8.SoundPlaylist]
    public string sfxWrong;

    private int[] mChoiceIndices;
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
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
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

        int playerLocalX = player.col - levelGrid.originCol, playerLocalY = player.row - levelGrid.originRow;

        //initialize answers
        for(int i = 0; i < answers.Length; i++)
            answers[i].Init(playerLocalX, playerLocalY, (QuadrantType)(i + 1), answerTakeEnter, answerStringFormat);

        //shuffle answers
        M8.ArrayUtil.Shuffle(mChoiceIndices);

        for(int i = 0; i < answers.Length; i++) {
            answers[mChoiceIndices[i]].rootGO.transform.SetAsLastSibling();
        }

        mAnswerIndex = -1;

        //setup coord display
        var cam = Camera.main;
        var coordUIPos = cam.WorldToScreenPoint(player.position);

        coordRoot.position = coordUIPos;

        coordText.text = string.Format(coordStringFormat, "??", "??");
        //

        //move player
        player.position = new Vector2(-1000, -1000);

        //hide gameplay
        levelGrid.entitiesRoot.gameObject.SetActive(false);
        levelGrid.obstaclesRoot.gameObject.SetActive(false);
        levelGrid.wallRoot.gameObject.SetActive(false);

        //initialize quadrant
        if(mQuadrantIndex != -1)
            quadrantText.text = M8.Localize.Get(GameData.instance.GetQuadrantTextRef(quadType));

        //initialize displays
        if(nextButtonGO) nextButtonGO.SetActive(false);
    }

    void Awake() {
        mChoiceIndices = new int[answers.Length];

        for(int i = 0; i < answers.Length; i++) {
            mChoiceIndices[i] = i;
            int index = i;
            answers[i].button.onClick.AddListener(delegate () { AnswerClickIndex(index); });
        }
    }

    private void AnswerClickIndex(int index) {
        if(mAnswerIndex == -1) {
            for(int i = 0; i < answers.Length; i++)
                answers[i].button.interactable = false;

            mAnswerIndex = index;

            //check correct
            bool isCorrect = mAnswerIndex == mQuadrantIndex;

            //button anim
            //sound fx
            if(isCorrect) {
                answers[index].animator.Play(answerTakeCorrect);

                if(!string.IsNullOrEmpty(sfxCorrect))
                    M8.SoundPlaylist.instance.Play(sfxCorrect, false);
            }
            else {
                answers[index].animator.Play(answerTakeWrong);

                if(!string.IsNullOrEmpty(sfxWrong))
                    M8.SoundPlaylist.instance.Play(sfxWrong, false);
            }

            var levelGrid = PlayController.instance.levelGrid;
            var player = PlayController.instance.player;

            //reveal coordinates
            coordText.text = string.Format(coordStringFormat, player.defaultCellIndex.col - levelGrid.originCol, player.defaultCellIndex.row - levelGrid.originRow);

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

        for(int i = 0; i < answers.Length; i++) {
            answers[mChoiceIndices[i]].Show(answerTakeEnter);
            yield return wait;
        }
    }
}
