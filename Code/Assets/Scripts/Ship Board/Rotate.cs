using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {
	public float xspeed;
	public float yspeed;
	public float zspeed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (Vector3.right * xspeed + Vector3.up * zspeed + Vector3.forward * yspeed);
	}
}
