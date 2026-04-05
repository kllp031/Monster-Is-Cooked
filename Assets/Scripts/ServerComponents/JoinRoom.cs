using Fusion;
using TMPro;
using UnityEngine;

public class JoinRoom : MonoBehaviour
{
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] TMP_InputField roomIdInput;
    [SerializeField] int lobbySceneIndex = 4;
    [SerializeField] GameMode gameMode = GameMode.Shared;

    public async void OnJoinRoom()
    {
        if(roomIdInput == null) { Debug.LogError("Room ID input field is not assigned."); return; }
        if(usernameInput != null) NetworkManager.Instance.Username = usernameInput.text;

        StartGameResult res = await NetworkManager.Instance.JoinRoom(gameMode, roomIdInput.text, lobbySceneIndex);
        if (!res.Ok)
        {
            // Pop up an error message to the user
            Debug.LogError($"Failed to join room: {res.ShutdownReason}");
        }
    }
}
