using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GS : MonoBehaviour
{
    [SerializeField] private float gameTime;
    [SerializeField] private int pingDiff;
    public Matchmaker.Region region;
    [SerializeField] private int rankDiff;
    public bool isGameInProgress = false;
    public float endTime = 0;

    public Text tRegion;
    public Text tPingDiff;
    public Text tRankDiff;
    public Text tEndTime;
    public Text tInProgress;

    public void Install(Matchmaker.Region reg, float gametime)
    {
        isGameInProgress = false;
        region = reg;
        gameTime = gametime;
        gameObject.GetComponent<Image>().color = new Color(0, 255, 0);
        UpdateUI();
    }
    public void StartGame(List<Player> playerList, float currentTime)
    {
        int maxPing = 0;
        int minPing = 999;
        int maxRank = 0;
        int minRank = 9;
        float avgWaitTime = 0;
        float[] waitTimes = new float[10];

        endTime = gameTime + currentTime;
        isGameInProgress = true;
        for(int i = 0; i<playerList.Count;i++)
        {
            maxRank = Mathf.Max((int)playerList[i].playerRank, maxRank);
            maxPing = Mathf.Max(playerList[i].pingToRegion[(int)region], maxPing);
            minRank = Mathf.Min((int)playerList[i].playerRank, minRank);
            minPing = Mathf.Min(playerList[i].pingToRegion[(int)region], minPing);
            avgWaitTime += playerList[i].timeInQueue;
            waitTimes[i] = playerList[i].timeInQueue;
        }
        rankDiff = maxRank - minRank;
        pingDiff = maxPing - minPing;
        avgWaitTime /= 10;
        UpdateUI();
        gameObject.GetComponent<Image>().color = new Color(255, 0, 0);
        DataVault.Instance.AddMatch(gameTime, pingDiff, region, rankDiff, endTime, avgWaitTime, waitTimes);
    }

    public void ResetServer()
    {
        isGameInProgress = false;
        rankDiff = 0;
        pingDiff = 0;
        endTime = 0;
        gameObject.GetComponent<Image>().color = new Color(0, 255, 0);
        UpdateUI();
    }

    public void UpdateUI()
    {
        tRegion.text = "REGION: " + region.ToString();
        tPingDiff.text = "PING DIFF: " + pingDiff.ToString();
        tRankDiff.text = "RANK DIFF: " + rankDiff.ToString();
        tEndTime.text = "END TIME: " + endTime.ToString();
        tInProgress.text = "IN PROGRESS: " + isGameInProgress.ToString();
    }
}
