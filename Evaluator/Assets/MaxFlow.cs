using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Computes max flow for a provided sink destination graph.
/// Slight modifications made to code from:
/// https://github.com/thormighti/MaximumFlow/blob/master/MaximumFlow/Graph.cs
/// </summary>
class MaxFlow
{
    int vertice;
    int[,] matrix;
    public MaxFlow(int vertice)
    {
        this.vertice = vertice;
        matrix = new int[vertice, vertice];
    }
    public void AddEdge(int source, int destination, int weight)
    {
        matrix[source, destination] = weight;
    }
    // checking if path exist with bfs
    public bool DoesPathExist(int[,] residualGraph, int source, int destination, int[] parent)
    {
        bool pathFound = false;
        // creating an array to check those already visited 
        bool[] visited = new bool[vertice];

        Queue<int> Q = new Queue<int>();
        // inserting the source and marking it as visited 
        Q.Enqueue(source);
        parent[source] = 1;   // marking this
        visited[source] = true;
        while (Q.Count != 0)
        {
            int node = Q.Dequeue();
            // visiting all the adjacents nodes 
            for (int i = 0; i < vertice; i++)
            {
                if (visited[i] == false && residualGraph[node, i] > 0)  // residual capacity is strongly positive
                {
                    Q.Enqueue(i);
                    parent[i] = node;
                    visited[i] = true;
                }
            }
        }
        // checking if BFS is reached 
        pathFound = visited[destination];
        return pathFound;
    }
    public (int, int[,]) FindMaximumFlow(int source, int destination)
    {
        int[,] residualGraph = new int[vertice, vertice];
        // initiatilize Residual graph same as the Original graph
        for (int i = 0; i < residualGraph.GetLength(0); i++)
        {
            for (int j = 0; j < residualGraph.GetLength(1); j++)
            {
                residualGraph[i, j] = matrix[i, j];
            }
        }
        // parent var to store path of source to sink
        int[] parent = new int[vertice];
        int maxFlow = 0; // initializing the maximum flow
        while (DoesPathExist(residualGraph, source, destination, parent))
        {
            //finding the capacity which can be passed into parent
            int flow = int.MaxValue;
            int T = destination;
            while (T != source) // no self loop
            {
                int S = parent[T];
                flow = Math.Min(flow, residualGraph[S, T]); // getting the min
                T = S;

            }

            //update the residual graph
            //reduce the capacity of forward edge by flow
            // reduce the capacity of backward edge by flow
            T = destination;
            while (T != source)
            {
                int S = parent[T];
                residualGraph[S, T] -= flow; // residual capacity from source 
                residualGraph[T, S] += flow; // residual capacity from destination
                T = S;
            }
            maxFlow += flow;
        }

        return (maxFlow, residualGraph);
    }
}
