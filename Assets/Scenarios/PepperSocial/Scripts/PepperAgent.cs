using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PepperAgent : Agent
{

    public float ArenaDimensions = 20.0f;
    public float speed = 0.1f;
    // What the agent is chasing
    public Transform Target;
    public GameObject cam;

    Rigidbody rBody;

    private float previousDistance = float.MaxValue;
    private float previousPotentialLoss = float.MaxValue;

    private Rigidbody agentRigidbody;

    private float moveSpeed;
    private float turnSpeed;

    private int maxStepsPerEpoch;
    private int steps;

    private GroupManager gpManager;

	private MainAgentSocialForce maSocialForce;

	private bool demoMode = false;
	private int demoTrack = 0;
	private float allowedArea;
	private List<Vector3> lPositions = new List<Vector3>();
	private List<Vector3> groupPositions = new List<Vector3>();

    void MoveCamera()
    {
      // Vector3 offset = new Vector3(0.0f,1.0f,0.0f);
        Vector3 offset = 1f*rBody.transform.up + 0.1f*rBody.transform.forward;
        this.cam.transform.position = transform.position + offset;
        this.cam.transform.forward = Quaternion.AngleAxis(0, rBody.transform.right) * rBody.transform.forward;
    }

    void Awake()
    {
      rBody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        gpManager = GameObject.Find("GroupCenter").GetComponent<GroupManager>();
        if (!gpManager)
        {
            Debug.LogError("GroupManager was not attached");
        }

		maSocialForce = this.GetComponent<MainAgentSocialForce>();
		maxStepsPerEpoch = 300;



		allowedArea = ArenaDimensions * 0.9f;
		lPositions.Add(new Vector3(0.2f*allowedArea - allowedArea / 2,
								   0.16f,
								   0.2f*allowedArea - allowedArea / 2));


		lPositions.Add(new Vector3(0.5f*allowedArea - allowedArea / 2,
								   0.16f,
								   0.2f*allowedArea - allowedArea / 2));


		lPositions.Add(new Vector3(0.8f*allowedArea - allowedArea / 2,
								   0.16f,
								   0.2f*allowedArea - allowedArea / 2));

		lPositions.Add(new Vector3(0.2f*allowedArea - allowedArea / 2,
								   0.16f,
								   0.45f*allowedArea - allowedArea / 2));

		lPositions.Add(new Vector3(0.8f*allowedArea - allowedArea / 2,
								   0.16f,
								   0.55f*allowedArea - allowedArea / 2));

		lPositions.Add(new Vector3(0.2f*allowedArea - allowedArea / 2,
								   0.16f,
								   0.8f*allowedArea - allowedArea / 2));

		lPositions.Add(new Vector3(0.5f*allowedArea - allowedArea / 2,
								   0.16f,
								   0.8f*allowedArea - allowedArea / 2));

		lPositions.Add(new Vector3(0.8f*allowedArea - allowedArea / 2,
								   0.16f,
								   0.8f*allowedArea - allowedArea / 2));

		groupPositions.Add(new Vector3(0.5f*allowedArea - allowedArea / 2,
									   0.16f,
									   0.5f*allowedArea - allowedArea / 2));


    }

    public override void AgentReset()
    {
		if(!demoMode)
		{
			float allowedArea = ArenaDimensions * 0.9f;

			this.transform.position = new Vector3((Random.value * allowedArea) - (allowedArea / 2),
												  0.16f,
												  (Random.value * allowedArea) - (allowedArea / 2));
			float targetAllowedArea = ArenaDimensions * 0.6f;
			Target.transform.position = new Vector3((Random.value * targetAllowedArea) - (targetAllowedArea / 2),
													0.16f,
													(Random.value * targetAllowedArea) - (targetAllowedArea / 2));

			this.rBody.angularVelocity = Vector3.zero;
			this.rBody.velocity = Vector3.zero;
			float angle = Random.Range (0f, Mathf.PI * 2);
			gpManager.ResetAgents(angle);
			MoveCamera();
		}
		else{

			this.transform.position = lPositions[demoTrack%8];
			Target.transform.position =groupPositions[0];
			this.rBody.angularVelocity = Vector3.zero;
			this.rBody.velocity = Vector3.zero;
			gpManager.ResetAgents(90/360f * Mathf.PI*2);
			MoveCamera();
			demoTrack = demoTrack + 1;
		}
	}

    public override void CollectObservations()
    {
        // OBSERVARIONS FOR CameraSpeed
        float forwardSpeed = Vector3.Dot(rBody.velocity, transform.forward.normalized );
        float sidewaySpeed = Vector3.Dot(rBody.velocity, transform.right.normalized );
        AddVectorObs( forwardSpeed );
        AddVectorObs( sidewaySpeed );

//         // // OBSERVATION FOR BASELINE
//         float arenaEdgefromCenter = ArenaDimensions / 2;
//         // Calculate relative position
//         Vector3 relativePosition = Target.position - this.transform.position;
//
//         // Relative position
//         AddVectorObs(relativePosition.x / arenaEdgefromCenter);
//         AddVectorObs(relativePosition.z / arenaEdgefromCenter);
//
//         // Distance to edges of platform
//         AddVectorObs((this.transform.position.x + arenaEdgefromCenter) / arenaEdgefromCenter);
//         AddVectorObs((this.transform.position.x - arenaEdgefromCenter) / arenaEdgefromCenter);
//         AddVectorObs((this.transform.position.z + arenaEdgefromCenter) / arenaEdgefromCenter);
//         AddVectorObs((this.transform.position.z - arenaEdgefromCenter) / arenaEdgefromCenter);
//
//         // Agent velocity
//         AddVectorObs(rBody.velocity.x / arenaEdgefromCenter);
//         AddVectorObs(rBody.velocity.z / arenaEdgefromCenter);
//
// 		// Diatance to another two agents
// 		// Calculate the egocentric reward
// 		for(int i = 0; i < gpManager.numberOfAgent; i++)
// //		foreach(GameObject agent in gpManager.agents)
// 		{
// 			Vector3 relativePositionAgent = gpManager.agents[i].transform.position - this.transform.position;
// //			Vector3 relativePositionAgent = agent.transform.position - this.transform.position;
// 			AddVectorObs(relativePositionAgent.x / arenaEdgefromCenter);
// 			AddVectorObs(relativePositionAgent.z / arenaEdgefromCenter);
// 			AddVectorObs(gpManager.agentsSocialForces[i].GetrBody().velocity.x / arenaEdgefromCenter);
// 			AddVectorObs(gpManager.agentsSocialForces[i].GetrBody().velocity.z / arenaEdgefromCenter);
// //			AddVectorObs(agent.GetComponent<SocialForce>().GetrBody().velocity.x / arenaEdgefromCenter);
// //			AddVectorObs(agent.GetComponent<SocialForce>().GetrBody().velocity.z / arenaEdgefromCenter);
//
// 		}
//

	}

  void CalculateReward()
  {
    // Setting weights for rewards
    //float fastEpisodeWeight = 0.00001f;
    float potentialLossWeight = 0.012f;
    float noneIncreasingWeight = 2.4f; // Tendency of not increasing potential loss
    float tiresomeWeight = 0.16f;

    // egocentrism and altruism weights
    float egoismWeight = 0.0040f;
    float altruismWeight = 1f - egoismWeight;

    // Initializing the rewards from two sides
    float egoismReward = 0f;
    float altruismReward = 0f;

      // Checking the ending crfdfd
    float distanceToTarget = Vector3.Distance(this.transform.position,
                          Target.position);
    // Calculate the egoism reward
    float potentialLoss = Vector3.Dot(rBody.velocity, this.maSocialForce.GetFinalForce());
    egoismReward += potentialLoss * potentialLossWeight; 		// 1)


    if (potentialLoss > 0f)
    {
        egoismReward += noneIncreasingWeight; // in-creasing potential penalty // 2)
    }

    // Calculate the altruism reward
    for(int i = 0; i < gpManager.numberOfAgent; i++)
    {
        altruismReward += -Vector3.Dot(gpManager.agentsSocialForces[i].GetFinalForce(),
                       gpManager.agentsSocialForces[i].GetComponent<SocialForce>().GetrBody().velocity) * potentialLossWeight;
        altruismReward += -gpManager.agentsSocialForces[i].GetRepulsiveForce().magnitude/3.8f;
    }
    egoismReward += (-Mathf.Pow(rBody.velocity.magnitude,2) * tiresomeWeight);

    // Step cost
    egoismReward += -2.0f; //3)

    AddReward(egoismWeight * egoismReward + altruismWeight * altruismReward);

    // Calculate the final reward
    if (distanceToTarget < gpManager.oSpace)
    {
        this.steps = 0;
        AddReward(1f); // fast complesion tendensy
        Done();
    }
  }

	public void SavePositions()
	{
		// using (StreamWriter w = File.AppendText("agent.txt"))
    //     {
		// 	string agentPosition = JsonUtility.ToJson(transform.position);
    //         Log(agentPosition, w);
    //     }
		// for(int i = 0; i < gpManager.numberOfAgent; i++)
		// {
		// 	using (StreamWriter w = File.AppendText("human_"+i.ToString()+".txt"))
		// 	{
		// 		string humanPosition = JsonUtility.ToJson(gpManager.agents[i].transform.position);
		// 		Log(humanPosition, w);
		// 	}
		// }
	}
    public override void AgentAction(float[] vectorAction, string textAction)
    {
		SavePositions();

		CalculateReward();
        // Set orientation
        Vector3 deltaPosition = Target.position - transform.position;
        if (deltaPosition != Vector3.zero)
        {
            // Same effect as rotating with quaternions, but simpler to read
            transform.forward = deltaPosition;
        }
		// Perform action
        //Vector3 controlSignal = Vector3.zero;
        //controlSignal.x = Mathf.Clamp(vectorAction[0], -1, 1);
        //controlSignal.z = Mathf.Clamp(vectorAction[1], -1, 1);
        //rBody.AddForce(controlSignal * speed);

		Debug.Log(vectorAction[0]);
		HandleMovement(vectorAction);

		this.steps = this.steps + 1;
		//Debug.Log(this.steps);
		MoveCamera();
		if (this.steps == this.maxStepsPerEpoch)
        {
            this.steps = 0;
            Done();
        }
    }
    private void HandleMovement(float[] action)
    {
        Vector3 mForward = Vector3.zero;
        Vector3 mSideway = Vector3.zero;
		    moveSpeed= 0.05f;

        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            mForward = transform.forward * Mathf.Clamp(action[0], -1f, 1f);
            mSideway = transform.right * Mathf.Clamp(action[1], -1f, 1f);
        }
        else
        {

        }
        rBody.AddForce((mForward+mSideway) * moveSpeed, ForceMode.VelocityChange);

        //transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);

        if (rBody.velocity.sqrMagnitude > 25f) // slow it down
        {
            rBody.velocity *= 0.95f;
        }


    }
}
