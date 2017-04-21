using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nfs.cells
{
	interface ICellInput
	{
		float GetMoveForward ();
		float GetTurn ();
		bool GetSplit ();
	}
}