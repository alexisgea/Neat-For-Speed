using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nfs.nets.layered;

namespace nfs.cells
{
	public class NeuralCellInput : ICellInput
	{
		/// <summary>
		/// THE Neural Net brain of this beautiful creature.
		/// </summary>
		/// <value>The neural net.</value>
		public nfs.nets.layered.Network NeuralNet {set; get;}

		private CellSensor sensor;
		private Cell cell;

		public NeuralCellInput (CellSensor sensor, Cell cell, NeuralCellInput parent = null)
		{
			this.sensor = sensor;
			this.cell = cell;
			if (parent == null) {
				NeuralNet = new nfs.nets.layered.Network (new int[] { 5, 5, 3 }, "id know");
			}
			else {
				NeuralNet = Evolution.CreateMutatedOffspring (parent.NeuralNet, "mutated", 30, true, 0.1f, true, 0.1f, 0.6f, Random.Range (0.05f, 0.1f));
			}
		}

		public float GetMoveForward ()
		{
			UpdateNetwork ();
			return NeuralNet.GetOutputValues () [0] + 1f; // +1: never go backwards
		}

		public float GetTurn ()
		{
			return NeuralNet.GetOutputValues () [1];
		}

		public bool GetSplit ()
		{
			return NeuralNet.GetOutputValues () [2] * 5 + cell.Size > 28;
		}

		private void UpdateNetwork ()
		{
			NeuralNet.PingFwd (new float[] { sensor.Evaluate (-60f), sensor.Evaluate (0f), sensor.Evaluate (60f) });
		}
	}
}