using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat
{
    public string statName;
    public string statValue;
}

public class Statistic : MonoBehaviour
{
    public Stat statPrefab;
    public Transform contentPanel;

    public List<PlayerStat> statistic = new List<PlayerStat>();

    // Use this for initialization
    IEnumerator Start()
    {
        //troubles with syn 
        yield return StartCoroutine(DataBaseConnector.SelectStatistic(this));

        RefreshStatistic();
    }

    private void RefreshStatistic()
    {
        foreach (Transform t in contentPanel)
            Destroy(t.gameObject);

        foreach (var stat in statistic)
            AddStat(stat);
    }

    public void AddStat(PlayerStat stat)
    {
        Instantiate(statPrefab, contentPanel).InitStat(stat, this);
    }
}
