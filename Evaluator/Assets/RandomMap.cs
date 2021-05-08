using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains various random variables.
/// </summary>
public class RandomMap
{
    /// <summary>
    /// Gets a random road from a map with probability proportional to its length.
    /// </summary>
    /// <param name="map">The map we want a random road from.</param>
    /// <returns>A random road.</returns>
    public Road GetWeightedRandomRoad(Map map)
    {
        float rand = Random.Range(0, map.length - 0.1f);

        float x = 0;
        int i = 0;
        while (!(rand >= x && rand <= x + map.roads[i].length)) // while i is smaller than map and x is not inbetween length interval
        {
            x += map.roads[i].length;
            i++;
        }

        return map.roads[i];
    }

    /// <summary>
    /// Gets a random road from a map with probability uniform accross all roads.
    /// </summary>
    /// <param name="map">The map we want a random road from.</param>
    /// <returns>A random road.</returns>
    public Road GetUniformRandomRoad(Map map)
    {
        int rand = Random.Range(0, map.roads.Count - 1);
        return map.roads[rand];
    }

    /// <summary>
    /// Gets a random coordinate on the provided road.
    /// </summary>
    /// <param name="road">Road we want a random point on.</param>
    /// <returns>A coordinate representing a random point on the road.</returns>
    public Coordinate GetRandomPointOnRoad(Road road)
    {
        float target = Random.Range(0, road.length - 0.1f);

        float acc = 0;
        int i = 0;
        float distance = Vector3.Distance(road.roadPoints[i], road.roadPoints[i + 1]);
        while (!(target <= acc + distance)) // while i is smaller than map and x is not inbetween length interval
        {
            acc += distance;
            i++;
            distance = Vector3.Distance(road.roadPoints[i], road.roadPoints[i + 1]);
        }

        Vector3 location = road.roadPoints[i] + ((road.roadPoints[i + 1] - road.roadPoints[i]).normalized * (target - acc));

        return new Coordinate(road, location, target, i, target - acc);
    }

    public Color GetRandomColor()
    {
        return new Color32((byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)Random.Range(0, 256), 255);
    }

    public int GetRandomInt(int first, int last)
    {
        return Random.Range(first, last + 1);
    }

    //public Color32 GetSpecialCol(int x)
    //{
    //    Color32[] colors = new Color32[]
    //    {
    //        new Color32(84,48,5,255),
    //        new Color32(140,81,10,255),
    //        new Color32(191,129,45,255),
    //        new Color32(223,194,125,255),
    //        new Color32(246,232,195,255),
    //        new Color32(199,234,229,255),
    //        new Color32(128,205,193,255),
    //        new Color32(53,151,143,255),
    //        new Color32(1,102,94,255),
    //        new Color32(0,60,48,255)
    //    };

    //    return colors[x];
    //}
}
