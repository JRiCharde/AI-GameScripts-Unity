using UnityEngine;
using System.Collections;

public class NodeInfo : MonoBehaviour {


	private float nearbyPlayerWeight;
	private float playerLineOfSiteWeight;
	private float nearbyFriendlyWeight;
	private float totalWeight;

	private GUIText totalWeightScore;

	//public GameObject GUITextNODE;
	//private Transform GUITextContainer;

	private float distanceDijks;
	private int prevNode;
	private bool visited;

	private int nodeNum;
	private int distanceFromSource;

	private bool nearbyFrozenNPC = false;
	private int frozenNPCNumber = -1;
	
	// Use this for initialization
	void Start () {
	

		nearbyPlayerWeight = 0.0f;
		playerLineOfSiteWeight = 0.0f;
		nearbyFriendlyWeight = 0.0f;
		distanceDijks = 99999999f;
		prevNode = -1;  	// means no previous node
		visited = false;
		distanceFromSource = 0;

		//totalWeightScore = ((GameObject)Instantiate(GUITextNODE, Camera.main.WorldToViewportPoint(gameObject.transform.position), Quaternion.identity)).GetComponent<GUIText>();
		//GUITextContainer = GameObject.Find("Generic PreFabs").transform;
		
		//totalWeightScore.transform.parent = GUITextContainer;
		//displayText();

	}// end Start


	// Update is called once per frame
	void Update () 
	{
		//displayText();
		//totalWeightScore.transform.position = Camera.main.WorldToViewportPoint(transform.position);

		if (nearbyFrozenNPC)
			Debug.Log ("total weight " + (nearbyFriendlyWeight + playerLineOfSiteWeight + nearbyPlayerWeight) +
			           	" nearby NPC weight " + nearbyFriendlyWeight);

	}// end Update


	//SET METHODS

	public void setNodeNum( int num )
	{
		nodeNum = num;
	}


	// adjust the weight for the player being nearby
	public void setNearbyPlayerWeight( float weight )
	{
		this.nearbyPlayerWeight = weight;


	}


	// the distance to node using Dijkstra's method
	public void setDijkstraDistToNode( float dist )
	{
		this.distanceDijks = dist;
	} // end method setDijkstraDistToNode


	// updates the previous node in this path using Dijkstra
	public void setPrevNode( int connectNode )
	{
		this.prevNode = connectNode;
	}// end method setPrevNode


	// adjust the weight for being in the line of site of the player
	public void setPlayerLOSWeight ( float weight )
	{
		this.playerLineOfSiteWeight = weight;
	}


	// adjust the weight for nearby friendlies
	public void setNearbyFriendlyWeight( float weight )
	{
		this.nearbyFriendlyWeight +=  weight;
	}

	// reset the weight for nearby friendlies - done at beginning of each turn, just before adding new ones
	public void resetNearbyFriendlyWeight( )
	{
		this.nearbyFriendlyWeight =  0.0f;
	}


	public void setVisited( bool visited )
	{
		this.visited = visited;
	}

	// update how far from the source node this node is
	public void setDistanceFromSource( int n )
	{
		this.distanceFromSource = n;
	}

	// indicated nearby NPC is frozen
	public void setNodeFrozen( bool freeze )
	{
		nearbyFrozenNPC = freeze;
		renderer.enabled = freeze;
		this.nearbyFriendlyWeight = 0.0f;
		//Debug.Log ("total weight " + totalWeight + " nearby NPC weight " + nearbyFriendlyWeight);
	}



	// GET METHODS

	// return the weight that is based on the player's proximity
	public float getNearbyPlayerWeight( )
	{
		return nearbyPlayerWeight;
	}
	
	
	// return the weight that is based on being in the line of site of the player
	public float getPlayerLOSWeight ( )
	{
		return playerLineOfSiteWeight;
	}
	
	
	// return the weight that is based on nearby friendlies
	public float getNearbyFriendlyWeight( )
	{
		return nearbyFriendlyWeight;
	}


	public float getTotalWeight()
	{

		return ( nearbyPlayerWeight + nearbyFriendlyWeight + playerLineOfSiteWeight );
	}


	/*
	public void displayText() {
		totalWeightScore.text = ( nearbyPlayerWeight + nearbyFriendlyWeight + playerLineOfSiteWeight).ToString("0.0");

	}
	*/

	// the distance to node using Dijkstra's method
	public float getDijkstraDistToNode( )
	{
		return this.distanceDijks;
	} // end method setDijkstraDistToNode


	// which node is this?
	public int getNodeNum()
	{
		return nodeNum;
	}


	public bool wasVisited( )
	{
		return visited;
	}


	public int getDistanceFromSource()
	{
		return distanceFromSource;
	}


	public int getPrevNode()
	{
		return prevNode;
	}


	public bool nodeIsFrozen()
	{
		return nearbyFrozenNPC;
	}
}
