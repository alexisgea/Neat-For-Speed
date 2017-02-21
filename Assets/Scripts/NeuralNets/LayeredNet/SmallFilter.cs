using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nfs.car;

namespace nfs.layered {
    public class SmallFilter : MonoBehaviour {

        [SerializeField] int numberOfGeneration;
        public int Generation { private set; get; }

        [SerializeField] int numberOfCar = 20;
        public int CarAlive { private set; get; }
        [SerializeField] float survivorRate = 0.3f;
        [SerializeField] float survivorDisparity = 0.5f;
        [SerializeField] float mutationRate = 0.1f;
        [SerializeField] float mutationRange = 0.1f;
        [SerializeField] bool inputNbMutation = false;
        [SerializeField] bool hiddenNbMutation = false;
        [SerializeField] bool layerMutation = false;

        private int[] theChoosenIdx;
        private GameObject[] carPopulation;
        [SerializeField] Transform populationGroup;
        [SerializeField] Vector3 startPosition;
        [SerializeField] GameObject carPrefab;
        

        // Use this for initialization
        private void Start() {
            
        }

        private void InitializePopulation () {
            carPopulation = new GameObject[numberOfCar];

            Generation = 1; // put that in a reset function
            CarAlive = numberOfCar; // put that in a reset function

            carPopulation[0] = GameObject.Instantiate(carPrefab, startPosition, Quaternion.identity);
            carPopulation[0].transform.SetParent(populationGroup);
            carPopulation[0].GetComponent<VariableLayeredNet>().InitializeNeuralNetwork();
            carPopulation[0].GetComponent<CarBehaviour>().HitSomething += OnCarHit;
            
            for (int i=0; i <= numberOfCar; i++) {

            }
        }

        // NOTE FOR LATER
        // add the car to a list of dead object to have them in the order of death in advance
        // just go from the last to have the best ones
        private void OnCarHit (CarBehaviour who, string what) {
            if(what == "wall") {
                CarAlive -= 1;                
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
