using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
// CopyComponents - by Michael L. Croswell for Colorado Game Coders, LLC
// March 2010
//Modified by Kristian Helle Jespersen
//June 2011
//Modified by Connor Cadellin McKee for Excamedia
//April 2015
//Modified by Fernando Medina (fermmmm)
//April 2015
//Modified by Julien Tonsuso (www.julientonsuso.com)
//July 2015
//Changed into editor window and added instant preview in scene view
//Modified by Alex Dovgodko
//June 2017
//Made changes to make things work with Unity 5.6.1
public class ReplaceWithPrefab : EditorWindow
{
	public GameObject Prefab;
	public GameObject[] ObjectsToReplace;
	public List<GameObject> TempObjects = new List<GameObject>();
	public bool KeepOriginalNames = true;
	public bool EditMode = false;
	// Add menu named "My Window" to the Window menu
	[MenuItem("Window/ReplaceWithPrefab")]
	static void Init()
	{
		// Get existing open window or if none, make a new one:
		ReplaceWithPrefab window = (ReplaceWithPrefab)EditorWindow.GetWindow(typeof(ReplaceWithPrefab));
		window.Show();
	}
	void OnSelectionChange()
	{
		GetSelection();
		Repaint();
	}
	void OnGUI()
	{
		EditMode = GUILayout.Toggle(EditMode, "Edit");
		if (GUI.changed)
		{
			if(EditMode)
				GetSelection();
			else
				ResetPreview();
		}
		KeepOriginalNames = GUILayout.Toggle(KeepOriginalNames, "Keep names");
		GUILayout.Space(5);
		if (EditMode)
		{
			ResetPreview();

			GUI.color = Color.yellow;
			if (Prefab != null)
			{
				GUILayout.Label("Prefab: ");
				GUILayout.Label(Prefab.name);
			}else{
				GUILayout.Label("No prefab selected");
			}
			GUI.color = Color.white;

			GUILayout.Space(5);
			GUILayout.BeginScrollView(new Vector2());
			foreach (GameObject go in ObjectsToReplace)
			{
				GUILayout.Label(go.name);
				if (Prefab != null)
				{
					GameObject newObject;
					newObject = (GameObject)PrefabUtility.InstantiatePrefab(Prefab);
					newObject.transform.SetParent(go.transform.parent, true);
					newObject.transform.localPosition = go.transform.localPosition;
					newObject.transform.localRotation = go.transform.localRotation;
					newObject.transform.localScale = go.transform.localScale;
					TempObjects.Add(newObject);
					if (KeepOriginalNames)
						newObject.transform.name = go.transform.name;
					go.SetActive(false);
				}
			}
			GUILayout.EndScrollView();
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Apply"))
			{
				foreach (GameObject go in ObjectsToReplace)
				{
					DestroyImmediate(go);
				}
				EditMode = false;
			};
			if (GUILayout.Button("Cancel"))
			{
				ResetPreview();
				EditMode = false;
			};
			GUILayout.EndHorizontal();
		}
		else
		{
			ObjectsToReplace = new GameObject[0];
			TempObjects.Clear();
			Prefab = null;
		}

	}
	void OnDestroy()
	{
		ResetPreview();
	}
	void GetSelection()
	{
		if (EditMode && Selection.activeGameObject != null)
		{
			PrefabType t = PrefabUtility.GetPrefabType(Selection.activeGameObject);
			if (t == PrefabType.Prefab) //Here goes the fix
			{
				Prefab = Selection.activeGameObject;
			}
			else
			{
				ResetPreview();
				ObjectsToReplace = Selection.gameObjects;
			}
		}
	}
	void ResetPreview()
	{
		if (TempObjects != null)
		{
			foreach (GameObject go in TempObjects)
			{
				DestroyImmediate(go);
			}
		}
		foreach (GameObject go in ObjectsToReplace)
		{
			go.SetActive(true);
		}
		TempObjects.Clear();
	}
}