using UnityEngine;
using System.Collections;

public class LocalVehicleController : BaseVehicleController
{
    private VehicleState state;

    public LocalVehicleController()
    {
        state = new VehicleState();
        state.Position = Vector3.zero;
        state.Rotation = Quaternion.identity;
    }

    public override void Update(float dt)
    {
        state.Input.Forward = Input.GetKey(KeyCode.W) ? 1.0f : 0.0f;
        state.Input.Back = Input.GetKey(KeyCode.S) ? 1.0f : 0.0f;

        state.Input.Turn = 0.0f;

        if (Input.GetKey(KeyCode.D))
            state.Input.Turn += 1.0f;

        if (Input.GetKey(KeyCode.A))
            state.Input.Turn -= 1.0f;

        state.Update(Time.deltaTime, Parameters);

        Target.SetState(state);
    }

    public override void HandleIncomingState(VehicleState state)
    {
        // TODO: detect desync state changes etc
    }

    public override VehicleState GetState()
    {
        return state;
    }
}
