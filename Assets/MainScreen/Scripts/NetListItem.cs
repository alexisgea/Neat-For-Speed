using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class NetListItem : MonoBehaviour {

	[SerializeField] private Text nicknameField;
	[SerializeField] private Text idField;
	[SerializeField] private Text scoreField;
	public int ReferenceId {private set; get;}

	public event Action<int> Selected;

	public void SetFields(string nickname, string id, string score, int referenceId) {
		nicknameField.text = nickname;
		idField.text = id;
		scoreField.text = score;

		ReferenceId = referenceId;
	}

	public void RaiseSelected() {
		if(Selected != null)
			Selected.Invoke(ReferenceId);
	}
}
