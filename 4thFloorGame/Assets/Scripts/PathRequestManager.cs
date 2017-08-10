using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class PathRequestManager : MonoBehaviour {

//	Queue<PathRequest> reqQueue = new Queue<PathRequest> ();
//	PathRequest currentPathReq;

	Queue<PathResult> results = new Queue <PathResult> ();

	static PathRequestManager instance;
	PathF pathfinding;

	bool isProcessingPath;

	void Awake()
	{
		instance = this;
		pathfinding = GetComponent<PathF> ();
	}

	void Update(){
		if (results.Count > 0)
		{
			int itemsInQueue = results.Count;
			lock (results)
			{
				for (int i = 0; i < itemsInQueue; i++)
				{
					PathResult result = results.Dequeue ();
					result.callback (result.path, result.success);
				}
			}
		}
	}

	public static void RequestPath(PathRequest request)
	{
		/*PathRequest newReq = new PathRequest (pathStart, pathEnd, callback);
		instance.reqQueue.Enqueue (newReq);
		instance.TryProcessNext ();*/
		ThreadStart threadStart = delegate {
			instance.pathfinding.FindPath (request, instance.FinishedProcessingPath);
		};
		threadStart.Invoke ();
	}

	public void FinishedProcessingPath(PathResult result)
	{
		/*currentPathReq.callback (path, success);
		isProcessingPath = false;
		TryProcessNext ();*/
		lock (results)
		{
			results.Enqueue (result);
		}
	}
		
/*	void TryProcessNext()
	{
		if (!isProcessingPath && reqQueue.Count > 0)
		{
			currentPathReq = reqQueue.Dequeue ();
			isProcessingPath = true;
			pathfinding.StartFindPath (currentPathReq.pathStart, currentPathReq.pathEnd);
		}
	}*/
} 

public struct PathResult
{
	public Vector3[] path;
	public bool success;
	public Action<Vector3[], bool> callback;

	public PathResult(Vector3[] _path, bool _success, Action<Vector3[], bool> _callback)
	{
		path = _path;
		success = _success;
		callback = _callback;
	}
}

public struct PathRequest
{
	public Vector3 pathStart;
	public Vector3 pathEnd;
	public int charMoveLim;
	public Action<Vector3[],bool> callback;
	public bool check;
	public int team;

	public PathRequest(Vector3 _pathStart, Vector3 _pathEnd, int _moveLim, bool _check, int _team, Action<Vector3[],bool> _callback)
	{
		pathStart = _pathStart;
		pathEnd = _pathEnd;
		callback = _callback;
		charMoveLim = _moveLim;
		check = _check;
		team = _team;
	}
}