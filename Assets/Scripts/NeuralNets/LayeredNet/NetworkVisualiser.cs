using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

		
		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			if (neuralNet != null) {
				UpdateVisualisation();
			}
		}

		void UpdateVisualisation () {
			/*
			need a neuron counter
			need a synapse counter 

			For each layer
				for neurons of this layer
					update neuron value
					for synapse of this neuron
						update synapse value

			*/
		}

		void BuildVisualisation () {
			InstantiateLayers();
			InstantiateNeurons();

		}

		void InstantiateLayers() {
			layers = new GameObject[neuralNet.GetHiddenLayersSizes().Length + 2];
			layersSizes = new int[layers.Length];

			for (int i = 0; i < layers.Length; i++) {
				layers[i] = GameObject.Instantiate(baseLayer);
				layers[i].transform.parent = transform;

				if (i == 0) {
					layersSizes[i] = neuralNet.GetInputSize();
				} else if (i == layers.Length-1) {
					layersSizes[i] = neuralNet.GetOutputSize();
				} else {
					layersSizes[i] = neuralNet.GetHiddenLayersSizes()[i-1];
				}
			}
		}

		void InstantiateNeurons() {
			int totalNeurons = layersSizes.Sum();

			neurons = new GameObject[totalNeurons];

			int k = 0; // layer counter
			int neuronsInLayer = 0; // sum of neuron per layer already done
			for (int i=0; i < neurons.Length; i++) {
				neurons[i] = GameObject.Instantiate(baseNeuron);
				neurons[i].transform.parent = layers[k].transform;

				neuronsInLayer ++;
				if (neuronsInLayer == layersSizes[k]) {
					k++;
					neuronsInLayer = 0;
				}
			}
		}

		void InstantiateSynapses() {
			int totalSynapses = 0;

			for (int i=1; i<layers.Length; i++) {
				totalSynapses += layersSizes[i-1] * layersSizes[i];
			}

			synapses = new GameObject[totalSynapses];

			int synapseCounter = 0; // synapse total counter
			int k = 0; // layer counter
			//int synapsesInNeuron = 0; // sum of neuron per layer already done
			for (int i=0; i < neurons.Length; i++) {
				for (int s=0; s<layersSizes[k+1]; s++) {
					synapses[synapseCounter] = GameObject.Instantiate(baseSynapse);
					synapses[synapseCounter].transform.parent = neurons[i].transform;
					Vector3[] linePoints = new Vector3[]{neurons[i].transform.position, neurons[i+layersSizes[k+1]+s].transform.position};
					synapses[synapseCounter].GetComponent<LineRenderer>().SetPositions(linePoints); // need to set the number of total points first?
					synapseCounter++;

					// neuronsInLayer ++;
					// if (neuronsInLayer == layersSizes[k]) {
					// 	k++;
					// 	neuronsInLayer = 0;
					// }
				}
				
			}


		}

	}
}
