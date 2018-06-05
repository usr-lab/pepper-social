using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrainingDuplicator))]
public class TrainingDuplicatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (DrawDefaultInspector())
        {
            TrainingDuplicator dup = (TrainingDuplicator)target;
            if (dup != null)
            {
                dup.Regenerate();
            }
        }
    }
}
