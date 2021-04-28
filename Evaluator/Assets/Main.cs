using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    ImportMap importMap = new ImportMap();
    MapTransformer mapTrans = new MapTransformer();
    ImportTraj importTraj = new ImportTraj();
    RandomMap rand = new RandomMap();
    Drawer drawer = new Drawer();
    TrajGenerator trajGenerator = new TrajGenerator();
    TrajSaver trajSaver = new TrajSaver();
    SampleNeighbourhood sampleNeighbourhood = new SampleNeighbourhood();
    MapSaver mapSaver = new MapSaver();
    FrameExecuter frameExecuter = new FrameExecuter();

    bool done = false;

    void Update()
    {
        if (!done)
        {
            drawer.Refresh();
            frameExecuter.Add(() => { EvaluateLocalNeighbourhood(); });
        }

        done = true;
        if (done) frameExecuter.Update();
    }

    /// <summary>
    /// And example method used to run an experiment.
    /// </summary>
    void EvaluateLocalNeighbourhood()
    {
        drawer.Refresh();

        Map gt = importMap.ReadMap($"Chicago/Chicago-200-directed-50-100");
        Map cm = importMap.ReadMap($"Kharita/Directed/Chicago-200-50-100");

        Camera.main.transform.position = gt.GetCenter() + new Vector3(0, 4900, -500);
        Vector3 offset = new Vector3(0, 0, 0);
        drawer.DrawRoads(gt, new Color32(128, 148, 157, 255), -offset);
        drawer.DrawRoads(cm, new Color32(195, 172, 165, 255), offset);

        (float, float) precall = NeighbourhoodEval(gt, cm);
        //GameObject.Find("Text").GetComponent<Text>().text = precall.ToString();
        Debug.Log(precall);
    }


    /// <summary>
    /// Evaluates the similarity between two maps using the graph sampling evaluation of biagioni and eriksson.
    /// Parameters are hardcoded, based on the ones we used in the corresponding research.
    /// </summary>
    /// <param name="GT">The ground truth map</param>
    /// <param name="CT">the constructed map.</param>
    /// <param name="amount">the amount of neighbourhood evaluations we average over</param>
    /// <returns>A precision and recall value indicating the similarity between the maps.</returns>
    (float, float) MapEval(Map GT, Map CT, int amount)
    {
        float precision = 0;
        float recall = 0;

        for (int k = 0; k < amount; k++)
        {
            Coordinate originGT = rand.GetRandomPointOnRoad(rand.GetWeightedRandomRoad(GT));
            Coordinate originCT = CT.GetClosestPoint(originGT.location);

            /* An extra step that is sometimes used in the literature when ground truth map is not pruned.
            while (Vector3.Distance(originCT.location, originGT.location) > 50)
            {
                originGT = rand.GetRandomCoordinate(rand.GetWeightedRandomRoad(GT));
                originCT = CT.GetClosestPoint(originGT.location);
            }
            */

            List<Vector3> pointsGT = sampleNeighbourhood.GetNeighbourhood(GT, originGT, 500, 30);
            List<Vector3> pointsCT = sampleNeighbourhood.GetNeighbourhood(CT, originCT, 500, 30);

            MaxFlow flow = new MaxFlow(pointsGT.Count + pointsCT.Count + 2);

            for (int i = 0; i < pointsCT.Count; i++) flow.AddEdge(0, i + 1, 1);

            for (int i = 0; i < pointsCT.Count; i++)
            {
                Vector3 pointCT = pointsCT[i];

                for (int j = 0; j < pointsGT.Count; j++)
                {
                    Vector3 pointGT = pointsGT[j];
                    if (Vector3.Distance(pointCT, pointGT) < 20) flow.AddEdge(i + 1, j + pointsCT.Count + 1, 1);
                }
            }

            for (int i = 0; i < pointsGT.Count; i++) flow.AddEdge(i + pointsCT.Count + 1, 1 + pointsCT.Count + pointsGT.Count, 1);

            float matching = flow.FindMaximumFlow(0, 1 + pointsCT.Count + pointsGT.Count).Item1;
            precision += pointsCT.Count > 0 ? matching / pointsCT.Count : 0;
            recall += pointsGT.Count > 0 ? matching / pointsGT.Count : 0;
        }

        return (precision / amount, recall / amount);
    }


    /// <summary>
    /// Visualizes and evaluates a single random neighbourhood.
    /// </summary>
    /// <param name="GT">Ground truth map</param>
    /// <param name="CT">contructed map</param>
    /// <param name="originDistanceCondition"></param>
    /// <param name="matchDistance"></param>
    /// <returns></returns>
    (float, float) NeighbourhoodEval(Map GT, Map CT)
    {
        Coordinate originGT = rand.GetRandomPointOnRoad(rand.GetWeightedRandomRoad(GT));
        Coordinate originCT = CT.GetClosestPoint(originGT.location);

        /* An extra step that is sometimes used in the literature when ground truth map is not pruned.
        while (Vector3.Distance(originCT.location, originGT.location) > 50)
        {
            originGT = rand.GetRandomCoordinate(rand.GetWeightedRandomRoad(GT));
            originCT = CT.GetClosestPoint(originGT.location);
        }
        */

        drawer.DrawIntersection(originGT.location, Color.blue, "GT", 8);
        drawer.DrawIntersection(originCT.location, Color.red, "CT", 8);

        List<Vector3> pointsGT = sampleNeighbourhood.GetNeighbourhood(GT, originGT, 150, 30);
        List<Vector3> pointsCT = sampleNeighbourhood.GetNeighbourhood(CT, originCT, 150, 30);

        pointsGT.ForEach(x => drawer.DrawIntersection(x, new Color32(96, 96, 255, 255), "GT", 4));
        pointsCT.ForEach(x => drawer.DrawIntersection(x, new Color32(255, 48, 50, 255), "CT", 4));

        MaxFlow flow = new MaxFlow(pointsGT.Count + pointsCT.Count + 2);

        for (int i = 0; i < pointsCT.Count; i++) flow.AddEdge(0, i + 1, 1); // source edges

        for (int i = 0; i < pointsCT.Count; i++) // edges between CT and GT
        {
            Vector3 pointCT = pointsCT[i];

            for (int j = 0; j < pointsGT.Count; j++)
            {
                Vector3 pointGT = pointsGT[j];
                if (Vector3.Distance(pointCT, pointGT) < 20) flow.AddEdge(i + 1, j + pointsCT.Count + 1, 1);
            }
        }

        for (int i = 0; i < pointsGT.Count; i++) flow.AddEdge(i + pointsCT.Count + 1, 1 + pointsCT.Count + pointsGT.Count, 1); // sink edges

        float matching; 
        int[,] graph;
        (matching, graph) = flow.FindMaximumFlow(0, 1 + pointsCT.Count + pointsGT.Count);

        // While we got our score already, we would like to visualize the points that have not been matched for insight into the evaluation.
        bool[] matchedCT = new bool[pointsCT.Count];
        bool[] matchedGT = new bool[pointsGT.Count];
        Array.ForEach(matchedCT, x => x = false);
        Array.ForEach(matchedGT, x => x = false);

        for (int i = 0; i < pointsCT.Count; i ++)
        {
            int ix = i + 1;
            for (int j = 0; j < pointsGT.Count; j++)
            {
                int iy = 1 + pointsCT.Count + j;

                if(graph[iy, ix] == 1)
                {
                    drawer.DrawRoad(new Vector3[] { pointsCT[i], pointsGT[j] }, new Color32(195, 172, 165, 255), "Matches", false, Vector3.zero); //draws a road between matched points.
                    matchedCT[i] = true;
                    matchedGT[j] = true;
                }
            }
        }
        
        for(int i = 0; i < matchedCT.Length; i ++)
        {
            if(!matchedCT[i])
            {
                drawer.DrawIntersection(pointsCT[i], new Color32(255, 48, 50, 255), "UnmatchedCT", 4); //draw unmatched constructed map points indicating precision loss.
            }
        }
        
        for (int i = 0; i < matchedGT.Length; i++)
        {
            if (!matchedGT[i])
            {
                drawer.DrawIntersection(pointsGT[i], new Color32(96, 96, 255, 255), "UnmatchedGT", 4); //draw unmatched ground truth points indicating recall loss.
            }
        }

        float prec = pointsCT.Count > 0 ? matching / pointsCT.Count : 0;
        float recall = pointsGT.Count > 0 ? matching / pointsGT.Count : 0;
        return (prec, recall);
    }
}
