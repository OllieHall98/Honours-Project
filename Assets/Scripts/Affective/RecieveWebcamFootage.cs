using System.Collections;
using System.IO;
using Affective;
using UnityEngine.UI;
using UnityEngine;

public class RecieveWebcamFootage : MonoBehaviour
{
    [SerializeField] private FacialExpressionDetection facialExpressionDetection = null;
    [SerializeField] AffectiveManager affectiveManager;

    [SerializeField] GameObject webcamNotFoundImage = null;

    WebCamTexture webcamTex;

    public RawImage imageOutput;

    bool canRunFEA = true;

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

        imageOutput.transform.localScale = new Vector3(webcamTex.width * 0.06f, webcamTex.height * 0.06f, imageOutput.transform.localScale.z);

        imageOutput.texture = webcamTex;
        webcamTex.Play();
    }

    private void Update()
    {
        if (canRunFEA && webcamTex)
        {
            StartCoroutine(CheckFace());
        }
    }

    private IEnumerator CheckFace()
    {
        canRunFEA = false;

        Texture2D rawImageTexture = TextureToTexture2D(imageOutput.texture);
        byte[] imageData = rawImageTexture.EncodeToPNG();

        facialExpressionDetection.DetectFacialExpression(imageData);

        yield return new WaitForSecondsRealtime(0.1f);

        canRunFEA = true;
    }

    private Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);
        return texture2D;
    }
}
