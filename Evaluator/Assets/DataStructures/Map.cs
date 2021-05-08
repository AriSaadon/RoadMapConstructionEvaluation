using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Structure that represents a road graph with intersection and roads with possible different directedness (directed/undirected).
/// </summary>
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
    /// Updates enclosing bounds and calculate total road length.
    /// </summary>
    public void Refresh()
    {
        UpdateBounds();
        CalcRoadLength();
    }

    public void CalcRoadLength()
    {
        this.length = 0;
        foreach (Road road in roads) this.length += road.length;
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

    /// <summary>
    /// Hard coded coloring of all road depending on their popularity in the trajectory generation.
    /// </summary>
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

    /// <summary>
    /// Prunes away sections of the map that are outside of the a provided bounding box. Can be used for a smaller map.
    /// </summary>
    /// <param name="b">The rectangle of the map we want to keep.</param>
    public void SetBounds(Rect b)
    {
        float minX = b.x, maxX = minX + b.width;
        float minY = b.y, maxY = minY + b.height;

        List<Road> redundantRoads = roads.FindAll(y => y.roadPoints.Any(x => x.x < minX || x.x > maxX || x.z < minY || x.z > maxY));

        RemoveRoads(redundantRoads);

        Refresh();
    }

    /// <summary>
    /// Removes roads from the map and updates intersections correctly.
    /// </summary>
    /// <param name="trashRoads">Roads that we want to delete from our map.</param>
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

    /// <summary>
    /// Get center point of map, usefull for moving the camera.
    /// </summary>
    /// <returns>The center of the bounding box in which the map is contained.</returns>
    public Vector3 GetCenter()
    {
        return new Vector3(bounds.x + 0.5f * bounds.width, 0, bounds.y + 0.5f * bounds.height);
    }    

    /// <summary>
    /// Get coordinate withing the map that is the closest to the provided point.
    /// </summary>
    /// <param name="pos">The point for which we are trying to find the closes point to in the map.</param>
    /// <returns>A coordinate of the closest point on the map.</returns>
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