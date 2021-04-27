using System;
using System.Collections.Generic;
using System.IO;

namespace OpenStreetMapParser
{
    class OSMParser
    {
        static void Main(string[] args)
        {
            string[] lines = File.ReadAllLines("map.pycgr");

            List<string> vertices = new List<string>();
            List<string> edges = new List<string>();
            
            int vertCounts = int.Parse(lines[7]);
            int edgeID = 0;


            for (int i = 9; i < 9 + vertCounts; i++)
            {
                string[] token = lines[i].Split(' ');
                vertices.Add($"{token[0]},{token[1]},{token[2]}");
            }

            for (int i = 9 + vertCounts; i < lines.Length; i++)
            {
                string[] token = lines[i].Split(' ');
                edges.Add($"{edgeID},{token[0]},{token[1]},{(token[5] == "0" ? 1 : 0)}");
                edgeID++;
            }


            File.WriteAllLines("Vertices.txt", vertices);
            File.WriteAllLines("Edges.txt", edges);
        }
    }
}
