/*
    Various code snippets used for the experiments present in our thesis.
    These are for specific cases that used mostly data that we generated ourselves.
    Code is left here mainly for inspirational purposes and as a extra means to understand how to setup own experiments.


    void roadEvo(Map gt, Map cm, Func<Map, Road> randRoad, int r, Color clr)
    {
        List<float> evo = new List<float>();
        List<Vector3> evo2 = new List<Vector3>();
        int count = 200;
        bool t = randRoad.Method.Name == "GetUniformRandomRoad";

        for (int i = 0; i < count; i++)
        {
            Road road = randRoad(gt);
            Coordinate coordinate = rand.GetRandomPointOnRoad(road);

            (float, float) precall = SampleEvaluation3(gt, cm, coordinate, r);
            float f1 = (2 * precall.Item1 * precall.Item2) / (precall.Item1 + precall.Item2 + 0.001f);
            f1 = Mathf.Clamp(f1, 0, 1);
            evo.Add(precall.Item1);
        }

        for (int i = 0; i < evo.Count; i++)
        {
            float avg = 0;
            for (int j = 0; j <= i; j++)
            {
                avg += evo[j];
            }
            avg = avg / (i + 1);
            if (!t || (i % 1) == 0) evo2.Add(new Vector3(i * 1000.0f / (float)count, 0, avg * 1000));
        }

        
        drawer.DrawRoad(evo2.ToArray(), clr, "GT", false, Vector3.zero);
        //drawer.DrawTraj(evo2.ToArray(), "GT");
    }






    void SampleRoadNeighbourhoods(Map gt, Map cm, Road road, Color clr)
    {
        List<Vector3> evo = new List<Vector3>();
        for (int i = 0; i < 500; i++)
        {
            Coordinate coordinate = new Coordinate();

            if (i < 499)
            {
                float loc = i * (road.length / 499);

                float traversed = 0;
                int roadIndex = 0;

                while (traversed + Vector3.Distance(road.roadPoints[roadIndex], road.roadPoints[roadIndex + 1]) < loc)
                {
                    traversed += Vector3.Distance(road.roadPoints[roadIndex], road.roadPoints[roadIndex + 1]);
                    roadIndex++;
                }

                float remainder = loc - traversed;
                Vector3 location = road.roadPoints[roadIndex] + remainder * (road.roadPoints[roadIndex + 1] - road.roadPoints[roadIndex]).normalized;
                coordinate = new Coordinate(road, location, 0, roadIndex, remainder);
            }
            else
            {
                Vector3 eena = road.roadPoints[road.roadPoints.Count - 2];
                Vector3 laaste = road.roadPoints[road.roadPoints.Count - 1];
                Vector3 dir = (laaste - eena).normalized;
                float offset = 0.95f * Vector3.Distance(laaste, eena);
                Vector3 loci = eena + dir * offset;
                coordinate = new Coordinate(road, loci, 0, road.roadPoints.Count - 2, offset);
            }            

            (float, float) precall = SampleEvaluation3(gt, cm, coordinate, 500);
            Debug.Log(precall);
            evo.Add(new Vector3(precall.Item1 * 1000, 0, precall.Item2 * 1000));
        }

        //drawer.DrawRoad(evo.ToArray(), clr, "GT", false, Vector3.zero);
        drawer.DrawTraj(evo.ToArray(), "GT", clr);
    }







    void roadSample(Map gt, Map cm, Road road, int i)
    {
        drawer.Refresh();

        float loc = i * (road.length / 19);

        float traversed = 0;
        int roadIndex = 0;

        while (traversed + Vector3.Distance(road.roadPoints[roadIndex], road.roadPoints[roadIndex + 1]) < loc)
        {
            traversed += Vector3.Distance(road.roadPoints[roadIndex], road.roadPoints[roadIndex + 1]);
            roadIndex++;
        }

        float remainder = loc - traversed;
        Vector3 location = road.roadPoints[roadIndex] + remainder * (road.roadPoints[roadIndex + 1] - road.roadPoints[roadIndex]).normalized;
        Coordinate coordinate = new Coordinate(road, location, 0, roadIndex, remainder);

        drawer.DrawRoads(gt, new Color32(128, 148, 157, 255), new Vector3(0, 0, 0));
        road.line.material.SetColor("_Color", Color.green);

        drawer.DrawRoads(cm, new Color32(195, 172, 165, 255), new Vector3(0, 0, 0));
        (float, float) precall = SampleEvaluation2(gt, cm, coordinate);
        GameObject.Find("Text").GetComponent<Text>().text = precall.ToString();

        Debug.Log(precall);
    }

    void EvaluateLocalNeighbourhood()
    {
        drawer.Refresh();

        Map gt = importMap.ReadMap($"Chicago/Chicago-200-directed-50-100");
        Map cm = importMap.ReadMap($"Kharita/Directed/Chicago-200-50-100");
        
        Camera.main.transform.position = gt.GetCenter() + new Vector3(0, 4900, -500);
        Vector3 offset = new Vector3(0, 0, 0);
        drawer.DrawRoads(gt, new Color32(128, 148, 157, 255), -offset);
        drawer.DrawRoads(cm, new Color32(195, 172, 165, 255), offset);

        (float, float) precall = SampleEvaluation(gt, cm, 100, 20);
        Debug.Log(precall);
        GameObject.Find("Text").GetComponent<Text>().text = precall.ToString();
    }








    void EvaluateRoadGraphSimilarity(int precision, int sampling, int nr)
    {
        drawer.Refresh();

        Vector3 offset = new Vector3(5500, 0, 0);
        Map gt = importMap.ReadMap($"UnprunedAthens");
        Map cm = importMap.ReadMap($"Kharita/Athens");

        mapTrans.MakeUndirectedMap(gt);
        mapTrans.MakeUndirectedMap(cm);

        Camera.main.transform.position = gt.GetCenter() + new Vector3(0, 4900, -500);
        drawer.DrawRoads(gt, Color.blue, -offset);
        drawer.DrawRoads(cm, Color.black, offset);
        
        (float, float) precall = MapEval(gt, cm, 10, 100, 20);

        GameObject.Find("Text").GetComponent<Text>().text = $"Undirected Athens Bundling alg, n={nr}, (pos, samp)=({precision}, {sampling}).\n (prec,recall)={precall}, |CM|/|GT|={cm.length/gt.length}.";
    }








    void GenerateTrajectories(int precision, int sampling, bool dir, int amount)
    {
        drawer.Refresh();

        Map gt = importMap.ReadMap("Utrecht");

        string name = $"{gt.name}-{amount}-{(dir ? "directed" : "undirected")}-{precision}-{sampling}";

        Camera.main.transform.position = gt.GetCenter();

        if (!dir) mapTrans.MakeUndirectedMap(gt);
        else mapTrans.MakeDirectedMap(gt);

        List<List<(Vector3, int)>> tracks = trajGenerator.GenerateTracks(gt, precision, sampling, amount);
        gt.PruneRoads();

        mapSaver.SaveMap(gt, name);
        trajSaver.SaveTracks(tracks, $"../Data/{name}-proj");

        drawer.DrawRoads(gt, Color.black, Vector3.zero);
        //gt.ColorRoads();

        drawer.DrawTrajectories($"{name}-proj", amount, Vector3.zero, Color.magenta, importTraj);
    }







    void ViewMapAndTrajectories(int precision, int sampling)
    {
        drawer.Refresh();

        Map gt = importMap.ReadMap($"Chicago");

        int padX = 2000; int padY = 1000;
        gt.SetBounds(new Rect(gt.bounds.x + padX, gt.bounds.y + padY, gt.bounds.width - padX - 1100, gt.bounds.height - padY - 1000));
        gt.Refresh();

        drawer.DrawTrajectories("chicago_proj", 500, Vector3.zero, Color.magenta, importTraj);

        Camera.main.transform.position = gt.GetCenter() + new Vector3(0, 0, -500);
        drawer.DrawRoads(gt, Color.black, -Vector3.zero);
    }
 */