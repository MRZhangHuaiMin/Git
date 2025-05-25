using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 地图寻路演示
/// </summary>
public class GameDemo : MonoBehaviour
{
    public Transform player;
    public Transform aim;
    private List<MapGrid> path;

    void Start()
    {
        var startGrid = MapMgr.Instance.GetNearestGrid(player.position);
        var endGrid = MapMgr.Instance.GetNearestGrid(aim.position);

        if (startGrid == null || endGrid == null)
        {
            Debug.LogWarning("起点或终点无有效格子");
            return;
        }
        // path = MapMgr.Instance.FindPath(startGrid.ToVector3Int(), endGrid.ToVector3Int());
        // 这里只做示例，寻路函数可后续补全

        // 路径可视化
        for (int i = 1; path != null && i < path.Count; i++)
        {
            Debug.DrawLine(path[i - 1].WorldPos, path[i].WorldPos, Color.red, 10f);
        }
    }
}