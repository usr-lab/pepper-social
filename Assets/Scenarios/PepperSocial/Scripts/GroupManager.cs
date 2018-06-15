using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupManager : MonoBehaviour {

    public float bSpace = 15.0f;
    public float cSpace = 10.0f;
    public float oSpace = 3.0f;    
    public int numberOfAgent = 3;    
    private GameObject agentPrefab;
    private GameObject[] agents;
    private Vector3 oCenter = Vector3.zero;
    private GameObject centerSphere;
    private GameObject lineDrawPrefab;
    private GameObject lineDrawPrefabBSpace;
    private GameObject lineDrawPrefabCSpace;
    private GameObject lineDrawPrefabOSpace;

    // Use this for initialization
    void Start () {
        // naive spawn only one group
        SpawnAgents();
        GetPotentialAgents();
        centerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        centerSphere.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        centerSphere.GetComponent<Renderer>().material.color = Color.red;
        InitGroupCenter();
        lineDrawPrefab = Resources.Load("Prefab/lineDrawPrefab") as GameObject;
        lineDrawPrefabBSpace = GameObject.Instantiate(lineDrawPrefab) as GameObject;
        lineDrawPrefabCSpace = GameObject.Instantiate(lineDrawPrefab) as GameObject;
        lineDrawPrefabOSpace = GameObject.Instantiate(lineDrawPrefab) as GameObject;
    }
	
	// Update is called once per frame
	void Update () {
        DrawAreas(bSpace, Color.red, lineDrawPrefabBSpace.GetComponent<LineRenderer>());
        DrawAreas(cSpace, Color.yellow, lineDrawPrefabCSpace.GetComponent<LineRenderer>());
        DrawAreas(oSpace, Color.green, lineDrawPrefabOSpace.GetComponent<LineRenderer>());
    }

    public void SpawnAgents()
    {
        agentPrefab = Resources.Load("Prefab/Agent") as GameObject;
        for (int i = 0; i < numberOfAgent; i++)
        {
            float directionFacing = Random.Range(0f, 360f);

            // need to pick a random position around originPoint but inside spawnRadius
            // must not be too close to another agent inside spawnRadius
            Vector3 point = (Random.insideUnitSphere * bSpace) + this.transform.position;
            point.y = this.transform.position.y;
            Instantiate(agentPrefab, point, Quaternion.Euler(new Vector3(0f, directionFacing, 0f)));
        }
    }


    public void ResetAgents()
    {
        for (int i = 0; i < numberOfAgent; i++)
        {
            float directionFacing = Random.Range(0f, 360f);

            // need to pick a random position around originPoint but inside spawnRadius
            // must not be too close to another agent inside spawnRadius
            Vector3 point = (Random.insideUnitSphere * bSpace) + this.transform.position;
            point.y = this.transform.position.y;
            agents[i].transform.position = point;
            agents[i].transform.forward = new Vector3(Mathf.Cos(directionFacing * Mathf.PI / 180.0f), 0.0f, Mathf.Sin(directionFacing * Mathf.PI / 180.0f));
        }
        UpdateGroupCenter(agents);
    }

    public Vector3 GetGroupCenter()
    {
        return oCenter;
    }

    public int GetOSpaceAgents()
    {
        int count = 0;
        Collider[] colliders = Physics.OverlapSphere(transform.position, oSpace);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != this.gameObject && collider.tag == "Agent")
            {
                count++;
            }
        }
        // very naive return all agents in the group
        return count;
    }

    private void InitGroupCenter()
    {
        if (agents.Length != 0)
        {
            Vector3 center = Vector3.zero;
            foreach (var agent in agents)
            {
                center += agent.transform.position;
            }
            center /= agents.Length;
            oCenter = center;
        } else
        {
            Debug.LogError("Could not find any agent");
        }
        centerSphere.transform.position = oCenter;
    }

    private void UpdateGroupCenter(GameObject[] agents)
    {
        if (agents.Length != 0)
        {
            Vector3 center = Vector3.zero;
            foreach (var agent in agents)
            {
                center += agent.transform.position;
            }
            center /= agents.Length;
            oCenter = center;
        }
        else
        {
            Debug.LogError("Could not find any agent");
        }
        centerSphere.transform.position = oCenter;
    }

    public void UpdateGroupCenter(List<Vector3> listR)
    {
        if (listR.Count != 0)
        {
            Vector3 center = Vector3.zero;
            foreach (var ri in listR)
            {
                center += ri;
            }
            center /= listR.Count;
            oCenter = center;
        }
        else
        {
            Debug.LogError("Could not find any agent");
        }
        centerSphere.transform.position = oCenter;
    }

    private void GetPotentialAgents()
    {
        //for test, naively select all agents in the scene
        agents = GameObject.FindGameObjectsWithTag("Agent");
    }

    void DrawAreas(float radius, Color color, LineRenderer lineDrawer)
    {
        lineDrawer.SetWidth(0.05f, 0.05f);
        float theta = 0.0f;
        float thetaScale = 0.01f;
        int size = (int)((1.0f / thetaScale) + 2.0f);
        lineDrawer.material = new Material(Shader.Find("Particles/Additive"));
        lineDrawer.SetVertexCount(size);
        lineDrawer.SetColors(color, color);
        for (int i = 0; i < size; i++)
        {
            theta += (2.0f * Mathf.PI * thetaScale);
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            lineDrawer.SetPosition(i, new Vector3(x, 0.1f, y) + centerSphere.transform.position);
        }
    }

}
