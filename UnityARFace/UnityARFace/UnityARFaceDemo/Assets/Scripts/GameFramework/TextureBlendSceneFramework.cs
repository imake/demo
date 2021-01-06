using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureBlendSceneFramework : MonoBehaviour
{
    private static TextureBlendSceneFramework m_instance;

    public static TextureBlendSceneFramework Instance
    {
        get
        {
            return m_instance;
        }
    }

    private Material material;

    private Texture2D originalTexture;

    public RawImage image;

    public GameObject sphere;

    float original_height;
    float original_width;

    float mouth_height;
    float mouth_width;

    float leftEye_height;
    float leftEye_width;

    float rightEye_height;
    float rightEye_width;

    Texture2D newTexture;

    private void Awake()
    {
        m_instance = this;
    }

    private void Start()
    {
        material = sphere.GetComponent<MeshRenderer>().material;
        originalTexture = material.mainTexture as Texture2D;

        newTexture = Instantiate(originalTexture);

        original_height = newTexture.height;
        original_width = newTexture.width;
    }

    public void TextureBlend(Texture2D mouthTexture, Texture2D leftEyeTexture, Texture2D rightEyeTexture)
    {
        Destroy(newTexture);

        newTexture = Instantiate(originalTexture);

        mouth_height = mouthTexture.height;
        mouth_width = mouthTexture.width;

        for (int i = 0; i < mouth_width; i++)
        {
            for (int j = 0; j < mouth_height; j++)
            {
                float w = 448 + i;
                float h = 336 + j;

                Color wallColor = newTexture.GetPixel((int)w, (int)h);
                Color bulletColor = mouthTexture.GetPixel(i, j);

                if (bulletColor.a<=0.1f)
                {
                    newTexture.SetPixel((int)w, (int)h, wallColor);
                }
                else
                {
                    newTexture.SetPixel((int)w, (int)h, bulletColor);
                }

            }
        }

        leftEye_width = leftEyeTexture.height;
        leftEye_height = leftEyeTexture.width;
        for (int i = 0; i < leftEye_width; i++)
        {
            for (int j = 0; j < leftEye_height; j++)
            {
                float w = 416 + i;
                float h = 544 + j;

                Color wallColor = newTexture.GetPixel((int)w, (int)h);
                Color bulletColor = leftEyeTexture.GetPixel(i, j);

                if (bulletColor.a <= 0.1f)
                {
                    newTexture.SetPixel((int)w, (int)h, wallColor);
                }
                else
                {
                    newTexture.SetPixel((int)w, (int)h, bulletColor);
                }

            }
        }

        rightEye_width = rightEyeTexture.height;
        rightEye_height = rightEyeTexture.width;
        for (int i = 0; i < rightEye_width; i++)
        {
            for (int j = 0; j < rightEye_height; j++)
            {
                float w = 544 + i;
                float h = 544 + j;

                Color wallColor = newTexture.GetPixel((int)w, (int)h);
                Color bulletColor = rightEyeTexture.GetPixel(i, j);

                if (bulletColor.a <= 0.1f)
                {
                    newTexture.SetPixel((int)w, (int)h, wallColor);
                }
                else
                {
                    newTexture.SetPixel((int)w, (int)h, bulletColor);
                }

            }
        }

        newTexture.Apply();

        image.texture = newTexture;

        material.mainTexture = newTexture;
    }

    public void Rotate(int rotate_x, float rotate_y, float rotate_z)
    {
        sphere.transform.rotation = Quaternion.Euler(new Vector3(rotate_x, 90 + rotate_y, rotate_z));
    }
}
