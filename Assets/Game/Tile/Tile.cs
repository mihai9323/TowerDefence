using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	[HideInInspector]public int x,y;
	public bool canWalkOn;

	public Node node;

	public void Configure(int x, int y){
		this.x = x;
		this.y = y;
		node = new Node (x, y);
	}
	public override string ToString ()
	{
		return string.Format ("x:["+x+"]y:["+y+"]canWalkOn:["+canWalkOn+"]");
	}

}
