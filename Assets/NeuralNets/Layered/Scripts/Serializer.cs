using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;


namespace nfs.nets.layered {

	public enum Simulation {Car, Cells }

	public static class Serializer {

		public static SrNetwork SerializeNetwork(Network network) {
			SrSynapseNetwork allSynapseValues = SerializeSynapses(network);
            return new SrNetwork(network, allSynapseValues);
        }

		public static SrSpecies SerializeSpecies(Network network) {
			Stack<SrNetwork> speciesLineage =  network.SpeciesLineage;
			speciesLineage.Push(SerializeNetwork(network));
			return new SrSpecies(speciesLineage.ToArray());
		}

		public static SrSynapseNetwork SerializeSynapses(Network network) {

			int synpaseLayerNb = network.NumberOfSynapseLayers;
			SrSynapseLayer[] synapseLayers = new SrSynapseLayer[synpaseLayerNb];

			for(int l = 0; l < synpaseLayerNb; l++) {

				int neuronNb = network.LayersSizes[l];
				SrNeuronSynapses[] neuronSynapses = new SrNeuronSynapses[neuronNb];

				for(int n = 0; n < neuronNb; n++) {
					neuronSynapses[n] = new SrNeuronSynapses(network.GetNeuronSynapsesValues(l, n));
				}

				synapseLayers[l] = new SrSynapseLayer(neuronSynapses);
			}

			return new SrSynapseNetwork(synapseLayers);
		}

		public static void SaveNetworkAsSpecies(Network network, Simulation simulation) {
			List<SrSpecies> serializedSpeciesList = new List<SrSpecies>();

			if(File.Exists(GetPath(simulation))) {
				Debug.LogWarning("save file exist, loading data");
				SrLife savedSpecies = LoadAllSpecies(simulation);
				foreach(SrSpecies species in savedSpecies.Species){
					serializedSpeciesList.Add(species);
				}
			}

			serializedSpeciesList.Add(SerializeSpecies(network));
			SrLife allSpecies = new SrLife(serializedSpeciesList.ToArray());
			string jsonString = JsonUtility.ToJson(allSpecies);
			// File.WriteAllText(GetPath(simulation), jsonString);
			Debug.Log("json test: " + jsonString);		
		}

		public static SrLife LoadAllSpecies(Simulation simulation) {
			if(!File.Exists(GetPath(simulation))) {
				Debug.LogWarning("The save file does not exists yet. Please train and save a layered network first.");
				return null;
			}
			
			string jsonString = File.ReadAllText(GetPath(simulation));

			return JsonUtility.FromJson<SrLife>(jsonString);
		}

		private static string GetPath(Simulation simulation) {

			Debug.Log("saving at path: " + Application.persistentDataPath + "/" + simulation.ToString() + "/Layered Networks.txt");
			return Application.persistentDataPath + simulation.ToString() + "/Layered Networks.json";
		}



	}

	///<summary>
	/// Holds an array of species representing all saved data.
	///</summary>
	[Serializable]
    public class SrLife {
        public SrSpecies[] Species;

        public SrLife(SrSpecies[] species) {
            Species = species;           
        }
    }

	///<summary>
	/// Holds an array of network, idx 0 being the latest child and all the others it's parents in order.
	///</summary>
	[Serializable]
    public class SrSpecies {

		public string Name;
        public SrNetwork[] Lineage;

        public SrSpecies(SrNetwork[] networks) {
			Name = networks[0].Nickname == null? Time.time.ToString() : networks[0].Nickname;
            Lineage = networks;           
        }
    }

	///<summary>
	/// A network serialized to a jsonable format.
	///</summary>
	[Serializable]
    public class SrNetwork {

        public string Nickname;
        public string Id;
		public Color Colorisation;
        public string[] Ancestors;
        public float FitnessScore;
        public int[] LayersSizes;
        public SrSynapseNetwork SynapseValues;
        public string[] InputsNames;
        public string[] OutputsNames;

        public SrNetwork(Network network, SrSynapseNetwork synapseValues) {
            Nickname = network.Nickname;
            Id = network.Id;
			Colorisation = network.Colorisation;
            Ancestors = network.Ancestors;
            FitnessScore = network.FitnessScore;
            LayersSizes = network.LayersSizes;
            SynapseValues = synapseValues;
            InputsNames = network.InputsNames;
            OutputsNames = network.OutputsNames;
            
        }
    }


	// the synapses serializable object bellow were name to be able to call a specific value as:
	// allSynapses.Layers[0].Neurons[0].Synapses[0]

	///<summary>
	/// Holds all the synapses of a neural network (an array of layers of synapses).
	///</summary>
	[Serializable]
    public class SrSynapseNetwork {
        public SrSynapseLayer[] Layers;

        public SrSynapseNetwork(SrSynapseLayer[] layers) {
            Layers = layers;           
        }
    }

	///<summary>
	/// Holds all the synapses for a layer of neuron (an array of synapses).
	///</summary>
	[Serializable]
    public class SrSynapseLayer {
        public SrNeuronSynapses[] Neurons;

        public SrSynapseLayer(SrNeuronSynapses[] neurons) {
            Neurons = neurons;           
        }
    }

	// synapses.AllSynapses[0].Synapses[0].Values[0]
	// allSynapses.Layers[0].Neurons[0].Synapses[0]


	///<summary>
	/// Holds the synapse values going out from one single neuron (an array of float values).
	///</summary>
	[Serializable]
    public class SrNeuronSynapses {
        public float[] Synapes;

        public SrNeuronSynapses(float[] synapses) {
            Synapes = synapses;           
        }
    }

}
