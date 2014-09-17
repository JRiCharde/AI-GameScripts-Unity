using UnityEngine;
using System.Collections;

public class PathInfo : MonoBehaviour {

	private float adjustedCost;
	private float traveledDistance;
	private GameObject prevNode;
	private bool inQueue;

	private int row, col;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	// returns adjusted cost
	public float getAdjCost()
	{
		return adjustedCost;
	}

	// returns the traveled distance
	public float getTravDist()
	{
		return traveledDistance;
	}

	// returns the previous node in the path
	public GameObject getPrevNode()
	{
		return prevNode;
	}

	// returns true if this node was alredy visited
	public bool wasVisited()
	{
		return inQueue;
	}

	// returns the number of this node
	public int getNodeNumber()
	{
		return row + col*10;
	}



	// update the adjusted cost
	public void setAdjCost ( float adjCost )
	{
		this.adjustedCost = adjCost;
	}

	//update the travel distance to this node
	public void setTravDist ( float travDist )
	{
		this.traveledDistance = travDist;
	}

	// update whether this node was visisted
	public void setVisited ( bool visited )
	{
		this.inQueue = visited;
	}

	// updates the previous node in this path
	public void setPrevNode( GameObject connectNode )
	{
		this.prevNode = connectNode;
	}


	public void setRow( int row )
	{
		this.row = row;
	}

	public void setCol( int col )
	{
		this.col = col;
	}



}
