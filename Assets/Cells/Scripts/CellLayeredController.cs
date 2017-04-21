using UnityEngine;
using System;

namespace nfs.cells {

	// this is an example implementation of a base layered net controller

	///<summary>
	/// Implementation of a layered neural network controller.
	/// Owns an instance of a neural network and updates it (ping forward).
	///</summary>
	public class CellLayeredController : nets.layered.Controller, ICellInput {

		private CellSensor sensor;

		private CellSensor Sensor {
			get {
				if (sensor == null)
					sensor = GetComponentInChildren<CellSensor> ();
				return sensor;
			}
		}

//		public void Init (CellSensor sensor)
//		{
//			this.sensor = sensor;
//		}

		public float GetMoveForward ()
		{
			return outputValues[0];
		}

		public float GetTurn ()
		{
			return outputValues[1];
		}

		public bool GetSplit ()
		{
			return false;
		}

		/// <summary>
		/// Inits the input and output arrays.
		/// we initialise inputs and outputs array for values and names.
		/// </summary>
		protected override void InitInputAndOutputArrays() {
			// inputs and outputs values are already created from the neural net itself
			// but we want a bias neuron so we will transpit one input length and thus re-initialise it
			// output values NEEDS to be the same though, so no need to redo-it
			inputValues = new float[3];
			NeuralNet.InputsNames = new string[] {"sensor N"};
			NeuralNet.OutputsNames = new string[] {"Move", "Turn"};
		}

		/// <summary>
		/// Initialises the neural net controller.
		/// We get the references to the necessary component and suscribe to some events.
		/// </summary>
		protected override void InitialiseController() {
			
		}

		/// <summary>
		/// Updates the input values from the controller in the neural net.
		/// </summary>
		protected override void UpdateInputValues() {
//			inputValues[0] = Sensor.Evaluate ();
		}

		/// <summary>
		/// Uses the output values from the neural net in the controller.
		/// </summary>
		protected override void UseOutputValues() {
			
		}

		/// <summary>
		/// Checks the neural net alive status.
		/// </summary>
		protected override void CheckAliveStatus() {
	
		}
			
		/// <summary>
		/// Reset the specified position and orientation and other values of the controller.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="orientation">Orientation.</param>
		public override void Reset(Vector3 position, Quaternion orientation) {
			base.Reset(position, orientation);
		}

		/// <summary>
		/// Computes the fitness value of a network. 
		/// In this case from a mix distance and average speed where distance is more important than speed.
		/// </summary>
		protected override float CalculateFitness () {
			return 1f;
		}

	}
}
