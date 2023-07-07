using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdminPanel : MonoBehaviour
{
    public static AdminPanel Instance;
    public InputField serverNumber;
    public InputField[] regions = new InputField[3];
    public InputField simLoops;
    public Dropdown outputType;
    public Dropdown queueType;
    public Slider saveOutput;
    public int pointer = 0;
    public bool multipleSims = false;

    public Matchmaker matchmaker;
    public List<SimData> simDatas = new List<SimData>();


    public void Awake()
    {
        Instance = this;
        GenerateSimDatas();
    }

    public void NextSimData()
    {
        pointer++;
        Init();
    }

    public void GenerateSimDatas()
    {
        int[] serverNum = { 10, 15, 20 };
        float[] regs = { 34, 33, 33, 80, 10, 10, 60, 20, 20, 40, 30, 30, 50, 25, 25 };
        int[] queues = { 0, 1, 2, 3 };
        foreach(int integer in serverNum)
        {
            foreach(int queue in queues)
            {
                for(int i = 0;i<regs.Length - 2;i+=3)
                {
                    float[] reg = new float[3];
                    reg[0] = regs[i]/100f;
                    reg[1] = regs[i + 1]/100f;
                    reg[2] = regs[i + 2]/100f;
                    SimData sim = new SimData();
                    sim.reg = reg;
                    sim.serverNum = integer;
                    sim.simLoops = 100;
                    sim.queue = queue;
                    simDatas.Add(sim);
                }
            }
        }
    }

    public void Init()
    {
        if(multipleSims == false)
        {
            float[] reg = new float[3];
            for (int i = 0; i < 3; i++)
            {
                if (regions[i].text != null)
                {
                    reg[i] = (int.Parse(regions[i].text)) / 100f;
                }
                else
                {
                    reg[i] = 0.33f;
                }
            }
            matchmaker.InitializeData(reg, int.Parse(serverNumber.text), int.Parse(simLoops.text), getQueueType());
        }
        else
        {
            matchmaker.InitializeData(simDatas[pointer].reg, simDatas[pointer].serverNum, simDatas[pointer].simLoops, simDatas[pointer].queue);
        }
        
    }

    public int getOutputType()
    {
        return outputType.value;
    }

    public int getSaveState()
    {
        return ((int)saveOutput.value);
    }

    public int getQueueType()
    {
        return queueType.value;
    }
}
