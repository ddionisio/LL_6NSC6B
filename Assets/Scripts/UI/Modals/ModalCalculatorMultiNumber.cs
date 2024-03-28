using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

/// <summary>
/// Addon for ModalCalculator to display multiple input
/// </summary>
public class ModalCalculatorMultiNumber : MonoBehaviour, M8.IModalPush, M8.IModalPop {
	public const string parmInitValues = "initVals"; //int[], float[], etc.
	public const string parmInitIndex = "initInd"; //int

	[System.Serializable]
	public class NumericData {
		public GameObject activeGO;
		public TMP_Text numericLabel;
		public M8.UI.Graphics.ColorGroup colorGroup;

		public float currentValue { 
			get { return mCurVal; }
			set {
				if(mCurVal != value)
					ApplyValue(value);
			}
		}

		public Color inactiveColor { get { return colorGroup ? colorGroup.applyColor : Color.white; } set { if(colorGroup) colorGroup.applyColor = value; } }

		private float mCurVal;

		public void ApplyValue(float val) {
			mCurVal = val;

			if(numericLabel) {
				var iVal = Mathf.FloorToInt(mCurVal);
				numericLabel.text = iVal.ToString();
			}
		}

		public void SetActive(bool isActive) {
			if(isActive) {
				if(colorGroup) colorGroup.Revert();
				if(activeGO) activeGO.SetActive(true);
			}
			else {
				if(colorGroup) colorGroup.ApplyColor();
				if(activeGO) activeGO.SetActive(false);
			}
		}
	}

	[Header("Display")]
	public NumericData[] numericDisplays;
	public Color numericInactiveColor = Color.white; //for color group

	[Header("Signal Invoke")]
	public SignalFloatArray signalInvokeSubmit;

	public int currentIndex { 
		get { return mCurIndex; }
		set {
			var val = Mathf.Clamp(value, 0, numericDisplays.Length - 1);
			if(mCurIndex != val) {
				if(mCurIndex >= 0 && mCurIndex < numericDisplays.Length)
					numericDisplays[mCurIndex].SetActive(false);

				mCurIndex = val;

				numericDisplays[mCurIndex].SetActive(true);

				mModalCalculator.SetCurrentValue(numericDisplays[mCurIndex].currentValue);
			}
		}
	}

	private int mCurIndex;
	private float[] mValues; //used for submit
	private ModalCalculator mModalCalculator;

	public void Submit() {
		if(signalInvokeSubmit)
			signalInvokeSubmit.Invoke(mValues);
	}

	public void Previous() {
		if(currentIndex <= 0)
			currentIndex = numericDisplays.Length - 1;
		else
			currentIndex--;
	}

	public void Next() {
		if(currentIndex >= numericDisplays.Length - 1)
			currentIndex = 0;
		else
			currentIndex++;
	}

	void M8.IModalPop.Pop() {
		if(mModalCalculator) {
			if(mModalCalculator.signalValueUpdate)
				mModalCalculator.signalValueUpdate.callback -= OnValueChanged;
		}
	}

	void M8.IModalPush.Push(M8.GenericParams parms) {
		if(!mModalCalculator)
			mModalCalculator = GetComponent<ModalCalculator>();

		if(mValues == null)
			mValues = new float[numericDisplays.Length];

		//set all displays inactive
		for(int i = 0; i < numericDisplays.Length; i++) {
			var numericDisplay = numericDisplays[i];

			numericDisplay.inactiveColor = numericInactiveColor;
			numericDisplays[i].SetActive(false);
		}

		mCurIndex = -1;

		if(parms != null) {
			if(parms.ContainsKey(parmInitValues)) {
				var obj = parms.GetValue<object>(parmInitValues);
				if(obj is int[]) {
					var iVals = (int[])obj;
					var count = Mathf.Min(iVals.Length, numericDisplays.Length);
					for(int i = 0; i < count; i++)
						numericDisplays[i].ApplyValue(mValues[i] = iVals[i]);
				}
				else if(obj is float[]) {
					var fVals = (float[])obj;
					var count = Mathf.Min(fVals.Length, numericDisplays.Length);
					for(int i = 0; i < count; i++)
						numericDisplays[i].ApplyValue(mValues[i] = fVals[i]);
				}
				else if(obj is double[]) {
					var dVals = (double[])obj;
					var count = Mathf.Min(dVals.Length, numericDisplays.Length);
					for(int i = 0; i < count; i++)
						numericDisplays[i].ApplyValue(mValues[i] = (float)dVals[i]);
				}
			}

			if(parms.ContainsKey(parmInitIndex))
				currentIndex = parms.GetValue<int>(parmInitIndex);
		}

		if(mModalCalculator) {
			if(mModalCalculator.signalValueUpdate)
				mModalCalculator.signalValueUpdate.callback += OnValueChanged;
		}
	}

	void OnValueChanged(float val) {
		if(mCurIndex >= 0 && mCurIndex < numericDisplays.Length)
			numericDisplays[mCurIndex].currentValue = mValues[mCurIndex] = val;
	}
}
