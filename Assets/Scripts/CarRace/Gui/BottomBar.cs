using UnityEngine;
using UnityEngine.UI;

namespace nfs.car{

	/// <summary>
	/// Bottom UI bar.
	/// </summary>
	public class BottomBar : MonoBehaviour {

        [SerializeField] private Text generationLabel;
        [SerializeField] private Text allTimeBestLabel;
        [SerializeField] private Text generationTimer;
		[SerializeField] private Text liveNetworkCounter;

        private nets.layered.Trainer trainer;

        // Use this for initialization
        private void Start () {
            trainer = FindObjectOfType<nets.layered.Trainer>();
            trainer.NextGenerationTraining += UpdateLabels;
        }

		private void Update() {
			liveNetworkCounter.text = "live cars " + trainer.CurrentLiveNetworks;
			generationTimer.text = "Time: " + (Time.unscaledTime - trainer.GenerationStartTime).ToString ("F2");
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
	}
}
