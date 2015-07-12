using System;
using UnityEngine;

public struct VehicleInputState
{
    public float Turn;
    public float Forward;
    public float Back;
}

public struct VehicleState
{
    public VehicleParameters Parameters;

    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 LocalVelocity;

    public void Update(float dt, VehicleInputState input)
    {
        if (input.Forward > 0)
        {
            LocalVelocity += Vector3.forward * Parameters.Acceleration * dt;
        }

        if (input.Back > 0)
        {
            LocalVelocity -= Vector3.forward * Parameters.Acceleration * dt;
        }

        if (input.Turn != 0.0f)
        {
            float turnForwardFactor = Mathf.Clamp((LocalVelocity.z) / Parameters.MaxTurnForwardVel, -1.0f, 1.0f);

            Rotation = Rotation * Quaternion.Euler(0.0f, Parameters.TurnSpeed * input.Turn * dt * turnForwardFactor, 0.0f);
        }

        LocalVelocity -= new Vector3(LocalVelocity.x * Parameters.Drag.x,
            LocalVelocity.y * Parameters.Drag.y,
            LocalVelocity.z * Parameters.Drag.z);

        Position += Rotation * LocalVelocity * dt;
    }
}