using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class UpdateNodeInfo : MonoBehaviour {

	private Transform playerTransform;
	public GameObject player;
	public GameObject node;
	private GameObject[,] nodeGraph = new GameObject[10, 10];
	private Transform nodeParent;
	private NodeInfo[,] nodeVar = new NodeInfo[10, 10];
	private GameObject agents;

	private List<GameObject>[,] neighbors = new List<GameObject>[10,10];

	private AgentInfo agentScript;
	private GameObject[] agentList;

	public float playerLocWeight;
	public float friendlyLocWeight;
	public int nodeNum;

	private int n;  	// the count of the node 

	// Use this for initialization
	void Start () {
		 //createNodes ();
		 //getNodeGraph();

		playerTransform = player.transform;

		findAgentList();




		n = 0;

		// used for debugging to make sure adjacency list works
		//printAdjList();
	}
	
	// Update is called once per frame
	void Update () {
	
		//updateNodeWeights();
	}


	public void createNodes()
	{
		Vector3 location;

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
			nodeVar[i,j] = nodeGraph[i,j].GetComponent<NodeInfo>();

			nodeParent = GameObject.Find("Nodes").transform;
				
			nodeGraph[i,j].transform.parent = nodeParent;

			// Need to change the location of this later - until after i make the navigation graph

			// initialize each node 
			//nodeVar.setNearbyPlayerWeight(2.0f); 
		}// end creation and initialization of each node on the map
	}


	void updateNodeWeights() 
	{
		Vector3 distance;
		
		n = ( n + 1 ) % 10;
		
		int i = n;
		for ( int j = 0; j < 10; j++ )
		{
			// reset values for nodes
			nodeVar[i, j].setPlayerLOSWeight ( 0.0f );
			nodeVar[i, j].setNearbyPlayerWeight( 0.0f );
			
			//updateNodesWithNearbyPlayers( i, j ) ;
			
			
			// Get the script attached to the node, so we can manipulate the starting variables
			// update weights based on location with player
			distance = playerTransform.position - nodeGraph[i, j].transform.position;
			
			nodeVar[i, j].setNearbyPlayerWeight( playerLocWeight / distance.magnitude ); 
			
			
			// update if direct line of sight to player
			Vector3 directionToPlayer = playerTransform.position - nodeGraph[i,j].transform.position;
			
			
			RaycastHit hit = new RaycastHit();
			Ray nodeRay = new Ray();
			
			nodeRay = new Ray ( nodeGraph[i, j].transform.position, directionToPlayer );
			
			Debug.DrawRay(nodeGraph[i,j].transform.position, directionToPlayer, Color.yellow );
			
			if (Physics.Raycast (nodeRay, out hit ))
			{
				if (hit.collider.name == "Player")
				{
					nodeVar[i, j].setPlayerLOSWeight (6.0f);
				}
			}
			
		}
	}


	public void getNodeGraph()
	{
		player.collider.enabled = false;

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


	void printAdjList()
	{
		foreach ( GameObject neighNode in neighbors[ nodeNum%10, nodeNum/10  ] )
			neighNode.renderer.enabled = true;
	}


	// put list of NPCs into agentList
	void findAgentList()
	{
		agents = GameObject.Find("Agents");
		
		agentScript = agents.GetComponent<AgentInfo>();
		
		agentList = agentScript.getAgentList();
	}


	// change weights of nodes based on nearby NPC's to prevent clumping of NPCs
	void updateNodesWithNearbyPlayers( int row, int col )
	{
		nodeVar[ row, col ].resetNearbyFriendlyWeight( );

		foreach ( GameObject agent in agentList )
		{
			Vector3 distanceToNode = nodeGraph[ row, col ].transform.position - agent.transform.position;

			nodeVar[ row, col ].setNearbyFriendlyWeight( friendlyLocWeight / distanceToNode.magnitude );
		}
	}// end method updateNodesWithNearbyPlayers


	public NodeInfo[,] getNodeScripts()
	{
		return nodeVar;
	}

}
