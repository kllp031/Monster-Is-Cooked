using Fusion;
using UnityEngine;

public class InputsPoller : NetworkRunnerCallbacksAdapter
{
    void Start()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.NetworkRunner.AddCallbacks(this);
        }
    }

    public override void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (InputsManager.Instance != null)
        {
            InputData inputData = new InputData();

            inputData.MovementInput = InputsManager.Instance.PlayerInputs.MovementInput;

            input.Set(inputData);
        }
    }
}

// This struct stores all input data
// OnInput is
public struct InputData : INetworkInput
{
    public Vector2 MovementInput;
}
