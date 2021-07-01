/* Purpose: Adds the controls to move the camera around */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	public float speed = 0f;

	// Update is called once per frame
	void Update () {
		// If statements are used over else if so the user can move the camera in multiple directions.
		if(Input.GetKey("d")) {
			transform.Translate(Vector3.right * speed);
		}
		if(Input.GetKey("a")) {
			transform.Translate(Vector3.left * speed);
		}
		if(Input.GetKey("s")) {
			transform.Translate(Vector3.back * speed);
		}
		if(Input.GetKey("w")) {
			transform.Translate(Vector3.forward * speed);
		}
		if (Input.GetKey ("up")) {
			transform.Rotate(Vector3.left* speed);
		}
		if (Input.GetKey ("down")) {
			transform.Rotate(Vector3.right* speed);
		}
		if (Input.GetKey ("left")) {
			transform.Rotate(Vector3.down* speed);
		}
		if (Input.GetKey ("right")) {
			transform.Rotate(Vector3.up* speed);
		}
		if (Input.GetMouseButton (0)) {
			transform.position = Input.mousePosition;
		}
	}
}
