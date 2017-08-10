using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour {

	public GameObject[] players;
	public GameObject[] enemies;
	internal bool moving = false;
	internal int team = 1;
	List<GameObject> unitQueue;
	int charIndex;
	GameObject activeChar;
	ManageCursor manageCursor;

	// Use this for initialization
	void Awake () {
		manageCursor = FindObjectOfType<ManageCursor> ();
	}

	void Start()
	{
		foreach(GameObject unit in players)
		{
			Node occNode = manageCursor.grid.NodeFromWorldPoint (unit.transform.position);
			unit.transform.position = occNode.WorldPosition;
			occNode.occupied = 1;
		}
		foreach (GameObject unit in enemies)
		{
			Node occNode = manageCursor.grid.NodeFromWorldPoint (unit.transform.position);
			unit.transform.position = occNode.WorldPosition;
			occNode.occupied = 2;
		}
		unitQueue = new List<GameObject> ();
		for (int i = 0; i < players.Length; ++i)
		{
			unitQueue.Add (players [i]);
		}
		SwitchCharacter (0);
	}

	// Update is called once per frame
	void Update () {
		
	}

	public void SwitchCharacter(int newChar)
	{
		if (unitQueue.Count == 0)
		{
			if (team == 2)
			{
				team = 1;
				for (int i = 0; i < players.Length; ++i)
				{
					unitQueue.Add (players [i]);
				}
			}
			else
			{
				team = 2;
				for (int i = 0; i < enemies.Length; ++i)
				{
					unitQueue.Add (enemies [i]);
				}
			}
			SwitchCharacter (0);
			return;
		}
		charIndex = newChar;
		if (activeChar != null)
		{
			activeChar.GetComponent<SpriteRenderer> ().color = Color.white;
			activeChar.GetComponent<UnitMov> ().active = false;
		}
		activeChar = unitQueue[newChar];
		activeChar.GetComponent<UnitMov> ().active = true;
		activeChar.GetComponent<SpriteRenderer> ().color = team == 1 ? Color.green : Color.magenta;
		manageCursor.SwapTarget (activeChar);
	}

	public void SwitchRequest(GameObject newChar)
	{
		if(unitQueue.Contains(newChar))
		{
			int index = unitQueue.IndexOf (newChar);
			SwitchCharacter (index);
		}
	}

	public void StartMovement()
	{
		manageCursor.SetVisiblity (false);
		moving = true;
	}

	public void EndMovement()
	{
		manageCursor.SetVisiblity (true);
		moving = false;
		unitQueue.RemoveAt (charIndex);
		if (charIndex == unitQueue.Count)
		{
			SwitchCharacter (0);
		}
		else
		{
			SwitchCharacter (charIndex);
		}
	}
}
