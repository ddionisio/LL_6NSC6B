using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ModalDeploymentCoord : M8.ModalController, M8.IModalPush, M8.IModalPop, M8.IModalActive {
	[Header("Data")]
	public string modalCoordNumpad = "coordNumpad";

	[Header("Cell Highlight Data")]
	public float cellHighlightMoveDelay = 0.3f;

	[Header("Coord Data")]
	public Transform coordRoot;
	public TMP_Text coordText;
	public string coordStringFormat = "({0},  {1})";

	void M8.IModalActive.SetActive(bool aActive) {

	}

	void M8.IModalPop.Pop() {
	}

	void M8.IModalPush.Push(M8.GenericParams parms) {

	}

	//levelgrid.CellHighlightShow
}
