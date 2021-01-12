using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayWorld : MonoBehaviour
{
    private GameObject submarineGo;

    private int lastRate;

    void Awake()
    {
        BattleDataMgr.Instance.isGameOver = true;

        CreateManager.Instance.Init();

        GameObject go = ResourcesManager.Instance.GetResource(PrefabPathConst.SubmarinePath, typeof(GameObject), enResourceType.ScenePrefab, false).content as GameObject;
        submarineGo = Instantiate(go);
        submarineGo.name = go.name;
        submarineGo.transform.SetParent(AppObjConst.GamePlayGo.transform);
        InitBoxCollider(submarineGo);
        TomatoMonoComponent component = submarineGo.GetComponent<TomatoMonoComponent>();
        if (component == null)
        {
            component = submarineGo.AddComponent<TomatoMonoComponent>();
        }

        CreateManager.Instance.CreateEntity();
    }

    public void InitBoxCollider(GameObject go)
    {
        BoxCollider2D boxCollider2D = go.GetComponent<BoxCollider2D>();
        if (boxCollider2D == null)
        {
            boxCollider2D = go.AddComponent<BoxCollider2D>();
            boxCollider2D.size=new Vector2(1.6f,1.6f);
        }
        boxCollider2D.isTrigger = false;

        Rigidbody2D rigidbody2D = go.GetComponent<Rigidbody2D>();
        if (rigidbody2D == null)
        {
            rigidbody2D = go.AddComponent<Rigidbody2D>();
            rigidbody2D.gravityScale = 0;
            rigidbody2D.mass = 0.1f;
            rigidbody2D.drag = 0;
            rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
            rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        }
    }

    /// <summary>
    /// 生成障碍物规则
    /// </summary>
    private void CreateObstacle()
    {
        if (!BattleDataMgr.Instance.isGameOver)
        {
            return;
        }

        if (BattleDataMgr.Instance.lastObstacle != null&&6.5f - BattleDataMgr.Instance.lastObstacle.entity.transform.position.x >= 5)
        {
            CreateManager.Instance.CreateEntity();
        }
    }

    void Update()
    {
        if (!BattleDataMgr.Instance.isGameOver)
        {
            return;
        }

        Vector3 temp  = Vector3.zero;
        float targetScreensZ = Camera.main.WorldToScreenPoint(submarineGo.transform.position).z;

#if UNITY_EDITOR
        temp = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, targetScreensZ));
#else
        if (Input.touchCount >0) {
            temp = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, targetScreensZ));
        }
#endif

        submarineGo.transform.position = new Vector3(submarineGo.transform.position.x, temp.y, submarineGo.transform.position.z);

        //潜水艇穿过障碍物后，增加积分
        List<Obstacle> list = CreateManager.Instance.obstacleList;
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].isPass)
            {
                if (list[i].entity.transform.position.x < 0)
                {
                    BattleDataMgr.Instance.passObstacleCount++;
                    BattleDataMgr.Instance.integral += list[i].integral;
                    list[i].isPass = true;

                    CalculateSpeed();
                }
            }
        }

        CreateObstacle();
    }

    /// <summary>
    /// 计算速度
    /// </summary>
    private void CalculateSpeed()
    {
        int rate = BattleDataMgr.Instance.passObstacleCount / 10;
        if (lastRate == rate)
        {
            return;
        }
        lastRate = rate;
        BattleDataMgr.Instance.obstacleSpeed = BattleDataMgr.Instance.obstacleSpeed * (1 + rate * 0.1f);
    }

    private void OnDisable()
    {

    }
}
