using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nfs.cells
{
	public class Environnement : MonoBehaviour
	{

		public Transform foodPrefab;
		public Transform foodParent;
		public int foodInitialQuantity = 200;

		void Start ()
		{
			Spawn ();
			StartCoroutine (SpawnRoutine ());
		}

		void Spawn ()
		{
			for (int i = 0; i < foodInitialQuantity; i++)
			{
				SpawnOne ();
			}
		}

		IEnumerator SpawnRoutine ()
		{
			while (true) {
				yield return new WaitForSeconds (Random.Range (0f, 3f));
				SpawnOne ();
				SpawnOne ();
				SpawnOne ();
				SpawnOne ();

				yield return new WaitForSeconds (Random.Range (0f, 1f));
				SpawnOne ();
				SpawnOne ();
				SpawnOne ();
			}
		}

		void SpawnOne ()
		{
			var bounds = GetComponent<Collider2D>().bounds;
			var position = new Vector2 (Random.Range (bounds.min.x, bounds.max.x), Random.Range (bounds.min.y, bounds.max.y));
			var food = Instantiate<Transform>(foodPrefab, foodParent);
			food.position = position;
			food.localScale = foodPrefab.localScale;
		}
	}
}