using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Jun_MultiLanguageEditor : Jun_PopupWindow
{
    public static void Open (Rect rect,SerializedProperty property,string name)
    {
        Jun_MultiLanguageEditor windows = Jun_PopupWindow.Show<Jun_MultiLanguageEditor>();
        windows.position = new Rect(rect.x, rect.y, rect.width, 50);
        windows.languageProperty = property;
        windows.propertyName = name;
    }

    SerializedProperty languageProperty;
    string propertyName;

    void Update()
    {
        if (EditorWindow.focusedWindow != this)
           this.Close();
    }

    public override void OnGUI ()
    {	
        GUILayout.BeginVertical();
        Rect startRectHeight = GUILayoutUtility.GetRect(1, 1);
        languageProperty.serializedObject.Update();
        SerializedProperty languages = languageProperty.FindPropertyRelative("languages");

        if(GUILayout.Button(propertyName,EditorStyles.label))
        {
            GUI.FocusControl("");
        }

        if (GUILayout.Button("Add Language",EditorStyles.miniButton))
        {
            languages.arraySize += 1;
            if (languages.arraySize == 1)
            {
                SerializedProperty thisLanguages = languages.GetArrayElementAtIndex(0);
                SerializedProperty systemLanguage = thisLanguages.FindPropertyRelative("systemLanguage");
                SerializedProperty fontSize = thisLanguages.FindPropertyRelative("fontSize");
                systemLanguage.enumValueIndex = 10;
            }
            GUI.FocusControl("");
        }

        if (languages.arraySize > 0)
        {
            for (int i = 0; i < languages.arraySize; i++)
            {
                SerializedProperty thisLanguages = languages.GetArrayElementAtIndex(i);

                SerializedProperty systemLanguage = thisLanguages.FindPropertyRelative("systemLanguage");
                SerializedProperty languageText = thisLanguages.FindPropertyRelative("languageText");
                SerializedProperty fontSize = thisLanguages.FindPropertyRelative("fontSize");

                EditorGUILayout.BeginHorizontal();

                if(GUILayout.Button("★",EditorStyles.miniButton,GUILayout.Width(20)))
                {
                    
                }
                EditorGUILayout.PropertyField(systemLanguage, new GUIContent(""),GUILayout.Width(80));
                languageText.stringValue = EditorGUILayout.TextArea(languageText.stringValue,GUILayout.Width(position.width - 200));

                EditorGUIUtility.labelWidth = 30;
                fontSize.intValue = EditorGUILayout.IntField("Size:",fontSize.intValue,GUILayout.Width(60));

                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("-",EditorStyles.miniButton,GUILayout.Width(20)))
                {
                    languages.DeleteArrayElementAtIndex(i);
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
        }
        languageProperty.serializedObject.ApplyModifiedProperties();
        Rect lastRectHeight = GUILayoutUtility.GetRect(1, 1);
        GUILayout.EndVertical();

        Vector2 size = new Vector2(position.width, lastRectHeight.y - startRectHeight.y);

        if (size.y < 50)
            size = new Vector2(position.width, 50);

        minSize = maxSize = size;
            
    }
}
