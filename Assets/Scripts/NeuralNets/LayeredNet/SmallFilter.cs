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
    public class SmallFilter : MonoBehaviour {

        
        [SerializeField] int numberOfGeneration = 100;
        [SerializeField] float maxGenerationTime = 90;
        public int Generation { private set; get; }

        [SerializeField] int numberOfCar = 20;
        public int CarAlive { private set; get; }

        [SerializeField] int[] baseHiddenLayersSizes;

        [SerializeField] float survivorRate = 0.25f;
        private int breedingSample;
        [SerializeField] float freshBloodRate = 0.1f;
        [SerializeField] float synapsesMutationRate = 0.1f;
        [SerializeField] float synapsesMutationRange = 0.1f;
        [SerializeField] bool inputNbMutation = false;
        [SerializeField] bool hiddenNbMutation = false;
        [SerializeField] float hiddenMbMutationRate = 0.01f;
        [SerializeField] bool hiddenLayerNbMutation = false;
        [SerializeField] float hiddenLayerNbMutationRate = 0.001f;

        private GameObject[] carPopulation;
        private Stack<CarBehaviour> deadCars = new Stack<CarBehaviour>();
        private LayeredNetwork[] alltimeFittestNets;
        private LayeredNetwork[] generationFittestNets;
        [SerializeField] Transform populationGroup;
        [SerializeField] Vector3 startPosition = Vector3.zero;
        [SerializeField] GameObject carPrefab;

        [SerializeField] float maxDistFitness = 1000;
        private float generationStartTime;

        // world and race tracks references
        [SerializeField] Transform world;
        [SerializeField] GameObject[] tracks;
        private GameObject raceTrack;


        // Initialises the base popuplations
        private void Start() {
            InitializePopulation();
        }

        // starts the next generation when the time has come
        private void Update() {
            if (CarAlive <= 0 || Time.unscaledTime - generationStartTime > maxGenerationTime) {
                NextGeneration();
            }
        }

        ///<summary>
        /// Instantiate the base popuplation and initialises their neural networks
        ///</summary>
        public void InitializePopulation () {
            // create a new racetrack form the list
            raceTrack = GameObject.Instantiate(tracks[Random.Range(0, tracks.Length-1)]);
            raceTrack.transform.SetParent(world);

            // compute the amount of network which will have a descendance in each generation
            breedingSample = (int)(numberOfCar*survivorRate);
            breedingSample = breedingSample < 1 ? 1 : breedingSample;
            
            // creates the population
            carPopulation = new GameObject[numberOfCar];
            alltimeFittestNets = new LayeredNetwork[breedingSample];
            generationFittestNets = new LayeredNetwork[breedingSample];

            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i] = GameObject.Instantiate(carPrefab, startPosition, Quaternion.identity);
                carPopulation[i].transform.SetParent(populationGroup);
                carPopulation[i].GetComponent<LayeredNetController>().InitializeNeuralNetwork(hiddenSizes:baseHiddenLayersSizes);
                carPopulation[i].GetComponent<CarBehaviour>().HitSomething += OnCarHit; // we register to each car's signal for collision
            }

            // all is ready and set to start for the training
            Generation = 1;
            CarAlive = numberOfCar;
            generationStartTime = Time.unscaledTime;
        }

        ///<summary>
        /// Initiate the new generation by breeding offspring and inserting them in the current popuplation.
        ///</summary>
        private void NextGeneration () {
            
            Debug.Log("Generation " + Generation +  " best fitness:" + generationFittestNets[0].FitnessScore + " all time best fitness:" + alltimeFittestNets[0].FitnessScore);

            // we make sure everything is stopped and reset their state
            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i].GetComponent<CarBehaviour>().Stop = true;
                carPopulation[i].GetComponent<CarBehaviour>().Reset(startPosition, Quaternion.identity);
            }

            // we breed the next generation from the best cars
            BreedNextGeneration();
            // we make sure dead cars are cleared now that there is a new generation
            deadCars.Clear();
            // we also make sure that the generation fittest array can be beaten by the next one (easier than nullifying)
            for (int i = 0; i < breedingSample; i++) {
                generationFittestNets[i].FitnessScore = 0;
            }

            // we create a new track to avoid overfitting
            Destroy(raceTrack);
            raceTrack = GameObject.Instantiate(tracks[Random.Range(0, tracks.Length-1)]);
            raceTrack.transform.SetParent(world);
            
            // all is ready and set to start for the training
            Generation += 1;
            CarAlive = numberOfCar;
            generationStartTime = Time.unscaledTime;
        }

        ///<summary>
        /// Breed the next generation of networks from the current best and overall best.
        ///</summary>
        private void BreedNextGeneration() {

            int k = 0;
            int l = 0;
            for (int i=0; i < numberOfCar; i++) {
                if (i < numberOfCar*0.6) { // all time generation (60%)
                    carPopulation[i].GetComponent<LayeredNetController>().SetLayeredNework(CreateMutatedOffspring(alltimeFittestNets[k], l+1)); // l+1 as mutate coef to make each version more propable to mutate a lot
                    l += 1;
                    if (l> numberOfCar*0.6/breedingSample){
                        l = 0;
                        k += 1;
                    }

                } else if (i < numberOfCar*0.9) { // this generation (30% 0.6 + 0.3)
                    carPopulation[i].GetComponent<LayeredNetController>().SetLayeredNework(CreateMutatedOffspring(generationFittestNets[k], l+1));
                    l += 1;
                    if (l> numberOfCar*0.9/breedingSample){
                        l = 0;
                        k += 1;
                    }

                } else { // fresh blood (10%)
                    carPopulation[i].GetComponent<LayeredNetController>().InitializeNeuralNetwork(hiddenSizes:baseHiddenLayersSizes);   
                }

                if(k>=breedingSample)
                    k = 0;
            }
        } 

        ///<summary>
        /// Create a new neural net which will be a slightly different copy of a given one.
        ///</summary>
        private LayeredNetwork CreateMutatedOffspring(LayeredNetwork neuralNet, int mutateCoef) {
            
            int[] hiddenLayersSizes = neuralNet.GetHiddenLayersSizes();
            Matrix[] synapses = neuralNet.GetSynapsesClone();

            // TODO LATER
            // Implemenet here mutation for new input
            // have a fixe array of sensors and an array of int containing the sensor indx to read from
            // mutate this array of int

            // mutate number of hidden layers
            if(hiddenLayerNbMutation && Random.value < hiddenLayerNbMutationRate) {
                if (Random.value < 0.5f && hiddenLayersSizes.Length > 1) {
                    hiddenLayersSizes = RedimentionLayersNb(hiddenLayersSizes, -1);

                    synapses = RedimentionLayersNb(synapses, -1);
                    synapses[synapses.Length - 1].Redimension(hiddenLayersSizes[hiddenLayersSizes.Length - 1], neuralNet.GetOutputSize());

                } else {
                    hiddenLayersSizes = RedimentionLayersNb(hiddenLayersSizes, +1);
                    hiddenLayersSizes[hiddenLayersSizes.Length - 1] = neuralNet.GetOutputSize();

                    synapses = RedimentionLayersNb(synapses, +1);
                    synapses[synapses.Length - 1] = new Matrix(hiddenLayersSizes[hiddenLayersSizes.Length - 1], neuralNet.GetOutputSize()).SetAsSynapse();
                }
            }
 
            // mutated number of neurons in hidden layers
            if(hiddenNbMutation && Random.value < hiddenMbMutationRate) {
                int layerNb = Random.Range(0, hiddenLayersSizes.Length - 1);
                if (Random.value < 0.5f && hiddenLayersSizes[layerNb] > 1) {
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
                        if (Random.value < synapsesMutationRate) {
                            MutationType type = (MutationType)Random.Range(0, System.Enum.GetValues(typeof(MutationType)).Length-1);
                            switch(type) {
                                case MutationType.additive:
                                    synapses[n].Mtx[i][j] += Random.Range(-synapsesMutationRange, synapsesMutationRange);
                                    break;
                                case MutationType.multiply:
                                    synapses[n].Mtx[i][j] *= Random.Range(0.5f, 1.5f);
                                    break;
                                case MutationType.reverse:
                                    synapses[n].Mtx[i][j] *= -1;
                                    break;
                                case MutationType.replace:
                                    synapses[n].Mtx[i][j] = Random.Range(-2 / Mathf.Sqrt(synapses[n].J), 2 / Mathf.Sqrt(synapses[n].J));
                                    break;
                                case MutationType.nullify:
                                    synapses[n].Mtx[i][j] = 0f;
                                    break;
                                default:
                                    Debug.LogWarning("Unknown weight mutation type. Doing nothing.");
                                    break;
                            }  
                        }
                    }
                }
            }

            LayeredNetwork mutadedOffspring = new LayeredNetwork(neuralNet.GetInputSize(), neuralNet.GetOutputSize(), hiddenLayersSizes);
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

        ///<summary>
        /// Called by each car's colision.
        /// Updates the car alive counter and add the car to the deadCars array to not count them multiple tiem.
        ///</summary>
        private void OnCarHit (CarBehaviour who, string what) {
            // first we make sure the car hit a wall
            if(what == "wall" && !deadCars.Contains(who)) {

                CarAlive -= 1;
                deadCars.Push(who);
                who.Stop = true;

                // we retrive the neural network from the car and check it's fitness
                LayeredNetwork fitnessContender = who.GetComponent<LayeredNetController>().GetLayeredNetClone();
                fitnessContender.FitnessScore = CalculateFitness(who);

                // we then compare the fitness to the current best and store/sort it if good
                CompareFitness(alltimeFittestNets, fitnessContender);
                CompareFitness(generationFittestNets, fitnessContender);

            }
        }

        // compute the fitness value of a network from a mix distance and average speed
        // distance is more important than speed
        private float CalculateFitness (CarBehaviour who) {
            float distanceFitness = who.DistanceDriven / maxDistFitness;

            float timeElapsed = (Time.unscaledDeltaTime - generationStartTime);
            float speedFitness = timeElapsed== 0? 0f : (who.DistanceDriven / timeElapsed) / who.MaxForwardSpeed;

            float fitness = distanceFitness + distanceFitness * speedFitness;

            return fitness;
        }

        // compares a given neural network to a list of other and if better stores it at the correct rank
        // compares the network to the current generation as well as overall best network in all generations
        private void CompareFitness (LayeredNetwork[] fitnessRankings, LayeredNetwork fitnessContender) {

            // first we take care of the first case of an empty array (no other contender yet)
            if(fitnessRankings[breedingSample-1] == null){
                fitnessRankings[breedingSample - 1] = fitnessContender;
            } else if(fitnessRankings[breedingSample-1] != null && fitnessRankings[breedingSample-1].FitnessScore < fitnessContender.FitnessScore) {
                fitnessRankings[breedingSample - 1] = fitnessContender;
            }

            // then we go through the rest of the arrays
            if (breedingSample > 1) { // just making sure there is more than one network to breed (there can't be less)

                // we go from last to first in the loop
                for (int i = breedingSample - 2; i >= 0; i--) {
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
