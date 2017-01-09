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

			// axis input needs to go through every frame
			// it will be the case for AI
			// and it is important for the current implentation of the car controller
			car.Drive(Input.GetAxis("Vertical"));
			car.Turn(Input.GetAxis("Horizontal"));

        }

	}
}