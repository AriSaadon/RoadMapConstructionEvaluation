using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackGenerator
{
    RandomMap randomMap = new RandomMap();
    System.Random random = new System.Random();
    MatrixNormal norm;

    const int interpolationLength = 20;
    Vector3[] interpolationRands = new Vector3[interpolationLength];

    public List<List<(Vector3, int)>> GenerateTracks(Map map, int precisionError, int samplingError, int amount)
    {
        norm = null;
        map.roads.ForEach(x => x.popularities.ForEach(y => y = 0));
        List<List<(Vector3, int)>> allTrajectories = new List<List<(Vector3, int)>>();

        for (int i = 0; i < amount; i++)
        {
            List<(Vector3, int)> track = new List<(Vector3, int)>();
            List<Road> usedRoads = new List<Road>();
            track = GenerateTrack(map, precisionError, samplingError, 0);

            allTrajectories.Add(track);
        }
        
        return allTrajectories;
    }

    public List<(Vector3, int)> GenerateTrack(Map map, int precisionError, int samplingError, int samplingVariation)
    {
        Vector3[] path = null;
        Array.ForEach(interpolationRands, x => x = Vector3.zero);

        while (path == null)
        {
            Coordinate source = randomMap.GetRandomCoordinate(randomMap.GetRandomRoad(map));
            //drawer.DrawIntersection(source.location, Color.yellow, "Points", 40);

            Coordinate sink = randomMap.GetRandomCoordinate(randomMap.GetRandomRoad(map));
            while (sink.road == source.road) sink = randomMap.GetRandomCoordinate(randomMap.GetRandomRoad(map));
            //drawer.DrawIntersection(sink.location, Color.green, "Points", 40);

            //draw.DrawIntersection(source.location, Color.red, "CT", 10);
            //draw.DrawIntersection(sink.location, Color.green, "CT", 10);
            path = ShortestPath.AStarShortestPath(map, source, sink);
        }

        //draw.DrawRoad(path, new Color32(96,96,255,255), "GT", false, Vector3.zero);
        //draw.DrawTraj(path, "GT");

        int i = 0;
        float offset = 0;
        float elapsedTime = 0;
        float sigma = precisionError * precisionError + 0.001f;

        List<(Vector3, int)> track = new List<(Vector3, int)> { (path[0], (int)elapsedTime) };
        List<(Vector3, Vector3)> errors = new List<(Vector3, Vector3)> { GetRandom2D(sigma) };

        float sampleDistance = samplingError + UnityEngine.Random.Range(-samplingVariation, samplingVariation);
        elapsedTime += sampleDistance;

        while (i != path.Length - 1)
        {
            float remainingDistance = Vector3.Distance(path[i], path[i + 1]) - offset;

            if (remainingDistance < sampleDistance)
            {
                sampleDistance -= remainingDistance;
                i++;
                offset = 0;
            }
            else
            {
                offset += sampleDistance;
                track.Add((path[i] + (path[i + 1] - path[i]).normalized * offset, (int) elapsedTime));
                errors.Add(GetRandom2D(sigma));

                sampleDistance = samplingError + UnityEngine.Random.Range(-samplingVariation, samplingVariation);
                elapsedTime += sampleDistance;
            }
        }

        track.Add((path[path.Length - 1], (int) elapsedTime));
        errors.Add(GetRandom2D(sigma));

        Vector3[] tr = track.Select(x => x.Item1).ToArray();

        for (int j = 0; j < track.Count; j++)
        {
            tr[j] += errors[j].Item1;
        }

        //draw.DrawRoad(tr, new Color32(96, 96, 255, 255), "Matches", false, Vector3.zero);
        //draw.DrawTraj(tr, "Matches");

        for (int j = 0; j < track.Count; j++)
        {
            tr[j] = track[j].Item1 + errors[j].Item2 * 2;
            track[j] = (tr[j], track[j].Item2);
        }

        //draw.DrawRoad(tr, new Color32(96, 96, 255, 255), "UnmatchedGT", false, Vector3.zero);
        //draw.DrawTraj(tr, "UnmatchedGT");

        return track;
    }

    public double Gauss(double sigma)
    {
        double u1 = random.NextDouble();
        double u2 = random.NextDouble();
        double temp1 = Math.Sqrt(-2 * Math.Log(u1));
        double temp2 = 2 * Math.PI * u2;

        return sigma * (temp1 * Math.Cos(temp2));
    }

    public (Vector3,Vector3) GetRandom2D(float sigmaSquared)
    {
        if (norm == null)
        {
            Matrix<double> m = Matrix<double>.Build.Dense(2, 1);
            m[0, 0] = 0;
            m[1, 0] = 0;

            Matrix<double> v = Matrix<double>.Build.Dense(2, 2);
            v[0, 0] = sigmaSquared;
            v[0, 1] = 0;
            v[1, 0] = 0;
            v[1, 1] = sigmaSquared;

            Matrix<double> k = Matrix<double>.Build.Dense(1, 1);
            k[0, 0] = 1.0;

            norm = new MatrixNormal(m, v, k, random);
        }

        Matrix<double> samp = norm.Sample();
        Vector3 randVector = new Vector3((float)samp[0, 0], 0, (float)samp[1, 0]);
        Vector3 acc = Vector3.zero;

        for(int i = 0; i < interpolationRands.Length; i++)
        {
            if(i != interpolationRands.Length - 1)
            {
                interpolationRands[i] = interpolationRands[i + 1];
            }
            else
            {
                interpolationRands[i] = randVector;
            }
            acc += interpolationRands[i];
        }

        acc = acc / interpolationLength;

        return (randVector, acc);
    }
}
