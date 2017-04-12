using UnityEngine;

namespace nfs.nets.layered {

	public class VisualiserManager : MonoBehaviour {

		[SerializeField] private Camera cam;
		[SerializeField] private Visualiser[] visualisers;
		

		private Trainer trainer;

		private int lastI = 0;
        private Vector2 originalSize;
        private float minSize;
        private bool collapse = false;

        public bool ShowBest { set; get;}


        private void Start() {
			Debug.Assert(visualisers.Length > 0, "No visualisers referenced in the manager!");
            Debug.Assert(cam != null, "You need to assign a camera to the visualiser manager");

			trainer = FindObjectOfType<Trainer> ();

			foreach(Visualiser visualiser in visualisers) {
				visualiser.Cam = cam;
			}

			FindObjectOfType<Trainer> ().NextGenerationTraining += ResetForNewGen;
			ResetForNewGen ();

            originalSize = visualisers[0].GetComponent<RectTransform>().sizeDelta;
            minSize = visualisers[0].transform.FindChild("Top Info").GetComponent<RectTransform>().sizeDelta.y;
			CollapseView();

        }

		private void Update() {
			CheckFocusNetwork ();

			if(ShowBest)
            	BestNetwork();

        }

		public void NextAliveNetwork() {

			if(ShowBest)
                return;

            for (int i = lastI; i < trainer.HostPopulation.Length; i++) {
				
				Controller controller = trainer.HostPopulation [i].GetComponent<Controller> ();
				if(!controller.IsDead) {

					for (int j = visualisers.Length-1; j > 0; j--) {
						if(visualisers [j-1].Focus != null) {
							visualisers [j].AssignFocusNetwork (visualisers [j-1].Focus);
						}
					}

					visualisers [0].AssignFocusNetwork (controller);
					lastI = i+1;
					break;
				}
			}
		}


		public void BestNetwork() {
            Controller bestController = null;

            for (int i = 0; i < trainer.HostPopulation.Length; i++) {
				Controller controller = trainer.HostPopulation [i].GetComponent<Controller> ();

				if (!controller.IsDead) {
					if((bestController != null && controller.NeuralNet.FitnessScore > bestController.NeuralNet.FitnessScore)
					|| bestController == null) {
                    	bestController = controller;
					}
				}
			}

			if(bestController != null && bestController != visualisers[0].Focus) {
				visualisers [0].AssignFocusNetwork (bestController);
			}
		}

		public void NextBestNetwork() {

			if(ShowBest)
                return;

            Controller bestController = null;

            for (int i = 0; i < trainer.HostPopulation.Length; i++) {
				Controller controller = trainer.HostPopulation [i].GetComponent<Controller> ();

				if (!controller.IsDead) {

                    bool alreadyFocused = false;
                    for (int j = 0; j < visualisers.Length; j++) {
						if(visualisers [j].Focus == controller ) {
							alreadyFocused = true;
						}
					}

					if(!alreadyFocused
					&& ((bestController != null && controller.NeuralNet.FitnessScore > bestController.NeuralNet.FitnessScore)
					|| bestController == null)) {
                    	bestController = controller;
					}
				}
			}

			if(bestController != null) {
				visualisers [0].AssignFocusNetwork (bestController);
			}
		}

		public void StoreCurrentFocus() {

			if(visualisers [0].Focus == null)
                return;

            for (int j = visualisers.Length-1; j > 0; j--) {
				if(visualisers [j-1].Focus != null) {
					visualisers [j].AssignFocusNetwork (visualisers [j-1].Focus);
				}
			}

			visualisers [0].ClearCurrentVisualisation();
		}

		public void CollapseView() {
            collapse = !collapse;

            for (int j = 0; j < visualisers.Length; j++) {
                visualisers[j].GetComponent<RectTransform>().sizeDelta = new Vector2(originalSize.x, collapse? minSize : originalSize.y);
                visualisers[j].transform.FindChild("NetworkPanel").gameObject.SetActive(!collapse);
            }
		}

		private void ResetForNewGen() {
			foreach(Visualiser visualiser in visualisers) {
				visualiser.ClearCurrentVisualisation();
			}
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
			}
			else if (Input.GetMouseButtonDown(1)) { // change to mouse wheel click if to many errors
				ResetForNewGen ();
			}
		}


	}
}

