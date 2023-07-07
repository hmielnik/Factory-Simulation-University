using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Logger : MonoBehaviour
{

    public static Logger Instance;
    public GameObject LoggerContent;
    public GameObject textPrefab;
    public int logMemory = 30;
    public List<GameObject> logs = new List<GameObject>();

    public void Awake()
    {
        Instance = this;
    }

    public void LogMessage(string message)
    {
        var log = Instantiate(textPrefab);
        log.transform.parent = LoggerContent.transform;
        log.GetComponentInChildren<Text>().text = message;
        logs.Add(log);
        if(logs.Count>logMemory)
        {
            Destroy(logs[0]);
            logs.RemoveAt(0);
        }
    }
    
}
