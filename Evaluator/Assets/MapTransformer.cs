using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTransformer
{
    /// <summary>
    /// Transforms a map to a directed one by changing undirected roads into two directed.
    /// </summary>
    /// <param name="map">Map we would like to make directed.</param>
    public void MakeDirectedMap(Map map)
    {
        List<Road> newRoads = new List<Road>();

        foreach(Road road in map.roads)
        {
            if (!road.directed) //if undirected
            {
                Intersection end = road.end;

                road.directed = true;
                end.outgoing.Remove(road);

                Road twin = new Road();
                twin.directed = true;
                twin.start = road.end;
                twin.end = road.start;
                twin.length = road.length;
                twin.traversed = road.traversed;

                twin.roadPoints = new List<Vector3>();
                for (int i = road.roadPoints.Count - 1; i >= 0; i--) twin.roadPoints.Add(road.roadPoints[i]);

                twin.popularities = new List<int>();
                for (int i = road.roadPoints.Count - 1; i >= 0; i--) twin.popularities.Add(road.popularities[i]);

                end.outgoing.Add(twin);
                newRoads.Add(twin);
            }
        }

        map.roads.AddRange(newRoads);
        map.CalcRoadLength();
    }

    /// <summary>
    /// Transforms a map to an undirected one by making directed roads undirected.
    /// </summary>
    /// <param name="map">Map we would like to make undirected.</param>
    public void MakeUndirectedMap(Map map)
    {
        foreach (Road road in map.roads)
        {
            if (road.directed) //if directed
            {
                road.directed = false;
                if (road.end.outgoing == null) road.end.outgoing = new List<Road>();
                road.end.outgoing.Add(road);
            }
        }
    }
}
