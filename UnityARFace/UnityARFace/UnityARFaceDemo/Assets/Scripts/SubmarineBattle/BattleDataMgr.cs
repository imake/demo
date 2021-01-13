using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleDataMgr : Singleton<BattleDataMgr>
{
    /// <summary>
    /// 游戏是否结束
    /// </summary>
    public bool isGameOver = false;

    /// <summary>
    /// 障碍物移动速度
    /// </summary>
    public float obstacleSpeed = 5f;

    /// <summary>
    /// 积分
    /// </summary>
    public int integral = 0;

    /// <summary>
    /// 障碍物间隔距离
    /// </summary>
    public float obstacleDistance = 2f;

    /// <summary>
    /// 当前生成过的障碍物数量
    /// </summary>
    public int obstacleCount = 0;

    /// <summary>
    /// 移动过的障碍物数量
    /// </summary>
    public int passObstacleCount = 0;

    /// <summary>
    /// 障碍物位置
    /// </summary>
    public float obstaclePosition = 0;

    /// <summary>
    /// 最后生成的障碍物
    /// </summary>
    public Obstacle lastObstacle;

    /// <summary>
    /// 经过多少个障碍物积分增加一次
    /// </summary>
    public int integralAddSubNumber = 20;

    /// <summary>
    /// 积分增加速率
    /// </summary>
    public float integralAddRate = 0.05f;

    /// <summary>
    /// 潜水艇位置
    /// </summary>
    public float submarinePos = 1;

    public void ResetData()
    {
        isGameOver = false;
        obstacleSpeed = 5f;
        integral = 0;
        obstacleDistance = 2f;
        obstacleCount = 0;
        passObstacleCount = 0;
        obstaclePosition = 0;
        lastObstacle = null;
        submarinePos = 1;
    }
}
