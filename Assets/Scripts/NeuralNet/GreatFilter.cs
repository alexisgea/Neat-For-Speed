using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace airace {
	public class GreatFilter : MonoBehaviour {

		[SerializeField] int numberOfGeneration;
		public int generation {private set; get;}
		[SerializeField] int numberOfCar;

		[SerializeField] CarBehaviour car;
		[SerializeField] BasicNeuralNet[] neuralNet;


		// Use this for initialization
		void Start () {
			
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
