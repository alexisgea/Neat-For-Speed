using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HiddenLayerListItem : MonoBehaviour {

	public int LayerSize{set; get;}

	public static int MinLayerSize {get{return 2;}}
	public static int MaxLayerSize {get{return 7;}}

	public event Action HiddenLayerUpdated;

	private void Start() {
		ChangeLayerSize(2);
	}

	public void OnLayerSizeChanged(InputField input) {
		if(input.text == "") {
				return;
			}

		int number = System.Convert.ToInt32(input.text);
		ChangeLayerSize(number);
	}

	public void ChangeLayerSize(int number) {

		if(number < MinLayerSize) {
			Debug.LogWarning("You can't have less than 1 hidden layer.");
			return;
		}
		else if(number > MaxLayerSize) {
			Debug.LogWarning("You can't have less than 1 hidden layer.");
			return;
		}

		LayerSize = number;
		RaiseHiddenLayerUpdated();
	}

	private void RaiseHiddenLayerUpdated() {
		if(HiddenLayerUpdated != null) {
			HiddenLayerUpdated.Invoke();
		}
	}
}
