﻿using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

//only for siths
public class LevelEntityAbsolute : LevelEntityPlaceable {
    public const string parmAxisType = "axisType";

    [Header("Sprite Refs")]
    public Sprite spriteAxisX;
    public Sprite spriteAxisY;

    [Header("Display")]
    public SpriteRenderer iconSpriteRender;

    public AxisType axisType { get; private set; }

    protected override void Spawned(GenericParams parms) {
        axisType = AxisType.None;

        if(parms != null) {
            if(parms.ContainsKey(parmAxisType))
                axisType = parms.GetValue<AxisType>(parmAxisType);
        }

        if(iconSpriteRender) {
            switch(axisType) {
                case AxisType.X:
                    iconSpriteRender.sprite = spriteAxisX;
                    break;
                case AxisType.Y:
                    iconSpriteRender.sprite = spriteAxisY;
                    break;
            }
        }
    }
}