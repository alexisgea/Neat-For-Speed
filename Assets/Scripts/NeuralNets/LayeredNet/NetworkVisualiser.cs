using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using nfs.controllers;

namespace nfs.layered {

	public class NetworkVisualiser : MonoBehaviour {

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

			if (buildingVisualisation) {
				InstantiateSynapses();
				buildingVisualisation = false;
			}

			if (Input.GetMouseButtonDown(0)) {
				RaycastHit hitInfo = new RaycastHit();
				bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

				if (hit && hitInfo.transform.tag == "car" && hitInfo.transform.GetComponent<LayeredNetController>() != null)	{
					neuralNet = hitInfo.transform.GetComponent<LayeredNetController>().GetLayeredNet();
					BuildVisualisation ();
				}
			} else if (Input.GetMouseButtonDown(1)) {
				neuralNet = null;
				ClearCurrentVisualisation();
			}

			if (neuralNet != null && !buildingVisualisation) {
				UpdateVisualisation();
			}
		}

		void UpdateVisualisation () {

			int N = 0; // neuron counter
			int S = 0; // synapse counter

			for (int L=0; L < layers.Length; L++) {
				for (int n=0; n < layersSizes[L]; n++) {
					neurons[N].GetComponentInChildren<Text>().text = neuralNet.GetNeuronValue(L, n).ToString("F2");
					
					if (neuralNet.GetNeuronValue(L, n) >= 0) {
						neurons[N].GetComponent<Image>().color = Color.green * neuralNet.GetNeuronValue(L, n);
					} else {
						neurons[N].GetComponent<Image>().color = Color.red * -neuralNet.GetNeuronValue(L, n);						
					}

					// the last layer doesn't have any synapses
					// for (int s=0; s < layersSizes[L+1]; s++) {
					// 	if (L < layers.Length-1) {}
					// 		synapses[S].GetComponent<LineRenderer>().startColor = neurons[N].GetComponent<Image>().color;

					// 	if (L>1)
					// 		synapses[S-layersSizes[L]].GetComponent<LineRenderer>().endColor = neurons[N].GetComponent<Image>().color;

					// 	S++; // S will go over the total number of synapse, that's dangerous
						
					// }

					N ++;
				}
			}
		}

		void BuildVisualisation () {
			ClearCurrentVisualisation();
			InstantiateLayers();
			InstantiateNeurons();
			//InstantiateSynapses();
			buildingVisualisation = true;
		}

		void ClearCurrentVisualisation() {
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

			int k = 0; // layer counter
			int neuronsInLayer = 0; // sum of neuron per layer already done
			for (int i=0; i < neurons.Length; i++) {
				neurons[i] = GameObject.Instantiate(baseNeuron);
				neurons[i].transform.SetParent(layers[k].transform, false);

				neuronsInLayer ++;
				if (neuronsInLayer == layersSizes[k]) {
					k++;
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

							// we get the position values of the "from" and "to" neuron for the synapse
							// the next neuron is equal to the sum of neuron of all previous processed layer + the number of neuron in the current layer + the current synapse fo this neuron
							Vector3[] linePoints = new Vector3[] {
								neurons[N].transform.position + Vector3.forward * 0.1f,
								neurons[NLsum + layersSizes[L] + s].transform.position + Vector3.forward * 0.1f
								};
							synapses[S].GetComponent<LineRenderer>().SetPositions(linePoints);

							float width = Mathf.Abs(neuralNet.GetSynapseValue(L, n, s))*0.2f;
							synapses[S].GetComponent<LineRenderer>().widthMultiplier = width;

							//synapses[S].GetComponent<LineRenderer>().startColor = Color.red;
							//synapses[S].GetComponent<LineRenderer>().endColor = Color.red;

							// synapses[N].GetComponentInChildren<Text>().text = neuralNet.GetSynapseValue(L, n, s).ToString("F2");

							S++;
						}
					}
					N ++;
				}
				NLsum += layersSizes[L];
			}

		}

	}
}
