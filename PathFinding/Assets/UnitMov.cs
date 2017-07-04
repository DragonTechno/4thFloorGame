using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMov : MonoBehaviour {

	const float minPathUpdateTime = .2f;
	const float updateMoveThresh = .5f;
	//public Transform target;
	public Vector3 targetPos;
	public bool newTarget;
	public float checkTime = 0;
	public float speed = 1;
	Vector3[] path;
	int targetIndex;

	// Use this for initialization
	void Start () {
		targetPos = transform.position;
		StartCoroutine (UpdatePath ());
	}

	void Update()
	{
		if (Input.GetMouseButtonDown (0))
		{
			newTarget = true;
			Vector3 mouse = Input.mousePosition;
			mouse.z = 50;
			print (mouse);
			mouse = Camera.main.ScreenToWorldPoint (mouse);
			print (mouse);
			targetPos = mouse;
			print (targetPos);
		}
	}

	public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
	{
		if (pathSuccessful)
		{
			path = newPath;
			StopCoroutine("FollowPath");
			StartCoroutine ("FollowPath");
		}
	}

	IEnumerator UpdatePath()
	{
		if (Time.timeSinceLevelLoad < .3f)
		{
			yield return new WaitForSeconds (.3f);
		}
		PathRequestManager.RequestPath (new PathRequest (transform.position, targetPos, OnPathFound));

		while (true)
		{
			yield return new WaitForSeconds (minPathUpdateTime);
			if (newTarget)
			{
				PathRequestManager.RequestPath (new PathRequest (transform.position, targetPos, OnPathFound));
			}
		}

	}

	IEnumerator FollowPath()
	{
		targetIndex = 0;
		Vector3 currentWaypoint = path [0];

		while (true)
		{
			if (transform.position == currentWaypoint)
			{
				targetIndex++;
				if (targetIndex >= path.Length)
				{
					newTarget = false;
					yield break;
				}
				currentWaypoint = path [targetIndex];
			}

			transform.position = Vector3.MoveTowards (transform.position, currentWaypoint, speed*Time.deltaTime);
			yield return null;
		}
	}

	public void OnDrawGizmos()
	{
		if (path != null)
		{
			for (int i = targetIndex; i < path.Length; i++)
			{
				Gizmos.color = Color.magenta;
				Gizmos.DrawCube (path [i], Vector3.one);

				if (i == targetIndex)
				{
					Gizmos.DrawLine (transform.position, path [i]);
				}
				else
				{
					Gizmos.DrawLine (path [i - 1], path [i]);
				}
			}
		}
	}
}
