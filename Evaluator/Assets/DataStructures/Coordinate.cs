using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// A coordinate represents a point on our road graph/map.
/// We use a elaborate structure instead of a vector3 to keep track of topology surrounding the point.
/// </summary>
public struct Coordinate
{
    public Road road;
    public Vector3 location;
    public float distance;
    public int index;
    public float offset;

    /// <summary>
    /// Creates a coordinate.
    /// </summary>
    /// <param name="road">Road the coordinate is on.</param>
    /// <param name="location">position the coordinate is at.</param>
    /// <param name="distance">helper variable used only used to keep track of the distance traveled by a coordinate. This is usefull for some algorithms</param>
    /// <param name="index">index of the last roadpoint of the road we passed.</param>
    /// <param name="offset">offset from the last roadpoint indicated corresponding to the index</param>
    public Coordinate(Road road, Vector3 location, float distance, int index, float offset)
    {
        this.road = road;
        this.location = location;
        this.distance = distance;
        this.index = index;
        this.offset = offset;
    }
}
