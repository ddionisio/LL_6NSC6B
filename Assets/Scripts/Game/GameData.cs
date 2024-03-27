using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
public class GameData : M8.SingletonScriptableObject<GameData> {
    [System.Serializable]
    public class LevelInfo {
        public M8.SceneAssetPath level;
        public int progressCount;
    }

    [Header("Level Data")]
    public LevelInfo[] levels;
	public M8.SceneAssetPath levelEnd;

    [Header("Hint Data")]
    public int hintEditCount = 5; //how many times edit mode is done before showing it
    public float hintEditDelay = 36000f; //how long before we show the hint

    [Header("Quadrant Text Refs")]
    [M8.Localize]
    public string[] quadrantTextRefs; //1-4
    [M8.Localize]
    public string axisXTextRef;
    [M8.Localize]
    public string axisYTextRef;
    [M8.Localize]
    public string originTextRef;

    public bool isToolTipShown { get; set; } = false;

    public string GetQuadrantTextRef(CellIndex cell) {
        if(PlayController.isInstantiated && PlayController.instance.levelGrid) {
            var quadrant = PlayController.instance.levelGrid.GetQuadrant(cell);
            return GetQuadrantTextRef(quadrant);
        }

        return "";
    }

    public string GetQuadrantTextRef(QuadrantType quadrantType) {
        switch(quadrantType) {
            case QuadrantType.AxisY:
                return axisYTextRef;

            case QuadrantType.AxisX:
                return axisXTextRef;

            case QuadrantType.Origin:
                return originTextRef;

            case QuadrantType.Quadrant1:
            case QuadrantType.Quadrant2:
            case QuadrantType.Quadrant3:
            case QuadrantType.Quadrant4:
                return quadrantTextRefs[(int)quadrantType - 1];
        }

        return "";
    }

	public int GetLevelIndexFromProgress() {
		var curProgress = LoLManager.instance.curProgress;

        if(curProgress > 0) {
            var prog = 0;
            for(int i = 0; i < levels.Length; i++) {
                var level = levels[i];

                if(curProgress >= prog && curProgress < prog + level.progressCount)
                    return i;

                prog += level.progressCount;
            }

            return levels.Length; //used to check if we want to load end
        }

		return 0;
    }

    public int GetLevelIndexFromCurrentScene() {
        var curScene = M8.SceneManager.instance.curScene;

        for(int i = 0; i < levels.Length; i++) {
			var level = levels[i];
            if(level.level == curScene)
                return i;
		}

        return -1;
    }

    public bool IsCurrentProgressLevelBegin() {
		var curProgress = LoLManager.instance.curProgress;

		var prog = 0;
		for(int i = 0; i < levels.Length; i++) {
			var level = levels[i];

			if(curProgress == prog)
				return true;

			prog += level.progressCount;
		}

		return false;
	}

    public void SetProgressToNextLevel() {
		var curProgress = LoLManager.instance.curProgress;

		var prog = 0;
		for(int i = 0; i < levels.Length; i++) {
			var level = levels[i];

            if(curProgress >= prog && curProgress < prog + level.progressCount) {
                curProgress = prog + level.progressCount;
                break;
            }

			prog += level.progressCount;
		}

        LoLManager.instance.ApplyProgress(curProgress);
	}

    /// <summary>
    /// Load the next level based on current progress. Load end if progress is complete
    /// </summary>
    public void LoadLevelFromProgress() {
        var levelInd = GetLevelIndexFromProgress();
        if(levelInd < levels.Length)
            levels[levelInd].level.Load();
        else
            levelEnd.Load();
    }

	protected override void OnInstanceInit() {
		if(LoLManager.isInstantiated) {
            var prog = 0;
            for(int i = 0; i < levels.Length; i++) {
                prog += levels[i].progressCount;
            }

            LoLManager.instance.progressMax = prog;
        }
	}
}
