using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to construct a point representation of a neighbourhood for the graph sampling evaluation.
/// </summary>
public class SampleNeighbourhood
{  
    /// <summary>
    /// Gets a point set representing a neighbourhood 
    /// </summary>
    /// <param name="map">The map on which the neighbourhood is located.</param>
    /// <param name="coordinate">The origin point of the neighbourhood.</param>
    /// <param name="maxRadius">The maximum distance a point can lie on from the origin.</param>
    /// <param name="hopDistance">The distance between subsequent neighbourhood points.</param>
    /// <returns></returns>
    public List<Vector3> GetNeighbourhood(Map map, Coordinate coordinate, float maxRadius, float hopDistance)
    {
        List<Vector3> points = new List<Vector3>();

        map.roads.ForEach(a => a.traversed = false); //administer exploration.

        Queue<(Coordinate, bool, float)> queue = new Queue<(Coordinate, bool, float)>(); //coordinate, direction, distance left

        queue.Enqueue((coordinate, true, hopDistance));
        coordinate.road.traversed = true;
        if (!coordinate.road.directed)
        {
            queue.Enqueue((new Coordinate(coordinate.road, coordinate.location, coordinate.distance, coordinate.index + 1, coordinate.offset), false, hopDistance));
        }

        while (queue.Count != 0)
        {
            (Coordinate, bool, float) x = queue.Dequeue();
            float distLeft = ExploreRoad(coordinate, maxRadius, hopDistance, ref x, points);

            if (Vector3.Distance(x.Item1.location, coordinate.location) < maxRadius) //if inside of range
            {
                Intersection currInter = x.Item1.index == 0 ? x.Item1.road.start : x.Item1.road.end;
                if(currInter.outgoing != null)
                {
                    foreach (Road outgoing in currInter.outgoing.FindAll(a => a.traversed == false))
                    {
                        outgoing.traversed = true;
                        queue.Enqueue((new Coordinate(outgoing, currInter.position, 0, currInter == outgoing.start ? 0 : outgoing.roadPoints.Count - 1, 0), currInter == outgoing.start, distLeft));
                    }
                }
            }
        }

        return points;
    }

    /// <summary>
    /// Helper method handling the exploration of a single road for the neighbourhood sampling.
    /// </summary>
    /// <param name="origin">origin of our neighbourhood.</param>
    /// <param name="maxRadius">maximum distance away from the origin a point may be.</param>
    /// <param name="hopDistance">Distance between subsequent points</param>
    /// <param name="x">The current coordinate for which we explore its road from.</param>
    /// <param name="points">output array for the points we collect.</param>
    /// <returns></returns>
    public float ExploreRoad(Coordinate origin, float maxRadius, float hopDistance, ref (Coordinate, bool, float) x, List<Vector3> points)
    {
        float distLeft = x.Item3;
        int dir = x.Item2 ? 1 : -1;
        while (Vector3.Distance(x.Item1.location, origin.location) < maxRadius && x.Item1.index + dir != -1 && x.Item1.index + dir != x.Item1.road.roadPoints.Count)
        {
            Vector3 prevStop = x.Item1.road.roadPoints[x.Item1.index];
            Vector3 nextStop = x.Item1.road.roadPoints[x.Item1.index + dir];
            float coorDist = Vector3.Distance(x.Item1.location, nextStop);

            while (coorDist > distLeft && Vector3.Distance(x.Item1.location, origin.location) < maxRadius)
            {
                x.Item1.location += (nextStop - prevStop).normalized * distLeft;
                points.Add(x.Item1.location);
                coorDist = Vector3.Distance(x.Item1.location, nextStop);
                distLeft = hopDistance;
            }

            distLeft -= coorDist;
            x.Item1.location = nextStop;
            x.Item1.index += dir;
        }

        return distLeft;
    }
}
