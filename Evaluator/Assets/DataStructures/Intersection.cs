using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Intersections are vertices of degree larger than zero. 
/// However, degree 2 vertices, which function only as a bridge between other vertices (i.e. 1 incoming and 1 outgoing edge), are not intersections.
/// </summary>
public class Intersection
{
    public int id;
    public Vector3 position;
    public List<Road> outgoing;
    public AStar star;

    public Intersection(Vector3 x, int outgo)
    {
        position = x;
        if (outgo > 0)
        {
            outgoing = new List<Road>();
        }
        id = -1;
    }
}

/// <summary>
/// Help struct used for convenience in A* pathfinding algorithm.
/// </summary>
public struct AStar
{
    public Road parent;
    public float g;
}
