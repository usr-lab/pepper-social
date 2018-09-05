using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SocialForce : MonoBehaviour {

    public float publicDistance = 15.0f;
    public float socialDistance = 10.0f;
    public float personalDistance = 3.0f;
    public float stabilizeParameter = 0.1f;

    public float scale = 10.0f;

    private List<Vector3> neighbourPersonalObjects = new List<Vector3>();
    private List<Vector3> neighbourSocialObjects = new List<Vector3>();
    private List<Vector3> neighbourPublicObjects = new List<Vector3>();

    private float miniPersonalDistance;
    private GroupManager gpManager;
    private GameObject lineDrawPrefab;
    private GameObject lineDrawPrefabPersonal;
    private GameObject lineDrawPrefabSocial;
    private GameObject lineDrawPrefabPublic;

	private Vector3 finalForce = Vector3.zero;
	private Vector3 repulsiveForce = Vector3.zero;

	[HideInInspector]
    public Rigidbody rBody;

    void Awake()
      {
        rBody = this.gameObject.GetComponent<Rigidbody>();
        if (!rBody)
        {
            Debug.LogError("RigidBody was not attached");
        }
      }
  	// Use this for initialization
      void Start () {
          gpManager = GameObject.Find("GroupCenter").GetComponent<GroupManager>();
          if (!gpManager)
          {
              Debug.LogError("GroupManager was not attached");
          }

          lineDrawPrefab = Resources.Load("Prefab/lineDrawPrefab") as GameObject;
          lineDrawPrefabPersonal = GameObject.Instantiate(lineDrawPrefab) as GameObject;
          lineDrawPrefabSocial = GameObject.Instantiate(lineDrawPrefab) as GameObject;
          lineDrawPrefabPublic = GameObject.Instantiate(lineDrawPrefab) as GameObject;
      }

	// if using add force, it should be in FixedUpdate
	void FixedUpdate () {
        UpdateNeighbors();
	}

	public Vector3 GetFinalForce() {
		return this.finalForce;
	}

	public Vector3 GetRepulsiveForce() {
		return this.repulsiveForce;
	}

	public Rigidbody GetrBody() {
		return this.gameObject.GetComponent<Rigidbody>();
	}

    void UpdateNeighbors()
    {
        miniPersonalDistance = 100.0f;
        neighbourPersonalObjects.Clear();
        neighbourPublicObjects.Clear();
        neighbourSocialObjects.Clear();
        Collider[] colliders = Physics.OverlapSphere(transform.position, publicDistance);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != this.gameObject && (collider.tag == "Agent"|| collider.tag == "Pepper"))
            {
                var distance = Vector3.Distance(collider.gameObject.transform.position, gameObject.transform.position);
                if (distance <= publicDistance)
                {
                    neighbourPublicObjects.Add(collider.gameObject.transform.position);
                }

                if (distance <= socialDistance)
                {
                    neighbourSocialObjects.Add(collider.gameObject.transform.position);
                }

                if (distance <= personalDistance)
                {
                    miniPersonalDistance = Mathf.Min(miniPersonalDistance, distance);
                    neighbourPersonalObjects.Add(collider.gameObject.transform.position);
                }
            }
        }

        finalForce = Vector3.zero;
        repulsiveForce = Vector3.zero;
        Vector3 equalityForce = Vector3.zero;
        Vector3 equalityOrientation = Vector3.zero;
        Vector3 cohesionForce = Vector3.zero;
        Vector3 cohesionOrientation = Vector3.zero;
        int numberOfOSpaceAgent = gpManager.GetOSpaceAgents();
        Vector3 OrigConverCenter = gpManager.GetGroupCenter();
        Vector3 converCenter = new Vector3(OrigConverCenter.x, this.gameObject.transform.position.y, OrigConverCenter.z);

        GetReplusiveForce(personalDistance, miniPersonalDistance, neighbourPersonalObjects, this.transform.position, out repulsiveForce);
        GetEqualityForce(neighbourSocialObjects, this.transform.position, out equalityForce, out equalityOrientation);
        GetCohesionForce(neighbourPublicObjects, numberOfOSpaceAgent, gpManager.oSpace, converCenter, this.transform.position, out cohesionForce, out cohesionOrientation);

        if (Vector3.Distance(transform.position, converCenter) >= gpManager.oSpace + stabilizeParameter
            || Vector3.Distance(transform.position, converCenter) <= gpManager.oSpace - stabilizeParameter)
        {
            finalForce += 2.0f * cohesionForce.normalized;
            finalForce += equalityForce.normalized;
        }

        finalForce += 1.0f * repulsiveForce.normalized;

        //this.transform.position += finalForce.normalized * speed;
        Debug.Log(finalForce.magnitude);
        rBody.AddForce(finalForce.normalized * scale, ForceMode.Force);

        //if (equalityOrientation == Vector3.zero)
        //{
        //    Debug.Log("zeroEquality");
        //}
        //if (cohesionOrientation == Vector3.zero)
        //{
        //    Debug.Log("zeroCohesion");
        //}

        Vector3 finalOrientation = equalityOrientation + cohesionOrientation;
        if (finalOrientation != Vector3.zero)
        {
            this.transform.forward = new Vector3(finalOrientation.x, 0.0f, finalOrientation.z);
        }

        DrawArrow.ForDebug(transform.position+new Vector3(0.0f,0.9f,0.0f), cohesionForce.normalized, Color.black);
        DrawArrow.ForDebug(transform.position + new Vector3(0.0f, 0.7f, 0.0f), equalityForce.normalized, Color.red);
        DrawArrow.ForDebug(transform.position + new Vector3(0.0f, 0.5f, 0.0f), repulsiveForce.normalized, Color.yellow);
        DrawArrow.ForDebug(transform.position + new Vector3(0.0f, 0.3f, 0.0f), finalForce.normalized, Color.green);

        DrawAreas(personalDistance, Color.red, lineDrawPrefabPersonal.GetComponent<LineRenderer>());
        //DrawAreas(socialDistance, Color.yellow, lineDrawPrefabSocial.GetComponent<LineRenderer>());
        //DrawAreas(publicDistance, Color.green, lineDrawPrefabPublic.GetComponent<LineRenderer>());
    }

    void GetReplusiveForce(float deltaP, float dMin, List<Vector3> listR, Vector3 r, out Vector3 force)
    {
        Vector3 R = Vector3.zero;
        foreach (var ri in listR)
        {
            R += (ri - r);
        }

        force = -Mathf.Pow(deltaP - dMin, 2) * R.normalized;
    }

    void GetEqualityForce(List<Vector3> listR, Vector3 r, out Vector3 force, out Vector3 orientation)
    {
        //Vector3 c = 1 / (float)(listR.Count() + 1) * (r + new Vector3(listR.Select(x=>x.x).Sum(),
        //                                                       listR.Select(x => x.y).Sum(),
        //                                                       listR.Select(x => x.z).Sum()));
        Vector3 sum = Vector3.zero;
        foreach (var ri in listR)
        {
            sum += ri;
        }

        Vector3 c = 1.0f / (float)(listR.Count() + 1) * (r + sum);

        float normSum = 0.0f;
        orientation = Vector3.zero;
        foreach (var ri in listR)
        {
            normSum += (ri - c).magnitude;
            orientation += ri - r;
        }
        float m = (r - c).magnitude / (float)(listR.Count() + 1) * normSum;
        force = (1.0f - m / (c - r).magnitude) * (c - r);
    }

    void GetCohesionForce(List<Vector3> listR, int oAgent, float oRadius, Vector3 converCenter, Vector3 r, out Vector3 force, out Vector3 orientation)
    {
        float alpha = (float)listR.Count() / (float)(oAgent + 1);
        force = alpha * (1.0f - oRadius / (converCenter - r).magnitude) * (converCenter - r);
        orientation = Vector3.zero;
        foreach (var ri in listR)
        {
            orientation += ri - r;
        }
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
            lineDrawer.SetPosition(i, new Vector3(x, 0.1f, y) + transform.position);
        }
    }
}
