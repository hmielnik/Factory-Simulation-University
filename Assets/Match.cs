using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Match
{
    public float gameTime;
    public int pingDiff;
    public Matchmaker.Region region;
    public int rankDiff;
    public float endTime;
    public float avgPlayerWaitTime;
    public float[] waitTimes = new float[10];
    public float Q;
}
