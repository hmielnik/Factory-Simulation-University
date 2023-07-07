using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SimData 
{
    public int serverNum;
    public int queue;
    [SerializeField]public float[] reg = new float[3];
    public int simLoops;
}
