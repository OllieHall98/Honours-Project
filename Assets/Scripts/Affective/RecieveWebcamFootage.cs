using System.Collections;
using Affective;
using UnityEngine.UI;
using UnityEngine;
using System.Runtime.InteropServices;
using DlibDotNet;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using DlibDotNet.Extensions;
using Lean.Gui;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Windows;
using Color = UnityEngine.Color;

public class RecieveWebcamFootage : MonoBehaviour
{
    [SerializeField] private FacialExpressionDetection facialExpressionDetection = null;
    [SerializeField] AffectiveManager affectiveManager;

    [SerializeField] GameObject webcamNotFoundImage = null;

    WebCamTexture webcamTex;

    private Color32[] imageData;


    private Texture2D outputTexture;
    public RawImage imageOutput;
    
    private bool _coroutineExecuting = false;

    void Start()
    {
        webcamNotFoundImage.SetActive(true);

        SearchForWebcam();
    }

    void SearchForWebcam()
    {
        var devices = WebCamTexture.devices;

        for (var i = 0; i < devices.Length; i++)
        {
            if (devices[i].name.Contains("Webcam") || 
                devices[i].name.Contains("webcam") || 
                devices[i].name.Contains("Camera") || 
                devices[i].name.Contains("camera") ||
                devices[i].name.Contains("Cam")    ||
                devices[i].name.Contains("HD")    ||
                devices[i].name.Contains("cam"))
            {
                SelectWebcam(devices[i]);
                webcamNotFoundImage.SetActive(false);
                return;
            }
        }

        webcamNotFoundImage.SetActive(true);
        Debug.LogError("Webcam not found!");
    }


    void SelectWebcam(WebCamDevice cam)
    {
        Debug.Log("Using " + cam.name);

        webcamTex = new WebCamTexture(cam.name);
        webcamTex.Play();
        imageData = new Color32[webcamTex.width * webcamTex.height];
        
        outputTexture = new Texture2D(webcamTex.width, webcamTex.height, textureFormat:TextureFormat.RGB24 , false);
        
        //imageOutput.transform.localScale = new Vector3(webcamTex.width * 0.06f, webcamTex.height * 0.06f, imageOutput.transform.localScale.z);

        //imageOutput.texture = webcamTex;
        
    }

    private void Update()
    {
        if (!_coroutineExecuting && webcamTex)
        {
            StartCoroutine(CheckFace());
        }
    }
    
    [StructLayout(LayoutKind.Explicit)]
    public struct Color32Array
    {
        [FieldOffset(0)]
        public byte[] byteArray;

        [FieldOffset(0)]
        public Color32[] colors;
    }

    private IEnumerator CheckFace()
    {
        _coroutineExecuting = true;

        //var yeet = tex.GetRawTextureData();
      //ConvertToGrayscale(webcamTex);
      //byte[] processedImage = ConvertToGrayscale(webcamTex);
      //var byteArray = Color32ArrayToByteArray(webcamTex.GetPixels32(imageData));

      //var grayscaleTexture = ConvertToGrayscale(webcamTex);
      
      //var byteArray = grayscaleTexture.EncodeToPNG();
      
      outputTexture = ConvertToGrayscale(webcamTex);

      imageOutput.texture = outputTexture;
      
      var data = Dlib.LoadImageData<RgbPixel>(outputTexture.EncodeToPNG(), (uint)outputTexture.height, (uint)outputTexture.width, (uint)outputTexture.width * 4);
      
      ImageWindow im = new ImageWindow(data);
      
      //var byteArray = yeetus.EncodeToPNG();

      //var yeet = webcamTex.GetPixels32(imageData);
      
      // Color32Array colorArray = new Color32Array();
      // colorArray.colors = new Color32[webcamTex.width * webcamTex.height];
      // webcamTex.GetPixels32(colorArray.colors);
      
      //outputTexture.LoadRawTextureData(byteArray);
      //imageOutput.texture = outputTexture;
      
      //byte[] processedImage = Color32ArrayToByteArray(webcamTex.GetPixels32());

      // Bitmap bmp;
      // using (var ms = new MemoryStream(colorArray.byteArray))
      // {
      //     bmp = new Bitmap(ms);
      // }
      //
      //
      //
      // var bitmapData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
      //     System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
      //

      
     // var array = new byte[bitmapData.Stride * bitmapData.Height];
      //Marshal.Copy(bitmapData.Scan0, array, 0, array.Length);
      
      //var data = Dlib.LoadImageData<RgbPixel>(byteArray, (uint)grayscaleTexture.height, (uint)grayscaleTexture.width, (uint)640);

      

      //facialExpressionDetection.DetectFacialExpression(data);
      
      //ImageWindow im = new ImageWindow(data);
      
      //outputTexture.LoadRawTextureData(byteArray);
      //imageOutput.texture = outputTexture;
      
      
        yield return new WaitForSecondsRealtime(2f);

        _coroutineExecuting = false;
    }

    private static Bitmap BytesToBitmap(byte[] byteArray)
    {
        using var ms = new MemoryStream(byteArray);
        return new Bitmap(ms);
    }


    
    Texture2D ConvertToGrayscale(WebCamTexture input)
    {
        Texture2D result = new Texture2D(input.width, input.height);
        Color32[] pixels = input.GetPixels32();
        
        for (int x = 0; x < input.width; x++)
        {
            for (int y = 0; y < input.height; y++)
            {
                Color32 pixel = pixels[x + y * input.width];
                int p = ((256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
                int b = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int g = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int r = p % 256;
                float l = (0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f);
                Color c = new Color(l, l, l, 1);
                result.SetPixel(x, y, c);
            }
        }
        
        result.Apply(false);
        return result;
    }

    // private Array2D<byte> ConvertWebcamToArray2D()
    // {
    //     // byte[] byteArray = Color32ArrayToByteArray(webcamTex.GetPixels32(imageData));
    //     //
    //     // var array = Dlib.LoadImageData<RgbPixel>(byteArray, (uint)webcamTex.height, (uint)webcamTex.width, 1);
    //     // //var poop = Dlib.LoadImageData((uint)webcamTex.width, (uint)webcamTex.height, byteArray, )
    //     //
    //     // for (var y = 0; y < webcamTex.height; y += webcamTex.width)
    //     // {
    //     //     for (var x = 0; x < webcamTex.width; x++)
    //     //     {
    //     //         array[x][y] = byteArray[y + x];
    //     //     }
    //     // }
    //     //
    //     // return array;
    // }
    
    // private static byte[] Color32ArrayToByteArray(Color32[] colors)
    // {
    //     if (colors == null || colors.Length == 0)
    //         return null;
    //
    //     int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
    //     int length = lengthOfColor32 * colors.Length;
    //     byte[] bytes = new byte[length];
    //
    //     GCHandle handle = default(GCHandle);
    //     try
    //     {
    //         handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
    //         IntPtr ptr = handle.AddrOfPinnedObject();
    //         Marshal.Copy(ptr, bytes, 0, length);
    //     }
    //     finally
    //     {
    //         if (handle != default(GCHandle))
    //             handle.Free();
    //     }
    //
    //     return bytes;
    // }
    //
    public byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        byte[] bytes = new byte[colors.Length * 4];
        for (int i = 0; i < bytes.Length/4; i += 4)
        {
            bytes[i]= colors[i].r;
            bytes[i+1] = colors[i].g;
            bytes[i+2] = colors[i].b;
            bytes[i+3] = colors[i].a;
           
        }
        return bytes;
    }

    // private Texture2D TextureToTexture2D(Texture texture)
    // {
    //     int width = texture.width;
    //     int height = texture.height;
    //     
    //     //var newData = texture.GetRawTextureData<Color32>();
    //     var array = new Array2D<byte>();
    //     
    //     for (var y = 0; y < height; y++)
    //     {
    //         for (var x = 0; x < width; x++)
    //         {
    //             array[x][y] = texture.[x][y];
    //         }
    //     }
    //     
    //     //
    //     // Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
    //     // RenderTexture currentRT = RenderTexture.active;
    //     // RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
    //     // Graphics.Blit(texture, renderTexture);
    //     //
    //     // RenderTexture.active = renderTexture;
    //     // texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
    //     // texture2D.Apply();
    //     //
    //     // RenderTexture.active = currentRT;
    //     // RenderTexture.ReleaseTemporary(renderTexture);
    //     // return texture2D;
    // }
}
