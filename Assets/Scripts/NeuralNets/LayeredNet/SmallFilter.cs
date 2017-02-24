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
        [SerializeField] float survivorRate = 0.25f;
        private int survivorNb;
        [SerializeField] float synapsesMutationRate = 0.1f;
        [SerializeField] float synapsesMutationRange = 0.1f;
        [SerializeField] bool inputNbMutation = false;
        [SerializeField] bool hiddenNbMutation = false;
        [SerializeField] float hiddenMbMutationRate = 0.01f;
        [SerializeField] bool hiddenLayerNbMutation = false;
        [SerializeField] float hiddenLayerNbMutationRate = 0.001f;

        private GameObject[] carPopulation;
        private Stack<CarBehaviour> deadCars = new Stack<CarBehaviour>();
        private LayeredNetwork[] fittestNets;
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

            fittestNets = new LayeredNetwork[survivorNb];

            Generation = 1;
            CarAlive = numberOfCar;
            
            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i] = GameObject.Instantiate(carPrefab, startPosition, Quaternion.identity);
                carPopulation[i].transform.SetParent(populationGroup);
                carPopulation[i].GetComponent<LayeredNetController>().InitializeNeuralNetwork();
                carPopulation[i].GetComponent<CarBehaviour>().HitSomething += OnCarHit;
            }

            generationStartTime = Time.unscaledTime;

            Debug.Log("Generation " + Generation);            
        }

        private void NextGeneration () {

            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i].GetComponent<CarBehaviour>().Stop = true;
                carPopulation[i].GetComponent<CarBehaviour>().Reset(startPosition, Quaternion.identity);
            }

            Generation += 1;
            CarAlive = numberOfCar;

            BreedCurrentGeneration();

            generationStartTime = Time.unscaledTime;

            deadCars.Clear();

            Destroy(raceTrack);
            raceTrack = GameObject.Instantiate(tracks[Random.Range(0, tracks.Length-1)]);
            raceTrack.transform.SetParent(world);
            
            Debug.Log("Generation " + Generation);                        
        }

        private void BreedCurrentGeneration() {

            int[] survivorsOffspringsNb = new int[survivorNb];

            int totalChecker = 0;
            for(int i = 0; i<survivorNb; i++){

                if (i+1 > survivorNb*0.4f){
                    survivorsOffspringsNb[i] = (int)(Mathf.Floor(numberOfCar*0.1f)/Mathf.Ceil(survivorNb*0.5f));
                    survivorsOffspringsNb[i] = survivorsOffspringsNb[i] < 1 ? 1 : survivorsOffspringsNb[i];
                    totalChecker += survivorsOffspringsNb[i];

                } else if (i+1 > survivorNb*0.3f){
                    survivorsOffspringsNb[i] = (int)(Mathf.Floor(numberOfCar*0.2f)/Mathf.Ceil(survivorNb*0.3f));
                    survivorsOffspringsNb[i] = survivorsOffspringsNb[i] < 1 ? 1 : survivorsOffspringsNb[i];
                    totalChecker += survivorsOffspringsNb[i];

                } else if (i+1 > survivorNb*0.2f){
                    survivorsOffspringsNb[i] = (int)(Mathf.Floor(numberOfCar*0.3f)/Mathf.Ceil(survivorNb*0.2f));
                    survivorsOffspringsNb[i] = survivorsOffspringsNb[i] < 1 ? 1 : survivorsOffspringsNb[i];
                    totalChecker += survivorsOffspringsNb[i];

                } else {
                    survivorsOffspringsNb[i] = (int)(Mathf.Floor(numberOfCar*0.4f)/Mathf.Ceil(survivorNb*0.1f));
                    survivorsOffspringsNb[i] = survivorsOffspringsNb[i] < 1 ? 1 : survivorsOffspringsNb[i];
                    totalChecker += survivorsOffspringsNb[i];

                }

                //Debug.Log(survivorsOffsprings[i]);
            }

            if (numberOfCar - totalChecker < 0) {
                Debug.LogError("BreedCurrentGeneration algorythm broken, too many offsprings assigned: " + totalChecker);
            } else {
                survivorsOffspringsNb[0] += numberOfCar - totalChecker;
                //Debug.Log(totalChecker);
            }

            int k = 0;
            for (int i=0; i < numberOfCar; i++) {

                carPopulation[i].GetComponent<LayeredNetController>().SetLayeredNework(CreateMutatedOffspring(fittestNets[k]));

                survivorsOffspringsNb[k] -= 1;
                if (survivorsOffspringsNb[k] == 0) {
                    k += 1;
                }
            }

        } 

        private LayeredNetwork CreateMutatedOffspring(LayeredNetwork neuralNet) {
            
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
                Matrix mutationMat = new Matrix(synapses[i].I, synapses[i].J).SetAsMutator(synapsesMutationRate, synapsesMutationRange);
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

                CheckFitness(who);
            }
        }

        private void CheckFitness (CarBehaviour who) {
            float distanceFitness = who.DistanceDriven / maxDistFitness;
            float speedFitness = (who.DistanceDriven / (Time.unscaledDeltaTime - generationStartTime)) / who.MaxForwardSpeed;
            float fitness = distanceFitness + distanceFitness * speedFitness;

            LayeredNetwork fitnessContender = who.GetComponent<LayeredNetController>().GetLayeredNetCopy();
            fitnessContender.FitnessScore = fitness;

            if(fittestNets[survivorNb-1] != null && fittestNets[survivorNb-1].FitnessScore < fitnessContender.FitnessScore) {
                fittestNets[survivorNb - 1] = fitnessContender;
            }

            if (survivorNb > 1) {
                for (int i = survivorNb - 2; i >= 0; i--) {
                    if (fittestNets[i] == null) { // if the array is empty we fill it one step at a time
                        fittestNets[i] = fitnessContender;
                        fittestNets[i + 1] = null;
                    } else if(fittestNets[i].FitnessScore < fitnessContender.FitnessScore) {
                        LayeredNetwork stepDown = fittestNets[i];
                        fittestNets[i] = fitnessContender;
                        fittestNets[i + 1] = stepDown;
                    } else {
                        i = 0; // if the contender doesn't have a better score anymore we exit the loop
                    }
                }
            }
            

        }

    }
}
