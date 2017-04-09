using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CellSensor : MonoBehaviour {

	public BoxCollider2D sensor;
	public float radius;

	ContactFilter2D contactFilter;

	Collider2D[] overlapping;

	void Awake ()
	{
		contactFilter = new ContactFilter2D ();
		overlapping = new Collider2D[10];
	}

	void Update ()
	{
		Evaluate ();
	}

	public float Evaluate ()
	{
		float totalWeight = 0f;

		int count = sensor.OverlapCollider (contactFilter, overlapping);
		for (int i = 0; i < count; i++) {
			var distance = Vector2.Distance (transform.position, overlapping [i].transform.position);
			var weight = 1 - (distance / radius);
			Debug.DrawLine (transform.position, overlapping [i].transform.position, Color.yellow);
			totalWeight += weight;
			//Debug.Log (overlapping [i].name);

			//DrawText ((transform.position + overlapping [i].transform.position) * 0.5f, weight.ToString ());
		}

		//Debug.Log (totalWeight);
		return totalWeight;
	}

//	void OnDrawGizmos ()
//	{
//		DrawText (transform.position, "hey!");
//
//
//	}
//
//	void DrawText (Vector3 position, string text)
//	{
//		var center = (Vector2) Camera.current.WorldToScreenPoint (position);
//		center = new Vector2 (center.x, Camera.current.pixelHeight - center.y);
//
//		var size = new Vector2 (200f, 50f);
//		var rect = new Rect (center - (size*0.5f), size);
//
//		var centeredStyle = GUI.skin.GetStyle("Label");
//		centeredStyle.alignment = TextAnchor.UpperCenter;
//
//		Handles.BeginGUI ();
//		//GUILayout.BeginArea (rect);
//		GUI.Label (rect, text, centeredStyle);
//		//GUILayout.EndArea ();
//		Handles.EndGUI ();
//	}
}
