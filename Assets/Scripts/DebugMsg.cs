using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class DebugMsg : MonoBehaviour
{
    [SerializeField] private static bool debuggingOn = true;
    [SerializeField] private static int _logLevel = 4;
    [SerializeField] private static bool showTimings = true;


    private static Dictionary<int, Stopwatch> timerCharts = new Dictionary<int, Stopwatch>();



    public static void Begin(int id, string message, int logLevel) 
    {
        if (debuggingOn) 
        {
            if (logLevel <= _logLevel)
            {
                if (showTimings)
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    timerCharts.Add(id, watch);
                    Messenger(message);
                }
                else
                {
                    Messenger(message);
                }
            }
        }
    }

    
    public static void End(int id, string message, int logLevel) 
    {
        if (debuggingOn)
        {
            if(logLevel <= _logLevel) 
            {
                if (showTimings) 
                {
                    if (timerCharts.ContainsKey(id)) 
                    {
                        timerCharts[id].Stop();
                        double elapsed = timerCharts[id].Elapsed.TotalMilliseconds;
                        timerCharts.Remove(id);
                        Messenger(message + " Time Taken: " + elapsed + "ms");
                    }
                }
                else 
                {
                    Messenger(message);
                }      
            }
        }
    }

    
    public static void Notify(string message, int logLevel) 
    {
        if (debuggingOn)
        {
            if (logLevel <= _logLevel)
            {
                Messenger(message);
            }
        }
    }


    private static void Messenger(string message)
    {
        UnityEngine.Debug.Log("[" + DateTime.Now.ToString("hh:mm:ss") + "] " + message);
    }
}
