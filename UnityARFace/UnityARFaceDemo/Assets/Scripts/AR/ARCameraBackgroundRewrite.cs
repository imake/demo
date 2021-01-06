using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation.Samples
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(ARCameraManager))]
    public class ARCameraBackgroundRewrite : MonoBehaviour
    {
        public RawImage rawImage;

        /// <summary>
        /// Name for the custom rendering command buffer.
        /// </summary>
        const string k_CustomRenderPassName = "AR Background Pass (LegacyRP)";

        /// <summary>
        /// Name of the main texture parameter for the material
        /// </summary>
        const string k_MainTexName = "_MainTex";

        /// <summary>
        /// Name of the shader parameter for the display transform matrix.
        /// </summary>
        const string k_DisplayTransformName = "_UnityDisplayTransform";

        /// <summary>
        /// Property ID for the shader parameter for the display transform matrix.
        /// </summary>
        static readonly int k_DisplayTransformId = Shader.PropertyToID(k_DisplayTransformName);

        /// <summary>
        /// The Property ID for the shader parameter for the forward vector's scaled length.
        /// </summary>
        static readonly int k_CameraForwardScaleId = Shader.PropertyToID("_UnityCameraForwardScale");

        /// <summary>
        /// The camera to which the projection matrix is set on each frame event.
        /// </summary>
        Camera m_Camera;

        /// <summary>
        /// The camera manager from which frame information is pulled.
        /// </summary>
        ARCameraManager m_CameraManager;

        /// <summary>
        /// Command buffer for any custom rendering commands.
        /// </summary>
        CommandBuffer m_CommandBuffer;

        /// <summary>
        /// The previous clear flags for the camera, if any.
        /// </summary>
        CameraClearFlags? m_PreviousCameraClearFlags;

        /// <summary>
        /// Whether background rendering is enabled.
        /// </summary>
        bool m_BackgroundRenderingEnabled;

        /// <summary>
        /// The current <c>Material</c> used for background rendering.
        /// </summary>
        public Material material => defaultMaterial;

        /// <summary>
        /// The default material for rendering the background.
        /// </summary>
        /// <value>
        /// The default material for rendering the background.
        /// </value>
        Material defaultMaterial => m_CameraManager.cameraMaterial;

        /// <summary>
        /// Whether to use the legacy rendering pipeline.
        /// </summary>
        /// <value>
        /// <c>true</c> if the legacy render pipeline is in use. Otherwise, <c>false</c>.
        /// </value>
        bool useLegacyRenderPipeline => GraphicsSettings.currentRenderPipeline == null;

        /// <summary>
        /// Stores the previous culling state (XRCameraSubsystem.invertCulling).
        /// If the requested culling state changes, the command buffer must be rebuilt.
        /// </summary>
        bool m_CommandBufferCullingState;

        /// <summary>
        /// A function that can be invoked by
        /// [CommandBuffer.IssuePluginEvent](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.IssuePluginEvent.html).
        /// This function calls the XRCameraSubsystem method that should be called immediately before background rendering.
        /// </summary>
        /// <param name="eventId">The id of the event</param>
        [MonoPInvokeCallback(typeof(Action<int>))]
        static void BeforeBackgroundRenderHandler(int eventId)
        {
            if (s_CameraSubsystem != null)
                s_CameraSubsystem.OnBeforeBackgroundRender(eventId);
        }

        /// <summary>
        /// A delegate representation of <see cref="BeforeBackgroundRenderHandler(int)"/>. This maintains a strong
        /// reference to the delegate, which is converted to an IntPtr by <see cref="s_BeforeBackgroundRenderHandlerFuncPtr"/>.
        /// </summary>
        /// <seealso cref="AddBeforeBackgroundRenderHandler(CommandBuffer)"/>
        static Action<int> s_BeforeBackgroundRenderHandler = BeforeBackgroundRenderHandler;

        /// <summary>
        /// A pointer to a function to be called immediately before rendering that is implemented in the XRCameraSubsystem implementation.
        /// It is called via [CommandBuffer.IssuePluginEvent](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.IssuePluginEvent.html).
        /// </summary>
        static readonly IntPtr s_BeforeBackgroundRenderHandlerFuncPtr = Marshal.GetFunctionPointerForDelegate(s_BeforeBackgroundRenderHandler);

        /// <summary>
        /// Static reference to the active XRCameraSubsystem. Necessary here for access from a static delegate.
        /// </summary>
        static UnityEngine.XR.ARSubsystems.XRCameraSubsystem s_CameraSubsystem;

        /// <summary>
        /// Whether culling should be inverted. Used during command buffer configuration,
        /// see [CommandBuffer.SetInvertCulling](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.SetInvertCulling.html).
        /// </summary>
        /// <seealso cref="ConfigureLegacyCommandBuffer(CommandBuffer)"/>
        protected bool shouldInvertCulling => m_CameraManager?.subsystem?.invertCulling ?? false;

        void Awake()
        {
            Debug.Log("ARCameraBackgroundRewrite.初始化()");
            //SDKManager.Instance.Init();

            m_Camera = GetComponent<Camera>();
            m_CameraManager = GetComponent<ARCameraManager>();
        }

        void OnEnable()
        {
            //Screen.SetResolution(480, 640, true);
            // Ensure that background rendering is disabled until the first camera frame is received.
            m_BackgroundRenderingEnabled = false;
            m_CameraManager.frameReceived += OnCameraFrameReceived;
        }

        void OnDisable()
        {
            m_CameraManager.frameReceived -= OnCameraFrameReceived;
            DisableBackgroundRendering();

            // We are no longer setting the projection matrix so tell the camera to resume its normal projection matrix
            // calculations.
            m_Camera.ResetProjectionMatrix();
        }

        /// <summary>
        /// Enable background rendering by disabling the camera's clear flags, and enabling the legacy RP background
        /// rendering if we are in legacy RP mode.
        /// </summary>
        void EnableBackgroundRendering()
        {
            m_BackgroundRenderingEnabled = true;

            // We must hold a static reference to the camera subsystem so that it is accessible to the
            // static callback needed for calling OnBeforeBackgroundRender() from the render thread
            if (m_CameraManager)
                s_CameraSubsystem = m_CameraManager.subsystem;
            else
                s_CameraSubsystem = null;

            DisableBackgroundClearFlags();

            Material material = defaultMaterial;
            if (useLegacyRenderPipeline && (material != null))
            {
                EnableLegacyRenderPipelineBackgroundRendering();
            }
        }

        /// <summary>
        /// Disable background rendering by disabling the legacy RP background rendering if we are in legacy RP mode
        /// and restoring the camera's clear flags.
        /// </summary>
        void DisableBackgroundRendering()
        {
            m_BackgroundRenderingEnabled = false;

            DisableLegacyRenderPipelineBackgroundRendering();

            RestoreBackgroundClearFlags();

            s_CameraSubsystem = null;
        }

        /// <summary>
        /// Set the camera's clear flags to do nothing while preserving the previous camera clear flags.
        /// </summary>
        void DisableBackgroundClearFlags()
        {
            m_PreviousCameraClearFlags = m_Camera.clearFlags;
            m_Camera.clearFlags = CameraClearFlags.Skybox;
        }

        /// <summary>
        /// Restore the previous camera's clear flags, if any.
        /// </summary>
        void RestoreBackgroundClearFlags()
        {
            if (m_PreviousCameraClearFlags != null)
            {
                m_Camera.clearFlags = m_PreviousCameraClearFlags.Value;
            }
        }

        /// <summary>
        /// The list of [CameraEvent](https://docs.unity3d.com/ScriptReference/Rendering.CameraEvent.html)s
        /// to add to the [CommandBuffer](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.html).
        /// </summary>
        static readonly CameraEvent[] s_DefaultCameraEvents = new[]
        {
        CameraEvent.BeforeForwardOpaque,
        CameraEvent.BeforeGBuffer
        };

        /// <summary>
        /// The list of [CameraEvent](https://docs.unity3d.com/ScriptReference/Rendering.CameraEvent.html)s
        /// to add to the [CommandBuffer](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.html).
        /// By default, returns
        /// [BeforeForwardOpaque](https://docs.unity3d.com/ScriptReference/Rendering.CameraEvent.BeforeForwardOpaque.html)
        /// and
        /// [BeforeGBuffer](https://docs.unity3d.com/ScriptReference/Rendering.CameraEvent.BeforeGBuffer.html)}.
        /// Override to use different camera events.
        /// </summary>
        protected virtual IEnumerable<CameraEvent> legacyCameraEvents => s_DefaultCameraEvents;

        /// <summary>
        /// Configures the <paramref name="commandBuffer"/> by first clearing it,
        /// and then adding necessary render commands.
        /// </summary>
        /// <param name="commandBuffer">The command buffer to configure.</param>
        protected virtual void ConfigureLegacyCommandBuffer(CommandBuffer commandBuffer)
        {
            Texture texture = !material.HasProperty(k_MainTexName) ? null : material.GetTexture(k_MainTexName);

            commandBuffer.Clear();
            AddBeforeBackgroundRenderHandler(commandBuffer);
            m_CommandBufferCullingState = shouldInvertCulling;
            commandBuffer.SetInvertCulling(m_CommandBufferCullingState);
            commandBuffer.ClearRenderTarget(true, false, Color.clear);
            commandBuffer.Blit(texture, BuiltinRenderTextureType.None, material);
        }

        /// <summary>
        /// Enable background rendering getting a command buffer, and configure it for rendering the background.
        /// </summary>
        void EnableLegacyRenderPipelineBackgroundRendering()
        {
            if (m_CommandBuffer == null)
            {
                m_CommandBuffer = new CommandBuffer();
                m_CommandBuffer.name = k_CustomRenderPassName;

                ConfigureLegacyCommandBuffer(m_CommandBuffer);
                foreach (var cameraEvent in legacyCameraEvents)
                {
                    m_Camera.AddCommandBuffer(cameraEvent, m_CommandBuffer);
                }
            }
        }

        /// <summary>
        /// Disable background rendering by removing the command buffer from the camera.
        /// </summary>
        void DisableLegacyRenderPipelineBackgroundRendering()
        {
            if (m_CommandBuffer != null)
            {
                foreach (var cameraEvent in legacyCameraEvents)
                {
                    m_Camera.RemoveCommandBuffer(cameraEvent, m_CommandBuffer);
                }

                m_CommandBuffer = null;
            }
        }

        /// <summary>
        /// This adds a command to the <paramref name="commandBuffer"/> to make call from the render thread
        /// to a callback on the `XRCameraSubsystem` implementation. The callback handles any implementation-specific
        /// functionality needed immediately before the background is rendered.
        /// </summary>
        /// <param name="commandBuffer">The [CommandBuffer](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.html)
        /// to add the command to.</param>
        internal static void AddBeforeBackgroundRenderHandler(CommandBuffer commandBuffer)
        {
            commandBuffer.IssuePluginEvent(s_BeforeBackgroundRenderHandlerFuncPtr, 0);
        }

        /// <summary>
        /// Callback for the camera frame event.
        /// </summary>
        /// <param name="eventArgs">The camera event arguments.</param>
        void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            // Enable background rendering when first frame is received.
            if (m_BackgroundRenderingEnabled)
            {
                if (eventArgs.textures.Count == 0)
                {
                    DisableBackgroundRendering();
                }
                else if (m_CommandBuffer != null && m_CommandBufferCullingState != shouldInvertCulling)
                {
                    ConfigureLegacyCommandBuffer(m_CommandBuffer);
                }
            }
            else if (eventArgs.textures.Count > 0)
            {
                EnableBackgroundRendering();
            }

            Material material = this.material;
            if (material != null)
            {
                var count = eventArgs.textures.Count;
                for (int i = 0; i < count; ++i)
                {
                    material.SetTexture(eventArgs.propertyNameIds[i], eventArgs.textures[i]);
                }

                if (eventArgs.displayMatrix.HasValue)
                {
                    material.SetMatrix(k_DisplayTransformId, eventArgs.displayMatrix.Value);
                }

                SetMaterialKeywords(material, eventArgs.enabledMaterialKeywords, eventArgs.disabledMaterialKeywords); 
            }

            if (eventArgs.projectionMatrix.HasValue)
            {
               m_Camera.projectionMatrix = eventArgs.projectionMatrix.Value;
               
            }
            //Debug.Log("OnCameraFrameReceivedTime=" + Time.time);

            byte[] bytes = Helper.GetScreenTexture(Camera.main, new Rect(0, 0, 480, 960));

            SDKManager.Instance.RefreshWithBytes(bytes);
        }

        /// <summary>
        /// Callback for the occlusion frame event.
        /// </summary>
        /// <param name="eventArgs">The occlusion frame event arguments.</param>
        void OnOcclusionFrameReceived(AROcclusionFrameEventArgs eventArgs)
        {
            Material material = this.material;
            if (material != null)
            {
                var count = eventArgs.textures.Count;
                for (int i = 0; i < count; ++i)
                {
                    material.SetTexture(eventArgs.propertyNameIds[i], eventArgs.textures[i]);
                }

                SetMaterialKeywords(material, eventArgs.enabledMaterialKeywords, eventArgs.disabledMaterialKeywords);

                // Set scale: this computes the affect the camera's localToWorld has on the the length of the
                // forward vector, i.e., how much farther from the camera are things than with unit scale.
                var forward = transform.localToWorldMatrix.GetColumn(2);
                var scale = forward.magnitude;
                material.SetFloat(k_CameraForwardScaleId, scale);
            }
        }

        void SetMaterialKeywords(Material material, List<string> enabledMaterialKeywords,
                                    List<string> disabledMaterialKeywords)
        {
            if (enabledMaterialKeywords != null)
            {
                foreach (var materialKeyword in enabledMaterialKeywords)
                {
                    if (!material.IsKeywordEnabled(materialKeyword))
                    {
                        material.EnableKeyword(materialKeyword);
                    }
                }
            }

            if (disabledMaterialKeywords != null)
            {
                foreach (var materialKeyword in disabledMaterialKeywords)
                {
                    if (material.IsKeywordEnabled(materialKeyword))
                    {
                        material.DisableKeyword(materialKeyword);
                    }
                }
            }
        }


        public void CameraCloseButtonPressed()
        {
            //GameObject go = GameObject.FindGameObjectWithTag("SlothHead");

            //cameraBackground.enabled = false;
            //Camera.main.clearFlags = CameraClearFlags.Skybox;

            //Debug.Log("false```Camera.main = " + Camera.main.transform.localPosition+ "  Camera.main = " + Camera.main.transform.localEulerAngles);
            //Debug.Log("false```Head = " + go.transform.position+ "   Head = " + go.transform.eulerAngles);
        }

        byte[] bytes;
        public void CameraOpenButtonPressed()
        {
            //GameObject go = GameObject.FindGameObjectWithTag("SlothHead");
            //cameraBackground.enabled = true;
            //Camera.main.clearFlags = CameraClearFlags.SolidColor;

            //Debug.Log("true```Camera.main = " + Camera.main.transform.localPosition + "  Camera.main = " + Camera.main.transform.localEulerAngles);
            //Debug.Log("true```Head = " + go.transform.position + "   Head = " + go.transform.eulerAngles);

            StartCoroutine(Screenshot());
        }

        IEnumerator Screenshot()
        {
            Debug.Log("CameraOpenButtonPressedStartTime=" + Time.time);
            bytes = Helper.GetScreenTexture(Camera.main, new Rect(0, 0, 480, 640));

            Debug.Log("CameraOpenButtonPressedEndTime=" + Time.time + " " + bytes.Length);

            yield return new WaitForSeconds(0.5f);

            Debug.Log("ScreenshotTime=" + Time.time + " " + bytes.Length);

            //FileStream filestr = File.Create(UnityEngine.Application.dataPath + "/1.bytes");
            //Debug.Log(UnityEngine.Application.dataPath);
            //filestr.Write(bytes, 0, bytes.Length);
            //filestr.Flush(); //流会缓冲，此行代码指示流不要缓冲数据，立即写入到文件。
            //filestr.Close(); //关闭流并释放所有资源，同时将缓冲区的没有写入的数据，写入然后再关闭。
            //filestr.Dispose();//释放流所占用的资源，Dispose()会调用Close(),Close()会调用Flush(); 也会写入缓冲区内的数据。
        }

        private void OnPostRender()
        {
            /*
            byte[] bytes = Utility.GetScreenTexture(Camera.main, new Rect(0, 0, 480, 640));

            // 激活这个rt, 并从中读取像素。
            //RenderTexture.active = m_Camera.activeTexture;
            //Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            //Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            //screenShot.ReadPixels(rect, 0, 0);// 注：这个时候，它是从RenderTexture.active中读取像素
            //screenShot.Apply();

            //byte[] bytes = screenShot.EncodeToPNG();

            SDKManager.Instance.RefreshWithBytes(bytes);

            //Destroy(screenShot);


            //bytes转成Texture
            Texture2D imageTexture = new Texture2D(480, 640, TextureFormat.RGB24, false);
            imageTexture.LoadImage(bytes);
            imageTexture.Apply();

            // RenderTexture.active = null; // JC: added to avoid errors

            rawImage.texture = imageTexture;

            Destroy(imageTexture);



            //Debug.Log("OnPostRenderTime=" + m_Camera.activeTexture.width+"  ");
            */
        }

        private void OnDestroy()
        {
            Debug.Log("ARCameraBackgroundRewrite.删除()");
        }
    }
}
