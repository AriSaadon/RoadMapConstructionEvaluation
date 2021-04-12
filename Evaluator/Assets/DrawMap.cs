using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawMap
{
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

    public void DrawRoads(Map map, Color col, Vector3 offset)
    {
        GameObject mapObj = new GameObject(map.name + "Roads");
        mapObj.transform.parent = GameObject.Find("Maps").transform;

        foreach (Road road in map.roads)
        {
            road.line = DrawRoad(road.roadPoints.ToArray(), col, mapObj.name, road.directed, offset);
        }
    }

    public LineRenderer DrawRoad(Vector3[] positions, Color col, string parent, bool directed, Vector3 offset)
    {
        GameObject edge = new GameObject();
        LineRenderer renderer = edge.AddComponent<LineRenderer>();

        if (directed) renderer.startWidth = 1; else renderer.startWidth = 5;

        renderer.endWidth = 4;
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

    public void DrawIntersections(Map map, Color col)
    {
        GameObject mapObj = new GameObject(map.name + "Intersections");
        mapObj.transform.parent = GameObject.Find("Maps").transform;

        foreach (Intersection intersection in map.intersections)
        {
            DrawIntersection(intersection.position, col, mapObj.name, 10);
        }
    }

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

    public void DrawTraj(Vector3[] track, string name)
    {
        foreach(Vector3 point in track)
        {
            DrawIntersection(point, Color.black, name, 5);
        }
    }

    public void DrawTrajectories(string name, int amount, Vector3 offset, Color color, ImportTracjectory importTraj)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(GameObject.Find("Trajectories").transform);

        for (int i = 0; i < amount; i++)
        {
            Vector3[] traj = importTraj.Import(name, i);
            DrawRoad(traj, color, name, false, offset);
            DrawTraj(traj, name);
        }
    }
}
