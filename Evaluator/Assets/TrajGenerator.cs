using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class used to generate trajectories.
/// </summary>
public class TrajGenerator
{
    RandomMap randomMap = new RandomMap();
    System.Random random = new System.Random();
    MatrixNormal norm;

    const int interpolationLength = 20;
    Vector3[] interpolationRands = new Vector3[interpolationLength];

    /// <summary>
    /// Generates a random trajectory set for a map.
    /// </summary>
    /// <param name="map">The map we want to generate trajectories on.</param>
    /// <param name="precisionError">The precision error we want the trajectories of our set to contain.</param>
    /// <param name="samplingError">The sampling error we want the trajectories of our set to contain.</param>
    /// <param name="amount">The amount of random trajectories we want to generate.</param>
    /// <returns></returns>
    public List<List<(Vector3, int)>> GenerateTracks(Map map, int precisionError, int samplingError, int amount)
    {
        norm = null; //reset our distribution matrix.
        map.roads.ForEach(x => x.popularities.ForEach(y => y = 0)); //reset how often the edges of a road has been traversed.
        List<List<(Vector3, int)>> allTrajectories = new List<List<(Vector3, int)>>();

        for (int i = 0; i < amount; i++)
        {
            allTrajectories.Add(GenerateTrack(map, precisionError, samplingError));
        }
        
        return allTrajectories;
    }

    /// <summary>
    /// Helper function that generates a single trajectory.
    /// </summary>
    /// <param name="map">The map we want to generate our trajectory on.</param>
    /// <param name="precisionError">The amount of precision error we want our trajectory to exhibit.</param>
    /// <param name="samplingError">The amount of sampling error we want our trajectory to exhibit.</param>
    /// <returns>A list of location, time tuples.</returns>
    public List<(Vector3, int)> GenerateTrack(Map map, int precisionError, int samplingError)
    {
        Vector3[] path = null;
        Array.ForEach(interpolationRands, x => x = Vector3.zero);

        while (path == null)
        {
            Coordinate source = randomMap.GetRandomPointOnRoad(randomMap.GetWeightedRandomRoad(map));

            Coordinate sink = randomMap.GetRandomPointOnRoad(randomMap.GetWeightedRandomRoad(map));
            while (sink.road == source.road) sink = randomMap.GetRandomPointOnRoad(randomMap.GetWeightedRandomRoad(map)); //if source and sink are on the same road we reroll.

            path = ShortestPath.AStarShortestPath(map, source, sink);
        }

        int i = 0;
        float offset = 0;
        float elapsedTime = 0;
        float sigma = precisionError * precisionError + 0.001f;

        List<(Vector3, int)> track = new List<(Vector3, int)> { (path[0], (int)elapsedTime) };
        List<(Vector3, Vector3)> errors = new List<(Vector3, Vector3)> { GetRandom2D(sigma) };

        // We only use simple sampling rates that do not make use of speeds.
        // This would be the location however to implement such an advanced method of trajectory generation.
        float sampleDistance = samplingError;
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

                sampleDistance = samplingError;
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

        for (int j = 0; j < track.Count; j++)
        {
            tr[j] = track[j].Item1 + errors[j].Item2 * 2;
            track[j] = (tr[j], track[j].Item2);
        }

        return track;
    }

    /// <summary>
    /// deprecated and unused random 2d error generation.
    /// </summary>
    public double Gauss(double sigma)
    {
        double u1 = random.NextDouble();
        double u2 = random.NextDouble();
        double temp1 = Math.Sqrt(-2 * Math.Log(u1));
        double temp2 = 2 * Math.PI * u2;

        return sigma * (temp1 * Math.Cos(temp2));
    }

    /// <summary>
    /// Gets a random 2d error.
    /// </summary>
    /// <param name="sigmaSquared">The square of the sigma we want for our error.</param>
    /// <returns></returns>
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

        //We average the result of multiple subsequent points to smoothen the trajectories here.
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
