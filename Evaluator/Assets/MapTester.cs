using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class MapTester
{
    /// <summary>
    /// Redundant method used for testing duplicate roads of a map during development.
    /// </summary>
    /// <param name="map">map for which we wanted to run the test.</param>
    public void TestDuplicates(Map map)
    {
        for (int i = 0; i < map.roads.Count; i++)
        {
            Road currRoad = map.roads[i];

            for (int j = i + 1; j < map.roads.Count; j++)
            {
                if (Vector3.Distance(currRoad.start.position, map.roads[j].start.position) < 0.05f
                    && Vector3.Distance(currRoad.end.position, map.roads[j].end.position) < 0.05f) //if directed twin
                {
                    Debug.Log("possible duplicate road");
                    //map.roads.RemoveAt(j); //remove twin so that undirectification will not double the road.
                }
            }
        }
    }
}
