using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Image image;
    private void Awake()    {
        Debug.Log("场景" + SceneManager.GetActiveScene().name + "加载完成！");

        SDKManager.Instance.SendSetUnityViewUpToIosView();
    }

    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {


    }

    bool isColor = false;
    public void ChangeColor()
    {
        //Debug.Log("isColor="+ isColor+ " color=" + image.color);
        if (!isColor)
        {
            image.color = new Color(100 / 255.0f, 100 / 255.0f, 100 / 255.0f, 255 / 255.0f);
            isColor = true;
        }
        else
        {
            image.color = new Color(223/255.0f, 111 / 255.0f, 35 / 255.0f, 255 / 255.0f);
            isColor = false;
        }
    }


}