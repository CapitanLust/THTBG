using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour {

    public Text tx_PlayerList;
    public Button btn_StartGame;

    public Lobby lobby;

    public void OnSceneChangedFromLobby(Lobby lobby)
    {
        this.lobby = lobby;

        //lobby.onStartServer +=()=> btn_StartGame.gameObject.SetActive(true);

        //lobby.onAddPlayer +=()=> UpdateTxServerList();

        UpdateTxServerList(); // for those, who already there
        if(lobby.isServer)    // and if lobby will been loaded after ServerStart
            btn_StartGame.gameObject.SetActive(true);
    }

    public void Btn_StartGame()
    {
        lobby.StartGame();
    }

    public void UpdateTxServerList()
    {
        tx_PlayerList.text = "Players:\n";

        foreach (var p in lobby.players)
            tx_PlayerList.text += p.Nik
                + " <Color=" + p.Color.ToHex() + ">◘</Color>\n";
    }

}
