using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateManager : Singleton<CreateManager>
{
    public List<GameObject> obstacleList;

    /// <summary>
    /// 当前障碍生成位置
    /// </summary>
    private Vector3 curPos;
    public override void Init()
    {
        obstacleList = new List<GameObject>();
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

        GameObject obstacle = PoolManager.Instance.GetObject(GameConst.ObstacleName);
        if (obstacle == null)
        {
            GameObject go = ResourcesManager.Instance.GetResource(PrefabPathConst.ObstaclePath, typeof(GameObject), enResourceType.ScenePrefab, true).content as GameObject;
            obstacle = GameObject.Instantiate(go);
            obstacle.transform.SetParent(AppObjConst.GamePlayObstacleGO.transform);
            obstacle.name = go.name;
            PoolManager.Instance.PushObject(GameConst.ObstacleName, obstacle);
        }
        obstacle.transform.position = curPos;
        obstacleList.Add(obstacle);
    }

    private void SetCreatePosInfo()
    {
        //int posY = Random.Range(-2, 3);

        int seed = (int)System.DateTime.Now.Ticks;
        System.Random random = new System.Random(seed);
        int posY = random.Next(-2, 3);

        curPos = new Vector3(6.5f, posY, 0);
    }

    public void Update()
    {
        if (!BattleDataMgr.Instance.isGameOver)
        {
            return;
        }

        for (int i = 0; i < obstacleList.Count; i++)
        {
            obstacleList[i].transform.Translate(Vector3.left * 5 * Time.deltaTime);
        }
    }

    public void RemoveEntity(GameObject go)
    {
        if (obstacleList.Contains(go))
        {
            obstacleList.Remove(go);
        }
    }
}
