using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {
	/// <summary>
	/// Maximum force added in newton/second (I think :P )
	/// </summary>
	public float moveForwardForce = 2;

	/// <summary>
	/// Maximum rotation speed in degrees/seconds
	/// </summary>
	public float rotationSpeed = 180f;

	public Collider2D cellCollider;
	public ContactFilter2D foodFilter;

	Rigidbody2D rb;
	ICellInput input;

	void Awake ()
	{
		rb = GetComponent<Rigidbody2D> ();
		input = new PlayerCellInput ();
	}
			
	void FixedUpdate ()
	{
		rb.AddRelativeForce (Vector2.up * moveForwardForce * input.GetMoveForward ());
		rb.MoveRotation (rb.rotation + input.GetTurn () * -rotationSpeed * Time.deltaTime);

		Eat ();
	}

	void Eat ()
	{
		print ("yo"); 
		var results = new Collider2D[10];
		int count = cellCollider.OverlapCollider (foodFilter, results);

		for (int i = 0; i < count; i++) {
			Destroy (results[i].gameObject);
		}
	}
}
