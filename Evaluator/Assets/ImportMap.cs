using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class ImportMap
{ 
    public Map ReadMap(string name)
    {
        Map map = new Map(name);

        string[] vertLines = File.ReadAllLines($"Maps/{name}Vertices.txt");
        string[] edgeLines = File.ReadAllLines($"Maps/{name}Edges.txt");
        Dictionary<int, Vector3> vertices = new Dictionary<int, Vector3>();
        Dictionary<Vector3, List<(Vector3, Vector3, bool)>> adjacentDic = new Dictionary<Vector3, List<(Vector3, Vector3, bool)>>();
        List<(Vector3, Vector3, bool)> edges = new List<(Vector3, Vector3, bool)>();

        for (int i = 0; i < vertLines.Length; i++) // import vertices
        {
            string[] vars = vertLines[i].Split(',');
            Vector3 pos = new Vector3(float.Parse(vars[1]), 0, float.Parse(vars[2]));
            vertices.Add(int.Parse(vars[0]), pos);
            adjacentDic[pos] = new List<(Vector3, Vector3, bool)>();
        }
        
        for (int i = 0; i < edgeLines.Length; i++) //import edges and keep track of degrees so that intersections can be determined
        {
            string[] vars = edgeLines[i].Split(','); 

            Vector3 a = vertices[int.Parse(vars[1])];
            Vector3 b = vertices[int.Parse(vars[2])];
            bool c = vars.Length == 4 ? vars[3] == "1" : false; //if the map has directedness info then use it, otherwise everything is undirected
            adjacentDic[a].Add((a, b, c));
            adjacentDic[b].Add((a, b, c));
            edges.Add((a, b, c));  
        }


        foreach(Vector3 pos in vertices.Values) // make intersections for all the qualified degree vertices
        {
            List<(Vector3, Vector3, bool)> adjacents = adjacentDic[pos];
            if (adjacents.Count == 1 || adjacents.Count > 2)
            {
                Intersection i = new Intersection(pos, adjacents.Count(x => !x.Item3 || x.Item1 == pos )); //undirected or outgoing
                map.intersections.Add(i);
                adjacents.RemoveAll(x => pos == x.Item2);
            }
            else if (adjacents.Count == 2)
            {   //if different directedness, or all are outgoing or all are incoming.
                if (adjacents[0].Item3 != adjacents[1].Item3 || adjacents.Count(x => x.Item1 == pos) == 2 || adjacents.Count(x => x.Item2 == pos) == 2)
                {
                    Intersection i = new Intersection(pos, adjacents.Count(x => !x.Item3 || x.Item1 == pos)); //undirected or outgoing
                    map.intersections.Add(i);
                    adjacents.RemoveAll(x => pos == x.Item2);
                }
            }
        }

        foreach(Intersection intersection in map.intersections.FindAll(x => x.outgoing != null)) // make the roads for intersections of turfs that had outgoing edges.
        {
            List<(Vector3, Vector3, bool)> outEdges = adjacentDic[intersection.position];
            for(int i = 0; i < outEdges.Count; i ++)
            {
                Road road = new Road();
                road.start = intersection;

                road.directed = outEdges[i].Item3;
                road.roadPoints.Add(outEdges[i].Item1);
                road.roadPoints.Add(outEdges[i].Item2);

                Vector3 lastPoint = road.roadPoints[road.roadPoints.Count - 1];
                while (!map.intersections.Exists(x => x.position == lastPoint)) //while there is no intersection equal to the last point in the road, we expand the road
                {
                    (Vector3, Vector3, bool) next = adjacentDic[lastPoint].Find(x => x.Item1 == lastPoint);
                    if (next.Item3 != road.directed) Debug.Log("issue with intersection determination");
                    road.roadPoints.Add(next.Item2);
                    lastPoint = road.roadPoints[road.roadPoints.Count - 1];                    
                }

                road.end = map.intersections.Find(x => x.position == lastPoint);

                road.CalculateLength();

                road.start.outgoing.Add(road);
                if(!road.directed)
                {
                    road.end.outgoing.Add(road);
                }

                map.roads.Add(road);
            }
        }

        foreach (Road road in map.roads)
        {
            for (int i = 0; i < road.roadPoints.Count; i++)
            {
                road.popularities.Add(0);
            }
        }

        map.Refresh();        

        return map;        
    }
}