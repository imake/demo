using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TomatoMonoComponent : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag.Equals("Obstacle"))
        {
            Debug.Log("游戏结束！");

            BattleDataMgr.Instance.isGameOver = false;
        }
    }

}
