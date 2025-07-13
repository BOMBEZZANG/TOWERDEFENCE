using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SimpleTowerDefenseAgent : Agent
{
    [Header("Debug")]
    public bool debugMode = true;
    
    public override void Initialize()
    {
        if (debugMode) Debug.Log("SimpleTowerDefenseAgent: Initialize called");
    }
    
    public override void OnEpisodeBegin()
    {
        if (debugMode) Debug.Log("SimpleTowerDefenseAgent: OnEpisodeBegin called");
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        if (debugMode) Debug.Log("SimpleTowerDefenseAgent: CollectObservations called");
        
        // Simple observations - just 4 values
        sensor.AddObservation(1.0f);  // dummy money
        sensor.AddObservation(1.0f);  // dummy lives
        sensor.AddObservation(0.0f);  // dummy wave
        sensor.AddObservation(0.0f);  // dummy timer
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (debugMode) Debug.Log("SimpleTowerDefenseAgent: OnActionReceived called");
        
        // Do nothing for now - just log
        int action = actionBuffers.DiscreteActions[0];
        if (debugMode) Debug.Log($"Action received: {action}");
        
        // Give small reward to keep episode going
        AddReward(0.001f);
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0; // Always choose action 0
    }
    
    // RequestDecision()을 호출하던 Update() 메서드를 완전히 삭제했습니다.
}