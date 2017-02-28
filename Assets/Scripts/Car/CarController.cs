using UnityEngine;


namespace nfs.car {

    ///<summary>
	/// Base car controller class meant to be extended by a human controller or ai controller.
    /// The class will always update drive and turn values and requires and implementation
    /// of a derived update and start.
	///</summary>
	[RequireComponent (typeof (CarBehaviour))]
	public abstract class CarController : MonoBehaviour {
        
        protected CarBehaviour Car { private set; get; }

        private float driveInput = 0f;
        protected float DriveInput {
            set { driveInput = value; }
            get { return driveInput; }
        }

        private float turnInput = 0f;
        protected float TurnInput {
			set { turnInput = value; }
            get { return turnInput; }
        }

        // Use this for initialization
        private void Start () {
			Car = GetComponent<CarBehaviour>();

            DerivedStart();
        }
		
		// Update is called once per frame
		private void Update () {
			DerivedUpdate();

			Car.Drive(DriveInput);
			Car.Turn(TurnInput);
        }

		///<sumary>
		/// Is called in Start AFTER the base class function
		/// Base start get's the CarBehaviour component in Car and sets drive and turn input to 0f.
		///</sumary>
        protected abstract void DerivedStart();

		///<sumary>
		/// Is called in Update BEFORE the base class function
		/// Base update calls the Car.Drive and Car.Turn methods with the drive and turn input every frame.		
		///</sumary>
        protected abstract void DerivedUpdate();


    }
}