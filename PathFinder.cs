using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathFinder : MonoBehaviour {

	// Two list:open and close
	// Open: holds nodes that have not been checked yet
	// Closed: holds nodes that have been checked already
	public List<SearchNodes> listOpen = new List<SearchNodes>();
	public List<SearchNodes> listClosed = new List<SearchNodes>();
	
	public SearchNodes checkNode = null; // Next node on list
	public SearchNodes firstNode = null; // Very first Node
	public SearchNodes startingNode = null;	// the starting point
	public SearchNodes targetNode = null;	// the target node

	public int baseMoveCost = 10;
	public int nodeNumber = 0;

	public bool targetNodeReached = false; // Used to check if target node was reached

	// Use this for initialization
	void Start () {

		findHeuristics (firstNode);
		checkNode = firstNode;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (targetNodeReached == false) {
		
			findBestPath ();
		} 

		else {

			print ("Target Reached!");
		}
	}

	public void findHeuristics(SearchNodes start){

		// Start searching row by row and calculate cost values for the nodes
		SearchNodes theStart = startingNode;
		SearchNodes nextNode = theStart;

		int x1, x2, z1, z2;
		float cost;

		while(theStart != null){

			while(nextNode != null){

				// Use the manhattan distance as heuristic
				x1 = Mathf.FloorToInt(nextNode.transform.position.x);
				z1 = Mathf.FloorToInt(nextNode.transform.position.z);
				x2 = Mathf.FloorToInt(targetNode.transform.position.x);
				z2 = Mathf.FloorToInt(targetNode.transform.position.z);

				// Calculate cost
				cost = Mathf.Abs (x1-x2) + Mathf.Abs (z1-z2);
				nextNode.heuristicValue = (int)cost;
			}

			theStart = theStart.three;
			nextNode = theStart;
		}
	}

	public SearchNodes minTotalCost(){

		// Get the node with the minimum cost value
		float min = float.MaxValue;
		SearchNodes minNode = null;
		foreach (SearchNodes node in listOpen) {

			if(node.totalCost < min){

				min = node.totalCost;
				minNode = node;
			}
		}

		return minNode;
	}

	public void findBestPath(){

		// Find the shortest path
		if(targetNodeReached == false){

			if(checkNode.one != null){

				findNodeCost(checkNode, checkNode.one);
			}

			if(checkNode.two != null){
				
				findNodeCost(checkNode, checkNode.two);
			}

			if(checkNode.three != null){
				
				findNodeCost(checkNode, checkNode.three);
			}

			if(checkNode.four != null){
				
				findNodeCost(checkNode, checkNode.four);
			}

			// Move the current node to the close list
			// and remove from the open list
			AddToClosedList(checkNode);
			RemoveFromOpenList(checkNode);

			checkNode = minTotalCost();

			nodeNumber++;

			// Debug output
			print("Node # " + nodeNumber + " has been checked");
		}
	}

	public void findNodeCost(SearchNodes node1, SearchNodes node2){

		int newMovementCost;


		// If there's no other nodes to search exit;
		if (node2 == null) {

			return;
		}


		// Check to see if targetNode was found
		if(node2 == targetNode){

			targetNode.Init = node1;
			targetNodeReached = true;
			return;
		}

		// Detect none walkable areas
		if(node2.gameObject.tag == "Agent" || node2.gameObject.tag == "Building" || node2.gameObject.tag == "Crate" ||
		   node2.gameObject.tag == "TelephoneBooth" || node2.gameObject.tag == "Tower" || node2.gameObject.tag == "Wall" ||
		   node2.gameObject.tag == "Woodfence"){

			return;
		}

		// Make sure that node wasn't already checked
		if(listClosed.Contains (node2) == false){

			// Make sure the node is in the list of nodes that have not been checked
			if(listOpen.Contains (node2) == true){

				// Get the new cost for the movement
				newMovementCost = node1.costOfMove + baseMoveCost;

				if(newMovementCost < node2.moveCost){

					// if new move cost is lower, then we found a 
					// a new node to use the search from
					node2.Init = node1;
					node2.costOfMove = newMovementCost;
					node2.findTotalCost();
				}
			}

			// Otherwise calculate the move cost and add it to open list
			else{

				node2.Init = node1;
				node2.costOfMove = node1.costOfMove + baseMoveCost;
				node2.findTotalCost();
				AddToOpenList(node2);
			}
		}
	}

	public void AddToOpenList(SearchNodes node){

		listOpen.Add (node);
	}

	public void AddToClosedList(SearchNodes node){

		listClosed.Add (node);
	}

	public void RemoveFromOpenList(SearchNodes node){

		listOpen.Remove (node);
	}
}