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

    public override void Update()
    {
        VehicleInputState input = new VehicleInputState();

        if (Input.GetKey(KeyCode.W))
            input.Forward = 1.0f;

        if (Input.GetKey(KeyCode.S))
            input.Back = 1.0f;

        input.Turn = 0.0f;

        if (Input.GetKey(KeyCode.D))
            input.Turn += 1.0f;

        if (Input.GetKey(KeyCode.A))
            input.Turn -= 1.0f;

        state.Update(Time.deltaTime, input, Parameters);

        Target.SetState(state);
    }
}
