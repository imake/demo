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
}
