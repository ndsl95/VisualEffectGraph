using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Windows.Kinect;
using KinectVfx;

public class KinectPointCloudCSharp : MonoBehaviour
{
    public RenderTexture pointCloud;
    private KinectSensor sensor;
    private DepthFrameReader depthReader;//深度帧读取器
    private ComputeBuffer positionBuffer;
    public ComputeShader pointerCloudBaker;
    private ushort[] depthData;
    private Texture2D texture;
    private Color[] pixelColor;
    private int[] mapDimensions = new int[2];
    private int nearThreshold;//最近的有效距离
    private int farThreshold;//最远的有效距离
    private RenderTexture tempPositionTexture;//临时变量,结果使用Blid存到pointCloud中

    public Camera myCamera;
    // Start is called before the first frame update
    void Start()
    {
         myCamera.ScreenPointToRay(Input.mousePosition);
        sensor = KinectSensor.GetDefault();
        if(sensor!=null)
        {
            depthReader = sensor.DepthFrameSource.OpenReader();
            var frameDescript = sensor.DepthFrameSource.FrameDescription;
            depthData = new ushort[frameDescript.LengthInPixels];
            pixelColor = new Color[frameDescript.LengthInPixels];
            texture = new Texture2D(frameDescript.Width , frameDescript.Height);
            nearThreshold = sensor.DepthFrameSource.DepthMinReliableDistance;
            farThreshold = sensor.DepthFrameSource.DepthMaxReliableDistance;
        }

        if(!sensor.IsOpen)
        {
            sensor.Open();
        }
    }

    void Depth2ColorTexture()
    {
        for(int i=0 ;i<depthData.Length ;i++)
        {
            ushort data = depthData[i];
            if(data<nearThreshold)//小于可信距离,颜色设置为黑色
            {
                pixelColor[i].r = 0;
                pixelColor[i].g = 0;
                pixelColor[i].b =0;
            }
            else if(data >farThreshold)
            {
                pixelColor[i].r = 0;
                pixelColor[i].g = 0;
                pixelColor[i].b = 1;
            }
            else
            {
                int grade = data / nearThreshold;
                pixelColor[i].r = 1.0f - 0.125f * grade;
                pixelColor[i].g = 0.125f * grade;
                pixelColor[i].b = 0;
            }
            texture.SetPixels(pixelColor);
            texture.Apply();
            Graphics.Blit(texture , pointCloud);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(depthReader == null)
        {
            return;
        }
        var frame = depthReader.AcquireLatestFrame();
        if(frame!=null)
        {

            int depthFrameWidth = frame.FrameDescription.Width;
            int depthFrameHeight = frame.FrameDescription.Height;
            int depthFramePixelCount = depthFrameWidth * depthFrameHeight;

            float horizontalFov = frame.FrameDescription.HorizontalFieldOfView;
            float verticalFov = frame.FrameDescription.VerticalFieldOfView;
            mapDimensions[0] = depthFrameWidth;
            mapDimensions[1] = depthFrameHeight;
            if(tempPositionTexture!=null &&(tempPositionTexture.width!=depthFrameWidth || tempPositionTexture.height!=depthFrameHeight))
            {
                Destroy(tempPositionTexture);
                tempPositionTexture = null;
            }

            if(tempPositionTexture == null)
            {
                tempPositionTexture = new RenderTexture(depthFrameWidth , depthFrameHeight , 0 , RenderTextureFormat.ARGBHalf);
                tempPositionTexture.enableRandomWrite = true;
                tempPositionTexture.Create();
            }

            if(positionBuffer == null)
            {
                positionBuffer = new ComputeBuffer(depthFramePixelCount / 2 , sizeof(uint));
            }

            using (KinectBuffer depthBuffer = frame.LockImageBuffer())
            {
                positionBuffer.SetData(depthBuffer.UnderlyingBuffer , depthFramePixelCount , sizeof(uint));
                int bakeDepthkernel = pointerCloudBaker.FindKernel("BakeDepth");
                pointerCloudBaker.SetInts("MapDimensions" , mapDimensions);
                pointerCloudBaker.SetFloat("NearThreshold" , nearThreshold);
                pointerCloudBaker.SetFloat("FarThreshold" , farThreshold);
                pointerCloudBaker.SetBuffer(bakeDepthkernel , "PositionBuffer",positionBuffer);
                pointerCloudBaker.SetTexture(bakeDepthkernel , "PositionTexture" , tempPositionTexture);
                pointerCloudBaker.Dispatch(bakeDepthkernel , depthFrameHeight / 8 , depthFrameWidth / 8 , 1);
            }
            Graphics.Blit(tempPositionTexture , pointCloud);
               
            frame.Dispose();
            frame = null;
        }
    }

    void OnApplicationQuit ( )
    {
        if (depthReader != null)
        {
            depthReader.Dispose();
            depthReader = null;
        }
        if (sensor != null)
        {
            if (sensor.IsOpen)
            {
                sensor.Close();
            }
            sensor = null;

        }
    }
}
