using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCBehavior : MonoBehaviour {

	public float speed;
	public float friendlyLocWeight;
	public float rotateSpeed;
	public int dijkstraCycles;

	public GameObject iceCube;
	//private int range = AIContollerScript.range;

	private int destinationNode;

	private bool frozen;
	private int agentNumber;

	private Vector3 destination;

	private List<int>[,] neighbors = new List<int>[10,10];
	private GameObject[,] nodeGraph = new GameObject[10, 10];
	private NodeInfo[,] nodeVar = new NodeInfo[10, 10];
	
	private GameObject ai;
	private AIContollerScript AIScript;

	private int numAgents;

	private List<int> navPath = new List<int>();
	private List<int> priorityQueue = new List<int>();

	private float stateAdjFactor = 1.0f;

	private GameObject ice;

	int counter ;

	// Use this for initialization
	void Start () 
	{
		transform.GetChild(0).gameObject.renderer.enabled = false;
		frozen = false;

		getAI_InfoScripts();
		destinationNode = findNearestNode();
		destination = nodeVar[destinationNode/10,destinationNode%10].transform.position;
		counter = agentNumber;
		agentSeek();
	}


	// Update is called once per frame
	void Update () {

		counter = (counter + 1)%3;
		int i =0, j=0;



		if (AIContollerScript.helpFriends)
		{
			stateAdjFactor = 3.0f; // need to add find friends info here
			//if ( nodeVar[i,j].nodeIsFrozen() )
			//	;
		}

		else if( AIContollerScript.reallyHelpFriends )
		{
			stateAdjFactor = 5.0f;

		}

		else if ( AIContollerScript.flee )
		{
			stateAdjFactor = 1.0f;
		}

		//if ( (counter == 1) && ( (destination - transform.position ).magnitude < 4.0 )  )
		if ( (counter == 1) && (!frozen) && ( (destination - transform.position ).magnitude < 5.0 ) )
//		    	&& (nodeVar[destinationNode/10,destinationNode%10].getTotalWeight() > 4.0 ) )
				agentSeek();


		// Sets the destination to the next node in the path
		destination = nodeGraph[navPath[0]/10, navPath[0]%10].transform.position;
		
		destinationNode = navPath[0];
					
		// Moves, using Seek, to the next node in the path
		if (!frozen)
			Seek ( destination );
		else{
			Seek(transform.position);
			int frozenNode = findNearestNode( );
			if ( !nodeVar[frozenNode/10, frozenNode%10].nodeIsFrozen() )
			{
				//frozen = false;
				//AIContollerScript.frozenCount -= 1; 
				unFreeze ();
			}
		}
		
		//Debug.Log(nodeVar[navPath[0]/10, navPath[0]%10].getNodeNum() );
		
		// Once the destination node is found, it is removed from the waypoint list, and hidden from view
		// The next node on the waypoint list is set as the new destination node
		if (!frozen)
		{

		if ( ( (destination - transform.position ).magnitude < 4.0 )  &&  (navPath.Count > 1 ) )
		{
			//Debug.Log ("path count" + navPath.Count );
			navPath.RemoveAt(0);

			if ( nodeVar[destinationNode/10, destinationNode%10].nodeIsFrozen()  )
			{
			nodeVar[destinationNode/10, destinationNode%10].setNodeFrozen(false);
			}
			
			// if there are more nodes in the navigation path, set it to the new destination
			destination = nodeGraph[navPath[0]/10, navPath[0]%10].transform.position;
			destinationNode = navPath[0];
			
		}// end if to find if we have reached our destination
		
		if ( navPath.Count == 0 )	
		{
			destinationNode =  findNearestNode() ;
			navPath.Add (destinationNode);
		}
		}
		
	}// end method update


	public void setDestNode( int destNode )
	{
		this.destinationNode = destNode;
	}



	public void setAgentNumber( int n )
	{
		this.agentNumber = n;
	}



	public void turnFrozen( )
	{
		if (!frozen)
			AIContollerScript.frozenCount +=1;
		this.frozen = true;
		rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		transform.GetChild(0).gameObject.renderer.enabled = true;
	}

	public void unFreeze()
	{
		if (frozen)
			AIContollerScript.frozenCount -= 1;
		this.frozen = false;
		transform.GetChild(0).gameObject.renderer.enabled = false;
		rigidbody.constraints = RigidbodyConstraints.None;
	}


	public int getDestNode()
	{
		return destinationNode;
	}


	public bool isFrozen()
	{
		return frozen;
	}


	// get scripts and variables created under the AIController 
	public void getAI_InfoScripts()
	{
		ai = GameObject.Find("AIControls");
		
		AIScript = ai.GetComponent<AIContollerScript>();

		numAgents = AIScript.getNumAgents();
		
		nodeVar = AIScript.getNodeVar();

		nodeGraph = AIScript.returnNodeGraph();

		neighbors = AIScript.getNeighborsList();
		
	}// end method getNodeInfoScripts




	// 
	void agentSeek()
	{
		AIScript.updateNodesWithNearbyPlayers(agentNumber);

		navPath.Clear();
		priorityQueue.Clear();
		
		//Seek( findNearestNode().transform.position );
		Dijkstra( );
		
		/*
		// Debugging only.  Comment out when finished.
		foreach (int pathNodes in navPath )
		{
			//Debug.Log (pathNodes);
		}
		*/
		
	}// end method agentSeek
	
	
	
	
	void Seek(  Vector3 target )
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
	void rotateTowards (  Vector3 targetPosition )
	{
		Vector3 targetDirection = Quaternion.LookRotation(targetPosition).eulerAngles;
		targetDirection.x = 0;
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler (targetDirection), 
		                                               Time.deltaTime * rotateSpeed/10);
	}// end method rotateTowards
	
	
	
	// uses Dijkstra's method to find the best path using the weight of the nodes as the edge weight to the node
	public List<int>  Dijkstra( )
	{
		resetNodeValues();
		//int startNode = findNearestNode();
		int startNode = destinationNode;
		
		
		nodeVar[startNode/10, startNode%10].setDijkstraDistToNode(0);
		
		
		float targetValue = findTargetNodeValue( startNode )*1.1f;

		//Debug.Log ("target value is " + targetValue);
		//Debug.Log ("target value = " + targetValue);
		
		priorityQueue.Clear();
		
		//float lowestDist = 9999999f;
		//int bestNode = -1;
		
		//Debug.Log(startNode);
		
		
		// adjust the weights to each of the neighboring nodes in the list
		foreach ( int adjacNodeNum in neighbors[ startNode/10, startNode%10] )
		{
			int l = adjacNodeNum / 10, m = adjacNodeNum % 10;
			//Debug.Log ( nodeVar[l, m].getDijkstraDistToNode() );
			nodeVar[l, m].setDijkstraDistToNode( nodeVar[l, m].getTotalWeight() );
			//Debug.Log ("distance to Node " + adjNodeNum + ": " + nodeVar[l, m].getDijkstraDistToNode() );
			addToQueue( adjacNodeNum, nodeVar[l,m].getTotalWeight() );
			nodeVar[l, m].setDistanceFromSource(1);
			nodeVar[l, m].setPrevNode(startNode);
			/*
			if ( nodeVar[l,m].getTotalWeight() < lowestDist )
			{
				lowestDist = nodeVar[l,m].getTotalWeight();
				bestNode = adjacNodeNum;

			}
			*/  
		}
		
		//Debug.Log ("Node " + bestNode + " has a distance of " + lowestDist );
		//Debug.Log ("nearbyAgentWeight : " + nodeVar[bestNode/10, bestNode%10].getNearbyFriendlyWeight() );
		
		//priorityQueue.Insert(0, bestNode);
		
		//int bestNode = priorityQueue[0];
		
		int currentNode = priorityQueue[0];
		int currentRow = currentNode/10;
		int currentCol = currentNode%10;
		nodeVar[currentRow, currentCol].setVisited(true);
		int currentNodeStep = nodeVar[currentRow, currentCol].getDistanceFromSource();
		//priorityQueue.RemoveAt(0);  // remove 1st object from queue
		
		while ( ( currentNodeStep < dijkstraCycles )  ||  (nodeVar[currentRow, currentCol].getTotalWeight() < targetValue)  
		       || ( ( nodeVar[currentRow, currentCol].nodeIsFrozen() )
		    			&& ( nodeVar[currentRow, currentCol].getTotalWeight() < targetValue * stateAdjFactor ) ) ) 
		{
			priorityQueue.RemoveAt(0);  // remove 1st object from queue
			
			foreach ( int adjacNodeNum in neighbors[ currentRow, currentCol] )
			{
				
				int l = adjacNodeNum / 10, m = adjacNodeNum % 10;
				float newTentDijkstraDist = nodeVar[currentRow, currentCol].getDijkstraDistToNode()
					+ nodeVar[l, m].getTotalWeight();
				if ( newTentDijkstraDist < nodeVar[l, m].getDijkstraDistToNode() )
				{
					//Debug.Log ( nodeVar[l, m].getDijkstraDistToNode() );
					nodeVar[l, m].setDijkstraDistToNode( newTentDijkstraDist );
					//Debug.Log ("distance to Node " + adjacNodeNum + ": " + nodeVar[l, m].getDijkstraDistToNode() );
					
					priorityQueue.Remove( l*10 + m );
					addToQueue(l*10 + m, newTentDijkstraDist );
					
					nodeVar[l, m].setDistanceFromSource( currentNodeStep + 1);
					nodeVar[l, m].setPrevNode(currentNode);
				}
				
			}// end loop through each adjacent Node
			
			if ( priorityQueue.Count == 0 )
				priorityQueue.Add (destinationNode );
			
			
			currentNode = priorityQueue[0];
			currentRow = currentNode/10;
			currentCol = currentNode%10;
			nodeVar[currentRow, currentCol].setVisited(true);
			currentNodeStep = nodeVar[currentRow, currentCol].getDistanceFromSource();
			//priorityQueue.RemoveAt(0);  // remove 1st object from queue
			
		}
		
		navPath.Clear();
		// once the priorityQueue reaches the target, add the target node to the navigation pathh
		navPath.Add (currentNode);
		currentRow = navPath[0] / 10;
		currentCol = navPath[0] % 10;
		int nextNode = nodeVar[ currentRow, currentCol ].getPrevNode();
		
		
		// Keep adding the previous node in the path until we reach the start node
		while ( nextNode != -1 )
		{
			currentRow = nextNode / 10;
			currentCol = nextNode % 10;
			navPath.Add ( nextNode );
			nextNode = nodeVar[ currentRow, currentCol ].getPrevNode();
			
			
			
		}// end while loop
		
		navPath.Reverse();
		
		return navPath;
		
	}// end method Dijkstra
	
	
	
	// updated adjacent agent sensor used to find nearby nodes instead.
	// returns a list of nearby nodes
	int  findNearestNode( )
	{
		float currentDist = 99999f;
		int closestNode = 0; 
		
		// go through each node to see if it is in range
		for (int m = 0; m < 100; m++ )   //each ( GameObject travNode in travelNodes )
		{
			Vector3 diff = (nodeGraph[ m/10, m%10].transform.position - transform.position);
			diff.y = 0.0f;
			float distance = diff.magnitude;
			if (distance < currentDist)
			{
				currentDist = distance;
				closestNode = m;
			}
		}
		
		return closestNode;
	}// end method findNearbyNodes
	
	
	
	// insert into an ordered Queue 
	void addToQueue( int newNode, float dijkDistance )
	{
		int index = 0;
		
		// if there is nothing in the queue, add it to the beginning of the queue
		if ( priorityQueue.Count == 0 )
		{
			priorityQueue.Insert ( 0, newNode );
			return;
		}
		
		int currentNode = priorityQueue[index];
		
		// move to the next spot in the priority queue if its adjusted cost is larger than the next node in the queue
		while ( index < priorityQueue.Count && dijkDistance > nodeVar[currentNode/10, currentNode%10].getDijkstraDistToNode( ) )
		{
			
			
			currentNode = priorityQueue[index];
			index++;
			
		}
		
		// insert the node at the current spot (since the next node jhas larger adjusted cost or at end)
		priorityQueue.Insert(index, newNode);	
		
	}// end function addToQueue
	
	
	// resets all of the node values, so we can run Dijkstra again
	void resetNodeValues()
	{
		for ( int i = 0; i < 10; i++ )
		{
			for ( int j = 0; j < 10; j++ )
			{
				nodeVar[i,j].setVisited(false);
				nodeVar[i,j].setDijkstraDistToNode(999999f);
				nodeVar[i,j].setDistanceFromSource(0);
				nodeVar[i,j].setPrevNode(-1);
			}
		}
	}// end method resetNodeValues
	
	
	// finds the node within a range with the lowest weight value
	float findTargetNodeValue( int startNode )
	{
		float lowestNodeWeight = 999999f;
		
		for(int z = startNode/10 - AIContollerScript.range; z < startNode/10 + AIContollerScript.range; z++ )
		{
			for (int x = startNode%10 - AIContollerScript.range; x < startNode%10 + AIContollerScript.range; x++ )
			{
				// make sure it is within the allowed Node values
				// if there is a node with a lower node weight, set it equal to the lowest node weight
				if (z >= 0 && z < 10  && x >= 0 && x < 10)
				{
					if ( nodeVar[z,x].getTotalWeight() < lowestNodeWeight )
						lowestNodeWeight = nodeVar[z,x].getTotalWeight();
					
					//Debug.Log ("Node " +z +x + "has weight " + nodeVar[z,x].getTotalWeight() );
				}
			}
		}
		
		return lowestNodeWeight;
	}// end method findTargetNodeValue


	public void setTargetNodeFrozen()
	{
		int frozenNode = findNearestNode( );
		nodeVar[frozenNode/10, frozenNode%10].setNodeFrozen(true);
	}

}
