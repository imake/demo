using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureUtility : Singleton<TextureUtility>
{
    Texture2D newTexture;

    float original_height;
    float original_width;

    float face_height;
    float face_width;

    void Start()
    {
        
    }

    /// <summary>
    /// 把一张纹理贴图贴到另一张的对应位置（默认在中心位置）
    /// </summary>
    /// <param name="originalTexture"></param>
    /// <param name="smallTexture"></param>
    /// <returns></returns>
    public Texture2D BlendTexture(Texture2D originalTexture,Texture2D smallTexture)
    {
        newTexture = originalTexture;

        original_height = newTexture.height;
        original_width = newTexture.width;

        face_height = smallTexture.height;
        face_width = smallTexture.width;

        for (int i = 0; i < face_width; i++)
        {
            for (int j = 0; j < face_height; j++)
            {
                float w = original_width / 2 - face_width / 2 + i;
                float h = original_height / 2 - face_height / 2 + j;

                Color wallColor = newTexture.GetPixel((int)w, (int)h);
                Color bulletColor = smallTexture.GetPixel(i, j);

                newTexture.SetPixel((int)w, (int)h, bulletColor * wallColor);

            }
        }

        newTexture.Apply();
        //newTexture.alphaIsTransparency = true;

        return newTexture;
    }
}
