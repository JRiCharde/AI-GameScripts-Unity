using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlayerMovement : MonoBehaviour {

	// public variable to adjust speed in editor
	public float speed;
	public float rotateSpeed;
	private float direction = 0.0f;
	public GUIText displayText; 
	private Vector3 destination;
	private bool findTarget = false;
	private bool findSeekTarget = false;
	private Ray ray;

	//public GameObject AStar_Script;


	// A* variables - for A* implementation
	private Vector3 fromHere, toHere;
	private Vector3 location;
	private List<GameObject> navPath = new List<GameObject>();
	private GameObject[,] nodeGraph = new GameObject[10, 10];
	public GameObject node;
	private List<GameObject> navigationPath = new List<GameObject>();
	//public int startRow, startCol, endRow, endCol;  // Used to test A-Star pathfinding
	private List<GameObject> priorityQueue = new List<GameObject>();
	private GameObject startPointNode, destinationNode;
	private List<GameObject>[,] neighbors = new List<GameObject>[10,10];
	public int targetNode;

	// Use this for initialization
	void Start () 
	{
		navPath.Clear();
		//nodeGraph.Clear();
		navigationPath.Clear();
		priorityQueue.Clear();
		//neighbors.Clear();

		// create a 10 x 10 matrix of nodes on the ground
		for ( int i = 0; i < 10; i++ )
			for ( int j = 0; j < 10; j++ )
		{
			// Space nodes 10 units apart
			location = new Vector3 ( i*10 - 45, 0, j*10 - 45 );

			// Create nodes from a prefab, and hide them from view
			nodeGraph[i, j] = Instantiate(node, location, Quaternion.identity) as GameObject;
			nodeGraph[i, j].renderer.enabled = false;
			
			// Get the script attached to the node, so we can manipulate the starting variables
			PathInfo nodeVar = nodeGraph[i,j].GetComponent<PathInfo>();

			// initialize each node 
			nodeVar.setVisited(false);
			nodeVar.setRow ( i );
			nodeVar.setCol ( j );
			nodeVar.setPrevNode(null); 

		}// end creation and initialization of each node on the map


		// Shows the actual connections between all nodes.  Connections are dynamically created at runtime,
		// so new walls will change the connections.  
		//  Comment this out if running the game, so the graph won't interfere with other graphics
		showNodeConnections();

	}//end Start method


	// Update is called once per frame
	void Update () 
	{

		// Begin player movement controls for rotation
		float moveHorizontal = Input.GetAxis ("Horizontal");

		if (moveHorizontal != 0) 
		{	
			transform.Rotate (0, moveHorizontal * rotateSpeed * Time.deltaTime, 0); 
			direction = Vector3.Angle ( transform.forward, Vector3.forward);
			if (transform.forward.x < 0)
				direction = 360 - direction;

		} 

		displayText.text = "Player: \n"
			+ "\tangle: " + direction.ToString("0.0") + " degrees\n"
				+ "\tlocation: " + transform.position.x.ToString("0.0") + ", " + transform.position.z.ToString("0.0");

		// End player movement controls


		// Draws a line to each node that is close to the player
		foreach (GameObject closeNode in findNearbyNodes( ) )
		{
			Debug.DrawLine (transform.position, closeNode.transform.position, Color.yellow);

		}

		// Finds the closest node of the nearby nodes, and draws a blue line to it
		startPointNode = findClosestNode ( findNearbyNodes() );
		Debug.DrawLine (transform.position, startPointNode.transform.position, Color.blue);


		// If the alt/option key is pressed, the player will follow the A-Star path to the target node
		if ( findTarget )
		{
			// Sets the destination to the next node in the path
			destination = navPath[0].transform.position;

			// Moves, using Seek, to the next node in the path
			Seek ( destination );

			// Once the destination node is found, it is removed from the waypoint list, and hidden from view
			// The next node on the waypoint list is set as the new destination node

			if ( (destination - transform.position ).magnitude < 2.0 )
			{
				Debug.Log ("path count" + navPath.Count );
				navPath[0].renderer.enabled = false;
				navPath.RemoveAt(0);
				// if there are more nodes in the navigation path, set it to the new destination
				if (navPath.Count != 0 )
					destination = navPath[0].transform.position;
				// Once the last node on the navigation path is found, no longer seek 
				else
					findTarget = false;
			}// end if to find if we have reached our destination

		}// end if to findTarget - target was found
		
	}// end Update


	// returns the direction for display purposes
	public float GetDirection()
	{
		return direction;
	}// end method getDirection

	
	
	// this is where physics (like player controls) will go.
	void FixedUpdate ()
	{
		
		// find the forward/backword inputs from the player
		float moveVertical = Input.GetAxis ("Vertical");

		// moves forward or backward depending on player input
		if (moveVertical > 0 )
			rigidbody.AddForce (transform.forward * speed * Time.deltaTime);
		if (moveVertical < 0 )
			rigidbody.AddForce (-transform.forward * speed * Time.deltaTime);


		// Move to a target location directly with a direct line.  Cannot avoid obstacles.
		SeekOnMouseClick();



		// Uses A_Star with seek
		if ( Input.GetButton("Fire2") )  // alt/option button pressed
		{
			// Clear the navPath before reassigning nodes.  Avoids problems with creating cycle of nodes in navPath
			navPath.Clear ();

			// call A* method that finds the A* path to a target 
			navPath = A_Star( startPointNode, nodeGraph[targetNode%10,targetNode/10] );

			// indicate that we need to seek out the target once the path has been found
			findTarget = true;



			// Old code, could not get left mouse click to work here. Fix later.
			/*
			findTarget = true;
			RaycastHit hit = new RaycastHit();
			ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			//desiredTarget = ray.GetPoint(0);
			if (Physics.Raycast(ray, out hit)) //did we hit something?
				if ( hit.transform.name == "Ground"  ) //did we hit the ground?
					destination = hit.point;
			//Physics.Raycast(ray, out hit);
			
			*/


			// Used for debugging.  Commented out.
			/*
			int count = 0;
			foreach (GameObject waypoint in navPath)
			{

				count++;
			}
			Debug.Log (" node " + count);
			*/

		}// end if for alt/option key depressed.


	}// end method FixedUpdate



	// Seeks a direct line to the target location.  Cannot avoid obstacles directly.
	// Draws a line to the target.
	// Rotates player to target and moves towards target.
	void Seek( Vector3 target)
	{
		Vector3 desiredDirection = target - transform.position ;
		desiredDirection.Normalize();

		// rotate Towards the target
		rotateTowards( desiredDirection );

		// move towards the target
		rigidbody.AddForce (desiredDirection * speed * Time.deltaTime);

		// Draw line to target
		Debug.DrawLine(transform.position, target);

	}// end method seek


	// Rotate towards the target
	void rotateTowards ( Vector3 targetPosition )
	{
		Vector3 targetDirection = Quaternion.LookRotation(targetPosition).eulerAngles;
		targetDirection.x = 0;
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler (targetDirection), 
		                                      Time.deltaTime * rotateSpeed/10);
	}// end method rotateTowards


	//A* pathfinding algorithm that returns a list of nodes from the source node to the target node.

	public List<GameObject>  A_Star( GameObject sourceNode, GameObject targetNode)
	{
		// Inserts the sourcenode into the beginning of the priorityQueue (a List)
		priorityQueue.Insert (0, sourceNode );

		// changes the parameters of the source node: sets that it was visited, that it takes 0.0 units to get there, 
		// establishes its adjustedCost (A* cost) as the Euler distance, and sets the previous node to get to this node
		// as null (no prev node).
		PathInfo nodeVar = sourceNode.GetComponent<PathInfo>();
		nodeVar.setVisited( true );
		nodeVar.setAdjCost( ( sourceNode.transform.position - targetNode.transform.position ).magnitude );
		nodeVar.setTravDist( 0.0f);
		nodeVar.setPrevNode(null); 
		
		// Keep looping until the first element in the priorityQueue is the target node
		while ( priorityQueue[0] != targetNode )
		{

			// puts the current node into the priorityQueue in order based on its adjusted Cost
			prioritize ( priorityQueue[0], targetNode );

			// Next two lines are for debugging to show the distance traveled to get to this node and the adjusted cost
			PathInfo nextNodeVisited = priorityQueue[0].GetComponent<PathInfo>();
			Debug.Log("Node " + nextNodeVisited.getNodeNumber() + " traveled " + nextNodeVisited.getTravDist() 
			          + "with a total estimated dist of " + nextNodeVisited.getAdjCost() );
		}

		// once the priorityQueue reaches the target, add the target node to the navigation pathh
		navigationPath.Add (priorityQueue[0]);
		PathInfo nodeList = priorityQueue[0].GetComponent<PathInfo>();

		// Keep adding the previous node in the path until we reach the start node
		while (nodeList.getPrevNode() != null )
		{
			GameObject nextNode = nodeList.getPrevNode();
			navigationPath.Add ( nextNode );

			nodeList = nextNode.GetComponent<PathInfo>();
			
		}// end while loop
		
		// make each node in the navigation path visible
		foreach (GameObject pathNode in navigationPath)
		{
			pathNode.renderer.enabled = true;
		}

		// draw a line from each node to the next one in line
		for (int i = 0; i < navigationPath.Count - 1; i++)
			Debug.DrawLine( navigationPath[i].transform.position, navigationPath[i+1].transform.position );

		// reverse the navigation path since we added the nodes in reverse order
		navigationPath.Reverse();
		
		return navigationPath;

	}// end method A_Star
	


	void prioritize( GameObject currentNode, GameObject targetNode )
	{
		// remove the current node at the front of the list since we are adding its neighbors to the queue
		removeFromQueue( 0 );
		
		PathInfo currentNodeInfo = currentNode.GetComponent<PathInfo>();
		float traveledDistance = currentNodeInfo.getTravDist();

		// check to see if the current node has a path to each of its neighbors
		for ( int k = 0; k < 8; k++)
		{
			// create 8 angles from the current node and raycast from them
			Vector3 angle = Quaternion.Euler(0, 45 * k, 0) * currentNode.transform.forward;
			RaycastHit hit = new RaycastHit();
			Ray nodeRay = new Ray();
			nodeRay = new Ray ( currentNode.transform.position, angle );

			// if the ray hits something, check to see if it hit a node
			if (Physics.Raycast (nodeRay, out hit ))
			{
				if (hit.collider.tag == "Node")
				{
					// set the hit object to be teh adjacent node
					GameObject adjNode = hit.collider.gameObject;
					
					PathInfo adjNodeInfo = adjNode.GetComponent<PathInfo>();

					// calculate the distance traveled to the adjacent node and its adjusted cost
					// through the path through the curent node
					float newTravDist = traveledDistance + ( currentNode.transform.position
					                                        - adjNode.transform.position ).magnitude;
					
					
					float newAdjCost = newTravDist + ( targetNode.transform.position
					                                  - adjNode.transform.position ).magnitude;
					
					// if adjacent node is already in queue, get the distance info from it to see if it needs updating
					if ( adjNodeInfo.wasVisited() )
					{
						
						//adjNodeInfo.getAdjCost();
						if ( newTravDist < adjNodeInfo.getTravDist() ) {
							// if it needs updating, remove the old node from the queue, since we will add it back soon
							priorityQueue.Remove( adjNode );
							updateTravelPath( currentNode, adjNode, newTravDist, newAdjCost );
						}
						
					}

					// if it was not already in queue, add it there
					else
					{
						//need to add node to queue
						updateTravelPath( currentNode, adjNode, newTravDist, newAdjCost );
					}
					
				}// end hit.collider "Node"
				
			}// end RayCast
		}// end for loop through 8 directions
		
	}// end method prioritize
	

	// update the variables associated with this node (start node)
	void updateTravelPath( GameObject startNode, GameObject endNode, float travDist, float adjCost )
	{
		
		PathInfo endNodeInfo = endNode.GetComponent<PathInfo>();
		
		
		endNodeInfo.setTravDist( travDist );

		endNodeInfo.setAdjCost( adjCost );

		endNodeInfo.setVisited( true );
		
		endNodeInfo.setPrevNode( startNode );

		// add the node to the queue with the updated info
		addToQueue( endNode, adjCost );
	}
	//end method updateTravelPath
	
	
	
	//adds the new node to the priority queue in order
	void addToQueue( GameObject newNode, float adjCost )
	{
		int index = 0;

		// if there is nothing in the queue, add it to the beginning of the queue
		if ( priorityQueue.Count == 0 )
		{
			priorityQueue.Insert ( 0, newNode );
			return;
		}
		
		PathInfo pqNodeInfo = priorityQueue[index].GetComponent<PathInfo>();

		// move to the next spot in the priority queue if its adjusted cost is larger than the next node in the queue
		while ( index < priorityQueue.Count && adjCost > pqNodeInfo.getAdjCost() )
		{
			pqNodeInfo = priorityQueue[index].GetComponent<PathInfo>();
			index++;

		}

		// insert the node at the current spot (since the next node jhas larger adjusted cost or at end)
		priorityQueue.Insert(index, newNode);	
		
	}// end function addToQueue
	

	// remove node from queue at the given index
	void removeFromQueue(int index )
	{
		priorityQueue.RemoveAt(index);
		
	}// end function popFromQueue
	
	
	// Prints the neighbors graph - used to print neighbors
	void PrintNeighbors(Vector3 begin, Vector3 target)
	{
		Debug.DrawLine(begin, target);
		
	}


	// updated adjacent agent sensor used to find nearby nodes instead.
	// returns a list of nearby nodes
	List<GameObject>  findNearbyNodes( )
	{
		GameObject[] travelNodes;
		travelNodes = GameObject.FindGameObjectsWithTag("Node");
		List<GameObject> closeNodes = new List<GameObject>();

		// go through each node to see if it is in range
		foreach ( GameObject travNode in travelNodes )
		{
			Vector3 diff = (travNode.transform.position - transform.position);
			diff.y = 0.0f;
			float distance = diff.magnitude;
			if (distance < 10)
				closeNodes.Add (travNode);
		}

		return closeNodes;
	}// end method findNearbyNodes


	// returns the closest node
	GameObject  findClosestNode( List<GameObject> closeNodes )
	{
		float smallestDistance = 999;
		GameObject currentClosest = closeNodes[0];
		foreach( GameObject neighboringNode in closeNodes )
		{
			if ( (neighboringNode.transform.position - transform.position).magnitude < smallestDistance )
			{
				currentClosest = neighboringNode;
				smallestDistance = (neighboringNode.transform.position - transform.position).magnitude;
			}

		}
		return currentClosest;
	}// end method findClosestNode


	// shows the node connections.  First creates a list of neighbors for each node, then draws the connections.
	void showNodeConnections()
	{
		// remove the collider fro the player, so it does not get in the way of the node raycasts
		this.gameObject.collider.enabled = false;



		// creates a graph of adjacent neighbors for each node - commented out during runtime
		for ( int i = 0; i < 10; i++ )
			for ( int j = 0; j < 10; j++ )
		{
			// create a neighbors list for each node 
			neighbors[i, j] = new List<GameObject>();

			// for each node, create a list of adjacent neighbors by raycasting all 8 directions looking for neighbors.
			for ( int k = 0; k < 8; k++)
			{
				Vector3 angle = Quaternion.Euler(0, 45 * k, 0) * nodeGraph[i,j].transform.forward;
				RaycastHit hit = new RaycastHit();
				Ray nodeRay = new Ray();
				nodeRay = new Ray ( nodeGraph[i,j].transform.position, angle );
				
				if (Physics.Raycast (nodeRay, out hit ))
				{
					if (hit.collider.tag == "Node")
					{
						neighbors[i,j].Add ( hit.collider.gameObject );
					}
					
				}
				
			}// end raycas
			
		}// end loop though all nodes
	

		// creates and prints  the graph used to show the possible node connections
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				fromHere = nodeGraph[i,j].transform.position;
				
				foreach (GameObject g in neighbors[i, j])
					
				{	
					
					toHere = g.transform.position;
					PrintNeighbors(fromHere, toHere);
				}
			}
			
		}// end loop through all nodes

		// pauses game to keep graph visible
		//Debug.Break ();

	}//end function showNodeConnections


	// Seeks a target with a direct line.  Rotates player to the target, and moves directly to the target.
	// cannot avoid obstacles.  Works by a left mouse click on the target location.
	void SeekOnMouseClick()
	{


		// Direct seek on left mouse button.  Does not use pathfinding.

		if ( Input.GetMouseButton(0) )  // left mouse button pressed
		{
			findSeekTarget = true;

			//Find the positon on the ground that was targeted.
			RaycastHit hit = new RaycastHit();
			ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast(ray, out hit)) //did we hit something?
				if (hit.transform.name == "Ground") //did we hit the ground?
					destination = hit.point;

		}// end if for left mouse click

		// once we reach the target, stop seeking it.
		if ( Vector3.Distance( destination, transform.position ) < 1.0 )
			findSeekTarget = false;

		// If target activated, seek it out
		if ( findSeekTarget )
		{
			Seek ( destination );
		}

	}// end method SeekOnMouseClick

}
