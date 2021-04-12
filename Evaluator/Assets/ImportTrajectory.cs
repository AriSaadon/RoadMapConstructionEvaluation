using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImportTracjectory
{
    public Vector3[] Import(string map, int i)
    {
        GameObject trajectories = GameObject.Find("Trajectories");
        string[] trace = File.ReadAllLines($"../Data/{map}/trip_{i}.txt");
        Vector3[] positions = new Vector3[trace.Length];

        for (int j = 0; j < trace.Length; j++)
        {
            string[] sample = trace[j].Split(' ');
            Vector3 pos = new Vector3(float.Parse(sample[0]), 0, float.Parse(sample[1]));
            positions[j] = pos;
        }

        return positions;
    }
}
