using UnityEngine;


namespace nfs.car {

	// TODO use an interface or composition?
    ///<summary>
	/// Base car controller class meant to be extended by a human controller or ai controller.
    /// The class will always update drive and turn values and requires and implementation
    /// of a derived update and start.
	///</summary>
	[RequireComponent (typeof (CarBehaviour))]
	public abstract class CarController : MonoBehaviour {
        
		// reference to the car behaviour
        protected CarBehaviour Car { private set; get; }

		// the current input values
		protected float driveInput = 0f;
		protected float turnInput = 0f;

        // Use this for initialization
        private void Start () {
			Car = GetComponent<CarBehaviour>();

            DerivedStart(); // derived start is called in the child class
        }
		
		// Update is called once per frame
		private void Update () {
			DerivedUpdate(); // derived update is called in the child class

			// every frame we call drive and turn method of the car behaviour with the current values
			// the values may or may not have been updated by the child class in the derived update
			Car.Drive(driveInput);
			Car.Turn(turnInput);
        }

		///<sumary>
		/// Is called in Start AFTER the base class function
		///</sumary>
        protected abstract void DerivedStart();

		///<sumary>
		/// Is called in Update BEFORE the base class function	
		///</sumary>
        protected abstract void DerivedUpdate();


    }
}