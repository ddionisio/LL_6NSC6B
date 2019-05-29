using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
public class GameData : M8.SingletonScriptableObject<GameData> {
    [Header("Level Data")]
    public string levelSceneNamePrefix = "level";
    public M8.SceneAssetPath levelEnd;

    [Header("Quadrant Text Refs")]
    [M8.Localize]
    public string[] quadrantTextRefs; //1-4
    [M8.Localize]
    public string axisXTextRef;
    [M8.Localize]
    public string axisYTextRef;
    [M8.Localize]
    public string originTextRef;

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



    /// <summary>
    /// Load the next level based on current progress. Load end if progress is complete
    /// </summary>
    public void LoadLevelFromProgress() {
        int curProgress = LoLManager.instance.curProgress;
        if(curProgress < LoLManager.instance.progressMax) {
            var levelSceneName = levelSceneNamePrefix + curProgress.ToString();
            M8.SceneManager.instance.LoadScene(levelSceneName);
        }
        else
            levelEnd.Load();
    }
}
