using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public enum Rank
    {
        Iron = 0,
        Bronze = 1,
        Silver = 2,
        Gold = 3,
        Platinum = 4,
        Diamond = 5,
        Challenger = 6,
        Master = 7,
        Pro = 8
    }

    public int[] pingToRegion = new int[3];
    public float timeInQueue = 0;
    public Rank playerRank;
    public Matchmaker.Region bestRegion;
}
