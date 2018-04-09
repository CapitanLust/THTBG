using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stat : MonoBehaviour
{
    public PlayerStat playerStat;

    public Text tx_statName;
    public Text tx_statValue;

    private Statistic statistic;

    public void InitStat(PlayerStat playerStat, Statistic statistic)
    {
        this.playerStat = playerStat;
        this.statistic = statistic;

        tx_statName.text = playerStat.statName;
        tx_statValue.text = playerStat.statValue;
    }
}
