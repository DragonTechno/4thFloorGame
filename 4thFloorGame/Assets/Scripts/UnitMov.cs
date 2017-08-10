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
	public int moveLim = 10;
	Vector3[] path;
	int targetIndex;
	internal int team;
	internal bool active = false;
	TurnManager turnMan;

	void Awake()
	{
		turnMan = FindObjectOfType<TurnManager> ();	
	}

	// Use this for initialization
	void Start () {
		if (gameObject.tag == "Player")
		{
			team = 1;
		}
		else if (gameObject.tag == "Enemy")
		{
			team = 2;
		}
		else
		{
			team = 3;
		}
		targetPos = transform.position;
		PathRequestManager.RequestPath (new PathRequest (transform.position, targetPos, moveLim, false, team, OnPathFound));
	}

	void Update()
	{
		if (Input.GetMouseButtonDown (0))
		{
			Vector3 mouse = Input.mousePosition;
			mouse.z = 50;
			mouse = Camera.main.ScreenToWorldPoint (mouse);
			targetPos = mouse;
			targetPos.z = 0;
			if (active)
			{
				newTarget = true;
				PathRequestManager.RequestPath (new PathRequest (transform.position, targetPos, moveLim, false, team, OnPathFound));
			}
			else if ((transform.position-targetPos).sqrMagnitude < .1)
			{
				print ("Switch active");
				turnMan.SwitchRequest (gameObject);
			}
		}
	}

	public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
	{
		if (pathSuccessful && active)
		{
			path = newPath;
			turnMan.StartMovement ();
			StopCoroutine ("FollowPath");
			StartCoroutine ("FollowPath");
		}
	}

	/*IEnumerator UpdatePath()
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

	}*/

	IEnumerator FollowPath()
	{
		turnMan.moving = true;
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
					turnMan.EndMovement ();
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
