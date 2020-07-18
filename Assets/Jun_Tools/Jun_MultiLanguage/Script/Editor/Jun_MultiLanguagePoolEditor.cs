using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Jun_MultiLanguagePool))]
public class Jun_MultiLanguagePoolEditor : Editor 
{

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty m_languageDatas = serializedObject.FindProperty("m_languageDatas");

        if(GUILayout.Button ("Add Language",EditorStyles.miniButton))
        {
            m_languageDatas.arraySize++;
        }

        if(m_languageDatas != null)
        {
            for (int i = 0; i < m_languageDatas.arraySize; i++)
            {
                SerializedProperty thisData = m_languageDatas.GetArrayElementAtIndex(i);
                SerializedProperty m_key = thisData.FindPropertyRelative("m_key");
                SerializedProperty m_language = thisData.FindPropertyRelative("m_language");

                GUILayout.BeginHorizontal();
                GUILayout.Label("Key:", GUILayout.Width(30));
                EditorGUILayout.PropertyField(m_key, new GUIContent(""),GUILayout.Width(80));
                GUILayout.Label("Language:", GUILayout.Width(50));
                EditorGUILayout.PropertyField(m_language, new GUIContent(""));
                if(GUILayout.Button("-",EditorStyles.miniButton,GUILayout.Width(20)))
                {
                    m_languageDatas.DeleteArrayElementAtIndex(i);
                    break;
                }
                GUILayout.EndHorizontal();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
