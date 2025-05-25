using UnityEngine;
using System.Collections;

/// <summary>
/// 战斗角色控制器
/// </summary>
[RequireComponent(typeof(Animator))]
public class BattleCharacterController : MonoBehaviour
{
    public string CharacterName;
    public bool IsPlayer;
    public int HP = 100;
    public int MaxHP = 100;
    public int Attack = 20;
    public float MoveSpeed = 3f;
    public float AttackRange = 1.5f;
    public bool IsAlive => HP > 0;

    public MapGrid CurrentGrid { get; private set; }

    private Animator anim;

    public void Setup(string name, bool isPlayer, MapGrid grid)
    {
        CharacterName = name;
        IsPlayer = isPlayer;
        CurrentGrid = grid;
        anim = GetComponent<Animator>();
        transform.position = grid.WorldPos;
    }

    public IEnumerator MoveToGrid(Vector3Int gridPos)
    {
        // 假定MapMgr.Instance有A*寻路接口，返回格子列表
        var path = MapMgr.Instance.FindPath(CurrentGrid, gridPos);
        foreach (var g in path)
        {
            Vector3 dest = g.WorldPos;
            anim.SetBool("Run", true);
            while (Vector3.Distance(transform.position, dest) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, dest, Time.deltaTime * MoveSpeed);
                yield return null;
            }
            CurrentGrid = g;
        }
        anim.SetBool("Run", false);
    }

    public IEnumerator CastSkill(BattleCharacterController target, GameObject effectPrefab)
    {
        anim.SetTrigger("Skill");
        yield return new WaitForSeconds(0.3f);
        // 播放特效
        var effect = Instantiate(effectPrefab, target.transform.position + Vector3.up * 1f, Quaternion.identity);
        Destroy(effect, 1.5f);
        // 受击动画
        target.anim.SetTrigger("Hit");
        target.HP -= this.Attack;
        Debug.Log($"{CharacterName} 对 {target.CharacterName} 施放技能，造成 {Attack} 伤害，{target.CharacterName} 剩余HP：{target.HP}");
        yield return new WaitForSeconds(0.5f);
        if (target.HP <= 0)
        {
            target.anim.SetTrigger("Dead");
            yield return new WaitForSeconds(0.5f);
            Destroy(target.gameObject);
        }
    }
}