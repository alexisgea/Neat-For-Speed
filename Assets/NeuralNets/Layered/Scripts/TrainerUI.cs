using UnityEngine;
using UnityEngine.UI;

namespace nfs.nets.layered{

	/// <summary>
	/// Bottom UI bar.
	/// </summary>
	public class TrainerUI : MonoBehaviour {

        [SerializeField] private Text generationLabel;
        [SerializeField] private Text allTimeBestLabel;
        [SerializeField] private Text generationTimer;
		[SerializeField] private Text liveNetworkCounter;

		Trainer trainer;

        // Use this for initialization
        private void Start () {

			trainer = FindObjectOfType<Trainer>();

            Debug.Assert(trainer != null, "No object Trainer found!");

            trainer.NextGenerationTraining += UpdateLabels;
        }

		private void Update() {
			liveNetworkCounter.text = "live nets: " + trainer.CurrentLiveNetworks;
			generationTimer.text = "Time: " + (Time.unscaledTime - trainer.GenerationStartTime).ToString ("F2");

			if(Input.GetButtonDown("Cancel")) {
            	MainScreen.GoToScene(Scenes.MainScreen);
        	}
		}

		/// <summary>
		/// Updates the labels.
		/// </summary>
		private void UpdateLabels () {
            generationLabel.text = "Generation: " + trainer.GenerationNb;
            if(trainer.GenerationNb > 1) {
				allTimeBestLabel.text = "All-time best: " + trainer.AlltimeFittestNets[0].Id + " : " + trainer.AlltimeFittestNets[0].FitnessScore.ToString("F2");
			}

			generationTimer.text = "0.00";
        }

		public void JumpToNextGeneration() {
			trainer.NextGeneration();
		}

		public void EndTraining() {
			trainer.SaveBestNetwork();
			MainScreen.GoToScene(Scenes.MainScreen);
		}




	}
}
