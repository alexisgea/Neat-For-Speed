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

	private Rigidbody2D rb;
	private ICellInput input;

	void Awake ()
	{
		rb = GetComponent<Rigidbody2D> ();
		input = new PlayerCellInput ();
	}
			
	void FixedUpdate () {
		rb.AddRelativeForce (Vector2.up * moveForwardForce * input.GetMoveForward ());
		rb.MoveRotation (rb.rotation + input.GetTurn () * -rotationSpeed * Time.deltaTime);
	}
}
