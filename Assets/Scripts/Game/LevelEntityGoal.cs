using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntityGoal : LevelEntity {
    public enum State {
        Normal,
        Collected
    }

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeNormal;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeCollect;

    [Header("Signal Listen")]
    public M8.Signal signalListenReset;

    public State state {
        get { return mState; }
        set {
            if(mState != value) {
                mState = value;
                ApplyCurState();
            }
        }

    }

    private State mState = State.Normal;

    void OnDisable() {
        if(!Application.isPlaying)
            return;

        if(levelGrid)
            levelGrid.RemoveEntity(this);

        signalListenReset.callback -= OnSignalReset;

        if(PlayController.isInstantiated)
            PlayController.instance.modeChangedCallback -= OnModeChanged;
    }

    void OnEnable() {
        if(!Application.isPlaying)
            return;

        if(levelGrid)
            levelGrid.AddEntity(this);

        ApplyCurState();

        signalListenReset.callback += OnSignalReset;

        PlayController.instance.modeChangedCallback += OnModeChanged;
    }

    void OnSignalReset() {
        state = State.Normal;
    }

    void OnModeChanged(PlayController.Mode mode) {
        switch(mode) {
            case PlayController.Mode.Editing:
                state = State.Normal;
                break;
        }
    }

    private void ApplyCurState() {
        switch(mState) {
            case State.Normal:
                if(animator && !string.IsNullOrEmpty(takeNormal))
                    animator.Play(takeNormal);
                break;

            case State.Collected:
                if(animator && !string.IsNullOrEmpty(takeCollect))
                    animator.Play(takeCollect);
                break;
        }
    }
}
