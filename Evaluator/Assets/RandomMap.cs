using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMap
{
    public Road GetRandomRoad(Map map)
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

    public Coordinate GetRandomCoordinate(Road road)
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
}
