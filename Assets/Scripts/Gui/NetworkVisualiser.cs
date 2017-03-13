using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using nfs.layered;

namespace nfs.gui {

	public class NetworkVisualiser : MonoBehaviour {

		[SerializeField] Camera cam;

		[SerializeField] GameObject baseLayer;
		[SerializeField] GameObject baseNeuron;
		[SerializeField] GameObject baseSynapse;

		int[] layersSizes;

		GameObject[] layers;
		GameObject[] neurons;
		GameObject[] synapses;

		LayeredNetwork neuralNet;

		bool buildingVisualisation = false;
		
		// Update is called once per frame
		void Update () {

			// if (buildingVisualisation) {
			// 	InstantiateSynapses();
			// 	buildingVisualisation = false;
			// }

			if (neuralNet != null /*&& !buildingVisualisation*/) {
				UpdateVisualisation();
			}
		}

		void UpdateVisualisation () {

			int N = 0; // neuron counter
			int S = 0; // synapse counter
			int NLsum = 0;

			for (int L=0; L < layers.Length; L++) {
				for (int n=0; n < layersSizes[L]; n++) {
					//neurons[N].GetComponentInChildren<Text>().text = neuralNet.GetNeuronValue(L, n).ToString("F2");
					neurons[N].transform.FindChild("value").GetComponent<Text>().text = neuralNet.GetNeuronValue(L, n).ToString("F2");
					
					if (neuralNet.GetNeuronValue(L, n) >= 0) {
						neurons[N].GetComponent<Image>().color = Color.green * neuralNet.GetNeuronValue(L, n);
					} else {
						neurons[N].GetComponent<Image>().color = Color.red * -neuralNet.GetNeuronValue(L, n);						
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

		void BuildVisualisation () {
			InstantiateLayers();
			InstantiateNeurons();
			InstantiateSynapses();
			//buildingVisualisation = true;
		}


		void InstantiateLayers() {
			layers = new GameObject[neuralNet.NumberOfLayers];
			layersSizes = neuralNet.LayersSizes;

			for (int i = 0; i < layers.Length; i++) {
				layers[i] = GameObject.Instantiate(baseLayer);
				layers[i].transform.SetParent(transform, false);
				
			}
		}

		void InstantiateNeurons() {
			int totalNeurons = layersSizes.Sum();

			neurons = new GameObject[totalNeurons];

			int L = 0; // layer counter
			int neuronsInLayer = 0; // sum of neuron per layer already done
			for (int i=0; i < neurons.Length; i++) {
				neurons[i] = GameObject.Instantiate(baseNeuron);
				neurons[i].transform.SetParent(layers[L].transform, false);
				
				if (L == 0) {
					switch(i) {
						case(0):
							neurons[i].transform.FindChild("label").GetComponent<Text>().text = "Sensor NW";
							break;

						case(1):
							neurons[i].transform.FindChild("label").GetComponent<Text>().text = "Sensor N";
							break;

						case(2):
							neurons[i].transform.FindChild("label").GetComponent<Text>().text = "Sensor NE";
							break;
						
						case(3):
							neurons[i].transform.FindChild("label").GetComponent<Text>().text = "Bias input";
							break;
					}

				} else 	if (L == layersSizes.Length -1) { // last layer
					if(i == neurons.Length-2)
						neurons[i].transform.FindChild("label").GetComponent<Text>().text = "Drive";

					else if(i == neurons.Length-1)
						neurons[i].transform.FindChild("label").GetComponent<Text>().text = "Turn";

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

							float width = Mathf.Abs(neuralNet.GetSynapseValue(L, n, s))*0.05f;
							line.widthMultiplier = width;

							if (neuralNet.GetSynapseValue(L, n, s) >= 0 ) {
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


		public void AssignFocusNetwork (LayeredNetwork newFocusNet) {
			ClearCurrentVisualisation();
			neuralNet = newFocusNet;
			BuildVisualisation ();
		}

		public void ClearCurrentVisualisation() {
			neuralNet = null;

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
