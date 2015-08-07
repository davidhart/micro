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
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 LocalVelocity;

    public void Update(float dt, VehicleInputState input, VehicleParameters parameters)
    {
        float acceleration = parameters.MaxAcceleration * (1.0f - Mathf.Clamp01(Mathf.Abs(LocalVelocity.y) / parameters.MaxSpeed));

        if (input.Forward > 0)
        {
            LocalVelocity += Vector3.forward * acceleration * dt;
        }

        if (input.Back > 0)
        {
            LocalVelocity -= Vector3.forward * acceleration * dt;
        }

        if (input.Turn != 0.0f)
        {
            float turnForwardFactor = Mathf.Clamp((LocalVelocity.z) / parameters.MaxTurnForwardVel, -1.0f, 1.0f);

            Rotation = Rotation * Quaternion.Euler(0.0f, parameters.TurnSpeed * input.Turn * dt * turnForwardFactor, 0.0f);
        }

        LocalVelocity -= new Vector3(LocalVelocity.x * parameters.Drag.x,
            LocalVelocity.y * parameters.Drag.y,
            LocalVelocity.z * parameters.Drag.z);

        Position += Rotation * LocalVelocity * dt;
    }
}