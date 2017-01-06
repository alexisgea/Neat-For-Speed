using UnityEngine;

namespace airace {

	///<summary>
	/// Basic player input controller to drive a car.
	///</summary>
	[RequireComponent (typeof (CarController))]
	public class PlayerControl : MonoBehaviour {

		private CarController car;

		void Start () {
			car = GetComponent<CarController>();
		}
		
		void Update () {
			if(Input.GetAxis("Vertical") > 0f)
				car.Accelerate();

			if(Input.GetAxis("Vertical") < 0f)
				car.Reverse();

			if(Input.GetAxis("Horizontal") > 0f)
				car.TurnRight();

			if(Input.GetAxis("Horizontal") < 0f)
				car.TurnLeft();
		}

	}
}