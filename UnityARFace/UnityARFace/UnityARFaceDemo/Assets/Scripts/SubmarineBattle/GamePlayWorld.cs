using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayWorld : MonoBehaviour
{
    private TimerInfo timer_CreateObstacle;

    private GameObject submarineGo;
    void Awake()
    {
        BattleDataMgr.Instance.isGameOver = true;

        CreateManager.Instance.Init();
        timer_CreateObstacle = TimerUtil.GamePlayTimer.AddLoopTimer(1f, OnCreateObstacle);

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
    }

    public void InitBoxCollider(GameObject go)
    {
        BoxCollider2D boxCollider2D = go.GetComponent<BoxCollider2D>();
        if (boxCollider2D == null)
        {
            boxCollider2D = go.AddComponent<BoxCollider2D>();
            boxCollider2D.size=new Vector2(1.4f,1.6f);
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

    private void OnCreateObstacle(TimerInfo obj)
    {
        if (!BattleDataMgr.Instance.isGameOver)
        {
            return;
        }
        CreateManager.Instance.CreateEntity();
    }

    void Update()
    {
        if (!BattleDataMgr.Instance.isGameOver)
        {
            return;
        }

        Vector3 temp;
        float targetScreensZ = Camera.main.WorldToScreenPoint(submarineGo.transform.position).z;

#if UNITY_EDITOR
        temp = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, targetScreensZ));
#else
        if (Input.touchCount >0) {
            temp = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, targetScreensZ));
        }
#endif

        submarineGo.transform.position = new Vector3(submarineGo.transform.position.x, temp.y, submarineGo.transform.position.z);

        List<GameObject> list = CreateManager.Instance.obstacleList;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].transform.position.x < -6.5f)
            {
                PoolManager.Instance.ReclaimActiveObject(list[i].name, list[i]);
                CreateManager.Instance.RemoveEntity(list[i]);
            }
        }
    }

    private void OnDisable()
    {
        if (TimerMgr.Instance != null)
        {
            if (TimerUtil.GamePlayTimer != null)
            {
                TimerUtil.GamePlayTimer.RemoveTimer(timer_CreateObstacle);
            }
        }
    }
}
