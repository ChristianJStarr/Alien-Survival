using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Jun_MultiLanguageText))]
public class Jun_MultiLanguageTextEditor : Editor
{
    Jun_MultiLanguagePool[] multiLanguagePools;
    string[] multiLanguagePoolsNames;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty settingType = serializedObject.FindProperty("settingType");
        SerializedProperty multiLanguage = serializedObject.FindProperty("multiLanguage");
        SerializedProperty m_languagePool = serializedObject.FindProperty("m_languagePool");
        SerializedProperty m_languageKey = serializedObject.FindProperty("m_languageKey");
        SerializedProperty m_languageID = serializedObject.FindProperty("m_languageID");

        int curTypeID = Jun_EditorTools.DrawTagGUI(settingType.enumValueIndex, new string[2] { "Custom", "LanguagePool" });
        int currentPoolID = 0;
        if (curTypeID == 1)
        {
            multiLanguagePools = Resources.LoadAll<Jun_MultiLanguagePool>("");
            multiLanguagePoolsNames = new string[multiLanguagePools.Length];
            for (int i = 0; i < multiLanguagePools.Length; i++)
            {
                multiLanguagePoolsNames[i] = multiLanguagePools[i].name;
                if (m_languagePool.objectReferenceValue == multiLanguagePools[i])
                    currentPoolID = i;
            }
        }
        settingType.enumValueIndex = curTypeID;

        switch (settingType.enumValueIndex)
        {
            case 0:
                EditorGUILayout.PropertyField(multiLanguage, new GUIContent("MultiLanguage"));
                break;

            case 1:
                int poolID = EditorGUILayout.Popup("LanguagePools:",currentPoolID, multiLanguagePoolsNames);
                if (poolID < multiLanguagePools.Length)
                    m_languagePool.objectReferenceValue = multiLanguagePools[poolID];
                
                if(m_languagePool.objectReferenceValue != null)
                {
                    Jun_MultiLanguagePool thisPool = (Jun_MultiLanguagePool)m_languagePool.objectReferenceValue;

                    if(!string.IsNullOrEmpty(m_languageKey.stringValue))
                    {
                        int keyID = thisPool.GetKeyID(m_languageKey.stringValue);
                        if(keyID >= 0)
                        {
                            m_languageID.intValue = keyID;
                        }
                    }

                    int currentSelectID = EditorGUILayout.Popup("Key:",m_languageID.intValue, thisPool.keys);
                    //if(currentSelectID != m_languageID.intValue)
                    //{
                        m_languageID.intValue = currentSelectID;
                        m_languageKey.stringValue = thisPool.GetKey(m_languageID.intValue);
                    //}
                }
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
