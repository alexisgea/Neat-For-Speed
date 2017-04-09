using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCellInput : ICellInput {

	public float GetMoveForward () {
		return Input.GetAxis ("Vertical");
	}

	public float GetTurn () {
		return Input.GetAxis ("Horizontal");
	}
}
