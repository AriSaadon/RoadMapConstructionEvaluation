using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MapPruner
{
    /// <summary>
    /// Prunes the roads of a map that have not been used in any trajectory generation.
    /// </summary>
    /// <param name="map">The map that we want to prune.</param>
    public void PruneRoads(Map map)
    {
        List<Road> unusedRoads = new List<Road>();
        List<Road> newRoads = new List<Road>();

        foreach (Road road in map.roads) //remove roads that have entirely been unused.
        {
            if (road.popularities.All(y => y == 0)) unusedRoads.Add(road);
        }

        map.RemoveRoads(unusedRoads);
        unusedRoads.Clear();

        foreach (Road road in map.roads) //remove parts of the road that have not been used.
        {
            PruneEdges(map, road, unusedRoads, newRoads);
        }

        map.RemoveRoads(unusedRoads);
        map.roads.AddRange(newRoads);
        map.CalcRoadLength();
    }

    /// <summary>
    /// Helpermethod to Prune a road perfectly based on its popularities.
    /// </summary>
    /// <param name="map">map we are pruning</param>
    /// <param name="road">road we would like to prune in it</param>
    /// <param name="unusedRoads">Roads that will be deleted by PruneRoad method.</param>
    /// <param name="newRoads">Roads that will be made by pruneRoad method.</param>
    public void PruneEdges(Map map, Road road, List<Road> unusedRoads, List<Road> newRoads)
    {
        int startChain = -1; //at which index does are start road end
        int tailChain = road.popularities.Count; //at which index does our end road end

        while (startChain + 1 < road.popularities.Count && road.popularities[startChain + 1] > 0) startChain++;
        while (tailChain - 1 >= 0 && road.popularities[tailChain - 1] > 0) tailChain--;

        if (startChain == road.popularities.Count - 1) return; //road is completely traveled by trajectories

        if (startChain < 1) // road has no start
        {
            //do nothing
        }
        else //road has a start
        {    
            Road startRoad = new Road();
            Intersection startRoadEnd = new Intersection(road.roadPoints[road.roadPoints.Count - 1], road.directed ? 0 : 1);

            startRoad.start = road.start;
            for (int j = 0; j <= startChain; j++)
            {
                startRoad.roadPoints.Add(road.roadPoints[j]);
                startRoad.popularities.Add(road.popularities[j]);
            }
            startRoad.end = startRoadEnd;

            startRoad.directed = road.directed;
            startRoad.traversed = road.traversed;
            startRoad.CalculateLength();


            startRoad.start.outgoing.Add(startRoad);
            if (!startRoad.directed) startRoad.end.outgoing.Add(startRoad);

            newRoads.Add(startRoad);
            map.intersections.Add(startRoadEnd);
        }

        if (tailChain > road.roadPoints.Count - 2) // road has no tail
        {
            //do nothing
        }
        else //road has a tail
        {
            Road tailRoad = new Road();
            Intersection tailRoadStart = new Intersection(road.roadPoints[tailChain], 1);

            tailRoad.start = tailRoadStart;
            for (int j = tailChain; j < road.roadPoints.Count; j++)
            {
                tailRoad.roadPoints.Add(road.roadPoints[j]);
                tailRoad.popularities.Add(road.popularities[j]);
            }
            tailRoad.end = road.end;

            tailRoad.directed = road.directed;
            tailRoad.traversed = road.traversed;
            tailRoad.CalculateLength();

            tailRoad.start.outgoing.Add(tailRoad);
            if (!tailRoad.directed) tailRoad.end.outgoing.Add(tailRoad);

            newRoads.Add(tailRoad);
            map.intersections.Add(tailRoadStart);
        }

        unusedRoads.Add(road);
    }

}

