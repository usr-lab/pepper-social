using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PepperAgent : Agent
{

    public float ArenaDimensions = 20.0f;
    public float speed = 20f;
    // What the agent is chasing
    public Transform Target;

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
	
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        gpManager = GameObject.Find("GroupCenter").GetComponent<GroupManager>();
        if (!gpManager)
        {
            Debug.LogError("GroupManager was not attached");
        }

		maSocialForce = this.GetComponent<MainAgentSocialForce>();
		maxStepsPerEpoch = 1000;

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
    }

    public override void CollectObservations()
    {
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
        Vector3 relativePositionAgentOne = gpManager.agents[0].transform.position - this.transform.position;
        Vector3 relativePositionAgentTwo = gpManager.agents[1].transform.position - this.transform.position;
        AddVectorObs(relativePositionAgentOne.x / arenaEdgefromCenter);
        AddVectorObs(relativePositionAgentOne.z / arenaEdgefromCenter);
        AddVectorObs(relativePositionAgentTwo.x / arenaEdgefromCenter);
        AddVectorObs(relativePositionAgentTwo.z / arenaEdgefromCenter);
	}

	void CalculateReward()
	{
		// Setting weights for rewards
		float fastEpisodeWeight = 0.5f;
		float potentialLossWeight = 1f;
		float noneIncreasingWeight = 4f; // Tendency of not increasing potential loss

		// egocentrism and altruism weights
		float egocentrismWeight = 0.1f;
		float altruismWeight = 1f - egocentrismWeight;

		// Initializing the rewards from two sides
		float egocentrismReward = 0f;
		float altruismReward = 0f;
		
	    // Checking the ending criteria
		float distanceToTarget = Vector3.Distance(this.transform.position,
                                                  Target.position);

		// Calculate the egocentric reward
		float potentialLoss = Vector3.Dot(rBody.velocity, this.maSocialForce.GetFinalForce());
		egocentrismReward += potentialLoss * potentialLossWeight; 		// Potential loss is the reward

        if (potentialLoss < 0f)
        {
			egocentrismReward += potentialLoss * noneIncreasingWeight; // increasing potential penalty
        }
        if (distanceToTarget < gpManager.oSpace)
        {

        }

		// Calculate the egocentric reward
		foreach (GameObject agent in gpManager.agents)
		{
			altruismReward += agent.GetComponent<SocialForce>().GetFinalForce().magnitude;
		}

		AddReward(egocentrismWeight * egocentrismReward + altruismWeight * altruismReward);
		// Calculate the final reward
        if (distanceToTarget < gpManager.oSpace)
        {
			AddReward(((this.maxStepsPerEpoch-this.steps) * fastEpisodeWeight) * egocentrismWeight); // fast complesion tendensy
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
