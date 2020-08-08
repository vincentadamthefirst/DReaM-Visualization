using System;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {

    public List<EvaluationAgentPosition> vehiclePositions;

    public List<EvaluationAgentPosition> pedestrianPositions;


    public void OnDrawGizmos() {
        foreach (var vehicle in vehiclePositions) {
            Gizmos.DrawCube(vehicle.position, Vector3.one);
        }

        foreach (var pedestrian in pedestrianPositions) {
            Gizmos.DrawSphere(pedestrian.position, .5f);
        }
    }
}

[Serializable]
public class EvaluationAgentPosition {
    public Vector3 position;
    
}
