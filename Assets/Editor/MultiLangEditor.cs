using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(MultiLangText))]
public class MultiLangEditor : Editor
{
    string[] _choices;
    int _choiceIndex = 0;

    
    public override void OnInspectorGUI()
    {

        _choices = MultiLangSystem.GetKeys();
        MultiLangText myTarget = (MultiLangText)target;
        DrawDefaultInspector();
        if(_choices != null && _choices.Length > 0) 
        {
            _choices = MultiLangSystem.GetKeys();
            _choiceIndex = EditorGUILayout.Popup("Key Lookup", _choiceIndex, _choices);
            // Update the selected choice in the underlying object
            myTarget.key = _choices[_choiceIndex];
            myTarget.UpdateText();
        }
    }
}
