using UnityEngine;
using System.Collections;

// Controls player movement
public class Movement : MonoBehaviour {

	public float speed = 5;
	public Vector3 targetPosition;

	// Use this for initialization
	void Awake () {
	
		targetPosition.x = (float)-54.1568;
		targetPosition.y = (float)0.4435616;
		targetPosition.z = (float)-190.5751;
	}
	
	// Update is called once per frame
	void Update () {

		// Move the player towards the targets position
		transform.position = Vector3.MoveTowards (transform.position, targetPosition, speed * Time.deltaTime);
	}
}
