using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour {

    public Text tx_PlayerList;
    public Button btn_StartGame;

    public Lobby lobby;

    public void UpdateTxServerList()
    {
        tx_PlayerList.text = "Players:\n";

        foreach (var p in lobby.players)
            tx_PlayerList.text += p.Nik
                + " <Color=" + p.Color.ToHex() + ">◘</Color>\n";
    }

}
