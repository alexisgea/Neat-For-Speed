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
				car.Drive(Dir.Forward);

			else if(Input.GetAxis("Vertical") < 0f)
				car.Drive(Dir.Reverse);

			if(Input.GetAxis("Horizontal") > 0f)
				car.Turn(Dir.Right);

			else if(Input.GetAxis("Horizontal") < 0f)
				car.Turn(Dir.Left);
		}

	}
}