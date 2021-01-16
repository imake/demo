using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class FacePosition : MonoBehaviour
{
    ARFace face;
    // Start is called before the first frame update
    void Awake()
    {
        face = GetComponent<ARFace>();
    }

    // Update is called once per frame
    void Update()
    {
        BattleDataMgr.Instance.submarinePos = face.leftEye.position.y;
    }
}
