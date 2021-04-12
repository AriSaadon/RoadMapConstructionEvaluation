using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map
{
    public string name;
    public List<Intersection> intersections =  new List<Intersection>();
    public List<Road> roads = new List<Road>();

    public float length;
    public Rect bounds = new Rect(0, 0, 0, 0);

    public Map(string name)
    {
        this.name = name;
    }

    /// <summary>
    /// Updates bounds and calc road length.
    /// </summary>
    public void Refresh()
    {
        UpdateBounds();
        CalcRoadLength();
    }

    public void CalcRoadLength()
    {
        float length = 0;

        foreach(Road road in roads)
        {
            length += road.length;
        }
        this.length = length;
    }

    public void UpdateBounds()
    {
        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;

        foreach(Intersection i in intersections)
        {
            if (i.position.x < minX) minX = i.position.x;
            if (i.position.x > maxX) maxX = i.position.x;
            if (i.position.z < minY) minY = i.position.z;
            if (i.position.z > maxY) maxY = i.position.z;
        }

        bounds.x = minX;
        bounds.y = minY;
        bounds.width = maxX - minX;
        bounds.height = maxY - minY;
    }

    public Rect GetBounds()
    {
        return bounds;
    }

    public void PruneRoads()
    {
        List<Road> unusedRoads = new List<Road>();

        foreach (Road road in roads)
        {
            if (road.popularities.All(y => y == 0))
                unusedRoads.Add(road);
        }
            
        RemoveRoads(unusedRoads);

        foreach (Road road in roads)
        {
            for (int i = road.popularities.Count - 1; i >= 0; i--)
            {
                if (road.popularities[i] == 0)
                {
                    road.roadPoints.RemoveAt(i);
                }
            }
        }

        unusedRoads.Clear();
        foreach (Road road in roads)
        {
            if (road.roadPoints.Count < 2)
                unusedRoads.Add(road);
        }
        RemoveRoads(unusedRoads);

        CalcRoadLength();
    }

    public void ColorRoads()
    {
        foreach(Road road in roads.FindAll(x => x.line != null))
        {
            int popularity = road.popularities.Max();
            if(popularity < 3)
            {
                road.line.material.SetColor("_Color", new Color32(206, 147, 84, 255));
            }
            else if(popularity < 5)
            {
                road.line.material.SetColor("_Color", new Color32(146, 25, 66, 255));
            }
            else 
            {
                road.line.material.SetColor("_Color", new Color32(1, 0, 24, 255));
            }
        }
    }

    public void SetBounds(Rect b)
    {
        float minX = b.x, maxX = minX + b.width;
        float minY = b.y, maxY = minY + b.height;

        List<Road> redundantRoads = roads.FindAll(y => y.roadPoints.Any(x => x.x < minX || x.x > maxX || x.z < minY || x.z > maxY));

        RemoveRoads(redundantRoads);

        Refresh();
    }

    public void RemoveRoads(List<Road> trashRoads)
    {
        foreach (Road road in trashRoads)
        {
            roads.Remove(road);

            if (!roads.Any(r => r.start == road.start || r.end == road.start))
                intersections.Remove(road.start);
            if (!roads.Any(r => r.start == road.end || r.end == road.end))
                intersections.Remove(road.end);
        }

        foreach (Intersection intersect in intersections)
        {
            if (intersect.outgoing != null)
            {
                intersect.outgoing.RemoveAll(y => trashRoads.Contains(y));
            }
        }
    }

    public Vector3 GetCenter()
    {
        return new Vector3(bounds.x + 0.5f * bounds.width, 3400, bounds.y + 0.5f * bounds.height);
    }    

    public Coordinate GetClosestPoint(Vector3 pos)
    {
        Coordinate shortest = roads[0].GetNearestCoordinate(pos);

        foreach (Road road in roads)
        {
            Coordinate current = road.GetNearestCoordinate(pos);

            if(Vector3.Distance(current.location, pos) < Vector3.Distance(shortest.location, pos))
            {
                shortest = current;
            }
        }

        return shortest;
    }
}

public class Intersection
{
    public Vector3 position;
    public List<Road> outgoing;
    public AStar star;
    public int id;

    public Intersection (Vector3 x, int outgo)
    {
        position = x;
        if(outgo > 0)
        {
            outgoing = new List<Road>();
        }
        id = -1;
    }
}

public class Road
{
    public Intersection start;
    public Intersection end;
    public LineRenderer line;

    public bool directed;
    public float length;

    public bool traversed;

    public List<Vector3> roadPoints = new List<Vector3>();
    public List<int> popularities = new List<int>();

    public void CalculateLength()
    {
        length = 0;
        for (int i = 0; i < roadPoints.Count - 1; i ++)
        {
            length += Vector3.Distance(roadPoints[i], roadPoints[i+1]);
        }
    }

    public Coordinate GetNearestCoordinate(Vector3 pos)
    {
        Coordinate coordinate = new Coordinate();
        float closest = float.MaxValue;
        float acc = 0;
        for (int i = 0; i < roadPoints.Count - 1; i++)
        {
            Vector3 a = roadPoints[i];
            Vector3 b = roadPoints[i + 1];
            Vector3 c = Line.ClosestPoint(a, b, pos);
            float distance = Vector3.Distance(pos, c);
            float offset = Vector3.Distance(a, c);

            if (distance < closest)
            {
                closest = distance;
                coordinate = new Coordinate(this, c, acc + offset, i, offset);
            }

            acc += Vector3.Distance(a, b);
        }

        return coordinate;
    }
}

public static class Line
{
    public static Vector3 ClosestPoint(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 x = p - a;
        Vector3 y = (b - a);

        float num = Vector3.Dot(x, y);
        float denom = Vector3.Dot(y, y);

        float t = num / denom;

        return a + y * t.Clamp(0,1); 
    }

    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0) return min;
        else if (val.CompareTo(max) > 0) return max;
        else return val;
    }
}


public struct Coordinate
{
    public Road road;
    public Vector3 location;
    public float distance;
    public int index;
    public float offset;
    public Coordinate(Road road, Vector3 location, float distance, int index, float offset)
    {
        this.road = road;
        this.location = location;
        this.distance = distance;
        this.index = index;
        this.offset = offset;
    }
}

public struct AStar
{
    public Road parent;
    public float g;
}