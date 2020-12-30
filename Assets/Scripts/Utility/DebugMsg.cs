using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class DebugMsg : MonoBehaviour
{
    private static bool debuggingOn = true;
    private static int _logLevel = 4;
    private static bool showTimings = true;


    private static Dictionary<int, Stopwatch> timerCharts;
    private void Start() 
    {
        if (timerCharts == null)
        {
            timerCharts = new Dictionary<int, Stopwatch>();
        }
    }

    public static void Begin(int id, string message, int logLevel) 
    {
        if (timerCharts == null)
        {
            timerCharts = new Dictionary<int, Stopwatch>();
        }
        if (debuggingOn) 
        {
            if (logLevel <= _logLevel)
            {
                if (showTimings)
                {
                    if (!timerCharts.ContainsKey(id))
                    {
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        timerCharts.Add(id, watch);
                        if(message.Length > 0)
                            Messenger(message);
                    }
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
                    if (timerCharts.ContainsKey(id))
                    {
                        Messenger(message);
                        timerCharts.Remove(id);
                    }
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
