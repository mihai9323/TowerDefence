using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class AStar
{
	
	private static List<Node> OpenList, ClosedList;
	
	public static void CleanNodes ()
	{
		for (int x = 0; x<Board.xSize; x++) {
			for (int y = 0; y<Board.ySize; y++) {
				
				Board.GameBoard [x, y].node = new Node (x, y);
				
			}
		}
	}
	
	public static int CalculateDistance(Tile start, Tile finish)
	{
		
		Tile[] path = CalculatePath(start, finish).ToArray();
		
		if (path == null)
			return int.MaxValue;
		if (path.Length == 0)
			return int.MaxValue;
		return path.Length;
	}
	
	
	public static List<Tile> CalculatePath(Tile startTile, Tile finalTile)
	{
		List<Tile> path = new List<Tile>();
		OpenList = new List<Node>();
		ClosedList = new List<Node>();
		Node startNode = startTile.node;
		Node finalNode = finalTile.node;
		
		OpenList.Add(startNode);
		int nrOp = 0;
		while (OpenList.Count>0 && !ClosedList.Contains(finalNode))
		{
			Node currentNode = selectBestNode(OpenList.ToArray());
			if (nrOp > 300)
			{
				break;
			}
			//Debug.Log (OpenList.Count+" before "+ClosedList.Count);
			OpenList.Remove(currentNode);
			ClosedList.Add(currentNode);
			//Debug.Log (OpenList.Count+" after "+ClosedList.Count);
			for (int x  = Mathf.Max (0,currentNode.x-1); x<=Mathf.Min (Board.xSize-1,currentNode.x+1); x++)
			{
				for (int y  = Mathf.Max (0,currentNode.y-1); y<=Mathf.Min (Board.ySize-1,currentNode.y+1); y++)
				{
					nrOp++;
					if (Mathf.Abs(x - currentNode.x) != Mathf.Abs(y - currentNode.y))
					{
						Tile checkTile = Board.GameBoard [x, y];
						if (checkTile.canWalkOn)
						{
							if (!ClosedList.Contains(checkTile.node))
							{
								if (OpenList.Contains(checkTile.node))
								{
									if (checkTile.node.gCost > CalculateGCost(currentNode, x, y))
									{
										checkTile.node.parent = currentNode;
										checkTile.node.gCost = CalculateGCost(currentNode, x, y);
										checkTile.node.fCost = checkTile.node.gCost + checkTile.node.hCost;
									}
								} else
								{
									checkTile.node.parent = currentNode;
									checkTile.node.gCost = CalculateGCost(currentNode, x, y);
									checkTile.node.hCost = CalculateHCost(checkTile.node, finalNode);
									checkTile.node.fCost = checkTile.node.gCost + checkTile.node.hCost;
									OpenList.Add(checkTile.node);
								}
							}
							
							
						}
					}
				}
			}
		}
		
		Node auxNode = finalNode;
		if (ClosedList.Contains(finalNode))
		{
			while (auxNode!=null)
			{
				path.Add(Board.GameBoard [auxNode.x, auxNode.y]);
				auxNode = auxNode.parent;
			}
		}
		path.Reverse();
		return path;
	}
	
	public static Node selectBestNode(Node[] list)
	{
		float bestScore = float.MaxValue;
		Node bestNode = list [0];
		
		foreach (Node n in list)
		{
			if (n.fCost < bestScore)
			{
				bestScore = n.fCost;
				bestNode = n;
			}
		}
		return bestNode;
	}
	
	public static int CalculateGCost(Node prevNode, int x, int y)
	{
		
		return prevNode.gCost + Mathf.Abs(prevNode.x - x) + Mathf.Abs(prevNode.y - y);
	}
	
	public static int CalculateHCost(Node nodeCalculated, Node finalNode)
	{
		return Mathf.Abs(nodeCalculated.x - finalNode.x) + Mathf.Abs(finalNode.y - nodeCalculated.y); 
	}
}

public class Node
{
	
	
	public int hCost;
	public int gCost;
	public int fCost;
	public int x, y;
	public Node parent;
	
	public Node(int x, int y)
	{
		this.x = x;
		this.y = y;
	}
	
	public Node(int x, int y, Node parent)
	{
		this.parent = parent;
		this.x = x;
		this.y = y;
	}
	
	public bool SameNode(Node n)
	{
		if (this.x == n.x && this.y == n.y)
		{
			return true;
		} else
			return false;
	}
	
	public static bool SameNode(Node n1, Node n2)
	{
		return (n1.x == n2.x && n1.y == n2.y);
	}
}
