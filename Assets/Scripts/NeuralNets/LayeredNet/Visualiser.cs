using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace nfs.nets.layered {

	/// <summary>
	/// Display the neural network layout on a canvas and update it.
	/// </summary>
	public class Visualiser : MonoBehaviour {

		// reference to a camera for choosing a network
		[SerializeField] private Camera cam;

		// prefabs of object to display on the canvas
		[SerializeField] private GameObject baseLayer;
		[SerializeField] private GameObject baseNeuron;
		[SerializeField] private GameObject baseSynapse;

		// reference to the layers sizes of the current neural net to go faster
		private int[] layersSizes;

		// array of objected displayed on the canvas
		private GameObject[] layers;
		private GameObject[] neurons;
		private GameObject[] synapses;

		// the current neural net
		private Controller focus;

		//bool buildingVisualisation = false;
		
		// Update is called once per frame
		void Update () {

			// if (buildingVisualisation) {
			// 	InstantiateSynapses();
			// 	buildingVisualisation = false;
			// }

			if (focus != null /*&& !buildingVisualisation*/) {
				UpdateVisualisation();
			}
		}

		/// <summary>
		/// Updates the neural net visualisation.
		/// </summary>
		void UpdateVisualisation () {

			int N = 0; // neuron counter
			int S = 0; // synapse counter
			int NLsum = 0;

			for (int L=0; L < layers.Length; L++) {
				for (int n=0; n < layersSizes[L]; n++) {
					//neurons[N].GetComponentInChildren<Text>().text = neuralNet.GetNeuronValue(L, n).ToString("F2");
					neurons[N].transform.FindChild("value").GetComponent<Text>().text = focus.NeuralNet.GetNeuronValue(L, n).ToString("F2");
					
					if (focus.NeuralNet.GetNeuronValue(L, n) >= 0) {
						neurons[N].GetComponent<Image>().color = Color.green * focus.NeuralNet.GetNeuronValue(L, n);
					} else {
						neurons[N].GetComponent<Image>().color = Color.red * -focus.NeuralNet.GetNeuronValue(L, n);						
					}

					// the last layer doesn't have any synapses
					if (L < layers.Length-1) {
						for (int s=0; s < layersSizes[L+1]; s++) {
							Vector3[] linePoints = new Vector3[] {
									neurons[N].transform.position + cam.transform.forward * 0.1f,
									neurons[NLsum + layersSizes[L] + s].transform.position + cam.transform.forward * 0.1f
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
		void BuildVisualisation () {
			InstantiateLayers();
			InstantiateNeurons();
			InstantiateSynapses();
			//buildingVisualisation = true;
		}

		/// <summary>
		/// Instantiates the neural net layers on the canvas.
		/// </summary>
		void InstantiateLayers() {
			layers = new GameObject[focus.NeuralNet.NumberOfLayers];
			layersSizes = focus.NeuralNet.LayersSizes;

			for (int i = 0; i < layers.Length; i++) {
				layers[i] = GameObject.Instantiate(baseLayer);
				layers[i].transform.SetParent(transform, false);
				
			}
		}

		/// <summary>
		/// Instantiates the neurons in the layers on the canvas.
		/// </summary>
		void InstantiateNeurons() {
			int totalNeurons = layersSizes.Sum();

			neurons = new GameObject[totalNeurons];

			int L = 0; // layer counter
			int neuronsInLayer = 0; // sum of neuron per layer already done
			for (int i=0; i < neurons.Length; i++) {
				neurons[i] = GameObject.Instantiate(baseNeuron);
				neurons[i].transform.SetParent(layers[L].transform, false);
				
				if (L == 0) {
					if(i < focus.InputNames.Length) {
						neurons[i].transform.FindChild("label").GetComponent<Text>().text = focus.InputNames[i];						
					} else {
						neurons[i].transform.FindChild("label").GetComponent<Text>().text = "Bias";
					}

				} else 	if (L == layersSizes.Length -1) { // last layer
					if(neurons.Length-1 - i < focus.OutputNames.Length) {
						neurons[i].transform.FindChild("label").GetComponent<Text>().text = focus.OutputNames[neurons.Length-1 - i];						
					} else {
						neurons[i].transform.FindChild("label").GetComponent<Text>().text = "";
					}

				} else {
					neurons[i].transform.FindChild("label").GetComponent<Text>().text = "";

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
		void InstantiateSynapses() {
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
							// the next neuron is equal to the sum of neuron of all previous processed layer + the number of neuron in the current layer + the current synapse fo this neuron
							Vector3[] linePoints = new Vector3[] {
								neurons[N].transform.position + cam.transform.forward * 0.1f,
								neurons[NLsum + layersSizes[L] + s].transform.position + cam.transform.forward * 0.1f
								};
							line.SetPositions(linePoints);

							float width = Mathf.Abs(focus.NeuralNet.GetSynapseValue(L, n, s))*0.05f;
							line.widthMultiplier = width;

							if (focus.NeuralNet.GetSynapseValue(L, n, s) >= 0 ) {
								line.material.color = Color.green;
							} else {
								line.material.color = Color.red;
							}

							// synapses[N].GetComponentInChildren<Text>().text = neuralNet.GetSynapseValue(L, n, s).ToString("F2");

							S++;
						}
					}
					N ++;
				}
				NLsum += layersSizes[L];
			}

		}

		/// <summary>
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
		}

	}
}
