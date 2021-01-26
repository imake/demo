using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMgr : MonoBehaviour
{
    //// Start is called before the first frame update
    //void Start()
    //{
    //    Debug.Log(Screen.width + " ," + Screen.height);
    //    Camera.main.orthographicSize = Screen.height / 100 / 2;
    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    public float initOrthoSize;
    public float initWidth;
    public float initHeight;

    float factWidth;
    float factHeight;

    void Start()
    {
        factWidth = Screen.width;
        factHeight = Screen.height;
        //实际正交视口 = 初始正交视口 * 初始宽高比 / 实际宽高比
        GetComponent<Camera>().orthographicSize = (initOrthoSize * (initWidth / initHeight)) / (factWidth / factHeight);

        //GetComponent<Camera>().fieldOfView = (initOrthoSize * (initWidth / initHeight)) / (factWidth / factHeight);
    }
}
