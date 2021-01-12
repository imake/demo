using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateManager : Singleton<CreateManager>
{
    public List<Obstacle> obstacleList;

    /// <summary>
    /// 当前障碍生成位置
    /// </summary>
    private Vector3 curPos;
    public override void Init()
    {
        obstacleList = new List<Obstacle>();
        InitData();
    }

    private void InitData()
    {
        AppObjConst.GamePlayObstacleGO = new GameObject(AppObjConst.GamePlayObstacleGOName);
        AppObjConst.GamePlayObstacleGO.transform.SetParent(AppObjConst.GamePlayGo.transform);

        PoolManager.Instance.PushData(GameConst.ObstacleName);
    }

    public void CreateEntity()
    {
        SetCreatePosInfo();

        GameObject obstacleGo = PoolManager.Instance.GetObject(GameConst.ObstacleName);
        if (obstacleGo == null)
        {
            GameObject go = ResourcesManager.Instance.GetResource(PrefabPathConst.ObstaclePath, typeof(GameObject), enResourceType.ScenePrefab, true).content as GameObject;
            obstacleGo = GameObject.Instantiate(go);
            obstacleGo.transform.SetParent(AppObjConst.GamePlayObstacleGO.transform);
            obstacleGo.name = go.name;
            PoolManager.Instance.PushObject(GameConst.ObstacleName, obstacleGo);
        }
        obstacleGo.transform.position = curPos;

        //障碍物数量加一
        BattleDataMgr.Instance.obstacleCount++;

        Obstacle obstacle = new Obstacle();
        obstacle.number = BattleDataMgr.Instance.obstacleCount;
        obstacle.entity = obstacleGo;
        obstacle.InitIntegral();

        BattleDataMgr.Instance.lastObstacle = obstacle;

        obstacleList.Add(obstacle);
    }

    private void SetCreatePosInfo()
    {
        int seed = (int)System.DateTime.Now.Ticks;
        System.Random random = new System.Random(seed);
        int posY = random.Next(-2, 3);
        BattleDataMgr.Instance.obstaclePosition = posY;

        curPos = new Vector3(6.5f, BattleDataMgr.Instance.obstaclePosition, 0);
    }

    public void Update()
    {
        if (!BattleDataMgr.Instance.isGameOver)
        {
            return;
        }

        for (int i = 0; i < obstacleList.Count; i++)
        {
            obstacleList[i].entity.transform.Translate(Vector3.left * BattleDataMgr.Instance.obstacleSpeed * Time.deltaTime);
        }

        //障碍物出界回收
        for (int i = 0; i < obstacleList.Count; i++)
        {
            if (obstacleList[i].entity.transform.position.x < -6.5f)
            {
                PoolManager.Instance.ReclaimActiveObject(obstacleList[i].entity.name, obstacleList[i].entity);
                RemoveEntity(obstacleList[i]);
            }
        }
    }

    public void RemoveEntity(Obstacle obstacle)
    {
        if (obstacleList.Contains(obstacle))
        {
            obstacleList.Remove(obstacle);
        }
    }

    public void RemoveAllEntity()
    {
        obstacleList.Clear();
        PoolManager.Instance.Destory();
    }
}
