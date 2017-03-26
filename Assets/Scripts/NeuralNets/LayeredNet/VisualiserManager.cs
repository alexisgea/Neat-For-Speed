using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nfs.nets.layered {

	public class VisualiserManager : MonoBehaviour {

		[SerializeField] private Camera cam;
		[SerializeField] private Visualiser[] visualisers;

		private Trainer trainer;

		private int lastI = 0;



		private void Start() {
			SecurityCheck ();

			trainer = FindObjectOfType<Trainer> ();

			foreach(Visualiser visualiser in visualisers) {
				visualiser.Cam = cam;
			}

			FindObjectOfType<Trainer> ().NextGenerationTraining += ResetForNewGen;

		}

		private void Update() {
			CheckFocusNetwork ();

		}

		private void SecurityCheck() {
			if(visualisers.Length == 0) {
				Debug.LogError ("No visualisers referenced in the manager!");
			}

			if(cam == null) {
				Debug.LogError ("You need to assign a camera to the visualiser manager");
			}

		}

		public void NextAliveNetwork() {
			
			for (int i = lastI; i < trainer.HostPopulation.Length; i++) {
				
				Controller controller = trainer.HostPopulation [i].GetComponent<Controller> ();
				if(!controller.IsDead) {

					for (int j = visualisers.Length-1; j > 0; j--) {
						if(visualisers [j-1].Focus != null) {
							visualisers [j].AssignFocusNetwork (visualisers [j-1].Focus);
						}
					}

					visualisers [0].AssignFocusNetwork (controller);
					lastI = i;
					break;
				}
			}
		}

		private void ResetForNewGen() {
			lastI = 0;
		}

		/// <summary>
		/// Checks if the player clicked on a network to focus it.
		/// </summary>
		private void CheckFocusNetwork() { // add an overall "if not Input.GetMouseButton(1)"
			if (Input.GetMouseButtonDown(0)) {
				RaycastHit hitInfo = new RaycastHit();
				bool hit = Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hitInfo);

				if (hit && hitInfo.transform.tag == "car" && hitInfo.transform.GetComponent<Controller>() != null)	{
					Controller focusNet = hitInfo.transform.GetComponent<Controller>();
					visualisers[0].AssignFocusNetwork (focusNet);
				}
			} else if (Input.GetMouseButtonDown(1)) { // change to mouse wheel click if to many errors
				foreach(Visualiser visualiser in visualisers) {
					visualiser.ClearCurrentVisualisation();
				}
			}
		}


	}
}

