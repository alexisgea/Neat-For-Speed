using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nfs.car;
using nfs.tools;

namespace nfs.layered {
    public class SmallFilter : MonoBehaviour {

        [SerializeField] int numberOfGeneration = 100;
        public int Generation { private set; get; }

        [SerializeField] int numberOfCar = 20;
        public int CarAlive { private set; get; }
        [SerializeField] float survivorRate = 0.25f;
        [SerializeField] float survivorDisparity = 0.5f;
        [SerializeField] float mutationRate = 0.1f;
        [SerializeField] float mutationRange = 0.1f;
        [SerializeField] bool inputNbMutation = false;
        [SerializeField] bool hiddenNbMutation = false;
        [SerializeField] float hiddenMbMutationRate = 0.01f;
        [SerializeField] bool hiddenLayerNbMutation = false;
        [SerializeField] float hiddenLayerNbMutationRate = 0.001f;

        private GameObject[] carPopulation;
        private Stack<CarBehaviour> deadCars;
        [SerializeField] Transform populationGroup;
        [SerializeField] Vector3 startPosition = Vector3.zero;
        [SerializeField] GameObject carPrefab;
        

        // Use this for initialization
        private void Start() {
            
        }

        private void Update() {
            if (CarAlive <= 0) {
                // 
            }
        }

        public void InitializePopulation () {
            carPopulation = new GameObject[numberOfCar];

            Generation = 1; // put that in a reset function
            CarAlive = numberOfCar; // put that in a reset function

            carPopulation[0] = GameObject.Instantiate(carPrefab, startPosition, Quaternion.identity);
            
            for (int i=0; i <= numberOfCar; i++) {
                carPopulation[i] = GameObject.Instantiate(carPrefab, startPosition, Quaternion.identity);
                carPopulation[i].transform.SetParent(populationGroup);
                carPopulation[i].GetComponent<LayeredNetController>().InitializeNeuralNetwork();
                carPopulation[i].GetComponent<CarBehaviour>().HitSomething += OnCarHit;
            }
        }

        private void NextGeneration () {

        }

        private void BreedCurrentGeneration() {
            int survivorNb = (int)(numberOfCar*survivorRate);
            LayeredNetwork[] survivorNets = new LayeredNetwork[survivorNb];
            for (int i = 0; i < survivorNb; i++) {
                survivorNets[i] = deadCars.Pop().GetComponent<LayeredNetController>().GetLayeredNetCopy();
            }

            // NOTE FOR LATER
            // loop through all networks and insert a mutated net
            int k = 0;
            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i].GetComponent<LayeredNetController>().SetLayeredNework(CreateMutatedOffspring(survivorNets[k])); // IS THIS PASS BY REFERENCE
            }

        } 

        private LayeredNetwork CreateMutatedOffspring(LayeredNetwork neuralNet) {
            
            int[] hiddenLayersSizes = neuralNet.GetHiddenLayersSizes();
            Matrix[] synapses = neuralNet.GetSynapsesCopy();

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

            // mutate synapses values
            for (int i=0; i<synapses.Length; i++) {
                //
            }

            LayeredNetwork mutadedOffspring = new LayeredNetwork(neuralNet.GetInputSize(), neuralNet.GetOutputSize(), hiddenLayersSizes);
            mutadedOffspring.InsertSynapes(synapses);

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
            if(what == "wall") {
                CarAlive -= 1;
                deadCars.Push(who);                
            }
        }

        // create a list of registered car

        // when all cars are dead (or simulation end) sort the cars by there fitness
        // select the best 10% reistantiate a selection of all these cars to the max
        // run the new lot throught the genetic algorythm
        // increase generation counter and start the simulation again
        // do this for the given number of generation
        // NOTES
        // best way for this is maybe to get a copy of the networks of these cars
        // reinstantiate new blanc cars and copy the saved network in them
        // like this we can also keep track of the generation evolution

        // genertic algorythm, select a random number of weight and apply a random mutation


    }
}
