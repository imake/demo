using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle
{
    /// <summary>
    /// 障碍物的GameObject
    /// </summary>
    public GameObject entity;

    /// <summary>
    /// 当前障碍物的积分
    /// </summary>
    public int integral;

    /// <summary>
    /// 障碍物是否已经穿过潜水艇
    /// </summary>
    public bool isPass = false;

    /// <summary>
    /// 当前障碍物的编号
    /// </summary>
    public int number;

    /// <summary>
    /// 初始化障碍物积分
    /// </summary>
    public void InitIntegral()
    {
        Debug.Log("number=" + number);
        integral = (int)(1 + (number / 10.0f) * 0.1f);
    }
}
