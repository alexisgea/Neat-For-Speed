using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nfs.cells
{
	public class PlayerCellInput : ICellInput
	{
		private bool split;


		public float GetMoveForward ()
		{
			return Input.GetAxis ("Vertical");
		}

		public float GetTurn ()
		{
			return Input.GetAxis ("Horizontal");
		}

		public bool GetSplit ()
		{
			return Input.GetKeyDown (KeyCode.Space);
		}
	}
}