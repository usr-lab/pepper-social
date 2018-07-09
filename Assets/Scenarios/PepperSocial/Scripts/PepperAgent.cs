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

    void MoveCamera()
    {
      // Vector3 offset = new Vector3(0.0f,1.0f,0.0f);
        Vector3 offset = 1f*rBody.transform.up + 0.1f*rBody.transform.forward;
        this.cam.transform.position = transform.position + offset;
        this.cam.transform.forward = Quaternion.AngleAxis(0, rBody.transform.right) * rBody.transform.forward;
    }

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        gpManager = GameObject.Find("GroupCenter").GetComponent<GroupManager>();
        if (!gpManager)
        {
            Debug.LogError("GroupManager was not attached");
        }

		maSocialForce = this.GetComponent<MainAgentSocialForce>();
		maxStepsPerEpoch = 300;
    }

    public override void AgentReset()
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
        gpManager.ResetAgents();
        MoveCamera();
    }

    public override void CollectObservations()
    {
		///*
        float arenaEdgefromCenter = ArenaDimensions / 2;

        // Calculate relative position
        Vector3 relativePosition = Target.position - this.transform.position;

        // Relative position
        AddVectorObs(relativePosition.x / arenaEdgefromCenter);
        AddVectorObs(relativePosition.z / arenaEdgefromCenter);

        // Distance to edges of platform
        AddVectorObs((this.transform.position.x + arenaEdgefromCenter) / arenaEdgefromCenter);
        AddVectorObs((this.transform.position.x - arenaEdgefromCenter) / arenaEdgefromCenter);
        AddVectorObs((this.transform.position.z + arenaEdgefromCenter) / arenaEdgefromCenter);
        AddVectorObs((this.transform.position.z - arenaEdgefromCenter) / arenaEdgefromCenter);

        // Agent velocity
        AddVectorObs(rBody.velocity.x / arenaEdgefromCenter);
        AddVectorObs(rBody.velocity.z / arenaEdgefromCenter);

		// Diatance to another two agents
		// Calculate the egocentric reward
		for(int i = 0; i < gpManager.numberOfAgent; i++)

//		foreach(GameObject agent in gpManager.agents)
		{
			Vector3 relativePositionAgent = gpManager.agents[i].transform.position - this.transform.position;
//			Vector3 relativePositionAgent = agent.transform.position - this.transform.position;
			AddVectorObs(relativePositionAgent.x / arenaEdgefromCenter);
			AddVectorObs(relativePositionAgent.z / arenaEdgefromCenter);
			AddVectorObs(gpManager.agentsSocialForces[i].GetrBody().velocity.x / arenaEdgefromCenter);
			AddVectorObs(gpManager.agentsSocialForces[i].GetrBody().velocity.z / arenaEdgefromCenter);
//			AddVectorObs(agent.GetComponent<SocialForce>().GetrBody().velocity.x / arenaEdgefromCenter);
//			AddVectorObs(agent.GetComponent<SocialForce>().GetrBody().velocity.z / arenaEdgefromCenter);

		}
		//*/
	}

	void CalculateReward()
	{
		// Setting weights for rewards
		float fastEpisodeWeight = 0.2f;
		float potentialLossWeight = 1f;
		float noneIncreasingWeight = 12f; // Tendency of not increasing potential loss
		float tiresomeWeight = 0.4f;

		// egocentrism and altruism weights
		float egoismWeight = 0.045f;
		float altruismWeight = 1f - egoismWeight;

		// Initializing the rewards from two sides
		float egoismReward = 0f;
		float altruismReward = 0f;

	    // Checking the ending criteria
		float distanceToTarget = Vector3.Distance(this.transform.position,
                                                  Target.position);

		// Calculate the egoism reward
		float potentialLoss = Vector3.Dot(rBody.velocity, this.maSocialForce.GetFinalForce());
		egoismReward += potentialLoss * potentialLossWeight; 		// Potential loss is the reward
        if (potentialLoss < 0f)
        {
			egoismReward += potentialLoss * noneIncreasingWeight; // increasing potential penalty
        }
		AddReward(-rBody.velocity.magnitude * tiresomeWeight);

		// Calculate the egoism reward
		for(int i = 0; i < gpManager.numberOfAgent; i++)
		{
			altruismReward += -Vector3.Dot(gpManager.agentsSocialForces[i].GetFinalForce(),
										   gpManager.agentsSocialForces[i].GetComponent<SocialForce>().GetrBody().velocity);

		}

		AddReward(egoismWeight * egoismReward + altruismWeight * altruismReward);
		// Calculate the final reward
        if (distanceToTarget < gpManager.oSpace)
        {
			AddReward(((this.maxStepsPerEpoch-this.steps) * fastEpisodeWeight) * egoismWeight); // fast complesion tendensy
            Done();
        }

	}

    public override void AgentAction(float[] vectorAction, string textAction)
    {

		CalculateReward();

        // Set orientation
        Vector3 deltaPosition = Target.position - transform.position;
        if (deltaPosition != Vector3.zero)
        {
            // Same effect as rotating with quaternions, but simpler to read
            transform.forward = deltaPosition;
        }

		// Perform action
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = Mathf.Clamp(vectorAction[0], -1, 1);
        controlSignal.z = Mathf.Clamp(vectorAction[1], -1, 1);
        rBody.AddForce(controlSignal * speed);

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
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;

        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            dirToGo = transform.forward * Mathf.Clamp(action[0], -1f, 1f);
            rotateDir = transform.up * Mathf.Clamp(action[1], -1f, 1f);
        }
        else
        {
            switch ((int)(action[0]))
            {
                case 1:
                    dirToGo = -transform.forward;
                    break;
                case 2:
                    rotateDir = -transform.up;
                    break;
                case 3:
                    rotateDir = transform.up;
                    break;
            }
        }
        agentRigidbody.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
        transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);

        if (agentRigidbody.velocity.sqrMagnitude > 25f) // slow it down
        {
            agentRigidbody.velocity *= 0.95f;
        }


    }
}
