using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class PathF : MonoBehaviour {

	Grid grid;
	public int moveLim = 30;

	void Awake()
	{
		grid = GetComponent<Grid> ();
	}

	public void FindPath(PathRequest request, Action<PathResult> callback)
	{
		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

		Node startNode = grid.NodeFromWorldPoint (request.pathStart);
		startNode.gCost = 0;
		Node targNode = grid.NodeFromWorldPoint (request.pathEnd);

		if(startNode.walkable && targNode.walkable)
		{
			Heap<Node> openSet = new Heap<Node> (grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node> ();
			openSet.Add (startNode);

			while (openSet.Count > 0)
			{
				Node currentNode = openSet.RemoveFirst ();

				closedSet.Add (currentNode);

				if (currentNode == targNode)
				{
					//print (currentNode.gCost);
					if (currentNode.gCost < moveLim)
					{
						pathSuccess = true;
					}
					break;
				}

				foreach (Node neighbor in grid.GetNeighbors(currentNode))
				{
					if (!neighbor.walkable || closedSet.Contains (neighbor))
					{
						continue;
					}

					int newMoveCost = currentNode.gCost + GetDistance (currentNode, neighbor) + neighbor.movePenalty;
					if (newMoveCost < neighbor.gCost || !openSet.Contains (neighbor))
					{
						neighbor.gCost = newMoveCost;
						neighbor.parent = currentNode;
						neighbor.hCost = GetDistance (neighbor, targNode);
						if (!openSet.Contains (neighbor))
						{
							openSet.Add (neighbor);
						}
						else
						{
							openSet.UpdateItem (neighbor);
						}
					}
				}
			}
		}
		if(pathSuccess)
		{
			waypoints = RetracePath(startNode, targNode);
			pathSuccess = waypoints.Length > 0;
		}
		callback (new PathResult (waypoints, pathSuccess, request.callback));
	}

	Vector3[] RetracePath(Node startNode, Node endNode)
	{
		List<Node> path = new List<Node> ();
		Node currentNode = endNode;
		while(currentNode != startNode)
		{
			path.Add (currentNode);
			currentNode = currentNode.parent;
		}
		path.Add (startNode);
		Vector3[] waypoints = SimplifyPath (path);
		Array.Reverse (waypoints);
		return waypoints;
	}

	Vector3[] SimplifyPath(List<Node> path)
	{
		List<Vector3> waypoints = new List<Vector3> ();
		Vector2 directionOld = Vector2.zero;

		for (int i = 1; i < path.Count; i++)
		{
			Vector2 directionNew = new Vector2 (path [i - 1].gridX - path [i].gridX, path [i - 1].gridY - path [i].gridY);
			if (directionNew != directionOld)
			{
				waypoints.Add(path [i-1].WorldPosition);
			}
			directionOld = directionNew;
		}
		return waypoints.ToArray();
	}

	int GetDistance(Node nodeA, Node nodeB)
	{
		int yDist = Mathf.Abs (nodeA.gridY - nodeB.gridY);
		int xDist = Mathf.Abs (nodeA.gridX - nodeB.gridX);
		if (yDist > xDist)
		{
			return 14 * xDist + 10 * (yDist - xDist);
		}
		else
		{
			return 14 * yDist + 10 * (xDist - yDist);
		}
	}
}
