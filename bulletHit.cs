using UnityEngine;
using System.Collections;

// The basic idea was borrowed from online Unity forums to compat bullets passing through walls.  It only sort of fixed
// the problem.

public class bulletHit : MonoBehaviour {


	private Vector3 prevPosition;


	// Use this for initialization
	void Start () {
		prevPosition = transform.position;
		Destroy (gameObject, 2);
	}
	
	// Update is called once per frame
	void Update () {
	
		hitTest ();



	}

	// checks to see if there was a collider between here and the previous location
	void hitTest()
	{
		Vector3 forwardDirection = prevPosition - transform.position;
		float raycastDistance = Vector3.Distance(transform.position, prevPosition);
		RaycastHit hit = new RaycastHit();

		// Makes a raycast that returns true if it hits any colliders.
		if (Physics.Raycast (prevPosition, forwardDirection, out hit, raycastDistance) ) {
			// -- Hitted a collider --
			OnBulletHit(hit);
		}

		
		prevPosition = transform.position;
	}


	// when the bullet hits something, this is what you do.
	void OnBulletHit(RaycastHit hit ){

		GameObject hitObject  = hit.transform.gameObject;
		if( hitObject.name == "Agent(Clone)" )	
		{
			NPCBehavior NPC = hitObject.GetComponent<NPCBehavior>();
			//NPC = 
			NPC.turnFrozen();
			NPC.setTargetNodeFrozen();
			Debug.Log ("freeze");
		}
			
		if( hitObject.tag == "Wall" )
			Destroy(gameObject);
		
	}
}
