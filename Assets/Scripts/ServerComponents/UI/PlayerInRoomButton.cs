using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInRoomButton : MonoBehaviour
{
    [SerializeField] Lobby lobby;
    [SerializeField] int id;
    [SerializeField] TextMeshProUGUI idField;
    [SerializeField] string username;
    [SerializeField] TextMeshProUGUI usernameField;
    [SerializeField] Button kickButton;

    public int ID { get => id; set { id = value; if (idField != null) idField.text = id.ToString(); } }
    public string Username { get => username; set { username = value; if (usernameField != null) usernameField.text = username; } }

    private void OnValidate()
    {
        if (kickButton == null) Debug.LogWarning("A reference to the kick button is missing!");
    }

    private void OnEnable()
    {
        if (kickButton != null)
        {
            kickButton.onClick.AddListener(OnKickPlayer);
            ResetValue();
        }
    }

    private void OnDisable()
    {
        if (kickButton != null)
        {
            kickButton.onClick.RemoveListener(OnKickPlayer);
        }
    }

    public void SetUsername(/*int playerId, */string userName)
    {
        //Debug.Log("Set: " + playerId + " " + userName);
        /*if (playerId == id) */Username = userName;
    }

    public void ResetValue()
    {
        if (NetworkManager.Instance == null) return;
        if (id == NetworkManager.Instance.NetworkRunner.LocalPlayer.PlayerId && kickButton != null) kickButton.gameObject.SetActive(false);
        else kickButton.gameObject.SetActive(true);
    }

    private void OnKickPlayer()
    {
        if (NetworkManager.Instance == null || lobby == null) return;
        if (!NetworkManager.Instance.NetworkRunner.IsSharedModeMasterClient)
        {
            Debug.Log("You don't have this permission!");
            return;
        }
        lobby.AskToLeaveRoom(id);
    }
}
