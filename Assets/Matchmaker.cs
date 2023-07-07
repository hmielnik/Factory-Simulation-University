using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Matchmaker : MonoBehaviour
{
    public int serverNumber = 3;
    public float[] regPercent = new float[3];
    public int numberOfNewPlayersPerT = 30;
    public float finalTTick = 30;
    public bool startSimulation = false;
    public int queueType;

    [SerializeField] private float T = 5;
    private float TFINAL;
    private float _lastTimeStamp = -1;
    [SerializeField]private float timeElapsed = 0;

    public List<GS> freeServers = new List<GS>();
    public List<GS> usedServers = new List<GS>();
    public List<Player> playersInQueue = new List<Player>();

    public GS GSPrefab;

    public GameObject GSFreecontentListUI;
    public GameObject GSUsedcontentListUI;

    public void InitializeData(float[] reg, int serverNum = 3, int simLoops = 30, int queueT = 0)
    {
        regPercent = reg;
        serverNumber = serverNum;
        finalTTick = simLoops;
        T = 0.1f;
        numberOfNewPlayersPerT = (int)RandomFromDistribution.RandomNormalDistribution(60, 4);
        TFINAL = finalTTick * T;
        queueType = queueT;
        DataVault.Instance.matchHistory.matchHistory.Clear();
        StartSimulation();
    }

    public void RestartData()
    {
        regPercent = new float[3];
        serverNumber = 3;
        finalTTick = 10;
        T = 0.1f;
        numberOfNewPlayersPerT = (int)RandomFromDistribution.RandomNormalDistribution(60, 4);
        TFINAL = finalTTick * T;
        queueType = 0;
        freeServers.Clear();
        usedServers.Clear();
        playersInQueue.Clear();
        timeElapsed = 0;
        _lastTimeStamp = -1;
        foreach(GS gs in GSFreecontentListUI.GetComponentsInChildren<GS>())
        {
            Destroy(gs.gameObject);
        }

        foreach (GS gs in GSUsedcontentListUI.GetComponentsInChildren<GS>())
        {
            Destroy(gs.gameObject);
        }
    }

    public void StartSimulation()
    {
        SpawnServers(regPercent);
        startSimulation = true;
    }

    public void SpawnServers(float[] regions)
    {
        //Wyliczamy właściwą liczbę serwerów dla każdego z regionów
        int na = (int)Mathf.Round((serverNumber*regions[1]));
        int sa = (int)Mathf.Round((serverNumber * regions[2]));
        int eu = (int)Mathf.Round((serverNumber * regions[0]));
        if(na+sa+eu < serverNumber)
        {
            eu++;
        }
        //------------------
        // Do listy wolnych serwerów dopisujemy nowy serwer. Dodajemy do niego informację o jego regionie i czasie rozgrywanych na nim gier.
        for (int i = 0; i < eu; i++)
        {
            var server = Instantiate(GSPrefab);
            server.transform.parent = GSFreecontentListUI.transform;
            server.Install(Region.EU, RandomFromDistribution.RandomNormalDistribution(3, 1) * T);
            freeServers.Add(server);
        }
        for (int i = 0; i<na; i++)
        {
            var server = Instantiate(GSPrefab);
            server.transform.parent = GSFreecontentListUI.transform;
            server.Install(Region.NA, RandomFromDistribution.RandomNormalDistribution(3, 1) * T);
            freeServers.Add(server);
        }
        for (int i = 0; i < sa; i++)
        {
            var server = Instantiate(GSPrefab);
            server.transform.parent = GSFreecontentListUI.transform;
            server.Install(Region.SA, RandomFromDistribution.RandomNormalDistribution(3, 1) * T);
            freeServers.Add(server);
        }
    }

    public void UpdateServerStatus()
    {
        List<GS> l = new List<GS>();
        l.InsertRange(0, usedServers);
        foreach (GS server in l)
        {
            if (server.endTime <= timeElapsed)
            {
                server.ResetServer();
                usedServers.Remove(server);
                freeServers.Add(server);
                server.transform.parent = GSFreecontentListUI.transform;
            }
        }
        l.Clear();
        l.InsertRange(0, freeServers);
        foreach (GS server in l)
        {
            if (server.isGameInProgress)
            {
                usedServers.Add(server);
                freeServers.Remove(server);
                server.transform.parent = GSUsedcontentListUI.transform;
            }
        }
        l.Clear();
    }

    public void SpawnNextPlayerTick()
    {
        //Dodaje ustaloną liczbę graczy do kolejki. Dla każdego z graczy wybiera jego główny region a następnie ustawia ping do serwerów w konkretnych regionach
        foreach(Player p in playersInQueue)
        {
            p.timeInQueue += T;
        }
        for(int i = 0; i<numberOfNewPlayersPerT;i++)
        {
            Player p = new Player();

            p.bestRegion = (Region)(int)Random.Range(0, 3);
            switch(p.bestRegion)
            {
                case Region.EU:
                    p.pingToRegion[0] = (int)RandomFromDistribution.RandomRangeLinear(10, 60, 0);
                    p.pingToRegion[1] = (int)RandomFromDistribution.RandomRangeLinear(80, 180, 0);
                    p.pingToRegion[2] = (int)RandomFromDistribution.RandomRangeLinear(200, 300, 0);
                    break;
                case Region.NA:
                    p.pingToRegion[1] = (int)RandomFromDistribution.RandomRangeLinear(10, 60, 0);
                    p.pingToRegion[0] = (int)RandomFromDistribution.RandomRangeLinear(80, 180, 0);
                    p.pingToRegion[2] = (int)RandomFromDistribution.RandomRangeLinear(200, 300, 0);
                    break;
                case Region.SA:
                    p.pingToRegion[2] = (int)RandomFromDistribution.RandomRangeLinear(10, 60, 0);
                    p.pingToRegion[1] = (int)RandomFromDistribution.RandomRangeLinear(80, 180, 0);
                    p.pingToRegion[0] = (int)RandomFromDistribution.RandomRangeLinear(200, 300, 0);
                    break;
            }

            p.playerRank = (Player.Rank)RandomFromDistribution.RandomNormalDistribution(4,1);

            playersInQueue.Add(p);
        }
    }

    //Dobieranie graczy do meczu bez dodatkowych parametrów
    public void AssignQueuedPlayers()
    {
        foreach(GS server in freeServers)
        {
            List<Player> p = new List<Player>();
            if (playersInQueue.Count>=10)
            {
                for (int i = 0; i < 10; i++)
                {
                    p.Add(playersInQueue[i]);
                }
                playersInQueue.RemoveRange(0, 10);
                server.StartGame(p, timeElapsed);
            }
            else
            {
                return;
            }
        }
    }

    //Dobieranie graczy do meczu na podstawie rangi
    public void AssignQueuedByRank()
    {
        int nextRankQueue = 0;
        foreach (GS server in freeServers)
        {
            List<Player> filteredList = new List<Player>();
            List<Player> p = new List<Player>();
            int j = 0;
            while (filteredList.Count<10 && j<9)
            {
                //Filtrowanie całej listy graczy pod kątem graczy z konkretną rangą
                filteredList = playersInQueue.Where(r => (int)r.playerRank == nextRankQueue).ToList(); 
                if (filteredList.Count >= 10)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        p.Add(filteredList[i]);
                    }
                    foreach (Player pl in p)
                    {
                        playersInQueue.Remove(pl);
                    }
                    server.StartGame(p, timeElapsed); 
                }
                else
                {
                    nextRankQueue = (nextRankQueue+1)%9;
                    j++;
                }
            }
            
        }

    }

    //Dobieranie graczy do meczu na podstawie najlepszego regionu
    public void AssignQueuedByRegion()
    {
        List<Player> filteredList = new List<Player>();
        foreach (GS server in freeServers)
        {
            List<Player> p = new List<Player>();
            //Filtrowanie całej listy graczy pod kątem graczy z głównym regionem takim samym jak region instalacji serwera
            filteredList = playersInQueue.Where(r => r.bestRegion == server.region).ToList();
            if (filteredList.Count >= 10)
            {
                for (int i = 0; i < 10; i++)
                {
                    p.Add(filteredList[i]);
                }
                foreach (Player pl in p)
                {
                    playersInQueue.Remove(pl);
                }
                server.StartGame(p, timeElapsed);
            }
        }
    }
    //Dobieranie graczy do meczu na podstawie zarówno rangi jak i regionu
    public void AssignQueuedByRankRegion()
    {
        int nextRankQueue = 0;
        foreach (GS server in freeServers)
        {
            List<Player> filteredList = new List<Player>();
            List<Player> p = new List<Player>();
            int j = 0;
            while (filteredList.Count < 10 && j < 9)
            {
                filteredList = playersInQueue.Where(r => (int)r.playerRank == nextRankQueue && r.bestRegion == server.region).ToList();
                if (filteredList.Count >= 10)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        p.Add(filteredList[i]);
                    }
                    foreach (Player pl in p)
                    {
                        playersInQueue.Remove(pl);
                    }
                    server.StartGame(p, timeElapsed);
                }
                else
                {
                    nextRankQueue = (nextRankQueue + 1) % 9;
                    j++;
                }
            }

        }
    }

    public void Update()
    {
        if(startSimulation)
        {
            if (TFINAL >= timeElapsed)
            {
                timeElapsed += Time.deltaTime;
            }
            else
            {
                Logger.Instance.LogMessage("Simulation ended with average Q of: " + DataVault.Instance.CalculateAVGQ(DataVault.Instance.matchHistory));
                if(AdminPanel.Instance.getSaveState() == 1)
                {
                    DataVault.Instance.SaveData(AdminPanel.Instance.getQueueType(), AdminPanel.Instance.getOutputType());
                }
                startSimulation = false;
                RestartData();
                AdminPanel.Instance.NextSimData();
            }
            if (timeElapsed > 0 /*&& timeElapsed % T == 0*/ && timeElapsed > _lastTimeStamp+T)
            {
                _lastTimeStamp = timeElapsed;
                numberOfNewPlayersPerT = (int)RandomFromDistribution.RandomNormalDistribution(60, 4);
                UpdateServerStatus();
                SpawnNextPlayerTick();
                switch(queueType)
                {
                    case 0:
                        AssignQueuedPlayers();
                        break;
                    case 1:
                        AssignQueuedByRank();
                        break;
                    case 2:
                        AssignQueuedByRegion();
                        break;
                    case 3:
                        AssignQueuedByRankRegion();
                        break;
                }
                UpdateServerStatus();
            }
        }
        
    }

    public enum Region
    {
        EU = 0,
        NA = 1,
        SA = 2
    }
}
