using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageCursor : MonoBehaviour {

	public GameObject cursor;
	internal bool visible = true;
	GameObject currentCharacter;
	int moveLim = 0;
	Vector3 startPos;
	Vector3 targetPos;
	internal Grid grid;
	Node gridPos;

	void Awake()
	{
		grid = GetComponent<Grid> ();
	}
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		startPos = currentCharacter.transform.position;
		Vector3 mouse = Input.mousePosition;
		mouse.z = 50;
		mouse = Camera.main.ScreenToWorldPoint (mouse);
		targetPos = mouse;
		Node newNode = grid.NodeFromWorldPoint (targetPos);
		int team = currentCharacter.GetComponent<UnitMov> ().team;
		if (newNode != gridPos)
		{
			gridPos = newNode;
			PathRequestManager.RequestPath (new PathRequest (startPos, targetPos, moveLim, true, team, SetPosition));
		}
	}

	public void SwapTarget(GameObject newChar)
	{
		currentCharacter = newChar;
		startPos = currentCharacter.transform.position;
		moveLim = newChar.GetComponent<UnitMov> ().moveLim;
	}

	public void SetPosition(Vector3[] newPath, bool pathSuccessful)
	{
		Color newColor;
		if (newPath.Length == 0 || !visible)
		{
			newColor = Color.clear;
		}
		else if (pathSuccessful)
		{
			newColor = new Color (0, 1, 1, 1);//cyan
		}
		else
		{
			newColor = new Color (1, 0, 0, 1);//red
		}
		cursor.GetComponent<SpriteRenderer> ().color = newColor;
	}

	public void SetVisiblity(bool state)
	{
		visible = state;
	}
}
