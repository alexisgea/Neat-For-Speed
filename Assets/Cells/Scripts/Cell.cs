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

		public float shrinkSizePenality = 0.01f;

		public float minimumSize = 0.8f;

		public Transform body;
		public Collider2D cellCollider;
		public ContactFilter2D foodFilter;

		Rigidbody2D rb;
		ICellInput input;

		[SerializeField]
		float size = Mathf.PI;
		[SerializeField]
		Vector3 initialScale;
		[SerializeField]
		bool wasInitialized = false;

		public Cell parent;

		public float Size {
			get { return size; }
		}

		void Awake ()
		{
			rb = GetComponent<Rigidbody2D> ();
			//input = new PlayerCellInput ();
			var sensor = GetComponentInChildren<CellSensor> ();

			if (!wasInitialized) { 
				initialScale = body.localScale;
				wasInitialized = true;
			}
			input = new NeuralCellInput (sensor, this, parent != null ? (NeuralCellInput) parent.input : null);
			UpdateScale ();
		}

		void SetupChild (Cell parent)
		{
			this.parent = parent;
		}
				
		void FixedUpdate ()
		{
			rb.AddRelativeForce (Vector2.up * moveForwardForce * input.GetMoveForward ());
			rb.MoveRotation (rb.rotation + input.GetTurn () * -rotationSpeed * Time.deltaTime);

			Eat ();
			Shrink ();

			if (input.GetSplit ())
				Split ();
			
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
			size += 2;
			//UpdateScale ();
		}

		void Shrink ()
		{
			size -= (shrinkRate + size * shrinkSizePenality) * Time.deltaTime;

			if (size < minimumSize) {
				Destroy (gameObject);
			}
			//UpdateScale ();
		}

		void Split ()
		{
			size *= 0.5f;
			var child = Instantiate<Cell>(this);
			child.SetupChild (this);
		}

		void UpdateScale ()
		{
			var area = size; // for now area is equivalent to size
			var radius = Mathf.Sqrt (area / Mathf.PI);
			body.localScale = initialScale * radius;
		}
	}
}