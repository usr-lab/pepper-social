using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TrainingDuplicator : MonoBehaviour
{
    #region Unity Inspector Variables
    [Tooltip("The agent prefab to generate instances of")]
    public GameObject AgentPrefab;

    [Tooltip("The brain to link to each agent scene.")]
    public Brain Brain;

    [Tooltip("The total number of columns to generate")]
    public int Columns = 3;

    [Tooltip("The spacing between generated columns")]
    public float ColumnSpacing = 5f;

    [Tooltip("The total number of rows to generate")]
    public int Rows = 3;

    [Tooltip("The spacing between generated rows")]
    public float RowSpacing = 5f;

    [Tooltip("The total number of stacks to generate")]
    public int Stacks = 3;

    [Tooltip("The spacing between generated stacks")]
    public float StackSpacing = 5f;
    #endregion // Unity Inspector Variables

    static private void SafeDestroy(GameObject obj)
    {
        if (obj != null)
        {
            if (Application.isEditor)
            {
                Object.DestroyImmediate(obj);
            }
            else
            {
                Object.Destroy(obj);
            }
        }
    }

    #region Internal Methods
    public void ClearInstances()
    {
        /*
        if (AgentPrefab == null) { return; }

        foreach (Transform childTransform in transform)
        {
            GameObject child = childTransform.gameObject;
            PrefabType type = PrefabUtility.GetPrefabType(child);

            if (type == PrefabType.PrefabInstance)
            {
                UnityEngine.Object childPrefab = PrefabUtility.GetPrefabParent(child);
                if (childPrefab == AgentPrefab)
                {
                    SafeDestroy(child);
                }
            }
        }
        */
        Debug.Log($"About to destroy {transform.childCount} object(s)");

        int count = 0;
        for (int i=transform.childCount-1; i >=0; i--)
        {
            count++;
            SafeDestroy(transform.GetChild(i).gameObject);
        }
        Debug.Log($"Destroyed {count} object(s)");
    }

    public void Regenerate()
    {
        // Clear all currently generated instaces
        ClearInstances();

        // If invalid configuration or brain, bail
        if ((AgentPrefab == null) || (Brain == null) || (Columns < 1) || (Stacks < 1)) { return; }

        // Generate new
        var count = 1;
        float totalWidth = Columns * ColumnSpacing;
        float totalDepth = Rows * RowSpacing;
        float totalHeight = Stacks * StackSpacing;

        // Initial Position
        Vector3 Cursor = new Vector3(-((totalWidth/2f) - (ColumnSpacing/2f)), 0, -((totalDepth / 2f) - (RowSpacing/2f)));

        for (var y=0; y < Stacks; y++)
        {
            for (var z=0; z < Rows; z++)
            {
                for (var x=0; x < Columns; x++)
                {
                    var env = Instantiate(AgentPrefab, Cursor, Quaternion.identity);
                    env.transform.SetParent(transform, worldPositionStays: false);
                    env.name = "Environment" + count;
                    count++;

                    var agentScript = env.GetComponentInChildren<Agent>();
                    agentScript.GiveBrain(Brain);

                    // Grow Column
                    Cursor.x += ColumnSpacing;
                }

                // Grow Row
                Cursor.z += RowSpacing;

                // Reset Column
                Cursor.x = -((totalWidth / 2f) - (ColumnSpacing / 2f));
            }

            // Grow Height
            Cursor.y += StackSpacing;

            // Reset Column and Row
            Cursor.x = -((totalWidth / 2f) - (ColumnSpacing / 2f));
            Cursor.z = -((totalDepth / 2f) - (RowSpacing / 2f));
        }
    }
    #endregion // Internal Methods

    // Use this for initialization
    private void OnEnable()
    {
        Regenerate();
    }

    private void Awake()
    {
    }
}