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
            if(Input.GetButton("Vertical"))
				car.Drive(Input.GetAxis("Vertical"));

			if(Input.GetButton("Horizontal"))
				car.Turn(Input.GetAxis("Horizontal"));

        }

	}
}