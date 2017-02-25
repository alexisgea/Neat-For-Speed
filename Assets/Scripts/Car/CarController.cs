using UnityEngine;


namespace nfs.car {

	[RequireComponent (typeof (CarBehaviour))]
	public abstract class CarController : MonoBehaviour {

        protected CarBehaviour Car { private set; get; }

        private float driveInput = 0f;
        protected float DriveInput {
            //set { driveInput = Mathf.Clamp01(value); }
            set { driveInput = value; }
            get { return driveInput; }
        }

        private float turnInput = 0f;
        protected float TurnInput {
			//set { turnInput = Mathf.Clamp01(value); }
			set { turnInput = value; }
            get { return turnInput; }
        }

        // Use this for initialization
        private void Start () {
			Car = GetComponent<CarBehaviour>();

            ChildStart();
        }
		
		// Update is called once per frame
		private void Update () {
			ChildUpdate();

			Car.Drive(DriveInput);
			Car.Turn(TurnInput);
        }

		///<sumary>
		/// Is called in Start AFTER the base class function
		/// Base start get's the CarBehaviour component in Car and sets drive and turn input to 0f.
		///</sumary>
        protected abstract void ChildStart();

		///<sumary>
		/// Is called in Update BEFORE the base class function
		/// Base update calls the Car.Drive and Car.Turn methods with the drive and turn input every frame.		
		///</sumary>
        protected abstract void ChildUpdate();


    }
}