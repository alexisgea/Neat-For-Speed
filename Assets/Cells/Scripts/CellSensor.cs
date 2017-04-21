using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace nfs.cells
{
	public class CellSensor : MonoBehaviour
	{
		public Collider2D sensor;
		public float radius;

		public ContactFilter2D contactFilter;

		Collider2D[] overlapping;

		private float a;


		void Awake ()
		{
			overlapping = new Collider2D[20];
		}

		void Update ()
		{
			a = Evaluate (0);
		}

		public float Evaluate (float degrees)
		{
			float totalWeight = 0f;

			int count = sensor.OverlapCollider (contactFilter, overlapping);
			for (int i = 0; i < count; i++) {
				var delta = overlapping [i].transform.position - transform.position;
				var dot = Vector2.Dot (delta.normalized, Quaternion.Euler (0, 0, degrees) * transform.up);
				var alignedFactor = Mathf.Clamp01 (dot);

				var distance = delta.magnitude;

				var weight = alignedFactor * (1 - (distance / radius));
				Debug.DrawLine (transform.position, overlapping [i].transform.position, Color.yellow);
				totalWeight += weight * 1f;
				//Debug.Log (overlapping [i].name);

				//DrawText ((transform.position + overlapping [i].transform.position) * 0.5f, weight.ToString ());
			}

			//Debug.Log (totalWeight);

			return totalWeight;
		}

		private float Sigmoid(float t) {
			return 1f / (1f + Mathf.Exp(-t));
		}


		void OnDrawGizmos ()
		{
			DrawText (transform.position, a.ToString ());
		}
		
		void DrawText (Vector3 position, string text)
		{
			var center = (Vector2) Camera.current.WorldToScreenPoint (position);
			center = new Vector2 (center.x, Camera.current.pixelHeight - center.y - 10f);
			
			var size = new Vector2 (200f, 50f);
			var rect = new Rect (center - (size*0.5f), size);
			
			var centeredStyle = GUI.skin.GetStyle("Label");
			centeredStyle.alignment = TextAnchor.UpperCenter;
			
			Handles.BeginGUI ();
			//GUILayout.BeginArea (rect);
			GUI.Label (rect, text, centeredStyle);
			//GUILayout.EndArea ();
			Handles.EndGUI ();
		}
	}
}
