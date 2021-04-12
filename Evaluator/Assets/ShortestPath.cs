using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ShortestPath
{
    public static Vector3[] AStarShortestPath(Map map, Coordinate source, Coordinate sink)
    {
        List<Vector3> path = new List<Vector3>();

        PriorityQueue<Intersection> open = new PriorityQueue<Intersection>();
        map.intersections.ForEach(x => x.star.g = 0); //a distance of zero means not visited yet

        source.road.end.star.parent = null;
        source.road.end.star.g = source.road.length - source.distance;
        open.Enqueue(source.road.end, source.road.end.star.g + Vector3.Distance(source.road.end.position, sink.location));
        if (!source.road.directed)
        {
            source.road.start.star.parent = null;
            source.road.start.star.g = source.distance;
            open.Enqueue(source.road.start, source.road.start.star.g + Vector3.Distance(source.road.start.position, sink.location));
        }

        Intersection currIntersect = open.Dequeue();

        while (currIntersect != null && currIntersect != sink.road.start && (currIntersect != sink.road.end || sink.road.directed)) //if we are not yet at the sink and we can still explore
        {
            if (currIntersect.outgoing != null) //if this guy has outgoing roads
            {
                foreach (Road road in currIntersect.outgoing) //explore the current nodes
                {
                    Intersection next;
                    if (road.start == currIntersect) next = road.end; //directed and undirected.
                    else next = road.start;

                    if (next.star.g == 0) //if unexplored
                    {
                        next.star.parent = road; //save parent for chaining
                        next.star.g = currIntersect.star.g + road.length; //calc g based on parent g

                        open.Enqueue(next, next.star.g + Vector3.Distance(next.position, sink.location)); //f = g + heuristic
                    }
                }
            }

            currIntersect = open.Dequeue(); //get the next lowest node.
        }

        if (currIntersect == null) return null;

        path.Add(sink.location); //the location of the sink

        if (currIntersect == sink.road.start) //The path to the end of the road of the sink.
        {
            path.InsertRange(0, sink.road.roadPoints.GetRange(0, sink.index + 1));
            for (int i = 0; i < sink.index + 1; i++)
                sink.road.popularities[i] += 1;
        }
        else
        {
            for (int i = sink.index + 1; i < sink.road.roadPoints.Count; i++)
            {
                path.Insert(0, sink.road.roadPoints[i]);
                sink.road.popularities[i] += 1;
            }
        }

        while (currIntersect.star.parent != null) //the path along the roads to the other coordinate road
        {
            if (currIntersect.star.parent.start == currIntersect) //if we are at the start of the polyline
            {
                for (int i = 0; i < currIntersect.star.parent.roadPoints.Count; i++)
                {
                    path.Insert(0, currIntersect.star.parent.roadPoints[i]);
                    currIntersect.star.parent.popularities[i] += 1;
                }

                currIntersect = currIntersect.star.parent.end;
            }
            else //if we have the end of the polyline next
            {
                path.InsertRange(0, currIntersect.star.parent.roadPoints);

                for (int i = 0; i < currIntersect.star.parent.popularities.Count; i++)
                    currIntersect.star.parent.popularities[i] += 1;

                currIntersect = currIntersect.star.parent.start;
            }
        }

        if (currIntersect == source.road.start) //The path from the end of the road of the source.
        {
            for (int i = 0; i <= source.index; i++)
            {
                path.Insert(0, source.road.roadPoints[i]);
                source.road.popularities[i] += 1;
            }
        }
        else //from end of path to the source. Note that get range second parameter wants length and not end index...
        {
            path.InsertRange(0, source.road.roadPoints.GetRange(source.index + 1, source.road.roadPoints.Count - source.index - 1));
            for (int i = source.index + 1; i < source.road.roadPoints.Count; i++)
                source.road.popularities[i] += 1;
        }

        path.Insert(0, source.location); //the location of the source

        return path.Distinct().ToArray();
    }

}

public class PriorityQueue<T>
{
    private List<KeyValuePair<T, float>> elements = new List<KeyValuePair<T, float>>();

    public int Count
    {
        get { return elements.Count; }
    }

    public void Enqueue(T item, float priority)
    {
        elements.Add(new KeyValuePair<T, float>(item, priority));
    }

    // Returns the Location that has the lowest priority
    public T Dequeue()
    {
        if (elements.Count == 0) return default;
        int bestIndex = 0;

        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].Value < elements[bestIndex].Value)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].Key;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}
