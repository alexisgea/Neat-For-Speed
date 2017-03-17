using System.Collections.Generic;
using UnityEngine;
using nfs.car;
using nfs.controllers;
using nfs.tools;

namespace nfs.layered {

    ///<summary>
    /// All the different time of synapses mutation.
    ///</summary>
    public enum MutationType { additive, multiply, reverse, replace, nullify }

    ///<summary>
    /// Genetic algorythm managing all the neural network,
    /// checking their fitness and producing mutated offsprings.
    /// Also cycles through different tracks to avoid overfitting.
    ///</summary>
    public class SmallFilter {

        private float _survivorRate; // how many of a generation will be considered for breeding
        private float _breedingRepartitionCoef; // what proportion of a new generation will be alltime fittest
        private float _freshBloodCoef; // what proportion of a new generation will be brand new nets
        private float _synapsesMutationRate; // how much is a synapses likely to mutate
        private float _synapsesMutationRange; // how much can be the mutation of a synapse at maximum
        private bool _inputNbMutation; // is it possible to modify the number of input (Input discovery)
        private float _inputNbMutationRate; // what is the probability of a new neuron in input layer
        private bool _hiddenLayerNbMutation; // is it possible to modify the number of hidden layer
        private float _hiddenLayerNbMutationRate; // what is the probability of a new hidden layer
        private bool _hiddenNbMutation; // is it possible to modify the number of neurons in hidden layers
        private float _hiddenMbMutationRate; // what is the probability of a new neuron in hidden layer

        private int _breedingSample;

        public int PopulationSize {get {return Population.Length;}}
        public LayeredNetwork[] Population {private set; get;}
        private LayeredNetwork[] _alltimeFittestNets;
        private LayeredNetwork[] _generationFittestNets;

        private int[] _baseNetLayersSizes;



        public SmallFilter (int popSize, int[] networkLayersSizes,
                            float survivorRate = 0.25f, float breedingRepartitionCoef = 0.6f, float freshBloodCoef = 0.1f,
                            float synapsesMutationRate = 0.1f, float synapsesMutationRange = 0.1f,
                            bool inputNbMutation = false, float inputNbMutationRate = 0.01f,
                            bool hiddenLayerNbMutation = true, float hiddenLayerNbMutationRate = 0.001f,
                            bool hiddenNbMutation = true, float hiddenMbMutationRate = 0.01f) {

            _baseNetLayersSizes = networkLayersSizes;
            _survivorRate = survivorRate;
            _breedingRepartitionCoef = breedingRepartitionCoef;
            _freshBloodCoef = freshBloodCoef;
            _synapsesMutationRate = synapsesMutationRate;
            _synapsesMutationRange = synapsesMutationRange;
            _inputNbMutation = inputNbMutation;
            _inputNbMutationRate = inputNbMutationRate;
            _hiddenLayerNbMutation = hiddenLayerNbMutation;
            _hiddenLayerNbMutationRate = hiddenLayerNbMutationRate;
            _hiddenNbMutation = hiddenNbMutation;
            _hiddenMbMutationRate = hiddenMbMutationRate;

            InitializePopulation( popSize, _baseNetLayersSizes);

        }

        ///<summary>
        /// Instantiate the base popuplation and initialises their neural networks
        ///</summary>
        private void InitializePopulation (int popSize, int[] networkLayersSizes) {

            // creates the population
            Population = new LayeredNetwork[popSize];

            // compute the amount of network which will have a descendance in each generation
            _breedingSample = (int)(PopulationSize*_survivorRate);
            _breedingSample = _breedingSample < 1 ? 1 : _breedingSample;
            
            // create the best fitness reference
            _alltimeFittestNets = new LayeredNetwork[_breedingSample];
            _generationFittestNets = new LayeredNetwork[_breedingSample];

            for (int i=0; i < PopulationSize; i++) {
                Population[i] = new LayeredNetwork(networkLayersSizes);
            }
        }

        ///<summary>
        /// Breed the next generation of networks from the current best and overall best.
        ///</summary>
        public void BreedNextGeneration() {

            int k = 0;
            int l = 0;
            for (int i=0; i < PopulationSize; i++) {
                if (i < PopulationSize * _breedingRepartitionCoef) { // all time generation
                    Population[i] = CreateMutatedOffspring(_alltimeFittestNets[k], l+1); // l+1 as mutate coef to make each version more propable to mutate a lot
                    l += 1;
                    if (l > PopulationSize * _breedingRepartitionCoef / _breedingSample){
                        l = 0;
                        k += 1;
                    }

                } else if (i < PopulationSize * (1 - _freshBloodCoef)) { // this generation
                    Population[i] = CreateMutatedOffspring(_generationFittestNets[k], l+1);
                    l += 1;
                    if (l > PopulationSize * (1 - _freshBloodCoef) / _breedingSample){
                        l = 0;
                        k += 1;
                    }

                } else { // fresh blood (10%)
                    Population[i] = new LayeredNetwork(_baseNetLayersSizes);   
                }

                if(k>=_breedingSample)
                    k = 0;
            }
        } 

        ///<summary>
        /// Create a new neural net which will be a slightly different copy of a given one.
        ///</summary>
        private LayeredNetwork CreateMutatedOffspring(LayeredNetwork neuralNet, int mutateCoef) {
            
            int[] hiddenLayersSizes = neuralNet.HiddenLayersSizes;
            Matrix[] synapses = neuralNet.GetSynapsesClone();

            // TODO LATER
            // Implemenet here mutation for new input
            // have a fixe array of sensors and an array of int containing the sensor indx to read from
            // mutate this array of int

            // mutate number of hidden layers
            if(_hiddenLayerNbMutation && Random.value < _hiddenLayerNbMutationRate) {
                if (Random.value < 0.5f && hiddenLayersSizes.Length > 1) { // random to get positive vs negative value
                    hiddenLayersSizes = RedimentionLayersNb(hiddenLayersSizes, -1);

                    synapses = RedimentionLayersNb(synapses, -1);
                    synapses[synapses.Length - 1].Redimension(hiddenLayersSizes[hiddenLayersSizes.Length - 1], neuralNet.OutputSize);

                } else {
                    hiddenLayersSizes = RedimentionLayersNb(hiddenLayersSizes, +1);
                    hiddenLayersSizes[hiddenLayersSizes.Length - 1] = neuralNet.OutputSize;

                    synapses = RedimentionLayersNb(synapses, +1);
                    float weightRange = neuralNet.StandardSynapseRange(synapses[synapses.Length - 1].J);
                    synapses[synapses.Length - 1] = new Matrix(hiddenLayersSizes[hiddenLayersSizes.Length - 1], neuralNet.OutputSize).SetAsSynapse(weightRange);
                }
            }
 
            // mutated number of neurons in hidden layers
            if(_hiddenNbMutation && Random.value < _hiddenMbMutationRate) {
                int layerNb = Random.Range(0, hiddenLayersSizes.Length - 1);
                if (Random.value < 0.5f && hiddenLayersSizes[layerNb] > 1) { // random to get positive vs negative value
                    hiddenLayersSizes[layerNb] -= 1;
                } else {
                    hiddenLayersSizes[layerNb] += 1;
                }
                // need to use the previous synapses values here as we might be going from/to oustide of the hidden layers
                synapses[layerNb].Redimension(synapses[layerNb].I, hiddenLayersSizes[layerNb]);
                synapses[layerNb+1].Redimension(hiddenLayersSizes[layerNb], synapses[layerNb+1].J);
            }

            // mutate synapses values
            for (int n=0; n<synapses.Length; n++) {

                for (int i = 0; i < synapses[n].I; i++) {
                    for (int j=0; j < synapses[n].J; j++) {
                        if (Random.value < _synapsesMutationRate) {
                            MutationType type = (MutationType)Random.Range(0, System.Enum.GetValues(typeof(MutationType)).Length-1);
                            float mutatedValue = synapses[n].GetValue(i, j);;
                            switch(type) {
                                case MutationType.additive:
                                    mutatedValue += Random.Range(-_synapsesMutationRange, _synapsesMutationRange);
                                    break;
                                case MutationType.multiply:
                                    mutatedValue *= Random.Range(1f - 5f *_synapsesMutationRange, 1f + 5f * _synapsesMutationRange);
                                    break;
                                case MutationType.reverse:
                                    mutatedValue *= -1;
                                    break;
                                case MutationType.replace:
                                    float weightRange = neuralNet.StandardSynapseRange(synapses[n].J);
                                    mutatedValue = Random.Range(-weightRange, weightRange);
                                    break;
                                case MutationType.nullify:
                                    mutatedValue = 0f;
                                    break;
                                default:
                                    Debug.LogWarning("Unknown weight mutation type. Doing nothing.");
                                    break;
                            }

                            synapses[n].SetValue(i, j, mutatedValue);  
                        }
                    }
                }
            }

            LayeredNetwork mutadedOffspring = new LayeredNetwork(neuralNet.LayersSizes);
            mutadedOffspring.InsertSynapses(synapses);

            return mutadedOffspring;
        }

        // redimension an array of in for the hidden layers
        private int[] RedimentionLayersNb (int[] currentLayers, int sizeMod) {
            int[] newLayers = new int[currentLayers.Length + sizeMod];
            for (int i = 0; i < Mathf.Min(currentLayers.Length, newLayers.Length); i++) {
                newLayers[i] = currentLayers[i];
            }

            return newLayers;
        }

        // redimension an array of matrix for the synapses
        private Matrix[] RedimentionLayersNb (Matrix[] currentLayers, int sizeMod) {
            Matrix[] newLayers = new Matrix[currentLayers.Length + sizeMod];
            for (int i = 0; i < Mathf.Min(currentLayers.Length, newLayers.Length); i++) {
                newLayers[i] = currentLayers[i];
            }

            return newLayers;
        }

        // compares a given neural network to a list of other and if better stores it at the correct rank
        // compares the network to the current generation as well as overall best network in all generations
        private void CompareFitness (LayeredNetwork[] fitnessRankings, LayeredNetwork fitnessContender) {

            // first we take care of the first case of an empty array (no other contender yet)
            if(fitnessRankings[_breedingSample-1] == null){
                fitnessRankings[_breedingSample - 1] = fitnessContender;
            } else if(fitnessRankings[_breedingSample-1] != null && fitnessRankings[_breedingSample-1].FitnessScore < fitnessContender.FitnessScore) {
                fitnessRankings[_breedingSample - 1] = fitnessContender;
            }

            // then we go through the rest of the arrays
            if (_breedingSample > 1) { // just making sure there is more than one network to breed (there can't be less)

                // we go from last to first in the loop
                for (int i = _breedingSample - 2; i >= 0; i--) {
                    if (fitnessRankings[i] == null) { // if the array is empty we fill it one step at a time
                        fitnessRankings[i] = fitnessContender;
                        fitnessRankings[i + 1] = null;
                    } else if(fitnessRankings[i].FitnessScore < fitnessContender.FitnessScore) {
                        LayeredNetwork stepDown = fitnessRankings[i];
                        fitnessRankings[i] = fitnessContender;
                        fitnessRankings[i + 1] = stepDown;

                    } else {
                        i = 0; // if the contender doesn't have a better score anymore we exit the loop
                    }
                }
            }
        }

    }
}
