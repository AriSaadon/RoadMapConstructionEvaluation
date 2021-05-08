using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A road is a chain of vertices, that starts with an intersection and ends with an intersection.
/// </summary>
public class Road
{
    public Intersection start;
    public Intersection end;

    public bool directed;
    public float length;
    
    public LineRenderer line;

    public bool traversed;

    public List<Vector3> roadPoints = new List<Vector3>();
    public List<int> popularities = new List<int>();

    /// <summary>
    /// Calculate total length of the road.
    /// </summary>
    public void CalculateLength()
    {
        length = 0;
        for (int i = 0; i < roadPoints.Count - 1; i++)
        {
            length += Vector3.Distance(roadPoints[i], roadPoints[i + 1]);
        }
    }

    /// <summary>
    /// Get a coordinate that is nearest to the point given as parameter.
    /// </summary>
    /// <param name="pos">point for which we determine the closest point to it on our road.</param>
    /// <returns>a coordinate that is the closest point  to pos on our road.</returns>
    public Coordinate GetNearestCoordinate(Vector3 pos)
    {
        Coordinate coordinate = new Coordinate();
        float closest = float.MaxValue;
        float acc = 0;
        for (int i = 0; i < roadPoints.Count - 1; i++)
        {
            Vector3 a = roadPoints[i];
            Vector3 b = roadPoints[i + 1];
            Vector3 c = ClosestPoint(a, b, pos);
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

    /// <summary>
    /// Calculates closest point on a line segment to a specified point.
    /// </summary>
    /// <param name="a">start point of linesegment</param>
    /// <param name="b">end point of linesegment</param>
    /// <param name="p">specified point for which we are trying to find the closest point on our linesegment.</param>
    /// <returns></returns>
    Vector3 ClosestPoint(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 x = p - a;
        Vector3 y = (b - a);

        float num = Vector3.Dot(x, y);
        float denom = Vector3.Dot(y, y);

        float t = num / denom;

        return a + y * Mathf.Clamp(t, 0, 1);
    }
}
