using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

	public bool displayGridGizmos;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	public TerrainType[] walkableRegions;
	public float rayAdjust;
	LayerMask walkableMask;
	Node[,] grid;
	Dictionary<int,int> walkableDict = new Dictionary<int, int>();

	float nodeDiameter;
	int gridSizeX, gridSizeY;

	void Awake()
	{
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
		foreach (TerrainType region in walkableRegions)
		{
			walkableMask.value |= region.terrainMask.value;
			walkableDict.Add ((int)Mathf.Log(region.terrainMask.value,2),region.terrainPenalty);
		}
		CreateGrid ();
	}

	public int MaxSize
	{
		get {
			return gridSizeX * gridSizeY;
		}	
	}

	void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * (gridWorldSize.x / 2 - rayAdjust) - Vector3.up * gridWorldSize.y / 2;
		int movementPenalty = 0;

		for (int x = 0; x < gridSizeX; x++)
		{
			Vector3 startPoint = worldBottomLeft + Vector3.right * x * nodeRadius * 2;
			RaycastHit2D[] column = Physics2D.RaycastAll (startPoint, Vector3.up, gridSizeY * nodeRadius * 2, walkableMask);
			foreach(RaycastHit2D hit in column)
			{
				bool setGrid = false;
				GameObject other = hit.collider.gameObject;
				int[] gridPos = GridPosFromWorldPoint(hit.transform.position+Vector3.up*.5f);
				int gridY = gridPos [1];
				if (other.tag == "Ground")
				{
					gridY += 1;
				}
				if (grid [x, gridY] == null)
				{
					setGrid = true;
				}
				else if(grid[x,gridY].tag == "Climbable")
				{
					setGrid = true;
				}
				if (setGrid)
				{
					walkableDict.TryGetValue (hit.collider.gameObject.layer, out movementPenalty);
					Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (gridY * nodeDiameter + nodeRadius);
					grid[x, gridY] = new Node (true, worldPoint,x,gridY,movementPenalty,other.tag);
				}
			}
			for (int y = 0; y < gridSizeY; y++)
			{
				if (grid [x, y] == null)
				{
					Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
					grid[x, y] = new Node (false, worldPoint,x,y,0,"");
				}
			}
		}
		/*for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);	

				int movementPenalty = 0;

				if (walkable)
				{
					Ray ray = new Ray (worldPoint + Vector3.forward * 50, Vector3.back);
					RaycastHit hit;
					if(Physics.Raycast(ray,out hit, 100, walkableMask))
					{
						walkableDict.TryGetValue (hit.collider.gameObject.layer, out movementPenalty);
					}
				}

				grid[x, y] = new Node (walkable, worldPoint,x,y,movementPenalty);
			}
		}*/
	}

	public List<Node> GetNeighbors(Node node)
	{
		List<Node> neighbors = new List<Node> ();
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == y || x == -y)
				{
					continue;
				}
				int checkX = node.gridX + x;
				int checkY = node.gridY + y;
				if (checkY >= 0 && checkX >= 0 && checkY < gridSizeY && checkX < gridSizeX)
				{
					neighbors.Add (grid [checkX, checkY]);
				}
			}
		}

		return neighbors;
	}

	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		int[] coord= GridPosFromWorldPoint(worldPosition);
		return grid [coord[0], coord[1]];
	}

	public int[] GridPosFromWorldPoint (Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01 (percentX);
		percentY = Mathf.Clamp01 (percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY*1.05f-.5f);
		int[] coord = {x,y};
		return coord;
	}

	public Vector3 NodePosFromWorldPoint (Vector3 worldPosition)
	{
		Vector3 worldBottomLeft = transform.position - Vector3.right * (gridWorldSize.x / 2 - rayAdjust) - Vector3.up * gridWorldSize.y / 2;
		int[] gridPos = GridPosFromWorldPoint (worldPosition);
		Vector3 worldPoint = worldBottomLeft + Vector3.right * (gridPos[0] * nodeDiameter + nodeRadius) + Vector3.up * (gridPos[1] * nodeDiameter + nodeRadius);
		return worldPoint;
	}
		
	void OnDrawGizmos() {
		Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,gridWorldSize.y,1));

		if (grid != null && displayGridGizmos)
		{
			foreach(Node n in grid)
			{	
				if (n.tag == "Climbable")
				{
					Gizmos.color = Color.blue;
				}
				else if (n.tag == "Ground")
				{
					Gizmos.color = Color.white;
				}
				else
				{
					Gizmos.color = Color.clear;
				}	
				Gizmos.DrawCube (n.WorldPosition, Vector3.one * (nodeDiameter-.1f));
			}
		}
	}

	[System.Serializable]
	public class TerrainType
	{
		public LayerMask terrainMask;
		public int terrainPenalty;
	}
}
