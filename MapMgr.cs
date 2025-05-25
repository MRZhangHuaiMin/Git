using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图管理器（格子管理与寻路）
/// </summary>
public class MapMgr : MonoSingleton<MapMgr>
{
    public Dictionary<Vector3Int, MapGrid> AllGrids = new Dictionary<Vector3Int, MapGrid>();

    public int MaxStepHeight = 1;
    public GridMapType gridType = GridMapType.Square;

    public void LoadMap(string key = "MapGrid")
    {
        AllGrids.Clear();
        var data = DataManager.LoadJson<MapGridData>(key); // 使用Json加载
        if (data != null && data.entries != null)
        {
            foreach (var entry in data.entries)
            {
                var g = entry.value;
                AllGrids[entry.key.ToVector3Int()] = g;
            }
        }
        else
        {
            Debug.LogWarning($"未找到地图数据: {key}");
        }
    }

    public void SaveMap(string key = "MapGrid")
    {
        var data = new MapGridData();
        foreach (var kv in AllGrids)
        {
            var entry = new GridEntry
            {
                key = new SerializableVector3Int(kv.Key),
                value = kv.Value,
            };
            data.entries.Add(entry);
        }
        DataManager.SaveJson(key, data); // 使用Json保存
    }

    public MapGrid GetGrid(Vector3Int pos)
    {
        AllGrids.TryGetValue(pos, out var grid);
        return grid;
    }

    public bool IsWalkable(Vector3Int pos)
    {
        var grid = GetGrid(pos);
        return grid != null && grid.IsWalkable;
    }

    public MapGrid GetNearestGrid(Vector3 worldPos)
    {
        MapGrid nearest = null;
        float minDist = float.MaxValue;
        foreach (var kv in AllGrids)
        {
            float d = Vector3.Distance(worldPos, kv.Value.WorldPos);
            if (d < minDist)
            {
                nearest = kv.Value;
                minDist = d;
            }
        }
        return nearest;
    }

    // ----------- 补全A*寻路 -----------
    public List<MapGrid> FindPath(MapGrid start, Vector3Int endPos)
    {
        if (start == null || !AllGrids.TryGetValue(endPos, out var end)) return null;
        var openSet = new SimplePriorityQueue<MapGrid>();
        var cameFrom = new Dictionary<MapGrid, MapGrid>();
        var gScore = new Dictionary<MapGrid, float>();
        var fScore = new Dictionary<MapGrid, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Vector3.Distance(start.WorldPos, end.WorldPos);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            if (current == end)
            {
                // reconstruct path
                List<MapGrid> path = new();
                while (current != null)
                {
                    path.Add(current);
                    cameFrom.TryGetValue(current, out current);
                }
                path.Reverse();
                return path;
            }

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!neighbor.IsWalkable) continue;
                float tentativeG = gScore[current] + Vector3.Distance(current.WorldPos, neighbor.WorldPos);
                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Vector3.Distance(neighbor.WorldPos, end.WorldPos);
                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    else
                        openSet.UpdatePriority(neighbor, fScore[neighbor]);
                }
            }
        }
        return null;
    }

    private IEnumerable<MapGrid> GetNeighbors(MapGrid grid)
    {
        var dirs = new Vector3Int[]
        {
            new(1,0,0), new(-1,0,0), new(0,0,1), new(0,0,-1)
        };
        foreach (var d in dirs)
        {
            var pos = new Vector3Int(grid.X + d.x, grid.Y + d.y, grid.Z + d.z);
            if (AllGrids.TryGetValue(pos, out var g))
                yield return g;
        }
    }
}