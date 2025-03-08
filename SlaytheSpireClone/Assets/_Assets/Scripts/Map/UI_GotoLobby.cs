using UnityEngine;
using UnityEngine.SceneManagement;
public class UI_GotoLobby : MonoBehaviour
{
    public void OnGotoLobby()
    {
        SceneManager.LoadScene("0.Lobby");
    }
}
