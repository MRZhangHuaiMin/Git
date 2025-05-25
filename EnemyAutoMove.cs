using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 敌人自动寻路Demo
/// </summary>
public class EnemyAutoMove : MonoBehaviour
{
    public Transform target;
    private List<MapGrid> path;
    private int pathIndex;
    public float moveSpeed = 3f;

    void Start()
    {
        var startGrid = MapMgr.Instance.GetNearestGrid(transform.position);
        var endGrid = MapMgr.Instance.GetNearestGrid(target.position);
        if (startGrid == null || endGrid == null)
        {
            Debug.LogWarning("起点或终点无有效格子");
            return;
        }
        // path = MapMgr.Instance.FindPath(startGrid.ToVector3Int(), endGrid.ToVector3Int());
        // 这里只做示例，寻路函数可后续补全
        pathIndex = 0;
    }

    void Update()
    {
        if (path == null || pathIndex >= path.Count) return;
        Vector3 dest = path[pathIndex].WorldPos;
        transform.position = Vector3.MoveTowards(transform.position, dest, Time.deltaTime * moveSpeed);

        if (Vector3.Distance(transform.position, dest) < 0.1f)
            pathIndex++;
    }
}