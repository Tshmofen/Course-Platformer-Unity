﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdParty.QPathFinder.Script
{
    public enum Execution
    {
        Synchronous,
        Asynchronously
    }

    ///
    ///    PathFinder instance uses GraphData to find the shorted path between Nodes
    ///

    public class PathFinder : MonoBehaviour
    {
        private static PathFinder _instance;
        public static PathFinder Instance { get { return _instance; } }

        public GraphData graphData = new GraphData();

        public void Awake()
        {
            _instance = this;
        }

        public void OnDestroy()
        {
            _instance = null;
        }

        /// Finds shortest path between Nodes.
        /// Once the path if found, it will return the path as List of nodes (not positions, but nodes. If you need positions, use FindShortestPathOfPoints).
        /// <returns> Returns list of **Nodes**</returns>
        /// <param name="fromNodeID">Find the path from this node</param>
        /// <param name="toNodeID">Find the path to this node</param>
        /// <param name="executionType">Synchronous is immediate and locks the control till path is found and returns the path.
        /// Asynchronous type runs in coroutines without locking the control. If you have more than 50 Nodes, Asynchronous is recommended</param>
        /// <param name="callback">Callback once the path is found</param>
        public void FindShortestPathOfNodes(int fromNodeID, int toNodeID, Execution executionType, System.Action<List<Node>> callback)
        {
            if (executionType == Execution.Asynchronously)
            {
                if (Logger.CanLogInfo) Logger.LogInfo(" FindShortestPathAsynchronous triggered from " + fromNodeID + " to " + toNodeID, true);
                StartCoroutine(FindShortestPathAsynchonousInternal(fromNodeID, toNodeID, callback));
            }
            else
            {
                if (Logger.CanLogInfo) Logger.LogInfo(" FindShortestPathSynchronous triggered from " + fromNodeID + " to " + toNodeID, true);
                callback(FindShortedPathSynchronousInternal(fromNodeID, toNodeID));
            }
        }

        public int FindNearestNode(Vector3 point)
        {
            float minDistance = float.MaxValue;
            Node nearestNode = null;

            foreach (var node in graphData.nodes)
            {
                if (Vector3.Distance(node.Position, point) < minDistance)
                {
                    minDistance = Vector3.Distance(node.Position, point);
                    nearestNode = node;
                }
            }

            return nearestNode != null ? nearestNode.autoGeneratedID : -1;
        }

        public void EnableNode(int nodeID, bool enable)
        {
            if (graphData == null)
            {
                Debug.LogError("Graph Data not found");
                return;
            }

            Node node = graphData.GetNode(nodeID);
            if (node == null)
            {
                Debug.LogError("Node not found");
                return;
            }
            node.SetAsOpen(enable);
        }

        public void EnablePath(int pathID, bool enable)
        {
            if (graphData == null)
            {
                Debug.LogError("Graph Data not found");
                return;
            }

            Path path = graphData.GetPath(pathID);
            if (path == null)
            {
                Debug.LogError("Path not found");
                return;
            }
            path.isOpen = (enable);
        }

        /*** Protected & Private ***/

        #region PRIVATE

        protected IEnumerator FindShortestPathAsynchonousInternal(int fromNodeID, int toNodeID, System.Action<List<Node>> callback)
        {
            if (callback == null)
                yield break;

            int startPointID = fromNodeID;
            int endPointID = toNodeID;
            bool found = false;

            graphData.ReGenerateIDs();

            Node startPoint = graphData.nodesSorted[startPointID];
            Node endPoint = graphData.nodesSorted[endPointID];

            foreach (var point in graphData.nodes)
            {
                point.heuristicDistance = -1;
                point.previousNode = null;
            }

            List<Node> completedPoints = new List<Node>();
            List<Node> nextPoints = new List<Node>();
            List<Node> finalPath = new List<Node>();

            startPoint.pathDistance = 0;
            startPoint.heuristicDistance = Vector3.Distance(startPoint.Position, endPoint.Position);
            nextPoints.Add(startPoint);

            while (true)
            {
                Node leastCostPoint = null;

                float minCost = 99999;
                foreach (var point in nextPoints)
                {
                    if (point.heuristicDistance <= 0)
                        point.heuristicDistance = Vector3.Distance(point.Position, endPoint.Position) + Vector3.Distance(point.Position, startPoint.Position);

                    if (minCost > point.combinedHeuristic)
                    {
                        leastCostPoint = point;
                        minCost = point.combinedHeuristic;
                    }
                }

                if (leastCostPoint == null)
                    break;

                if (leastCostPoint == endPoint)
                {
                    found = true;
                    Node prevPoint = leastCostPoint;
                    while (prevPoint != null)
                    {
                        finalPath.Insert(0, prevPoint);
                        prevPoint = prevPoint.previousNode;
                    }

                    if (Logger.CanLogInfo)
                    {
                        if (finalPath != null)
                        {
                            string str = "";
                            foreach (var a in finalPath)
                            {
                                str += "=>" + a.autoGeneratedID.ToString();
                            }
                            Logger.LogInfo("Path found between " + fromNodeID + " and " + toNodeID + ":" + str, true);
                        }
                    }
                    callback(finalPath);
                    yield break;
                }

                foreach (var path in graphData.paths)
                {
                    if (path.IDOfA == leastCostPoint.autoGeneratedID
                    || path.IDOfB == leastCostPoint.autoGeneratedID)
                    {
                        if (path.isOneWay)
                        {
                            if (leastCostPoint.autoGeneratedID == path.IDOfB)
                                continue;
                        }

                        if (!path.isOpen)
                            continue;

                        Node otherPoint = path.IDOfA == leastCostPoint.autoGeneratedID ?
                                                graphData.nodesSorted[path.IDOfB] : graphData.nodesSorted[path.IDOfA];

                        if (!otherPoint.IsOpen)
                            continue;

                        if (otherPoint.heuristicDistance <= 0)
                            otherPoint.heuristicDistance = Vector3.Distance(otherPoint.Position, endPoint.Position) + Vector3.Distance(otherPoint.Position, startPoint.Position);

                        if (completedPoints.Contains(otherPoint))
                            continue;

                        if (nextPoints.Contains(otherPoint))
                        {
                            if (otherPoint.pathDistance >
                                (leastCostPoint.pathDistance + path.cost))
                            {
                                otherPoint.pathDistance = leastCostPoint.pathDistance + path.cost;
                                otherPoint.previousNode = leastCostPoint;
                            }
                        }
                        else
                        {
                            otherPoint.pathDistance = leastCostPoint.pathDistance + path.cost;
                            otherPoint.previousNode = leastCostPoint;
                            nextPoints.Add(otherPoint);
                        }
                    }
                }

                nextPoints.Remove(leastCostPoint);
                completedPoints.Add(leastCostPoint);

                yield return null;
            }

            if (!found)
            {
                if (Logger.CanLogWarning) Logger.LogWarning("Path not found between " + fromNodeID + " and " + toNodeID, true);
                callback(null);
                yield break;
            }

            if (Logger.CanLogError) Logger.LogError("Unknown error while finding the path!", true);

            callback(null);
            yield break;
        }

        private List<Node> FindShortedPathSynchronousInternal(int fromNodeID, int toNodeID)
        {
            int startPointID = fromNodeID;
            int endPointID = toNodeID;
            bool found = false;

            graphData.ReGenerateIDs();

            Node startPoint = graphData.nodesSorted[startPointID];
            Node endPoint = graphData.nodesSorted[endPointID];

            foreach (var point in graphData.nodes)
            {
                point.heuristicDistance = -1;
                point.previousNode = null;
            }

            List<Node> completedPoints = new List<Node>();
            List<Node> nextPoints = new List<Node>();
            List<Node> finalPath = new List<Node>();

            startPoint.pathDistance = 0;
            startPoint.heuristicDistance = Vector3.Distance(startPoint.Position, endPoint.Position);
            nextPoints.Add(startPoint);

            while (true)
            {
                Node leastCostPoint = null;

                float minCost = 99999;
                foreach (var point in nextPoints)
                {
                    if (point.heuristicDistance <= 0)
                        point.heuristicDistance = Vector3.Distance(point.Position, endPoint.Position) + Vector3.Distance(point.Position, startPoint.Position);

                    if (minCost > point.combinedHeuristic)
                    {
                        leastCostPoint = point;
                        minCost = point.combinedHeuristic;
                    }
                }

                if (leastCostPoint == null)
                    break;

                if (leastCostPoint == endPoint)
                {
                    found = true;
                    Node prevPoint = leastCostPoint;
                    while (prevPoint != null)
                    {
                        finalPath.Insert(0, prevPoint);
                        prevPoint = prevPoint.previousNode;
                    }

                    if (Logger.CanLogInfo)
                    {
                        if (finalPath != null)
                        {
                            string str = "";
                            foreach (var a in finalPath)
                            {
                                str += "=>" + a.autoGeneratedID.ToString();
                            }
                            Logger.LogInfo("Path found between " + fromNodeID + " and " + toNodeID + ":" + str, true);
                        }
                    }

                    return finalPath;
                }

                foreach (var path in graphData.paths)
                {
                    if (path.IDOfA == leastCostPoint.autoGeneratedID
                    || path.IDOfB == leastCostPoint.autoGeneratedID)
                    {
                        if (path.isOneWay)
                        {
                            if (leastCostPoint.autoGeneratedID == path.IDOfB)
                                continue;
                        }

                        if (!path.isOpen)
                            continue;

                        Node otherPoint = path.IDOfA == leastCostPoint.autoGeneratedID ?
                                                graphData.nodesSorted[path.IDOfB] : graphData.nodesSorted[path.IDOfA];

                        if (!otherPoint.IsOpen)
                            continue;

                        if (otherPoint.heuristicDistance <= 0)
                            otherPoint.heuristicDistance = Vector3.Distance(otherPoint.Position, endPoint.Position) + Vector3.Distance(otherPoint.Position, startPoint.Position);

                        if (completedPoints.Contains(otherPoint))
                            continue;

                        if (nextPoints.Contains(otherPoint))
                        {
                            if (otherPoint.pathDistance >
                                (leastCostPoint.pathDistance + path.cost))
                            {
                                otherPoint.pathDistance = leastCostPoint.pathDistance + path.cost;
                                otherPoint.previousNode = leastCostPoint;
                            }
                        }
                        else
                        {
                            otherPoint.pathDistance = leastCostPoint.pathDistance + path.cost;
                            otherPoint.previousNode = leastCostPoint;
                            nextPoints.Add(otherPoint);
                        }
                    }
                }

                nextPoints.Remove(leastCostPoint);
                completedPoints.Add(leastCostPoint);
            }

            if (!found)
            {
                if (Logger.CanLogWarning) Logger.LogWarning("Path not found between " + fromNodeID + " and " + toNodeID, true);
                return null;
            }

            if (Logger.CanLogError) Logger.LogError("Unknown error while finding the path!", true);
            return null;
        }

        #endregion PRIVATE
    }
}