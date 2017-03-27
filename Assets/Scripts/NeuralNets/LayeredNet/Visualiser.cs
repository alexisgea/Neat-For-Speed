using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace nfs.nets.layered {

	/// <summary>
	/// Display the neural network layout on a canvas and update it,
	/// requires a camera for raycast purpose and drawing the synapses.
	/// </summary>
	public class Visualiser : MonoBehaviour {

		/// <summary>
		/// Reference to the camera on which the canvas will be drawn.
		/// </summary>
		public Camera Cam {set; get;}
		/// <summary>
		/// The text labelon which to display the current focus.
		/// </summary>
		[SerializeField] private Text networkIdLabel;
		/// <summary>
		/// The text label on which to display the lineage.
		/// </summary>
		[SerializeField] private Text networkLineageLabel;
		/// <summary>
		/// The network fitness label.
		/// </summary>
		[SerializeField] private Text networkFitnessLabel;
		/// <summary>
		/// The panel on which to display and parent the network nodes and synapses.
		/// </summary>
		[SerializeField] private Transform networkDisplay;

		/// <summary>
		/// Prefab of the base layer object.
		/// </summary>
		[SerializeField] private GameObject baseLayer;
		/// <summary>
		/// Prefab of a base neuron object.
		/// </summary>
		[SerializeField] private GameObject baseNeuron;
		/// <summary>
		/// Prefab of a base synapse object.
		/// </summary>
		[SerializeField] private GameObject baseSynapse;

		// reference to the layers sizes of the current neural net to go faster
		private int[] layersSizes;

		// array of objected displayed on the canvas
		private GameObject[] layers;
		private GameObject[] neurons;
		private GameObject[] synapses;


		private Controller focus;
		// the current neural net
		public Controller Focus { get { return focus; } }

		private bool inConstruction = false;
		

		/// <summary>
		/// Mono update.
		/// </summary>
		private void Update () {

			if (focus != null && !inConstruction) {
				UpdateVisualisation();
			}

			if(inConstruction) {
				inConstruction = false;
			}
		}

		/// <summary>
		/// Updates the neural net visualisation.
		/// </summary>
		private void UpdateVisualisation () {

			int N = 0; // neuron counter
			int S = 0; // synapse counter
			int NLsum = 0;

			for (int L=0; L < layers.Length; L++) {
				for (int n=0; n < layersSizes[L]; n++) {
					Text neuronValue = neurons [N].transform.FindChild ("value").GetComponent<Text> ();
					neuronValue.text = focus.NeuralNet.GetNeuronValue(L, n).ToString("F2");
					
					if (focus.NeuralNet.GetNeuronValue(L, n) >= 0) {
						neurons[N].GetComponent<Image>().color = Color.green * focus.NeuralNet.GetNeuronValue(L, n);
					} else {
						neurons[N].GetComponent<Image>().color = Color.red * -focus.NeuralNet.GetNeuronValue(L, n);						
					}

					// the last layer doesn't have any synapses
					if (L < layers.Length-1) {
						for (int s=0; s < layersSizes[L+1]; s++) {
							Vector3[] linePoints = new Vector3[] {
									neurons[N].transform.position + Cam.transform.forward * 0.1f,
									neurons[NLsum + layersSizes[L] + s].transform.position + Cam.transform.forward * 0.1f
									};
								synapses[S].GetComponent<LineRenderer>().SetPositions(linePoints);

							S++; // S will go over the total number of synapse, that's dangerous
						}
					}
					N ++;
				}
				NLsum += layersSizes[L];
			}
		}

		/// <summary>
		/// Builds the neural net visualisation.
		/// </summary>
		private void BuildVisualisation () {
			InstantiateLayers();
			InstantiateNeurons();
			InstantiateSynapses();
			UpdateLabels ();
		}

		private void UpdateLabels() {
			networkIdLabel.text = "Id: " + focus.NeuralNet.Id;

			string lineage = "";
			if(focus.NeuralNet.Lineage != null) {
				foreach(string ancestor in focus.NeuralNet.Lineage) {
					lineage += ancestor + ", ";
				}
			}

			if(lineage != ""){
				lineage.Remove (lineage.Length - 1);
			}

			networkLineageLabel.text = "Lineage: " + lineage;
		}

		/// <summary>
		/// Instantiates the neural net layers on the canvas.
		/// </summary>
		private void InstantiateLayers() {
			layers = new GameObject[focus.NeuralNet.NumberOfLayers];
			layersSizes = focus.NeuralNet.LayersSizes;

			for (int i = 0; i < layers.Length; i++) {
				layers[i] = GameObject.Instantiate(baseLayer);
				layers[i].transform.SetParent(networkDisplay, false);
				
			}
		}

		/// <summary>
		/// Instantiates the neurons in the layers on the canvas.
		/// </summary>
		private void InstantiateNeurons() {
			int totalNeurons = layersSizes.Sum();

			neurons = new GameObject[totalNeurons];

			int L = 0; // layer counter
			int neuronsInLayer = 0; // sum of neuron per layer already done
			for (int i=0; i < neurons.Length; i++) {
				neurons[i] = GameObject.Instantiate(baseNeuron);
				neurons[i].transform.SetParent(layers[L].transform, false);
				Text label = neurons[i].transform.FindChild("label").GetComponent<Text>();

				
				if (L == 0) {

					if(i < focus.InputNames.Length) {
						label.text = focus.InputNames[i];						
					} else {
						label.text = "Bias";
					}

				} else 	if (L == layersSizes.Length -1) { // last layer

					if(neurons.Length-1 - i < focus.OutputNames.Length) {
						label.text = focus.OutputNames[neurons.Length-1 - i];						
					} else {
						label.text = "";
					}

				} else {
					label.text = "";

				}
			
				neuronsInLayer ++;
				if (neuronsInLayer == layersSizes[L]) {
					L++;
					neuronsInLayer = 0;
				}
			}
		}

		/// <summary>
		/// Instantiates the synapses as child of neurons on the canvas.
		/// </summary>
		private void InstantiateSynapses() {
			int totalSynapses = 0;

			// -1 as there are no synapses on the last layer
			for (int i=0; i<layers.Length-1; i++) {
				totalSynapses += layersSizes[i] * layersSizes[i+1];
			}

			synapses = new GameObject[totalSynapses];


			int N = 0; // neuron counter
			int S = 0; // synapse counter
			int NLsum = 0;

			for (int L=0; L < layers.Length; L++) {
				for (int n=0; n < layersSizes[L]; n++) {
					// the last layer doesn't have any synapses
					if (L < layers.Length-1) {
						for (int s=0; s < layersSizes[L+1]; s++) {

							// we create the synapse and set it's parent as the current neuron
							synapses[S] = GameObject.Instantiate(baseSynapse);
							synapses[S].transform.SetParent(neurons[N].transform, false);
							LineRenderer line = synapses[S].GetComponent<LineRenderer>();

							// we get the position values of the "from" and "to" neuron for the synapse
							// the next neuron is equal to the sum of neuron of all previous processed layer
							// + the number of neuron in the current layer + the current synapse fo this neuron
							Vector3[] linePoints = new Vector3[] {
								neurons[N].transform.position + Cam.transform.forward * 0.1f,
								neurons[NLsum + layersSizes[L] + s].transform.position + Cam.transform.forward * 0.1f
								};
							line.SetPositions(linePoints);

							float width = Mathf.Abs(focus.NeuralNet.GetSynapseValue(L, n, s))*0.05f;
							line.widthMultiplier = width;

							if (focus.NeuralNet.GetSynapseValue(L, n, s) >= 0 ) {
								line.material.color = Color.green;
							} else {
								line.material.color = Color.red;
							}

							S++;
						}
					}
					N ++;
				}
				NLsum += layersSizes[L];
			}

		}


		/// Assigns the focus network.
		/// </summary>
		/// <param name="newFocusNet">New focus net.</param>
		public void AssignFocusNetwork (Controller newFocusNet) {
			ClearCurrentVisualisation();
			focus = newFocusNet;
			BuildVisualisation ();
		}


		/// <summary>
		/// Clears the current visualisation.
		/// </summary>
		public void ClearCurrentVisualisation() {
			inConstruction = true;
			focus = null;

			if (synapses != null) {
				for (int i=0; i < synapses.Length; i++){
					GameObject.Destroy(synapses[i]);
				}
			}

			if (neurons != null) {
				for (int i=0; i < neurons.Length; i++){
					GameObject.Destroy(neurons[i]);
				}
			}

			if (layers != null) {
				for (int i=0; i < layers.Length; i++){
					GameObject.Destroy(layers[i]);
				}
			}

			networkIdLabel.text = "Id: ";
			networkLineageLabel.text = "Lineage: ";

		}

	}
}
