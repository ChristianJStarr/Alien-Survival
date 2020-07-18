using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer (typeof(Jun_MultiLanguage))]
public class Jun_MultiLanguageDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		Rect thisRect = EditorGUI.IndentedRect(position);
		float hight = EditorGUIUtility.singleLineHeight;

		// Don't make child fields be indented
		var indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		GUIContent newLabel = new GUIContent(label.text, label.image, label.tooltip);

		GUI.Label(thisRect, newLabel);

        if(!string.IsNullOrEmpty(label.text))
        {
            thisRect.x += EditorGUIUtility.labelWidth;
            thisRect.width -= EditorGUIUtility.labelWidth;
        }

		Vector2 pos = EditorGUIUtility.GUIToScreenPoint(new Vector2(position.x, position.y));
		if (GUI.Button(thisRect, "", EditorStyles.popup))
		{
			Rect newRect = new Rect(pos.x, pos.y, position.width, position.height);
			Jun_MultiLanguageEditor.Open(newRect, property, label.text);
		}

        thisRect.x += 5;

        GUI.Label(thisRect, GetLanguageString(property),EditorStyles.miniLabel);

		// Set indent back to what it was
		EditorGUI.indentLevel = indent;

		EditorGUI.EndProperty();
	}

	public static string GetLanguageString(SerializedProperty property)
	{
		SerializedProperty languages = property.FindPropertyRelative("languages");
		string str = "None";
		for (int i = 0; i < languages.arraySize; i++)
		{
			SerializedProperty thisProperty = languages.GetArrayElementAtIndex(i);
			SerializedProperty systemLanguage = thisProperty.FindPropertyRelative("systemLanguage");
			SerializedProperty languageText = thisProperty.FindPropertyRelative("languageText");

            if (Jun_MultiLanguage.SameLanguages(systemLanguage.enumNames[systemLanguage.enumValueIndex], Application.systemLanguage.ToString()))
			{
				str = languageText.stringValue;
                break;
			}
		}

        if(str == "None" && languages.arraySize > 0)
        {
			SerializedProperty thisProperty = languages.GetArrayElementAtIndex(0);
			SerializedProperty languageText = thisProperty.FindPropertyRelative("languageText");
            str = languageText.stringValue;
        }
		return str;
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		return base.GetPropertyHeight (property,label);
	}
}
