using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class FacePosition : MonoBehaviour
{
    ARFace face;
    public GameObject nose;
    // Start is called before the first frame update
    void Awake()
    {
        face = GetComponent<ARFace>();
    }

    // Update is called once per frame
    void Update()
    {
        //BattleDataMgr.Instance.submarinePos = 45*(face.leftEye.position.y-0.28f);
        BattleDataMgr.Instance.submarinePos = 2*(10 * nose.transform.position.y-2);
        Debug.Log("坐标=" + BattleDataMgr.Instance.submarinePos);
    }
}
