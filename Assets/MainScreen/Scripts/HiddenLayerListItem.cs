using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenLayerListItem : MonoBehaviour {

	public int LayerSize = 2;

	public void OnValueChange(string nb) {
		Debug.Log("hidden item " + nb);
		int number = 1;
		//int number = System.Convert.ToInt32(nb);
		Debug.Assert(number >= 2, "You can't have less than 2 neurons in a hidden layer.");
		LayerSize = number;
	}
}
