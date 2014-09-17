using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// This class has the main functionality for creating nodes, agents.  The other gameObjects use the variables created
// here to talk with others.
public class AIContollerScript : MonoBehaviour {

	// the rate of fire of the gun
	private float fireRate = 0.3f;

	private const int numAgents = 6;
	private const int dijkstraCycles = 8;
	public static int range = 4;			// range used to determine how far to check for node with lowest weight
	
	public GameObject player;
	public GameObject node;

	public GUIText displayText;
	public GUIText youWinText;

	public GameObject bullet;
	private int shotforce = 120;

	private GameObject[,] nodeGraph = new GameObject[10, 10];
	private Transform nodeParent;
	private NodeInfo[,] nodeVar = new NodeInfo[10, 10];

	private GameObject agents;
	private NPCBehavior[] agentBehavior = new NPCBehavior[numAgents];
	
	//private List<GameObject>[,] neighbors = new List<GameObject>[10,10];
	private List<int>[,] neighbors = new List<int>[10,10];

	public float playerLocWeight;
	public float friendlyLocWeight;
	public int nodeNum;
	
	private int n = 0;  	// the count of the node 

	// variables for controlling and creating NPCs
	public GameObject agent;
	private GameObject[] agentList = new GameObject[numAgents];
//	private int agentNum = 0;
	private Transform agentParent;

//	private float speed = 5000.0f;
//	private float rotateSpeed = 60.0f;

//	private List<int> navPath = new List<int>();
//	private List<int> priorityQueue = new List<int>();
	private int z = 0;


	public static int frozenCount = 0;



	public static bool flee = true;
	public static bool helpFriends;
	public static bool reallyHelpFriends;


	// Use this for initialization
	void Start () 
	{
		// create the nodes, get the graph connecting the nodes, and create the agents.
		createNodes();
		getNodeGraph();

		createAgents ();

		InvokeRepeating("Shoot", 0.001f, fireRate);

		youWinText.text = "";
		// used for debugging to make sure adjacency list works
		//printAdjList();

		//nodeGraph[nodeNum/10, nodeNum%10].renderer.enabled = true;

	}
	
	// Update is called once per frame
	void Update () 
	{
		// each turn update the node weights based on player position, and nearby NPCs
		updateNodeWeights();

		int timeStep = 200;
		z = ( z + 1 ) % (numAgents*timeStep);

		displayText.text = "Frozen count: " + frozenCount.ToString() + " / " + numAgents.ToString();


		if ( frozenCount == numAgents )
			youWinText.text = "You Win !!!";
		/*
		// shoots bullets - prevents repeated fire
		if (Input.GetButtonDown("Fire1") )
		{

			InvokeRepeating("Shoot", 0.001f, fireRate);
		}
		//else if (Input.GetButtonUp("Fire1")) {
		//	CancelInvoke("Shoot");
		//}
*/


		// checks if states change
		checkForStatusChange();

		// quite game with space bar
		if (Input.GetKeyDown("space") )
			Application.Quit();

		//
		
	}// end method update
	

	// shoots the bullet
	void Shoot()
	{
		Vector3 bulletLoc = ( player.transform.position) ;
		Quaternion bulletDir = player.transform.rotation;
		
		GameObject shot = Instantiate(bullet, bulletLoc, bulletDir) as GameObject;
		shot.rigidbody.AddForce(player.transform.forward * shotforce);
	}// end function Shoot
	
	
	// create the nodes for NPC movement through the scene
	void createNodes()
	{
		Vector3 location;
		
		// create a 10 x 10 matrix of nodes on the ground
		for ( int i = 0; i < 10; i++ )
		{
			for ( int j = 0; j < 10; j++ )
			{
				// Space nodes 10 units apart
				location = new Vector3 ( j*10 - 45, 0, i*10 - 45 );
			
				// Create nodes from a prefab, and hide them from view
				nodeGraph[i, j] = Instantiate(node, location, Quaternion.identity) as GameObject;
				nodeGraph[i, j].renderer.enabled = false;
			
				// Get the script attached to the node, so we can manipulate the starting variables
				nodeVar[i,j] = nodeGraph[i,j].GetComponent<NodeInfo>();
			
				nodeParent = GameObject.Find("Nodes").transform;
			
				nodeGraph[i,j].transform.parent = nodeParent;

				nodeVar[i,j].setNodeNum( 10*i + j );
			
				// Need to change the location of this later - until after i make the navigation graph
			
				// initialize each node 
				//nodeVar.setNearbyPlayerWeight(2.0f); 
			}
		}// end creation and initialization of each node on the map
	}


	// create the graph of node connections
	public void getNodeGraph()
	{
		player.collider.enabled = false;
		
		// creates a graph of adjacent neighbors for each node - commented out during runtime
		for ( int i = 0; i < 10; i++ )
		{
			for ( int j = 0; j < 10; j++ )
			{
				// create a neighbors list for each node 
				//neighbors[i, j] = new List<GameObject>();
				neighbors[i, j] = new List<int>();

				int i_neighbors = -1;
				int j_neighbors = -1;
			
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
						switch(k) 
							{
							case 0:
								i_neighbors = i + 1; 
								j_neighbors = j;
								break;
							case 1:
								i_neighbors = i + 1; 
								j_neighbors = j + 1;
								break;
							case 2:
								i_neighbors = i; 
								j_neighbors = j + 1;
								break;
							case 3:
								i_neighbors = i - 1; 
								j_neighbors = j + 1;
								break;
							case 4:
								i_neighbors = i - 1; 
								j_neighbors = j;
								break;
							case 5:
								i_neighbors = i - 1; 
								j_neighbors = j - 1;
								break;
							case 6:
								i_neighbors = i; 
								j_neighbors = j - 1;
								break;
							case 7:
								i_neighbors = i + 1; 
								j_neighbors = j - 1;
								break;
								
							}

						neighbors[i,j].Add ( (i_neighbors*10) + j_neighbors );
						// Debug.Log ("added node " + i_neighbors + j_neighbors);
						}
					
					}
				
				}// end raycast for loop
			}
			
		}// end loop though all nodes
		
		turnOffColliders();
		player.collider.enabled = true;
		
	}// end method getNodeGraph


	// after getting the adjacency list for each node, turn off colliders
	void turnOffColliders()
	{
		for ( int i = 0; i < 10; i++ )
			for ( int j = 0; j < 10; j++ )
				
				nodeGraph[i,j].collider.enabled = false;
	}// end method turnOffColliders
	

	// used for debugging
	void printAdjList()
	{
		//foreach ( GameObject neighNode in neighbors[ nodeNum%10, nodeNum/10  ] )
		foreach ( int neighNode  in neighbors[ nodeNum/10, nodeNum%10 ])
			nodeGraph[neighNode/10, neighNode%10].renderer.enabled = true;

	}



	// updates the node weights for all except the weights for adjacent agents
	void updateNodeWeights() 
	{
		Vector3 distance;

		// used to make sure all agents are not calling Dijkstra on same turn
		n = ( n + 1 ) % 10;
		
		int i = n;
		for ( int j = 0; j < 10; j++ )
		{
			// reset values for nodes

			nodeVar[i, j].setNearbyPlayerWeight( 0.0f );
			nodeVar[i, j].setPlayerLOSWeight ( 0.0f );
			

			// Get the script attached to the node, so we can manipulate the starting variables
			// update weights based on location with player
			distance = player.transform.position - nodeGraph[i, j].transform.position;

			if (!nodeVar[i,j].nodeIsFrozen() )
				nodeVar[i, j].setNearbyPlayerWeight( playerLocWeight / ( distance.magnitude ) ); 
			
			
			// update if direct line of sight to player
			Vector3 directionToPlayer = player.transform.position - nodeGraph[i,j].transform.position;
			
			
			RaycastHit hit = new RaycastHit();
			Ray nodeRay = new Ray();
			
			nodeRay = new Ray ( nodeGraph[i, j].transform.position, directionToPlayer );
			
			Debug.DrawRay(nodeGraph[i,j].transform.position, directionToPlayer, Color.yellow );
			
			if (Physics.Raycast (nodeRay, out hit, Mathf.Infinity, ( (1<<8 )| (1<<9) ) ) )
			{
				if (hit.collider.name == "Player")
				{
					nodeVar[i, j].setPlayerLOSWeight (4.0f);
				}
				else if (hit.collider.tag == "Wall")
				{
					float distanceToWall = ( hit.point - nodeGraph[i,j].transform.position).magnitude;
					float distanceToPlayer = (player.transform.position - nodeGraph[i,j].transform.position).magnitude;

					if (!nodeVar[i,j].nodeIsFrozen() )
						nodeVar[i,j].setNearbyPlayerWeight( distanceToWall / distanceToPlayer );
				}
			}// end raycast for player and wall
			
		}
	}// end method update weights



	// creates a list of agents 
	public void createAgents()
	{
		Vector3 location;
		
		// create a 10 x 10 matrix of nodes on the ground
		for ( int i = 0; i < numAgents; i++ )
			
		{
			// Space nodes 10 units apart
			int p = (int)Random.Range(-4, 5);
			int q = (int)Random.Range(-4, 5);
			location = new Vector3 ( p*10 - 5, 1.0f, q*10 - 5 );
			
			// Create nodes from a prefab, and hide them from view
			agentList[i] = Instantiate(agent, location, Quaternion.identity) as GameObject;
			
			
			agentParent = GameObject.Find("Agents").transform;
			
			agentList[i].transform.parent = agentParent;

			agentBehavior[i] = agentList[i].GetComponent<NPCBehavior>();

			agentBehavior[i].setAgentNumber(i);

			
		}// end creation and initialization of each node on the map
	}


	// change weights of nodes based on nearby NPC's to prevent clumping of NPCs
	public void updateNodesWithNearbyPlayers( int agentNum )
	{

		for (int row = 0; row < 10; row++ )
		{
			for (int col = 0; col < 10; col++ )
			{
				nodeVar[ row, col ].resetNearbyFriendlyWeight( );
				
				for( int i = 0; i < numAgents; i++ )
				{
					// only do for other agents or if agent is not frozen
					if ( ( i != agentNum ) && !(agentBehavior[i].isFrozen() ) )
					{
						Vector3 distanceToNode = nodeGraph[ row, col ].transform.position - agentList[i].transform.position;
						
						nodeVar[ row, col ].setNearbyFriendlyWeight( friendlyLocWeight / distanceToNode.magnitude );
					}
				}
			}

		}
	}// end method updateNodesWithNearbyPlayers



	// checks to see if teh state has changed
	void checkForStatusChange()
	{
		//
		if ( frozenCount > 0 )
		{
			helpFriends = true;
			reallyHelpFriends = false;
			flee = false;
			range = 4;
		}
	
		if ( ( (float)frozenCount / (float)numAgents)  > 0.7 )
		{		
			reallyHelpFriends = true;
			helpFriends = false;
			flee = false;
			range = 5;
		}

	}



	public  NodeInfo[,] getNodeVar()
	{
		return nodeVar;
	}


	public int getNumAgents()
	{
		return numAgents;
	}

	public GameObject[,] returnNodeGraph ()
	{
		return nodeGraph;
	}

	public List<int>[,] getNeighborsList()
	{
		return neighbors;
	}

}
