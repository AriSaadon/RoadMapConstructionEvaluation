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
    ImportTracjectory importTraj = new ImportTracjectory();
    RandomMap rand = new RandomMap();
    DrawMap drawer = new DrawMap();
    TrackGenerator trackGenerator = new TrackGenerator();
    TrackSaver trackSaver = new TrackSaver();
    SampleNeighbourhood sampleNeighbourhood = new SampleNeighbourhood();
    MapSaver mapSaver = new MapSaver();
    MapTester mapTester = new MapTester();
    FrameExecuter frameExecuter = new FrameExecuter();
    bool done = false;

    void Update()
    {

        if(!done)
        {            
            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    //int precision = i * 50;
                    //int sampling = 50 + (j * 50);
                    frameExecuter.Add(() => { image(); });
                }
            }            

            //frameExecuter.Add(() => { drawer.Refresh(); });          
        }

        if (Input.GetKeyDown(KeyCode.Q)) ScreenCapture.CaptureScreenshot("track", 8);
        if(done) frameExecuter.Update();
        done = true;
    }

    /// <summary>
    /// Exports results
    /// </summary>
    /// 
    void image()
    {
        drawer.Refresh();

        Map gt = importMap.ReadMap("UnprunedUtrecht");

        Map cm;
        float min = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int precision = i * 50;
                int sampling = 50 + (j * 50);

                cm = importMap.ReadMap($"Utrecht/Utrecht-100-directed-{precision}-{sampling}");
                min += cm.length / gt.length;

                cm = importMap.ReadMap($"Utrecht/Utrecht-200-directed-{precision}-{sampling}");
                min += cm.length / gt.length;
            }
        }

        Debug.Log(min / 18);
    }

    void Export(int precision, int sampling, int nr)
    {
        drawer.Refresh();

        Vector3 offset = new Vector3(2500, 0, 0);
        Map gt = importMap.ReadMap($"Athens/Athens-{nr}-undirected-{precision}-{sampling}");
        Map cm = importMap.ReadMap($"Bundle/Athens-{nr}-undirected-{precision}-{sampling}-proj_");

        Camera.main.transform.position = gt.GetCenter() + new Vector3(0, 500, -500);
        drawer.DrawRoads(gt, Color.blue, -offset);
        drawer.DrawRoads(cm, Color.black, offset);
        drawer.DrawTrajectories($"A-proj/Athens-{nr}-undirected-{precision}-{sampling}-proj", nr, Vector3.zero, Color.magenta, importTraj);
        

        (float, float) precall = MapEval(gt, cm, 200, 100, 20);

        GameObject.Find("Text").GetComponent<Text>().text = $"Undirected Athens Bundling alg, n={nr}, (pos, samp)=({precision}, {sampling}).\n (prec,recall)={precall}, |CM|/|GT|={cm.length/gt.length}.";
        ScreenCapture.CaptureScreenshot($"undirected-Bundle-Athens-{nr}-{precision}-{sampling}.png", 4);
    }

    void Gen(int precision, int sampling, bool dir, int amount)
    {
        drawer.Refresh();

        Map gt = importMap.ReadMap("Utrecht");

        string name = $"{gt.name}-{amount}-{(dir ? "directed" : "undirected")}-{precision}-{sampling}";

        //int padX = 2000; int padY = 1000;
        //gt.SetBounds(new Rect(gt.bounds.x + padX, gt.bounds.y + padY, gt.bounds.width - padX - 1100, gt.bounds.height - padY - 1000));
        Camera.main.transform.position = gt.GetCenter();

        if (!dir) mapTrans.MakeUndirectedMap(gt);
        else mapTrans.MakeDirectedMap(gt);

        //mapTester.TestDuplicates(gt);        
        //Debug.Log($"{gt.name} is {gt.length}");

        List<List<(Vector3, int)>> tracks = trackGenerator.GenerateTracks(gt, precision, sampling, amount);
        gt.PruneRoads();

        //mapTester.TestDuplicates(gt);
        //Debug.Log($"Pruned {gt.name} is {gt.length}");

        mapSaver.SaveMap(gt, name);
        trackSaver.SaveTracks(tracks, $"../Data/{name}-proj");

        drawer.DrawRoads(gt, Color.black, Vector3.zero);
        //gt.ColorRoads();

        drawer.DrawTrajectories($"{name}-proj", amount, Vector3.zero, Color.magenta, importTraj);
        //drawer.DrawTrajectories($"{gt.name}-proj", 128, Vector3.zero, Color.blue, importTraj);       
    }

    void View(int precision, int sampling)
    {
        drawer.Refresh();

        Vector3 offset = Vector3.zero;
        Map gt = importMap.ReadMap($"Chicago");

        int padX = 2000; int padY = 1000;
        gt.SetBounds(new Rect(gt.bounds.x + padX, gt.bounds.y + padY, gt.bounds.width - padX - 1100, gt.bounds.height - padY - 1000));

        //int padX = 300; int padY = 1500;
        //gt.SetBounds(new Rect(gt.bounds.x + padX, gt.bounds.y + padY, gt.bounds.width - padX, gt.bounds.height - padY - 600));

        drawer.DrawTrajectories("chicago_proj", 500, Vector3.zero, Color.magenta, importTraj);

        //Map ct = importMap.ReadMap($"Kharita{precision}-{sampling}");
        // mapTrans.MakeUndirectedMap(gt);
        // mapTrans.MakeUndirectedMap(ct);

        Camera.main.transform.position = gt.GetCenter() + new Vector3(0, 0, -500);
        drawer.DrawRoads(gt, Color.black, -offset);
        // drawer.DrawRoads(ct, Color.black, offset);
        //
        gt.Refresh();
        Debug.Log(gt.length);
       // (float, float) precall = SampleEvaluation(gt, ct, 200, 100, 20);
       // Debug.Log(precall);
    }

    /// <summary>
    /// Evaluates a single map.
    /// </summary>
    (float, float) MapEval(Map GT, Map CT, int amount, int originDistanceCondition, float matchDistance)
    {
        float prec = 0;
        float recall = 0;
        for (int k = 0; k < amount; k++)
        {
            Coordinate originGT = rand.GetRandomCoordinate(rand.GetRandomRoad(GT));
            Coordinate originCT = CT.GetClosestPoint(originGT.location);

            /* Only with unpruned map.
            while (Vector3.Distance(originCT.location, originGT.location) > originDistanceCondition)
            {
                originGT = rand.GetRandomCoordinate(rand.GetRandomRoad(GT));
                originCT = CT.GetClosestPoint(originGT.location);
            }
            */

            List<Vector3> pointsGT = sampleNeighbourhood.Sample(GT, originGT, 500, 30);
            List<Vector3> pointsCT = sampleNeighbourhood.Sample(CT, originCT, 500, 30);

            MaxFlow flow = new MaxFlow(pointsGT.Count + pointsCT.Count + 2);

            for (int i = 0; i < pointsCT.Count; i++) flow.AddEdge(0, i + 1, 1);

            for (int i = 0; i < pointsCT.Count; i++)
            {
                Vector3 pointCT = pointsCT[i];

                for (int j = 0; j < pointsGT.Count; j++)
                {
                    Vector3 pointGT = pointsGT[j];
                    if (Vector3.Distance(pointCT, pointGT) < matchDistance) flow.AddEdge(i + 1, j + pointsCT.Count + 1, 1);
                }
            }

            for (int i = 0; i < pointsGT.Count; i++) flow.AddEdge(i + pointsCT.Count + 1, 1 + pointsCT.Count + pointsGT.Count, 1);

            float matching = flow.FindMaximumFlow(0, 1 + pointsCT.Count + pointsGT.Count).Item1;
            prec += pointsCT.Count > 0 ? matching / pointsCT.Count : 0;
            recall += pointsGT.Count > 0 ? matching / pointsGT.Count : 0;
        }

        return (prec / amount, recall / amount);
    }

    /// <summary>
    /// Visualizes and evaluates a single sample neighbourhood evaluation.
    /// </summary>
    (float, float) SampleEvaluation(Map GT, Map CT, int amount, int originDistanceCondition, float matchDistance)
    {
        Coordinate originGT = rand.GetRandomCoordinate(rand.GetRandomRoad(GT));
        Coordinate originCT = CT.GetClosestPoint(originGT.location);

        //while (Vector3.Distance(originCT.location, originGT.location) > originDistanceCondition)
        //{
            originGT = rand.GetRandomCoordinate(rand.GetRandomRoad(GT));
            originCT = CT.GetClosestPoint(originGT.location);
        //}

        drawer.DrawIntersection(originGT.location, new Color32(79, 253, 112, 255), "GT", 8);
        drawer.DrawIntersection(originCT.location, new Color32(79, 253, 112, 255), "CT", 8);

        List<Vector3> pointsGT = sampleNeighbourhood.Sample(GT, originGT, 150, 30);
        List<Vector3> pointsCT = sampleNeighbourhood.Sample(CT, originCT, 150, 30);

        pointsGT.ForEach(x => drawer.DrawIntersection(x, new Color32(96, 96, 255, 255), "GT", 4));
        pointsCT.ForEach(x => drawer.DrawIntersection(x, new Color32(255, 48, 50, 255), "CT", 4));

        MaxFlow flow = new MaxFlow(pointsGT.Count + pointsCT.Count + 2);

        for (int i = 0; i < pointsCT.Count; i++) flow.AddEdge(0, i + 1, 1);

        for (int i = 0; i < pointsCT.Count; i++)
        {
            Vector3 pointCT = pointsCT[i];

            for (int j = 0; j < pointsGT.Count; j++)
            {
                Vector3 pointGT = pointsGT[j];
                if (Vector3.Distance(pointCT, pointGT) < matchDistance) flow.AddEdge(i + 1, j + pointsCT.Count + 1, 1);
            }
        }

        for (int i = 0; i < pointsGT.Count; i++) flow.AddEdge(i + pointsCT.Count + 1, 1 + pointsCT.Count + pointsGT.Count, 1);

        float matching; 
        int[,] graph;
        (matching, graph) = flow.FindMaximumFlow(0, 1 + pointsCT.Count + pointsGT.Count);

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
                    drawer.DrawRoad(new Vector3[] { pointsCT[i], pointsGT[j] }, new Color32(195, 172, 165, 255), "Matches", false, Vector3.zero);
                    matchedCT[i] = true;
                    matchedGT[j] = true;
                }
            }
        }
        
        for(int i = 0; i < matchedCT.Length; i ++)
        {
            if(!matchedCT[i])
            {
                drawer.DrawIntersection(pointsCT[i], Color.yellow, "UnmatchedCT", 7);
            }
        }
        
        for (int i = 0; i < matchedGT.Length; i++)
        {
            if (!matchedGT[i])
            {
                drawer.DrawIntersection(pointsGT[i], Color.magenta, "UnmatchedGT", 7);
            }
        }

        float prec = pointsCT.Count > 0 ? matching / pointsCT.Count : 0;
        float recall = pointsGT.Count > 0 ? matching / pointsGT.Count : 0;
        return (prec, recall);
    }

}
