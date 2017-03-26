using UnityEngine;
using UnityEngine.UI;

namespace nfs.car{

	/// <summary>
	/// Bottom UI bar.
	/// </summary>
	public class BottomBar : MonoBehaviour {

        [SerializeField] private Text generationLabel;
        [SerializeField] private Text allTimeBestLabel;
        [SerializeField] private Text generationBestLabel;

        private nets.layered.Trainer trainer;

        // Use this for initialization
        private void Start () {
            trainer = FindObjectOfType<nets.layered.Trainer>();
            trainer.NextGenerationTraining += UpdateLabels;
        }

		/// <summary>
		/// Updates the labels.
		/// </summary>
		private void UpdateLabels () {
            generationLabel.text = "Generation: " + trainer.GenerationNb;
            if(trainer.GenerationNb > 1) {
            	allTimeBestLabel.text = "All-time best: " + trainer.AlltimeFittestNets[0].FitnessScore.ToString("F2");
            	generationBestLabel.text = "Last-gen best: " + trainer.GenerationFittestNets[0].FitnessScore.ToString("F2");
			}
        }
	}
}
