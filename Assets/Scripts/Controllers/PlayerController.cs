using UnityEngine;
using nfs.car;

namespace nfs.controllers {

	///<summary>
	/// Basic player input controller to drive a car.
	///</summary>
	public class PlayerController : CarController {

		// nothing here but implentation is required
		protected override void DerivedStart () {
		}
		
		// axis input needs to go through every frame
		// it will be the case for AI and it is important for the current implentation of the car controller
		protected override void DerivedUpdate () {
			driveInput = Input.GetAxis("Vertical");
			turnInput = Input.GetAxis("Horizontal");
        }

	}
}