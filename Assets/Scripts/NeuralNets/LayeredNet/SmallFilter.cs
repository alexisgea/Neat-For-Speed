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
        [SerializeField] float synapsesMutationRate = 0.1f;
        [SerializeField] float synapsesMutationRange = 0.1f;
        [SerializeField] bool inputNbMutation = false;
        [SerializeField] bool hiddenNbMutation = false;
        [SerializeField] float hiddenMbMutationRate = 0.01f;
        [SerializeField] bool hiddenLayerNbMutation = false;
        [SerializeField] float hiddenLayerNbMutationRate = 0.001f;

        private GameObject[] carPopulation;
        private Stack<CarBehaviour> deadCars = new Stack<CarBehaviour>();
        [SerializeField] Transform populationGroup;
        [SerializeField] Vector3 startPosition = Vector3.zero;
        [SerializeField] GameObject carPrefab;
        

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
            carPopulation = new GameObject[numberOfCar];

            Generation = 1;
            CarAlive = numberOfCar;
            
            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i] = GameObject.Instantiate(carPrefab, startPosition, Quaternion.identity);
                carPopulation[i].transform.SetParent(populationGroup);
                carPopulation[i].GetComponent<LayeredNetController>().InitializeNeuralNetwork();
                carPopulation[i].GetComponent<CarBehaviour>().HitSomething += OnCarHit;
            }

            Debug.Log("Generation " + Generation);            
        }

        private void NextGeneration () {

            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i].GetComponent<CarBehaviour>().Stop = true;
                carPopulation[i].GetComponent<CarBehaviour>().Reset(Vector3.zero, Quaternion.identity);
            }

            Generation += 1;
            CarAlive = numberOfCar;

            BreedCurrentGeneration();

            deadCars.Clear();
            
            Debug.Log("Generation " + Generation);                        
        }

        private void BreedCurrentGeneration() {
            int survivorNb = (int)(numberOfCar*survivorRate);
            survivorNb = survivorNb < 1 ? 1 : survivorNb;

            LayeredNetwork[] survivorNets = new LayeredNetwork[survivorNb];
            for (int i = 0; i < survivorNb; i++) {
                survivorNets[i] = deadCars.Pop().GetComponent<LayeredNetController>().GetLayeredNetCopy();
            }

            int[] survivorsOffsprings = new int[survivorNb];

            int totalChecker = 0;
            for(int i = 0; i<survivorNb; i++){

                if (i+1 > survivorNb*0.5f){
                    survivorsOffsprings[i] = (int)(Mathf.Floor(numberOfCar*0.1f)/Mathf.Ceil(survivorNb*0.5f));
                    survivorsOffsprings[i] = survivorsOffsprings[i] < 1 ? 1 : survivorsOffsprings[i];
                    totalChecker += survivorsOffsprings[i];

                } else if (i+1 > survivorNb*0.3f){
                    survivorsOffsprings[i] = (int)(Mathf.Floor(numberOfCar*0.2f)/Mathf.Ceil(survivorNb*0.3f));
                    survivorsOffsprings[i] = survivorsOffsprings[i] < 1 ? 1 : survivorsOffsprings[i];
                    totalChecker += survivorsOffsprings[i];

                } else if (i+1 > survivorNb*0.2f){
                    survivorsOffsprings[i] = (int)(Mathf.Floor(numberOfCar*0.3f)/Mathf.Ceil(survivorNb*0.2f));
                    survivorsOffsprings[i] = survivorsOffsprings[i] < 1 ? 1 : survivorsOffsprings[i];
                    totalChecker += survivorsOffsprings[i];

                } else {
                    survivorsOffsprings[i] = (int)(Mathf.Floor(numberOfCar*0.5f)/Mathf.Ceil(survivorNb*0.1f));
                    survivorsOffsprings[i] = survivorsOffsprings[i] < 1 ? 1 : survivorsOffsprings[i];
                    totalChecker += survivorsOffsprings[i];

                }

                //Debug.Log(survivorsOffsprings[i]);
            }

            if (numberOfCar - totalChecker < 0) {
                Debug.LogError("BreedCurrentGeneration algorythm broken, too many offsprings assigned.");
            } else {
                survivorsOffsprings[0] += numberOfCar - totalChecker;
            }

            int k = 0;
            for (int i=0; i < numberOfCar; i++) {

                carPopulation[i].GetComponent<LayeredNetController>().SetLayeredNework(CreateMutatedOffspring(survivorNets[k]));

                survivorsOffsprings[k] -= 1;
                if (survivorsOffsprings[k] == 0) {
                    k += 1;
                }
            }

        } 

        private LayeredNetwork CreateMutatedOffspring(LayeredNetwork neuralNet) {
            
            int[] hiddenLayersSizes = neuralNet.GetHiddenLayersSizes();
            Matrix[] synapses = neuralNet.GetSynapsesCopy();

            // TODO LATER
            // Implemenet here mutation for new input and output

            // mutate number of hidden layers
            if(hiddenLayerNbMutation) {
                if (Random.value < hiddenLayerNbMutationRate) {
                    if (Random.value < 0.5f && hiddenLayersSizes.Length > 1) {
                        hiddenLayersSizes = RedimLayersSizes(hiddenLayersSizes, hiddenLayersSizes.Length - 1);
                        synapses[synapses.Length - 1].Redimension(synapses[synapses.Length - 1].I, synapses[synapses.Length - 1].J - 1);
                    } else {
                        hiddenLayersSizes = RedimLayersSizes(hiddenLayersSizes, hiddenLayersSizes.Length + 1);
                        hiddenLayersSizes[hiddenLayersSizes.Length - 1] = neuralNet.GetOutputSize();
                        synapses[synapses.Length - 1].Redimension(synapses[synapses.Length - 1].I, synapses[synapses.Length - 1].J + 1);
                    }
                }
            }

            // mutated number of neurons in hidden layers
            if(hiddenNbMutation) {
                if (Random.value > hiddenMbMutationRate) {
                    int layerNb = Random.Range(0, hiddenLayersSizes.Length - 1);
                    if (Random.value < 0.5f && hiddenLayersSizes[layerNb] > 1) {
                        hiddenLayersSizes[layerNb] -= 1;
                    } else {
                        hiddenLayersSizes[layerNb] += 1;
                    }
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
            mutadedOffspring.InsertSynapes(synapses);

            //Debug.Log(synapses[0].GetValuesAsString());

            return mutadedOffspring;
        }

        private int[] RedimLayersSizes (int[] currentLayer, int newSize) {
            int[] newLayer = new int[newSize];
            for (int i = 0; i < Mathf.Min(currentLayer.Length, newSize); i++) {
                newLayer[i] = currentLayer[i];
            }

            return newLayer;
        }

        private void OnCarHit (CarBehaviour who, string what) {
            if(what == "wall" && !deadCars.Contains(who)) {
                CarAlive -= 1;
                deadCars.Push(who);
                who.Stop = true;
            }
        }

    }
}
