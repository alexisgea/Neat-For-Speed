using UnityEngine;
using System;

namespace nfs.nets.layered {

	/// <summary>
	/// Base class for a neural net controller to be inherited.
	/// The input and output initialisation must be overriden but the rest is obtional.
	/// You can find an example implementaiton in the car simulation.
	/// </summary>
	public abstract class Controller : MonoBehaviour {

		/// <summary>
		/// THE Neural Net brain of this beautiful creature.
		/// </summary>
		/// <value>The neural net.</value>
		public Network NeuralNet {set; get;}

		// array to send an receive the data to the network
		protected float[] inputValues;
		public string[] InputNames {protected set; get;}

		protected float[] outputValues;
		public string[] OutputNames {protected set; get;}

		/// <summary>
		/// Death event of the controller and it's neural net
		/// </summary>
		public event Action <Controller> Death;
		/// <summary>
		/// Gets or sets a value indicating whether this instance is dead.
		/// </summary>
		/// <value><c>true</c> if this instance is dead; otherwise, <c>false</c>.</value>
		public bool IsDead { set; get;}

	
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

		/// <summary>
		/// Calculates the fitness.
		/// </summary>
		/// <returns>The fitness.</returns>
		protected abstract float CalculateFitness ();

		/// <summary>
		/// Monobehaviour start.
		/// </summary>
		protected virtual void Start() {
			IsDead = false;
			InitialiseController ();
			InitInputAndOutputArrays();
		}


		/// <summary>
		/// Monobehaviour update.
		/// </summary>
		protected virtual void Update() {

			// every frame we ping the network forward
			if(NeuralNet != null) {
				UpdateInputValues ();
				UpdateOutputValues ();
				UseOutputValues ();
				UpdateFitnessScore ();
				CheckAliveStatus ();
			}
		}

		/// <summary>
		/// Inits the input and output arrays.
		/// </summary>
		protected virtual void InitInputAndOutputArrays() {
			inputValues = NeuralNet.GetInputValues ();
			outputValues = NeuralNet.GetOutputValues ();
		}

		/// <summary>
		/// Updates the output values.
		/// </summary>
		public virtual void UpdateOutputValues () {
			if (!IsDead) {
				outputValues = NeuralNet.PingFwd(inputValues);
			}
		}

		public virtual void UpdateFitnessScore() {
			if (!IsDead) {
				NeuralNet.FitnessScore = CalculateFitness();
			}
		}

		/// <summary>
		/// Calculates the fitness of the neural net instance and raise the death event.
		/// </summary>
		public virtual void Kill() {
			//NeuralNet.FitnessScore = CalculateFitness();
			IsDead = true;
			RaiseDeathEvent();

			for(int i = 0; i < outputValues.Length; i++) {
				outputValues [i] = 0f;
			}
		}

		/// <summary>
		/// Reset the specified position and orientation and other values of the controller.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="orientation">Orientation.</param>
		public virtual void Reset(Vector3 position, Quaternion orientation) {
			transform.position = position;
			transform.rotation = orientation;
			IsDead = false;
		}

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