using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CaveGenerator))]
public class CaveGeneratorEditor : Editor
{
    private CaveGenerator generator => (CaveGenerator) target;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
        {
            generator.GenerateCave();
            EditorUtility.SetDirty(generator);
        }
    }
}
