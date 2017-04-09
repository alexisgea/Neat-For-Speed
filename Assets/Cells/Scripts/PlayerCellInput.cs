using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nfs.cells
{
	public class PlayerCellInput : ICellInput
	{
		public float GetMoveForward ()
		{
			return Input.GetAxis ("Vertical");
		}

		public float GetTurn ()
		{
			return Input.GetAxis ("Horizontal");
		}
	}
}