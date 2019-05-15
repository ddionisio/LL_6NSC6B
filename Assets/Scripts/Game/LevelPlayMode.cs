using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
public class LevelPlayMode : M8.SingletonBehaviour<LevelPlayMode> {
    public enum Mode {
        None,
        Edit,
        Run,
        Pause //similar to run, but moving entities will stop
    }

    public const string sceneVarName = "playMode";

    public Mode currentMode {
        get {
            if(!M8.SceneState.isInstantiated)
                return Mode.None;

            return (Mode)M8.SceneState.instance.local.GetValue(sceneVarName);
        }

        set {
            if(M8.SceneState.isInstantiated) {
                if(!mIsInit) {
                    mIsInit = true;
                    M8.SceneState.instance.local.onValueChange += OnSceneStateVarChanged;
                }

                M8.SceneState.instance.local.SetValue(sceneVarName, (int)value, false);
            }
        }
    }

    public event System.Action<Mode> modeChangedCallback;

    private bool mIsInit;

    protected override void OnInstanceDeinit() {
        if(mIsInit) {
            if(M8.SceneState.isInstantiated)
                M8.SceneState.instance.local.onValueChange -= OnSceneStateVarChanged;

            mIsInit = false;
        }
    }

    void OnSceneStateVarChanged(string name, M8.SceneState.StateValue val) {
        if(name == sceneVarName)
            modeChangedCallback?.Invoke((Mode)val.ival);
    }
}