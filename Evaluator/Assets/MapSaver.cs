using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapSaver
{
    List<string> vertLines = new List<string>();
    List<string> edgeLines = new List<string>();
    int vertIndex = 0;
    int edgeIndex = 0;
    
    /// <summary>
    /// Saves a map in a vertices and edge file.
    /// </summary>
    /// <param name="map">Map that has to be saved.</param>
    /// <param name="name">Name prefix to save export with.</param>
    public void SaveMap(Map map, string name)
    {
        Refresh();

        //vert has id x and y
        //edge has id vertx, vert y and directedness.
        for (int i = 0; i < map.roads.Count; i ++)
        {
            Road road = map.roads[i];
            int dir = road.directed ? 1 : 0;

            if (road.roadPoints.Count == 2) //if it is an edge.
            {
                if (road.start.id < 0) //The start of the road. If not id'd yet
                {
                    road.start.id = vertIndex;
                    AddVert($"{vertIndex},{road.start.position.x},{road.start.position.z}");
                }
                if (road.end.id < 0) //The end of the road. If not id'd yet
                {
                    road.end.id = vertIndex;
                    AddVert($"{vertIndex},{road.end.position.x},{road.end.position.z}");
                }
                AddEdge($"{edgeIndex},{road.start.id},{road.end.id},{dir}");
            }
            else //if it is a polyline.
            {
                if (road.start.id < 0) //The start of the road. If not id'd yet
                {
                    road.start.id = vertIndex;
                    AddVert($"{vertIndex},{road.start.position.x},{road.start.position.z}");
                    AddEdge($"{edgeIndex},{road.start.id},{vertIndex},{dir}");
                }
                else //if already id'ed intersection.
                {
                    AddEdge($"{edgeIndex},{road.start.id},{vertIndex},{dir}");
                }


                for (int j = 1; j < road.roadPoints.Count - 2; j++) //for road points
                {
                    AddVert($"{vertIndex},{road.roadPoints[j].x},{road.roadPoints[j].z}");
                    AddEdge($"{edgeIndex},{vertIndex - 1},{vertIndex},{dir}");
                }


                //een na laatste
                AddVert($"{vertIndex},{road.roadPoints[road.roadPoints.Count - 2].x},{road.roadPoints[road.roadPoints.Count - 2].z}");

                if (road.end.id < 0) //The end of the road. If not id'd yet
                {
                    road.end.id = vertIndex;
                    AddVert($"{vertIndex},{road.end.position.x},{road.end.position.z}");
                    AddEdge($"{edgeIndex},{vertIndex - 2},{road.end.id},{dir}");
                }
                else //if already id'ed intersection.
                {
                    AddEdge($"{edgeIndex},{vertIndex - 1},{road.end.id},{dir}");
                }
            }
        }

        File.WriteAllLines($"Maps/{name}Vertices.txt", vertLines);
        File.WriteAllLines($"Maps/{name}Edges.txt", edgeLines);
    }

    void AddVert(string vert)
    {
        vertLines.Add(vert);
        vertIndex++;
    }

    void AddEdge(string edge)
    {
        edgeLines.Add(edge);
        edgeIndex++;
    }

    void Refresh()
    {
        vertLines.Clear();
        edgeLines.Clear();
        vertIndex = 0;
        edgeIndex = 0;
    }
}
