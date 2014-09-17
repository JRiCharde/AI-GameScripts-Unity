using UnityEngine;
using System.Collections;

public class SearchNodes : MonoBehaviour {

	public int moveCost = 0;
	public int totalCost = 0;
	public int heuristicValue = 0;

	public SearchNodes initial = null;
	// nodes next to current are : 1) Front 2) Right 3)Back 4)Left
	public SearchNodes one = null;
	public SearchNodes two = null;
	public SearchNodes three = null;
	public SearchNodes four = null;


	public int costValue{

		get{

			return totalCost;
		}
	}

	public int hValue{

		get{

			return heuristicValue;
		}

		set{

			heuristicValue = value;
		}
	}

	public int costOfMove{
	
		get{

			return moveCost = 0;
		}

		set{

			moveCost = value;
		}
	}

	public SearchNodes front{

		get{

			return one;
		}
	}

	public SearchNodes right{

		get{

			return two;
		}
	}

	public SearchNodes back{
		
		get{
			
			return three;
		}
	}

	public SearchNodes left{
		
		get{
			
			return four;
		}
	}

	public SearchNodes Init{
		
		get{
			
			return initial;
		}

		set{

			initial = value;
		}
	}

	// Get the total cost for calculating the shortest path
	public void findTotalCost(){

		totalCost = moveCost + heuristicValue;
	}

	// Modified adjacent sensor from assignment #1 to find adjacent nodes
	public void FindAdjacentNode(){

		RaycastHit hit;

		if(Physics.Raycast(this.transform.position, this.transform.forward, out hit) == true){

			one = hit.collider.GetComponent<SearchNodes>();
		}

		if(Physics.Raycast(this.transform.position, this.transform.right, out hit) == true){
			
			two = hit.collider.GetComponent<SearchNodes>();
		}

		if(Physics.Raycast(this.transform.position, this.transform.forward, out hit) == true){
			
			three = hit.collider.GetComponent<SearchNodes>();
		}

		if(Physics.Raycast(this.transform.position, this.transform.right, out hit) == true){
			
			four = hit.collider.GetComponent<SearchNodes>();
		}
	}
}