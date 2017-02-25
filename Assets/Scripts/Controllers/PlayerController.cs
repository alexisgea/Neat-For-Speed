using UnityEngine;
using nfs.car;

namespace nfs.controllers {

	///<summary>
	/// Basic player input controller to drive a car.
	///</summary>
	public class PlayerController : CarController {

		protected override void ChildStart () {
		}
		
		protected override void ChildUpdate () {

			// axis input needs to go through every frame
			// it will be the case for AI
			// and it is important for the current implentation of the car controller
			DriveInput = Input.GetAxis("Vertical")/*.Normalized()*/;
			TurnInput = Input.GetAxis("Horizontal")/*.Normalized()*/;

            //Debug.Log("normalized input " + TurnInput);

        }

	}
}