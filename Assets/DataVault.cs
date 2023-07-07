using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class DataVault : MonoBehaviour
{
    public static DataVault Instance;
    public string savePath;
    public MatchHistory matchHistory;

    public void Awake()
    {
        matchHistory = new MatchHistory();
        Instance = this;
        savePath = Application.persistentDataPath + "/";
    }

    public void AddMatch(float gameTime, int pingDiff, Matchmaker.Region region, int rankDiff, float endTime, float avgPlayerWaitTime, float[] waitTimes)
    {
        Match m = new Match();

        m.gameTime = gameTime;
        m.pingDiff = pingDiff;
        m.region = region;
        m.rankDiff = rankDiff;
        m.endTime = endTime;
        m.avgPlayerWaitTime = avgPlayerWaitTime;
        m.waitTimes = waitTimes;
        m.Q = (NormalizeInput(0,8, rankDiff) * 0.45f + NormalizeInput(10, 300, pingDiff) * 0.2f + avgPlayerWaitTime * 0.35f);

        matchHistory.matchHistory.Add(m);
        Logger.Instance.LogMessage("Game in region: " + m.region + " created with following stats: \n" + "Q: " + m.Q + " \nrankDiff: " + m.rankDiff + " \npingDiff: " + m.pingDiff + "  \navgQueueTime: " + m.avgPlayerWaitTime);
    }   

    public float NormalizeInput(int min,int max, float val)
    {
        return (val/(max-min))*100;
    }

    public float CalculateMatchQ(int rDiff, int pDiff, float avgWait)
    {
        return (NormalizeInput(0, 8, rDiff) * 0.45f + NormalizeInput(10, 300, pDiff) * 0.2f + avgWait * 0.35f);
    }

    public float CalculateAVGQ(MatchHistory match)
    {
        float q = 0;
        foreach (Match m in matchHistory.matchHistory)
        {
            q += m.Q;
        }
        return q / matchHistory.matchHistory.Count;
    }

    public void SaveData(int queueType, int type)
    {
        
        matchHistory.Q_avg = CalculateAVGQ(matchHistory);
        matchHistory.simData = AdminPanel.Instance.simDatas[AdminPanel.Instance.pointer];
        switch(type)
        {
            case 0:
                string save = JsonUtility.ToJson(matchHistory, true);
                File.WriteAllText(savePath + matchHistory.simData.serverNum + "_QUEUE" + matchHistory.simData.queue + "_REGS" + (int)(matchHistory.simData.reg[0]*100) + "_" + System.DateTime.Now.ToFileTime() + ".json", save);
                break;
            case 1:
                XmlSerializer serializer = new XmlSerializer(typeof(MatchHistory));
                FileStream stream = new FileStream(savePath + matchHistory.simData.serverNum + "_QUEUE" + matchHistory.simData.queue + "_REGS" + (int)matchHistory.simData.reg[0] + "_" + System.DateTime.Now.ToFileTime(), FileMode.Create);
                serializer.Serialize(stream, matchHistory);
                stream.Close();
                break;
        }
        Logger.Instance.LogMessage("CREATED A SAVEFILE \n" + "Saved at: " + savePath);
    }

}

[System.Serializable]
public class MatchHistory
{
    public float Q_avg = 0;
    [SerializeField] public SimData simData;
    [SerializeField] public List<Match> matchHistory = new List<Match>();
}
