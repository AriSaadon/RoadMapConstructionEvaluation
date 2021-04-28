using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Drawer
{
    /// <summary>
    /// Destroys entities present in the scene, so that new structures can be drawn.
    /// </summary>
    public void Refresh()
    {
        if(GameObject.Find("Maps") != null)
        {
            GameObject.DestroyImmediate(GameObject.Find("Maps"));
            GameObject.DestroyImmediate(GameObject.Find("Trajectories"));
            GameObject.DestroyImmediate(GameObject.Find("Sample"));
        }

        new GameObject("Trajectories");
        new GameObject("Maps");
        Transform p = new GameObject("Sample").transform;
        new GameObject("CT").transform.SetParent(p);
        new GameObject("GT").transform.SetParent(p);
        new GameObject("Matches").transform.SetParent(p);
        new GameObject("UnmatchedCT").transform.SetParent(p);
        new GameObject("UnmatchedGT").transform.SetParent(p);
    }

    /// <summary>
    /// Draws all the roads of a given map.
    /// </summary>
    /// <param name="map">The map to be drawn.</param>
    /// <param name="col">The color in which we want to draw.</param>
    /// <param name="offset">The offset we use when drawing, this can be used to position maps side by side.</param>
    public void DrawRoads(Map map, Color col, Vector3 offset)
    {
        GameObject mapObj = new GameObject(map.name + "Roads");
        mapObj.transform.parent = GameObject.Find("Maps").transform;

        foreach (Road road in map.roads)
        {
            road.line = DrawRoad(road.roadPoints.ToArray(), col, mapObj.name, road.directed, offset);
        }
    }

    /// <summary>
    /// Draws a single road.
    /// </summary>
    /// <param name="positions">The points defined by the road.</param>
    /// <param name="col">The color in which we would like to draw.</param>
    /// <param name="parent">The desired parent of the gameobject, making it easier to disable multiple roads (having the same parent) in the scene view.</param>
    /// <param name="directed">The direction of the road used for visualisation.</param>
    /// <param name="offset">The drawing offset.</param>
    /// <returns></returns>
    public LineRenderer DrawRoad(Vector3[] positions, Color col, string parent, bool directed, Vector3 offset)
    {
        GameObject edge = new GameObject();
        LineRenderer renderer = edge.AddComponent<LineRenderer>();

        if (directed) renderer.startWidth = 1; else renderer.startWidth = 3;

        renderer.endWidth = 3;
        renderer.material = Resources.Load("Mat") as Material;

        //if (directed) renderer.material.SetColor("_Color", new Color(0,0,155,255));
        renderer.material.SetColor("_Color", col);

        List<Vector3> p = new List<Vector3>(positions);
        for (int i = 0; i < p.Count; i++) { p[i] = p[i] + offset;  }
        renderer.positionCount = p.Count;
        renderer.SetPositions(p.ToArray());
        edge.transform.SetParent(GameObject.Find(parent).transform);

        return renderer;
    }

    /// <summary>
    /// Draw all the intersections of the map.
    /// </summary>
    /// <param name="map">The map we would like to draw all the intersections from.</param>
    /// <param name="col">The color we would like to draw the intersections in.</param>
    public void DrawIntersections(Map map, Color col)
    {
        GameObject mapObj = new GameObject(map.name + "Intersections");
        mapObj.transform.parent = GameObject.Find("Maps").transform;

        foreach (Intersection intersection in map.intersections)
        {
            DrawIntersection(intersection.position, col, mapObj.name, 10);
        }
    }

    /// <summary>
    /// Draw a single intersection.
    /// </summary>
    /// <param name="pos">The position of the intersection.</param>
    /// <param name="col">The color we would like to draw it in.</param>
    /// <param name="parent">The name of the parent object, used for grouping purposes.</param>
    /// <param name="size">The size we would like to draw it at.</param>
    public void DrawIntersection(Vector3 pos, Color col, string parent, int size)
    {
        GameObject vertex = new GameObject();
        LineRenderer renderer = vertex.AddComponent<LineRenderer>();
        renderer.startWidth = size * 2;
        renderer.endWidth = size * 2;
        renderer.material = Resources.Load("Mat") as Material;
        renderer.material.SetColor("_Color", col);
        renderer.positionCount = 2;
        renderer.SetPositions(new Vector3[] { pos - Vector3.right * size, pos + Vector3.right * size });
        vertex.transform.SetParent(GameObject.Find(parent).transform);
    }

    /// <summary>
    /// Draws the individual points of the trajectory.
    /// </summary>
    /// <param name="track">List of points that need to be drawn.</param>
    /// <param name="name">Desired name of the parent object, for grouping purposes.</param>
    /// <param name="clr">The color we would like to draw the points in.</param>
    public void DrawTraj(Vector3[] track, string name, Color clr)
    {
        foreach(Vector3 point in track)
        {
            DrawIntersection(point, clr, name, 1);
        }
    }

    /// <summary>
    /// Draw a subset of a trajectory set.
    /// </summary>
    /// <param name="name">The name of the trajectory set used to import it.</param>
    /// <param name="amount">The amount of trajectories we want to draw from the set.</param>
    /// <param name="offset">The offset we would like to use when drawing.</param>
    /// <param name="color">The color we want to draw the trajectories in.</param>
    /// <param name="importTraj">An importTraj object to import individual trajectories with.</param>
    public void DrawTrajectories(string name, int amount, Vector3 offset, Color color, ImportTraj importTraj)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(GameObject.Find("Trajectories").transform);

        for (int i = 0; i < amount; i++)
        {
            Vector3[] traj = importTraj.Import(name, i);
            DrawRoad(traj, color, name, false, offset);
            DrawTraj(traj, name, Color.black);
        }
    }
}
