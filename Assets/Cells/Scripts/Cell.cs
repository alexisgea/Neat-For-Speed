using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nfs.cells
{
	public class Cell : MonoBehaviour
	{
		/// <summary>
		/// Maximum force added in newton/second (I think :P )
		/// </summary>
		public float moveForwardForce = 2;

		/// <summary>
		/// Maximum rotation speed in degrees/seconds
		/// </summary>
		public float rotationSpeed = 180f;

		/// <summary>
		/// Amount of size lost per second
		/// </summary>
		public float shrinkRate = 0.5f;

		public float minimumSize = 1f;

		public Transform body;
		public Collider2D cellCollider;
		public ContactFilter2D foodFilter;

		Rigidbody2D rb;
		ICellInput input;

		float size = Mathf.PI;
		Vector3 initialScale;

		void Awake ()
		{
			rb = GetComponent<Rigidbody2D> ();
			input = new PlayerCellInput ();

			initialScale = body.localScale;
		}
				
		void FixedUpdate ()
		{
			rb.AddRelativeForce (Vector2.up * moveForwardForce * input.GetMoveForward ());
			rb.MoveRotation (rb.rotation + input.GetTurn () * -rotationSpeed * Time.deltaTime);

			Eat ();
			Shrink ();
			UpdateScale ();
		}

		void Eat ()
		{
			var results = new Collider2D[10];
			int count = cellCollider.OverlapCollider (foodFilter, results);

			for (int i = 0; i < count; i++) {
				Destroy (results[i].gameObject);
				Grow ();
			}
		}

		void Grow ()
		{
			size += 1;
			//UpdateScale ();
		}

		void Shrink ()
		{
			size -= shrinkRate * Time.deltaTime;
			if (size < minimumSize) {
				Destroy (gameObject);
			}
			//UpdateScale ();
		}

		void UpdateScale ()
		{
			var area = size; // for now area is equivalent to size
			var radius = Mathf.Sqrt (area / Mathf.PI);
			body.localScale = initialScale * radius;
		}
	}
}