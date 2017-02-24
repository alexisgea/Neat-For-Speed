using System.Collections.Generic;
using UnityEngine;
using nfs.car;
using nfs.controllers;
using nfs.tools;

namespace nfs.layered {
    public class SmallFilter : MonoBehaviour {

        [SerializeField] int numberOfGeneration = 100;
        public int Generation { private set; get; }

        [SerializeField] int numberOfCar = 20;
        public int CarAlive { private set; get; }

        [SerializeField] int[] baseHiddenLayersSizes;

        [SerializeField] float survivorRate = 0.25f;
        private int survivorNb;
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

        [SerializeField] Transform world;
        [SerializeField] GameObject[] tracks;
        private GameObject raceTrack;


        // Use this for initialization
        private void Start() {
            InitializePopulation();
        }

        private void Update() {
            if (CarAlive <= 0) {
                NextGeneration();
            }
        }

        public void InitializePopulation () {
            raceTrack = GameObject.Instantiate(tracks[Random.Range(0, tracks.Length-1)]);
            raceTrack.transform.SetParent(world);

            carPopulation = new GameObject[numberOfCar];

            survivorNb = (int)(numberOfCar*survivorRate);
            survivorNb = survivorNb < 1 ? 1 : survivorNb;

            //Debug.Log(survivorNb);

            alltimeFittestNets = new LayeredNetwork[survivorNb];
            generationFittestNets = new LayeredNetwork[survivorNb];

            Generation = 1;
            CarAlive = numberOfCar;
            
            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i] = GameObject.Instantiate(carPrefab, startPosition, Quaternion.identity);
                carPopulation[i].transform.SetParent(populationGroup);
                carPopulation[i].GetComponent<LayeredNetController>().InitializeNeuralNetwork(hiddenSizes:baseHiddenLayersSizes);
                carPopulation[i].GetComponent<CarBehaviour>().HitSomething += OnCarHit;
            }

            generationStartTime = Time.unscaledTime;

            //Debug.Log("Generation " + Generation);            
        }

        private void NextGeneration () {

            Debug.Log("Generation " + Generation +  " best fitness:" + generationFittestNets[0].FitnessScore + " all time best fitness:" + alltimeFittestNets[0].FitnessScore);

            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i].GetComponent<CarBehaviour>().Stop = true;
                carPopulation[i].GetComponent<CarBehaviour>().Reset(startPosition, Quaternion.identity);
            }

            Generation += 1;
            CarAlive = numberOfCar;

            BreedCurrentGeneration();

            generationStartTime = Time.unscaledTime;

            deadCars.Clear();
            
            // reset networks
            for (int i = 0; i < survivorNb; i++) {
                generationFittestNets[i] = generationFittestNets[i].GetClone();
                generationFittestNets[i].FitnessScore = 0;
            }

            Destroy(raceTrack);
            raceTrack = GameObject.Instantiate(tracks[Random.Range(0, tracks.Length-1)]);
            raceTrack.transform.SetParent(world);
            
            //Debug.Log("Generation " + Generation);                        
        }

        private void BreedCurrentGeneration() {

            int k = 0;
            int l = 0;
            for (int i=0; i < numberOfCar; i++) {
                if (i < numberOfCar*0.6) { // this generation (60%)
                    carPopulation[i].GetComponent<LayeredNetController>().SetLayeredNework(CreateMutatedOffspring(generationFittestNets[k], l+1)); // l+1 as mutate coef to make each version more propable to mutate a lot
                    l += 1;
                    if (l> numberOfCar*0.6/survivorNb){
                        l = 0;
                        k += 1;
                    }

                } else if (i < numberOfCar*0.9) { // all time generation (30% 0.6 + 0.3)
                    carPopulation[i].GetComponent<LayeredNetController>().SetLayeredNework(CreateMutatedOffspring(alltimeFittestNets[k], l+1));
                    l += 1;
                    if (l> numberOfCar*0.9/survivorNb){
                        l = 0;
                        k += 1;
                    }

                } else { // fresh blood (10%)
                    carPopulation[i].GetComponent<LayeredNetController>().InitializeNeuralNetwork(hiddenSizes:baseHiddenLayersSizes);   
                }

                if(k>=survivorNb)
                    k = 0;
            }

        } 

        private LayeredNetwork CreateMutatedOffspring(LayeredNetwork neuralNet, int mutateCoef) {
            
            int[] hiddenLayersSizes = neuralNet.GetHiddenLayersSizes();
            Matrix[] synapses = neuralNet.GetSynapsesCopy();

            // TODO LATER
            // Implemenet here mutation for new input
            // have a fixe array of sensors and an array of int containing the sensor indx to read from
            // mutate this array of int

            // mutate number of hidden layers
            if(hiddenLayerNbMutation) {
                if (Random.value < hiddenLayerNbMutationRate) {
                    //Debug.Log("Mutating number of layer.");
                    if (Random.value < 0.5f && hiddenLayersSizes.Length > 1) {
                        hiddenLayersSizes = RedimentionLayersNb(hiddenLayersSizes, -1);

                        synapses = RedimentionLayersNb(synapses, -1);
                        synapses[synapses.Length - 1].Redimension(hiddenLayersSizes[hiddenLayersSizes.Length - 1], neuralNet.GetOutputSize());

                    } else {
                        hiddenLayersSizes = RedimentionLayersNb(hiddenLayersSizes, +1);
                        hiddenLayersSizes[hiddenLayersSizes.Length - 1] = neuralNet.GetOutputSize();

                        synapses = RedimentionLayersNb(synapses, +1);
                        synapses[synapses.Length - 1] = new Matrix(hiddenLayersSizes[hiddenLayersSizes.Length - 1], neuralNet.GetOutputSize()).SetAsSynapse();
                        // not needed as we where previously going to the output and the new layer will have the same size as the output
                        //synapses[synapses.Length - 2].Redimension(hiddenLayersSizes[hiddenLayersSizes.Length - 2], hiddenLayersSizes[hiddenLayersSizes.Length - 1]);
                    }
                }
            }
 
            // mutated number of neurons in hidden layers
            if(hiddenNbMutation) {
                if (Random.value < hiddenMbMutationRate) {
                    //Debug.Log("Mutating number of neurons.");                    
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
            }

            //Debug.Log("Mutation " + Generation);

            // mutate synapses values
            for (int i=0; i<synapses.Length; i++) {
                Matrix mutationMat = new Matrix(synapses[i].I, synapses[i].J).SetAsMutator(synapsesMutationRate * mutateCoef, synapsesMutationRange * mutateCoef);
                synapses[i] = synapses[i].Add(mutationMat, true);

                //Debug.Log(synapses[i].GetValuesAsString());
            }

            LayeredNetwork mutadedOffspring = new LayeredNetwork(neuralNet.GetInputSize(), neuralNet.GetOutputSize(), hiddenLayersSizes);
            mutadedOffspring.InsertSynapses(synapses);

            //Debug.Log(synapses[0].GetValuesAsString());

            return mutadedOffspring;
        }

        private int[] RedimentionLayersNb (int[] currentLayers, int sizeMod) {
            int[] newLayers = new int[currentLayers.Length + sizeMod];
            for (int i = 0; i < Mathf.Min(currentLayers.Length, newLayers.Length); i++) {
                newLayers[i] = currentLayers[i];
            }

            return newLayers;
        }

        private Matrix[] RedimentionLayersNb (Matrix[] currentLayers, int sizeMod) {
            Matrix[] newLayers = new Matrix[currentLayers.Length + sizeMod];
            for (int i = 0; i < Mathf.Min(currentLayers.Length, newLayers.Length); i++) {
                newLayers[i] = currentLayers[i];
            }

            return newLayers;
        }

        private void OnCarHit (CarBehaviour who, string what) {
            if(what == "wall" && !deadCars.Contains(who)) {

                CarAlive -= 1;
                deadCars.Push(who);
                who.Stop = true;

                LayeredNetwork fitnessContender = who.GetComponent<LayeredNetController>().GetLayeredNetCopy();
                fitnessContender.FitnessScore = CalculateFitness(who);

                CompareFitness(alltimeFittestNets, fitnessContender);
                CompareFitness(generationFittestNets, fitnessContender);

            }
        }

        private float CalculateFitness (CarBehaviour who) {
            float distanceFitness = who.DistanceDriven / maxDistFitness;

            //Debug.Log(distanceFitness);
            float timeElapsed = (Time.unscaledDeltaTime - generationStartTime);
            float speedFitness = timeElapsed== 0? 0f : (who.DistanceDriven / timeElapsed) / who.MaxForwardSpeed;

            //Debug.Log(Time.unscaledTime - generationStartTime);

            float fitness = distanceFitness + distanceFitness * speedFitness;

            //fitness = fitness <= 0 ? 0f : fitness;

            return fitness;
        }

        private void CompareFitness (LayeredNetwork[] fitnessRankings, LayeredNetwork fitnessContender) {
            if(fitnessRankings[survivorNb-1] == null){
                fitnessRankings[survivorNb - 1] = fitnessContender;
            } else if(fitnessRankings[survivorNb-1] != null && fitnessRankings[survivorNb-1].FitnessScore < fitnessContender.FitnessScore) {
                fitnessRankings[survivorNb - 1] = fitnessContender;
            }

            if (survivorNb > 1) {
                for (int i = survivorNb - 2; i >= 0; i--) {
                    if (fitnessRankings[i] == null) { // if the array is empty we fill it one step at a time
                        fitnessRankings[i] = fitnessContender;
                        fitnessRankings[i + 1] = null;
                    } else if(fitnessRankings[i].FitnessScore < fitnessContender.FitnessScore) {
                        LayeredNetwork stepDown = fitnessRankings[i];
                        fitnessRankings[i] = fitnessContender;
                        fitnessRankings[i + 1] = stepDown;
                        // if(i==0) 
                        //     Debug.Log("New best fitness: " + fitnessContender.FitnessScore);

                    } else {
                        i = 0; // if the contender doesn't have a better score anymore we exit the loop
                    }
                }
            }
        }

    }
}
