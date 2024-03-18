using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class ModalReflectQuestion : M8.ModalController, M8.IModalPush, M8.IModalPop, M8.IModalActive {
    public const string parmReflectIndex = "index";

    public enum ReflectType {
        None,
        X,
        Y,
        XY
    }

    [System.Serializable]
    public struct AnswerData {
        public GameObject rootGO;
        public Button button;
        public TMP_Text text;
        public M8.Animator.Animate animator;

        public void Init(int cellX, int cellY, ReflectType reflectType, string takeReset, string stringFormat) {
            int ansX, ansY;

            switch(reflectType) {
                case ReflectType.X:
                    ansX = -cellX; ansY = cellY;
                    break;
                case ReflectType.Y:
                    ansX = cellX; ansY = -cellY;
                    break;
                case ReflectType.XY:
                    ansX = -cellX; ansY = -cellY;
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

    [Header("Coord Data")]
    public Transform coordSourceRoot;
    public TMP_Text coordSourceText;
    public Transform coordDestRoot;
    public TMP_Text coordDestText;
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
        int parmIndex = -1;

        if(parms != null) {
            if(parms.ContainsKey(parmReflectIndex))
                parmIndex = parms.GetValue<int>(parmReflectIndex);
        }

        //initialize play
        var levelGrid = PlayController.instance.levelGrid;
        var player = PlayController.instance.player;

        int playerLocalX = player.col - levelGrid.originCol, playerLocalY = player.row - levelGrid.originRow;

        //initialize answers
        for(int i = 0; i < answers.Length; i++)
            answers[i].Init(playerLocalX, playerLocalY, (ReflectType)i, answerTakeEnter, answerStringFormat);

        //shuffle answers
        M8.ArrayUtil.Shuffle(mChoiceIndices);

        for(int i = 0; i < answers.Length; i++) {
            answers[mChoiceIndices[i]].rootGO.transform.SetAsLastSibling();
        }

        mAnswerIndex = -1;

        //setup reflection
        CellIndex sourceCellIndex = new CellIndex(playerLocalY, playerLocalX);
        Vector2 sourcePosition = player.position;

        var reflectType = parmIndex != -1 ? (ReflectType)parmIndex : ReflectType.XY;
        switch(reflectType) {
            case ReflectType.X:
                sourceCellIndex.col = -sourceCellIndex.col;

                sourcePosition.x = -sourcePosition.x;
                break;
            case ReflectType.Y:
                sourceCellIndex.row = -sourceCellIndex.row;

                sourcePosition.y = -sourcePosition.y;
                break;
            case ReflectType.XY:
                sourceCellIndex.col = -sourceCellIndex.col;
                sourceCellIndex.row = -sourceCellIndex.row;

                sourcePosition.x = -sourcePosition.x;
                sourcePosition.y = -sourcePosition.y;
                break;
        }

        //setup coord display
        var cam = Camera.main;

        var coordUIPos = cam.WorldToScreenPoint(player.position);
        coordDestRoot.position = coordUIPos;
        coordDestText.text = string.Format(coordStringFormat, "??", "??");

        coordUIPos = cam.WorldToScreenPoint(sourcePosition);
        coordSourceRoot.position = coordUIPos;
        coordSourceText.text = string.Format(coordStringFormat, sourceCellIndex.col, sourceCellIndex.row);
        //

        //move player
        player.position = new Vector2(-1000, -1000);

        //hide gameplay
        levelGrid.entitiesRoot.gameObject.SetActive(false);
        levelGrid.obstaclesRoot.gameObject.SetActive(false);
        levelGrid.wallRoot.gameObject.SetActive(false);

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
            bool isCorrect = mAnswerIndex == 0; //first one is going to always contain the non-reflected player position

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
            coordDestText.text = string.Format(coordStringFormat, player.defaultCellIndex.col - levelGrid.originCol, player.defaultCellIndex.row - levelGrid.originRow);

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
