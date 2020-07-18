using UnityEngine;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Jun_EditorTools
{
	public static float titleSize
	{
		get{return 66f;}
	}

	#region<Create and load>
	static public T LoadObjectInAssets <T>(string saveID) where T:Component
	{
		if(!string.IsNullOrEmpty(EditorPrefs.GetString(saveID)))
		{
			T loadCom = LoadComponent<T> (EditorPrefs.GetString(saveID),saveID);
			if(loadCom != null)
				return loadCom;
		}

		List<T> prefabs = GetPrefabsFromAssets <T> (true,saveID);
		if (prefabs.Count > 0) 
		{
			if(prefabs [0] != null)
			    return prefabs [0];
		}

		return null;
	}

	static public T LoadComponent <T> (string path) where T:Component
	{
		return LoadComponent<T>(path,"");
	}

	static public T LoadComponent <T> (string path, string saveID) where T:Component
	{
		GameObject go = AssetDatabase.LoadAssetAtPath(path,typeof(GameObject)) as GameObject;
		if(go != null)
		{
			if(go.GetComponent<T>() != null)
			{
				T loadCom = go.GetComponent<T>();
				if(!string.IsNullOrEmpty(saveID)) EditorPrefs.SetString(saveID,path);
				return loadCom;
			}
		}
		else
		{
			Debug.Log("GameObject is null in path:" + path);
		}
		return null;
	}

	static public T GetPrefabDate <T>() where T:Component
	{
		string path = EditorUtility.OpenFilePanel("Load As", "", "prefab");
		if(string.IsNullOrEmpty(path))return null;
		path = path.Substring(path.IndexOf("Assets"));
		GameObject assetObj = AssetDatabase.LoadAssetAtPath(path,typeof(GameObject)) as GameObject;
		if(assetObj != null)
		{
			if(assetObj.GetComponent<T>() != null)
				return assetObj.GetComponent<T>();
		}
		return null;
	}

	static public T CreatePrefabDate <T>(string name) where T:Component
	{
		string path = EditorUtility.SaveFilePanelInProject("Save As", name + ".prefab", "prefab", "Save as...");
		if(!string.IsNullOrEmpty(path))
		{
			return CreatePrefabDateAtPath <T>(path,name);
		}
		return null;
	}

	static public T CreatePrefabDateAtPath <T>(string path,string name) where T:Component
	{
		if(!string.IsNullOrEmpty(path))
		{
			GameObject go = new GameObject();
			go.name = name;
			go.AddComponent<T>();
			PrefabUtility.CreatePrefab(path,go);
			Object.DestroyImmediate(go);
			return AssetDatabase.LoadAssetAtPath(path,typeof(T)) as T;
		}
		return null;
	}

	static public void DeletePrefabData (GameObject prefabData)
	{
		if(AssetDatabase.IsMainAsset (prefabData))
			AssetDatabase.DeleteAsset (AssetDatabase.GetAssetPath (prefabData));
	}

	static public List<T> GetPrefabsFromAssets <T> () where T:Component
	{
		return GetPrefabsFromAssets <T>(false,"");
	}

	static public List<T> GetPrefabsFromAssets <T> (bool isJustOne,string saveID) where T:Component
	{
		List<T> prefabs = new List<T> ();
		string[] paths = AssetDatabase.GetAllAssetPaths();
		for(int i = 0; i < paths.Length; i++)
		{
			EditorUtility.DisplayProgressBar("Loading...", "Searching items, please wait...", (float)i / paths.Length);
			if(paths[i].Contains(".prefab"))
			{
				T loadCom = LoadComponent<T>(paths[i],saveID);
				if(loadCom != null)
				{
					prefabs.Add(loadCom);
					if(isJustOne)
					{
						EditorUtility.ClearProgressBar();
						return prefabs;
					}
				}
			}
		}
		EditorUtility.ClearProgressBar();
		return prefabs;
	}
	#endregion

	#region<Draw GUI>
	public static int DrawTagGUI (int selectID,string[] tagNames,params GUILayoutOption[] options)
	{
		int startID = 0;
		return DrawTagGUI (selectID,tagNames,ref startID,tagNames.Length,options);
	}

	public static int DrawTagGUI (int selectID,string[] tagNames,ref int startID,int showTagCount,params GUILayoutOption[] options)
	{
		int curSelectID = selectID;
		startID = startID < 0?0:startID;
		GUILayout.BeginHorizontal(options);

		string[] showTagNames = new string[0];
		bool allShow = false;

		if(tagNames.Length <= showTagCount)
		{
			allShow = true;
			if(startID != 0 && tagNames.Length == showTagCount + startID)
			{
				allShow = false;
			}
		}

		if(allShow)
		{
			showTagNames = tagNames;
			startID = 0;
		}
		else
		{
			showTagNames = new string[showTagCount];
			for (int i = 0; i < showTagCount; i++)
			{
				if((i + startID) < tagNames.Length)
				{
					showTagNames[i] = tagNames[i + startID];
				}
				else
				{
					showTagNames[i] = "不存在的";
					if(startID > 0)
						startID --;
				}
			}
		}

		if(!allShow && startID > 0)
		{
			GUI.backgroundColor = Color.green;
			if(GUILayout.Button ("←",EditorStyles.miniButtonLeft,GUILayout.Width (20),GUILayout.Height(18)))
			{
				startID --;
				startID = startID < 0?0:startID;
			}
			GUI.backgroundColor = Color.white;
		}

		for (int i = 0; i < showTagNames.Length; i++)
		{
			bool click = false;
			bool curClick = false;
			if(selectID == i + startID)
				click = true;

			GUIStyle thisButtonStyle = EditorStyles.miniButtonMid;
			if(tagNames.Length > 1)
			{
				if(i == 0 && startID == 0)
					thisButtonStyle = EditorStyles.miniButtonLeft;

				if(i == showTagNames.Length - 1 && startID >= tagNames.Length - showTagCount)
					thisButtonStyle = EditorStyles.miniButtonRight;
			}
			else
			{
				thisButtonStyle = EditorStyles.miniButton;
			}

			curClick = GUILayout.Toggle (click,showTagNames[i],thisButtonStyle,GUILayout.Height(18));

			if(!click && curClick)
			{
				if(i + startID < tagNames.Length)
					curSelectID = i + startID;
				else
					curSelectID = tagNames.Length - 1;
			}
		}

		if(!allShow && startID < tagNames.Length - showTagCount)
		{
			GUI.backgroundColor = Color.green;
			if(GUILayout.Button ("→",EditorStyles.miniButtonRight,GUILayout.Width (20),GUILayout.Height(18)))
			{
				startID ++;
				startID = startID >= tagNames.Length - showTagCount?tagNames.Length - showTagCount:startID;
			}
			GUI.backgroundColor = Color.white;
		}

		GUILayout.EndHorizontal();

		return curSelectID;
	}

	public static float Slider (string name,float sliderValue,float leftValue,float rightValue,bool isHorizontal)
	{
		float curValue = 0;

		if(!isHorizontal)
		{
			GUILayout.BeginVertical();
			GUILayout.Label(name + leftValue + " ~ " + rightValue,EditorStyles.miniLabel);
			curValue = GUILayout.VerticalSlider(sliderValue,leftValue,rightValue);
			string labelText = ((int)(curValue*100.0f/rightValue)).ToString() + "%";
			GUILayout.Label(labelText,EditorStyles.boldLabel,GUILayout.Width(25));
			GUILayout.EndVertical();
		}
		else
		{
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Label(name);
			GUILayout.Label(leftValue + " ~ " + rightValue,EditorStyles.miniLabel);

			string labelText = ((int)(sliderValue*100.0f/rightValue)).ToString() + "%";
			GUILayout.Label(labelText,GUILayout.Width(33));
			GUILayout.EndHorizontal();

			curValue = GUILayout.HorizontalSlider(sliderValue,leftValue,rightValue);
			GUILayout.EndVertical();


		}
		return curValue;
	}

	public static void DrayLineInScene (Vector3[] pathPoints)
	{
		for (int i = 1; i < pathPoints.Length; i++)
		{
			Handles.DrawLine (pathPoints[i - 1],pathPoints[i]);
		}
	}
	
	public static void SpaceLine(float size)
	{
		GUI.backgroundColor = new Color(1,1,1,0.6f);
		GUILayout.Space(2);
        GUILayout.Label("",EditorStyles.textArea,GUILayout.Height(size));
		GUILayout.Space(2);
		GUI.backgroundColor = Color.white;
	}
	
	public static void TitleLine (string Title,float lineSize)
	{
		GUILayout.BeginVertical ();
		GUILayout.Space(2);
        GUILayout.Label (Title,EditorStyles.boldLabel);
        GUILayout.Label("",EditorStyles.textField,GUILayout.Height(lineSize));
		GUILayout.EndVertical ();
	}
	
	public static int IntField (string fieldName,int curValue,params GUILayoutOption[] options)
	{
		return IntField (fieldName,curValue,0,0,options);
	}
	
	public static int IntField (string fieldName,int curValue,int minValue,int maxValue,params GUILayoutOption[] options)
	{
		GUILayout.BeginHorizontal ();
		GUILayout.Label (fieldName,GUILayout.Width (titleSize));
		int setValue = EditorGUILayout.IntField (curValue,options);
		GUILayout.EndHorizontal ();
		if(minValue != 0 || maxValue != 0)
		{
			if(setValue < minValue)setValue = minValue;
			if(setValue > maxValue)setValue = maxValue;
		}
		return setValue;
	}
	
	public static float FloatField (string fieldName,float curValue,params GUILayoutOption[] options)
	{
		return FloatField (fieldName,curValue,0,0,options);
	}
	
	public static float FloatField (string fieldName,float curValue,float minValue,float maxValue,params GUILayoutOption[] options)
	{
		GUILayout.BeginHorizontal ();
		GUILayout.Label (fieldName,GUILayout.Width (titleSize));
		float setValue = EditorGUILayout.FloatField (curValue,options);
		GUILayout.EndHorizontal ();
		if(minValue != 0 || maxValue != 0)
		{
			if(setValue < minValue)setValue = minValue;
			if(setValue > maxValue)setValue = maxValue;
		}
		return setValue;
	}
	
	public static bool Toggle (string toggleName,bool curValue,params GUILayoutOption[] options)
	{
		GUILayout.BeginHorizontal ();
		GUILayout.Label (toggleName,GUILayout.Width (titleSize));
		curValue = EditorGUILayout.Toggle (curValue,options);
		GUILayout.EndHorizontal ();
		return curValue;
	}

	public static Object ObjectField(string fieldName,Object obj,System.Type objType,params GUILayoutOption[] options)
	{
		return ObjectField (fieldName,obj,objType,false,options);
	}

	public static Object ObjectField(string fieldName,Object obj,System.Type objType,bool showClearButton,params GUILayoutOption[] options)
	{
		GUILayout.BeginHorizontal ();
		GUILayout.Label (fieldName,GUILayout.Width (titleSize));
        obj = EditorGUILayout.ObjectField (obj, objType, true,options);

		if(showClearButton)
		{
			GUI.backgroundColor = Color.red;
			if(GUILayout.Button ("Clear",EditorStyles.miniButton,GUILayout.Width (40)))
			{
				if(EditorUtility.DisplayDialog("ClearObject","Are you clear this Object?","Yes","No"))
				{
					obj = null;
				}
			}
			GUI.backgroundColor = Color.white;
		}
		GUILayout.EndHorizontal ();
		return obj;
	}

    public static void ObjectField(string fieldName, SerializedProperty obj, params GUILayoutOption[] options)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(fieldName, GUILayout.Width(titleSize));

        //obj.objectReferenceValue = EditorGUILayout.ObjectField(obj.objectReferenceValue,typeof(T),false);
        EditorGUILayout.PropertyField(obj,new GUIContent(""),options);

		GUI.backgroundColor = Color.red;
		if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(40)))
		{
			if (EditorUtility.DisplayDialog("ClearObject", "Are you clear this Object?", "Yes", "No"))
			{
				obj.objectReferenceValue = null;
			}
		}
		GUI.backgroundColor = Color.white;

		GUILayout.EndHorizontal();
	}
	#endregion 

	#region<Other>
	public static string StringArray (string str,bool isHorizontal)
	{
		if(!isHorizontal)
			return StringToVertical (str);
		return str;
	}

	public static string StringToVertical (string str)
	{
		string newStr = "";
		for (int i = 0; i < str.Length; i++)
		{
			newStr += str.Substring(i,1);
			if(i != str.Length - 1)
				newStr += "\n";
		}
		return newStr;
	}
	#endregion
}
