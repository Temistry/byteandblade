// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// The logical representation of a map. It consists of a list of nodes and a list
    /// of logical coordinates for those nodes.
    /// </summary>
    [Serializable]
    public class Map
    {
        public List<Node> Nodes;
        public List<Coordinate> Path;

        public Map(List<Node> nodes, List<Coordinate> path)
        {
            Nodes = nodes;
            Path = path;
        }

        public Node GetNode(Coordinate point)
        {
            return Nodes.First(x => x.Coordinate.Equals(point));
        }

        public Node GetBossNode()
        {
            try
            {
                return Nodes.First(x => x.Type == NodeType.Boss);
            }
            catch
            {
                Debug.LogWarning("보스 노드를 찾을 수 없습니다. 기본 노드를 반환합니다.");
                // 가장 높은 Y 좌표를 가진 노드를 반환
                if (Nodes != null && Nodes.Count > 0)
                {
                    return Nodes.OrderByDescending(n => n.Coordinate.Y).First();
                }
                return null;
            }
        }

        public float DistanceBetweenFirstAndLastLayers()
        {
            var bossNode = GetBossNode();
            var firstLayerNode = Nodes.First(x => x.Coordinate.Y == 0);

            if (bossNode == null || firstLayerNode == null)
                return 0f;

            return bossNode.Position.y - firstLayerNode.Position.y;
        }

        public bool HasBossNode()
        {
            try
            {
                var bossNode = Nodes.FirstOrDefault(x => x.Type == NodeType.Boss);
                return bossNode != null;
            }
            catch
            {
                return false;
            }
        }
    }
}