using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class ImportMap
{ 
    /// <summary>
    /// Imports a road graph with directed and undirected edges.
    /// </summary>
    /// <param name="name">The name of the map used as prefix for the Verices and Edges file in the map folder.</param>
    /// <returns>A map with roads and intersections representing the road graph.</returns>
    public Map ReadMap(string name)
    {
        Map map = new Map(name);

        string[] vertLines = File.ReadAllLines($"Maps/{name}Vertices.txt");
        string[] edgeLines = File.ReadAllLines($"Maps/{name}Edges.txt");

        Dictionary<int, Vector3> vertices = new Dictionary<int, Vector3>(); // mapping of vertexID to position/vertex
        Dictionary<Vector3, List<(Vector3, Vector3, bool)>> adjactentEdges = new Dictionary<Vector3, List<(Vector3, Vector3, bool)>>(); //mapping of vertex to all its adjacent edges

        for (int i = 0; i < vertLines.Length; i++) // import vertices
        {
            string[] vars = vertLines[i].Split(',');
            Vector3 pos = new Vector3(float.Parse(vars[1]), 0, float.Parse(vars[2]));
            vertices.Add(int.Parse(vars[0]), pos);
            adjactentEdges[pos] = new List<(Vector3, Vector3, bool)>();
        }
        
        for (int i = 0; i < edgeLines.Length; i++) //import edges and keep track of degrees so that intersections can be determined
        {
            string[] vars = edgeLines[i].Split(','); 
            Vector3 start = vertices[int.Parse(vars[1])];
            Vector3 end = vertices[int.Parse(vars[2])];
            bool dir = vars.Length == 4 ? vars[3] == "1" : false; //if the map has directedness info then use it, otherwise everything is undirected
            adjactentEdges[start].Add((start, end, dir));
            adjactentEdges[end].Add((start, end, dir));
        }

        foreach(Vector3 pos in vertices.Values) // make intersections for all the qualified vertices
        {
            List<(Vector3, Vector3, bool)> adjacents = adjactentEdges[pos];
            if (adjacents.Count == 1 || adjacents.Count > 2) //if degree is 1 or larger than 2 a vertex is always an intersection.
            {
                Intersection i = new Intersection(pos, adjacents.Count(x => !x.Item3 || x.Item1 == pos )); //count amount of outgoing edges.
                map.intersections.Add(i);
                adjacents.RemoveAll(x => x.Item2 == pos); //remove adjacent edges where our new intersection is the end vertex.
            }
            else if (adjacents.Count == 2) // degree 2 vertices are intersections if the edges have different directedness, or are both outgoing or incoming.
            {   
                if (adjacents[0].Item3 != adjacents[1].Item3 || adjacents.Count(x => x.Item1 == pos) == 2 || adjacents.Count(x => x.Item2 == pos) == 2)
                {
                    Intersection i = new Intersection(pos, adjacents.Count(x => !x.Item3 || x.Item1 == pos));
                    map.intersections.Add(i);
                    adjacents.RemoveAll(x => x.Item2 == pos);
                }
            }
        }

        foreach(Intersection intersection in map.intersections.FindAll(x => x.outgoing != null)) // make the roads for intersections that had outgoing edges.
        {
            List<(Vector3, Vector3, bool)> outEdges = adjactentEdges[intersection.position];

            for(int i = 0; i < outEdges.Count; i ++)
            {
                Road road = new Road();
                road.directed = outEdges[i].Item3;

                road.start = intersection;
                road.roadPoints.Add(outEdges[i].Item1);
                road.roadPoints.Add(outEdges[i].Item2);

                Vector3 lastPoint = road.roadPoints[road.roadPoints.Count - 1]; //the last roadpoint found until now.
                while (!map.intersections.Exists(x => x.position == lastPoint)) //while there is no intersection equal to the last point in the road, we expand the road.
                {
                    (Vector3, Vector3, bool) next = adjactentEdges[lastPoint].Find(x => x.Item1 == lastPoint);
                    road.roadPoints.Add(next.Item2);
                    lastPoint = road.roadPoints[road.roadPoints.Count - 1];                    
                }

                road.end = map.intersections.Find(x => x.position == lastPoint);

                road.CalculateLength();

                road.start.outgoing.Add(road);
                if (!road.directed) road.end.outgoing.Add(road);

                map.roads.Add(road);
            }
        }

        foreach (Road road in map.roads) //initialization of data used to keep track of road coverage of trajectory generation for the purpose of pruning the map.
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