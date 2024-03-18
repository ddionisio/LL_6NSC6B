// (c) Eric Vander Wal, 2017 All rights reserved.
// Custom Action by DumbGameDev
// www.dumbgamedev.com

using UnityEngine;
using TMPro;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("TextMesh Pro UGUI Advanced")]
    [Tooltip("set Text Mesh Pro spacing options UGUI.")]
    public class setTextmeshProUGUISpacingOptions : FsmStateAction
    {
        [RequiredField]
        [CheckForComponent(typeof(TextMeshProUGUI))]
        [Tooltip("Textmesh Pro component is required.")]
        public FsmOwnerDefault gameObject;

        [Tooltip("Character spacing.")]
        public FsmFloat character;

        [Tooltip("Word spacing.")]
        public FsmFloat word;

        [Tooltip("Line spacing.")]
        public FsmFloat line;

        [Tooltip("Paragraph spacing.")]
        public FsmFloat paragraph;

        [Tooltip("Check this box to preform this action every frame.")]
        public FsmBool everyFrame;

        TextMeshProUGUI meshproScript;

        public override void Reset()
        {
            gameObject = null;
            word = null;
            character = null;
            line = null;
            paragraph = null;
            everyFrame = false;
        }

        public override void OnEnter()
        {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            meshproScript = go.GetComponent<TextMeshProUGUI>();

            DoMeshChange();


            if (!everyFrame.Value)
            {
                Finish();
            }
        }

        public override void OnUpdate()
        {
            if (everyFrame.Value)
            {
                DoMeshChange();
            }
        }

        void DoMeshChange()
        {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (go == null)
            {
                return;
            }

            if (meshproScript == null)
            {
                Debug.LogError("No textmesh pro ugui component was found on " + go);
                return;
            }

            meshproScript.wordSpacing = word.Value;
            meshproScript.characterSpacing = character.Value;
            meshproScript.lineSpacing = line.Value;
            meshproScript.paragraphSpacing = paragraph.Value;
        }
    }
}