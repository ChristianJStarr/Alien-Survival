using UnityEngine;

public class UtilAverage
{ 
    public string avgName;

    public bool minMax;

    public float average;
    public float min;
    public float max;
   
    
    public string readout;

    private int count;
    private float total;



    public void Input(float value)
    {
        count++;
        total += value;
        float average = total / count;
        readout = avgName + " AVG: " + average + " ";
        
        if (value < min) { min = value; }
        if (value > max) { max = value; }


        if (minMax) 
        {
            readout += "MIN: " + min + " MAX: " + max;
        }

    }




}
