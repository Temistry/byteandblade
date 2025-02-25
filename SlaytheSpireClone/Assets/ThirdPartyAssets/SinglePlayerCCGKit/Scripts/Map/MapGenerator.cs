// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = System.Random;

namespace CCGKit
{
    /// <summary>
    /// This component generates a new random map based on the passed-in configuration.
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private MapConfig config;
#pragma warning restore 649

        private Random rng;

        private static readonly List<NodeType> randomNodes = new List<NodeType>
        {
            NodeType.Enemy,
            NodeType.Rest,
            NodeType.Merchant,
            NodeType.Treasure,
            NodeType.Unknown
        };

        private List<float> layerDistances;
        private List<List<Coordinate>> paths;

        private readonly List<List<Node>> nodes = new List<List<Node>>();

        public Map GenerateMap(Random random)
        {
            rng = random;
            
            Debug.Log("맵 생성 시작");

            GenerateLayerDistances();

            for (var i = 0; i < config.Layers.Count; i++)
                PlaceLayer(i);
            
            // 레이어별 노드 타입 로깅
            for (int i = 0; i < nodes.Count; i++)
            {
                string nodeTypes = string.Join(", ", nodes[i].Select(n => n.Type.ToString()));
                Debug.Log($"레이어 {i} 노드 타입: {nodeTypes}");
            }

            EnsureBossNode();
            
            // 보스 노드 확인
            var lastLayer = nodes[nodes.Count - 1];
            bool hasBoss = lastLayer.Any(n => n.Type == NodeType.Boss);
            Debug.Log($"보스 노드 존재 여부: {hasBoss}");

            GeneratePaths();
            
            // 경로 생성 후 노드 연결 확인
            var nodesList = nodes.SelectMany(x => x).Where(x => x.Incoming.Count > 0 || x.Outgoing.Count > 0).ToList();
            int bossNodesInFinalList = nodesList.Count(n => n.Type == NodeType.Boss);
            Debug.Log($"최종 노드 목록에 포함된 보스 노드 수: {bossNodesInFinalList}");

            RandomizeNodePositions();
            SetupConnections();
            RemoveCrossConnections();

            // 최종 맵 생성
            var finalNodesList = nodes.SelectMany(x => x).Where(x => x.Incoming.Count > 0 || x.Outgoing.Count > 0).ToList();
            int finalBossNodes = finalNodesList.Count(n => n.Type == NodeType.Boss);
            Debug.Log($"최종 맵에 포함된 보스 노드 수: {finalBossNodes}");
            
            return new Map(finalNodesList, new List<Coordinate>());
        }

        private void GenerateLayerDistances()
        {
            var numLayers = config.Layers.Count;
            layerDistances = new List<float>(numLayers);
            foreach (var layer in config.Layers)
                layerDistances.Add(layer.DistanceFromPreviousLayer.GetValue(rng));
        }

        private void PlaceLayer(int index)
        {
            var layer = config.Layers[index];
            var gridWidth = config.GridWidth;
            var layerNodes = new List<Node>(gridWidth);

            var offset = layer.DistanceBetweenNodes * config.GridWidth/2f;

            // 마지막 레이어인지 확인
            bool isLastLayer = (index == config.Layers.Count - 1);

            for (var i = 0; i < gridWidth; i++)
            {
                // 마지막 레이어의 중앙 노드는 보스 노드로 강제 설정
                NodeType nodeType;
                if (isLastLayer && i == gridWidth / 2)
                {
                    nodeType = NodeType.Boss;
                    Debug.Log($"레이어 {index}, 위치 {i}에 보스 노드 강제 배치");
                }
                else
                {
                    nodeType = GetRandomNode(layer);
                }
                
                var node = new Node(nodeType, new Coordinate(i, index))
                {
                    Position = new Vector2(-offset + i * layer.DistanceBetweenNodes, GetDistanceToLayer(index))
                };
                layerNodes.Add(node);
            }

            nodes.Add(layerNodes);
        }

        private NodeType GetRandomNode(Layer layer)
        {
            var enemyChance = (int)(layer.EnemyNodeChance * 10);
            var eliteChance = (int)(layer.EliteNodeChance * 10);
            var restChance = (int)(layer.RestNodeChance * 10);
            var treasureChance = (int)(layer.TreasureNodeChance * 10);
            var merchantChance = (int)(layer.MerchantNodeChance * 10);
            var unknownChance = (int)(layer.UnknownNodeChance * 10);
            var bossChance = (int)(layer.BossNodeChance * 10);
            var totalRatio = enemyChance +
                eliteChance +
                restChance +
                treasureChance +
                merchantChance +
                unknownChance +
                bossChance;
            var x = rng.Next(0, totalRatio);
            if ((x -= enemyChance) < 0)
            {
                return NodeType.Enemy;
            }
            else if ((x -= eliteChance) < 0)
            {
                return NodeType.Elite;
            }
            else if ((x -= restChance) < 0)
            {
                return NodeType.Rest;
            }
            else if ((x -= treasureChance) < 0)
            {
                return NodeType.Treasure;
            }
            else if ((x -= merchantChance) < 0)
            {
                return NodeType.Merchant;
            }
            else if ((x -= unknownChance) < 0)
            {
                return NodeType.Unknown;
            }
            else
            {
                return NodeType.Boss;
            }
        }

        private NodeType GetRandomNode()
        {
            return randomNodes[rng.Next(0, randomNodes.Count)];
        }

        private float GetDistanceToLayer(int index)
        {
            return layerDistances.Take(index+1).Sum();
        }

        private void GeneratePaths()
        {
            var finalNode = GetFinalNode();
            paths = new List<List<Coordinate>>();
            var numStartingNodes = config.NumStartingNodes.GetValue(rng);
            var numPreBossNodes = config.NumPreBossNodes.GetValue(rng);

            var candidateXs = new List<int>();
            for (var i = 0;i < config.GridWidth; i++)
                candidateXs.Add(i);

            candidateXs.Shuffle(rng);

            var preBossXs = candidateXs.Take(numPreBossNodes);
            var preBossPoints = (from x in preBossXs select new Coordinate(x, finalNode.Y-1)).ToList();
            var attempts = 0;

            foreach (var point in preBossPoints)
            {
                var path = Path(point, 0, config.GridWidth);
                path.Insert(0, finalNode);
                paths.Add(path);
                attempts++;
            }

            while(!PathsLeadToAtLeastNDifferentPoints(paths, numStartingNodes) && attempts < 100)
            {
                var randomPreBossPoints = preBossPoints[rng.Next(0, preBossPoints.Count)];
                var path = Path(randomPreBossPoints, 0, config.GridWidth);
                path.Insert(0, finalNode);
                paths.Add(path);
                attempts++;
            }
        }

        private Coordinate GetFinalNode()
        {
            var y = config.Layers.Count-1;
            if (config.GridWidth % 2 == 1)
                return new Coordinate(config.GridWidth/2, y);

            return rng.Next(0, 2) == 0
                ? new Coordinate(config.GridWidth/2, y)
                : new Coordinate(config.GridWidth/2-1, y);
        }

        private bool PathsLeadToAtLeastNDifferentPoints(IEnumerable<List<Coordinate>> paths, int n)
        {
            return (from path in paths select path[path.Count-1].X).Distinct().Count() >= n;
        }

        private List<Coordinate> Path(Coordinate from, int toY, int width, bool firstStepUnconstrained = false)
        {
            if (from.Y == toY)
                return null;

            var direction = from.Y > toY ? -1 : 1;

            var path = new List<Coordinate> { from };
            while (path[path.Count-1].Y != toY)
            {
                var lastPoint = path[path.Count-1];
                var candidateXs = new List<int>();
                if (firstStepUnconstrained && lastPoint.Equals(from))
                {
                    for (var i = 0; i < width; i++)
                        candidateXs.Add(i);
                }
                else
                {
                    candidateXs.Add(lastPoint.X);
                    if (lastPoint.X-1 >= 0) candidateXs.Add(lastPoint.X-1);
                    if (lastPoint.X+1 < width) candidateXs.Add(lastPoint.X+1);
                }

                var nextPoint = new Coordinate(candidateXs[rng.Next(0, candidateXs.Count)], lastPoint.Y + direction);
                path.Add(nextPoint);
            }

            return path;
        }

        private void RandomizeNodePositions()
        {
            for (var index = 0; index < nodes.Count; index++)
            {
                var list = nodes[index];
                var layer = config.Layers[index];
                var distToNextLayer = index + 1 >= layerDistances.Count
                    ? 0f
                    : layerDistances[index + 1];
                var distToPrevLayer = layerDistances[index];

                foreach (var node in list)
                {
                    var xRnd = rng.NextFloatInRange(-0.5f, 0.5f);
                    var yRnd = rng.NextFloatInRange(-1f, 1f);

                    var x = xRnd * layer.DistanceBetweenNodes / 2f;
                    var y = yRnd < 0 ? distToPrevLayer * yRnd / 2f : distToNextLayer * yRnd / 2f;

                    node.Position += new Vector2(x, y) * layer.RandomizePosition;
                }
            }
        }

        private void SetupConnections()
        {
            foreach (var path in paths)
            {
                for (var i = 0; i < path.Count; i++)
                {
                    var node = GetNode(path[i]);

                    if (i > 0)
                    {
                        var nextNode = GetNode(path[i-1]);
                        nextNode.AddIncoming(node.Coordinate);
                        node.AddOutgoing(nextNode.Coordinate);
                    }

                    if (i < path.Count-1)
                    {
                        var prevNode = GetNode(path[i+1]);
                        prevNode.AddOutgoing(node.Coordinate);
                        node.AddIncoming(prevNode.Coordinate);
                    }
                }
            }
        }

        private Node GetNode(Coordinate p)
        {
            if (p.Y >= nodes.Count) return null;
            if (p.X >= nodes[p.Y].Count) return null;

            return nodes[p.Y][p.X];
        }

        private void RemoveCrossConnections()
        {
            for (var i = 0; i < config.GridWidth - 1; i++)
            {
                for (var j = 0; j < config.Layers.Count - 1; j++)
                {
                    var node = GetNode(new Coordinate(i, j));
                    if (node == null || node.HasNoConnections()) continue;

                    var right = GetNode(new Coordinate(i+1, j));
                    if (right == null || right.HasNoConnections()) continue;

                    var top = GetNode(new Coordinate(i, j+1));
                    if (top == null || top.HasNoConnections()) continue;

                    var topRight = GetNode(new Coordinate(i+1, j+1));
                    if (topRight == null || topRight.HasNoConnections()) continue;

                    if (!node.Outgoing.Any(x => x.Equals(topRight.Coordinate))) continue;
                    if (!right.Outgoing.Any(x => x.Equals(top.Coordinate))) continue;

                    node.AddOutgoing(top.Coordinate);
                    top.AddIncoming(node.Coordinate);

                    right.AddOutgoing(topRight.Coordinate);
                    topRight.AddIncoming(right.Coordinate);

                    var rnd = rng.NextFloatInRange(0f, 1f);
                    if (rnd < 0.2f)
                    {
                        node.RemoveOutgoing(topRight.Coordinate);
                        topRight.RemoveIncoming(node.Coordinate);

                        right.RemoveOutgoing(top.Coordinate);
                        top.RemoveIncoming(right.Coordinate);
                    }
                    else if (rnd < 0.6f)
                    {
                        node.RemoveOutgoing(topRight.Coordinate);
                        topRight.RemoveIncoming(node.Coordinate);
                    }
                    else
                    {
                        right.RemoveOutgoing(top.Coordinate);
                        top.RemoveIncoming(right.Coordinate);
                    }
                }
            }
        }

        private void EnsureBossNode()
        {
            // 마지막 레이어 확인
            var lastLayerIndex = config.Layers.Count - 1;
            
            if (lastLayerIndex < 0 || nodes.Count <= lastLayerIndex)
            {
                Debug.LogError("맵 레이어가 올바르게 생성되지 않았습니다.");
                return;
            }
            
            var lastLayer = nodes[lastLayerIndex];
            
            if (lastLayer == null || lastLayer.Count == 0)
            {
                Debug.LogError("마지막 레이어에 노드가 없습니다.");
                return;
            }
            
            // 보스 노드가 있는지 확인
            bool hasBoss = lastLayer.Any(n => n.Type == NodeType.Boss);
            
            if (!hasBoss)
            {
                // 중앙에 가까운 노드를 보스 노드로 변경
                int middleIndex = lastLayer.Count / 2;
                lastLayer[middleIndex].Type = NodeType.Boss;
                Debug.Log("보스 노드가 없어 마지막 레이어에 강제로 추가했습니다.");
                
                // 보스 노드가 제대로 추가되었는지 다시 확인
                hasBoss = lastLayer.Any(n => n.Type == NodeType.Boss);
                if (!hasBoss)
                {
                    Debug.LogError("보스 노드 추가 실패! 모든 노드를 검사합니다.");
                    
                    // 모든 노드 검사
                    bool foundBoss = false;
                    for (int i = nodes.Count - 1; i >= 0; i--)
                    {
                        var layer = nodes[i];
                        for (int j = 0; j < layer.Count; j++)
                        {
                            if (layer[j].Type == NodeType.Boss)
                            {
                                foundBoss = true;
                                Debug.Log($"보스 노드를 레이어 {i}, 인덱스 {j}에서 찾았습니다.");
                                break;
                            }
                        }
                        
                        if (!foundBoss && i == nodes.Count - 1)
                        {
                            // 마지막 레이어의 모든 노드를 보스로 설정 시도
                            for (int j = 0; j < layer.Count; j++)
                            {
                                layer[j].Type = NodeType.Boss;
                                Debug.Log($"레이어 {i}, 인덱스 {j}를 보스 노드로 강제 설정했습니다.");
                            }
                        }
                        
                        if (foundBoss) break;
                    }
                }
            }
        }
    }
}