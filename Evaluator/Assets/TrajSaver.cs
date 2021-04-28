using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class TrajSaver
{
    /// <summary>
    /// Saves a synthetically generated trajectory set.
    /// </summary>
    /// <param name="tracks">Complete trajectory set we would like to export. Tuples indicating location and time. Time is however not implemented currently.</param>
    /// <param name="outputDir">Directory we want to output at.</param>
    public void SaveTracks(List<List<(Vector3, int)>> tracks, string outputDir)
    {
        for(int i = 0; i < tracks.Count; i ++)
        {
            if (!Directory.Exists(outputDir)) //if the directory does not exist we create it.
            {
                Directory.CreateDirectory(outputDir);
            }

            if (i == 0) //delete all the trips in the directory before we write new trips to it.
            {
                DirectoryInfo dir = new DirectoryInfo(outputDir);
                foreach (FileInfo file in dir.GetFiles().ToList())
                {
                    if (file.Name != "dataset-config.yml")
                    {
                        file.Delete();
                    }
                }
            }

            List<string> output = new List<string>();

            int time = 0;
            foreach ((Vector3, int) point in tracks[i])
            {
                output.Add($"{point.Item1.x} {point.Item1.z} {time}");
                time += 4;
            }

            File.WriteAllLines(outputDir + $"/trip_{i}.txt", output.ToArray());
        }        
    }
}