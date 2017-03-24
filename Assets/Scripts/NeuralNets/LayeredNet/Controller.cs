using UnityEngine;
using System;

namespace nfs.nets.layered {

	/// <summary>
	/// Base implementation for a neural net controller.
	/// </summary>
	public abstract class Controller : MonoBehaviour {

		// THE Neural Network
		public Network NeuralNet {set; get;}

		// array to send an receive the data to the network
		protected float[] inputValues;
		protected float[] outputValues;

		public event Action <Controller> Death;

		// start
		private void Start() {
			
			InitialiseController ();
		}

		// update
		private void Update() {

			InitInputAndOutputArrays();
			UpdateInputValues ();

			if(NeuralNet != null) {
				outputValues = NeuralNet.PingFwd(inputValues);
			}

			UseOutputValues ();
			CheckAliveStatus ();
		}

		protected abstract void InitInputAndOutputArrays();

		/// <summary>
		/// Initialises the neural net controller.
		/// </summary>
		protected abstract void InitialiseController();

		/// <summary>
		/// Updates the input values from the controller in the neural net.
		/// </summary>
		protected abstract void UpdateInputValues();
		
		/// <summary>
		/// Uses the output values from the neural net in the controller.
		/// </summary>
		protected abstract void UseOutputValues();

		/// <summary>
		/// Checks the neural net alive status.
		/// </summary>
		protected abstract void CheckAliveStatus();

		public virtual void Kill() {
			NeuralNet.FitnessScore = CalculateFitness();
			RaiseDeathEvent();
		}

		public virtual void Reset(Vector3 position, Quaternion orientation) {
			transform.position = position;
			transform.rotation = orientation;
		}

		/// Computes the fitness value of a network. 
		/// In this case from a mix distance and average speed where distance is more important than speed.
		/// </summary>
        protected abstract float CalculateFitness ();

		/// <summary>
		/// If the network is dead, send a signal to whoever is listening.
		/// </summary>
		protected void RaiseDeathEvent() {
			if (Death != null) {
				Death.Invoke(this);
			}
		}


	}
}