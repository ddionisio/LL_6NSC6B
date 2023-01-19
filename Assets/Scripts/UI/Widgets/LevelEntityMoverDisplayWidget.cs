using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelEntityMoverDisplayWidget : MonoBehaviour {
    [Header("Data")]
    public LevelEntityMover entityMover;

    [Header("Display")]
    public Text cellText;
    public string cellTextFormat = "(x, y) = ({0}, {1})";

    void OnDisable() {
        if(entityMover)
            entityMover.moveUpdateCallback -= OnMoveUpdate;
    }

    void OnEnable() {
        OnMoveUpdate();

        if(entityMover)
            entityMover.moveUpdateCallback += OnMoveUpdate;
    }

    void OnMoveUpdate() {
        if(cellText)
            cellText.text = string.Format(cellTextFormat, entityMover.cellIndex.col - entityMover.levelGrid.originCol, entityMover.cellIndex.row - entityMover.levelGrid.originRow);
    }
}
