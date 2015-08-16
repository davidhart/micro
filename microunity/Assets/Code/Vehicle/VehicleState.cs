using System;
using UnityEngine;
using Gamemodes.Cars;

public struct VehicleInputState
{
    public float Turn;
    public float Forward;
    public float Back;
}

public struct VehicleState
{
    public VehicleInputState Input;

    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 LocalVelocity;

    // Convert to and from network state class as server should not reference unity classes
    public void FromNetworkState(VehicleNetworkState state)
    {
        Input.Forward = state.InputForward;
        Input.Back = state.InputBackward;
        Input.Turn = state.InputTurn;

        Position.x = state.PositionX;
        Position.y = 0.0f;
        Position.z = state.PositionY;

        Rotation = Quaternion.Euler(0.0f, state.Rotation, 0.0f);

        LocalVelocity.x = state.LocalVelocityX;
        LocalVelocity.y = 0.0f;
        LocalVelocity.z = state.LocalVelocityY;
    }

    public void ToNetworkState(VehicleNetworkState state)
    {
        state.InputForward = Input.Forward;
        state.InputBackward = Input.Back;
        state.InputTurn = Input.Turn;

        state.PositionX = Position.x;
        state.PositionY = Position.z;

        state.Rotation = Rotation.eulerAngles.y;

        state.LocalVelocityX = LocalVelocity.x;
        state.LocalVelocityY = LocalVelocity.y;
    }

    public void Update(float dt, VehicleParameters parameters)
    {
        float acceleration = parameters.MaxAcceleration * (1.0f - Mathf.Clamp01(Mathf.Abs(LocalVelocity.y) / parameters.MaxSpeed));

        if (Input.Forward > 0)
        {
            LocalVelocity += Vector3.forward * acceleration * dt;
        }

        if (Input.Back > 0)
        {
            LocalVelocity -= Vector3.forward * acceleration * dt;
        }

        if (Input.Turn != 0.0f)
        {
            float turnForwardFactor = Mathf.Clamp((LocalVelocity.z) / parameters.MaxTurnForwardVel, -1.0f, 1.0f);

            Rotation = Rotation * Quaternion.Euler(0.0f, parameters.TurnSpeed * Input.Turn * dt * turnForwardFactor, 0.0f);
        }

        LocalVelocity -= new Vector3(LocalVelocity.x * parameters.Drag.x,
            LocalVelocity.y * parameters.Drag.y,
            LocalVelocity.z * parameters.Drag.z);

        Position += Rotation * LocalVelocity * dt;
    }
}