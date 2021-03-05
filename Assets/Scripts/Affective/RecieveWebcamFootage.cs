using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Demo;
using UnityEngine;
using UnityEngine.UI;

namespace Affective
{
    public class RecieveWebcamFootage : MonoBehaviour
    {
        private FacialExpressionDetection _facialExpressionDetection = null;
        private GameObject _webcamNotFoundImage = null;

        private WebCamTexture _webcamTex;
        
        public bool wearingGlasses;
        public bool markFace;
        
        [Header("Haar Cascade files")]
        public TextAsset faces;
        public TextAsset eyes;
        public TextAsset eyesGlasses;
        public TextAsset shapes;

        private FaceProcessorLive<WebCamTexture> _processor;
    
        private Texture2D _outputTexture;
        private RawImage _imageOutput;
    
        private bool _coroutineExecuting = false;
        

        private void Awake()
        {
            _facialExpressionDetection = GetComponent<FacialExpressionDetection>();
            _webcamNotFoundImage = transform.GetChild(0).gameObject;
            
            _imageOutput = GetComponent<RawImage>();
        
            _processor = new FaceProcessorLive<WebCamTexture>();
            if(wearingGlasses)
                _processor.Initialize(faces.text, eyesGlasses.text, shapes.bytes);
            else
                _processor.Initialize(faces.text, eyes.text, shapes.bytes);
        
            // data stabilizer - affects face rects, face landmarks etc.
            _processor.DataStabilizer.Enabled = true;        // enable stabilizer
            _processor.DataStabilizer.Threshold = 2.0;       // threshold value in pixels
            _processor.DataStabilizer.SamplesCount = 2;      // how many samples do we need to compute stable data

            // performance data - some tricks to make it work faster
            _processor.Performance.Downscale = 512;          // processed image is pre-scaled down to N px by long side
            _processor.Performance.SkipRate = 21;             // we actually process only each Nth frame (and every frame for skipRate = 0)
        }

        void Start()
        {
            _webcamNotFoundImage.SetActive(true);

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
                    _webcamNotFoundImage.SetActive(false);
                    return;
                }
            }

            _webcamNotFoundImage.SetActive(true);
            Debug.LogError("Webcam not found!");
        }


        void SelectWebcam(WebCamDevice cam)
        {
            Debug.Log("Using " + cam.name);

            _webcamTex = new WebCamTexture(cam.name);
            _webcamTex.Play();

            _outputTexture = new Texture2D(_webcamTex.width, _webcamTex.height, textureFormat:TextureFormat.RGBA32 , false);
        }
    

        private void Update()
        {
            _processor.ProcessTexture(_webcamTex, new OpenCvSharp.Unity.TextureConversionParams());
            
            OutputProcessedImage();
            
            if (!_coroutineExecuting && _webcamTex)
            {
                StartCoroutine(CheckFace());
            }
        }
    
        private IEnumerator CheckFace()
        {
            _coroutineExecuting = true;
            
            // detect everything we're interested in
           // _processor.ProcessTexture(_webcamTex, new OpenCvSharp.Unity.TextureConversionParams());

            _facialExpressionDetection.DetectFacialExpression(_processor.Image);

           // OutputProcessedImage();

            yield return new WaitForSecondsRealtime(3f);

            _coroutineExecuting = false;
        }

        void OutputProcessedImage()
        {
            if (markFace)
            {
                // mark detected objects
                _processor.MarkDetected();
            }
      
            // processor.Image now holds data we'd like to visualize
            _outputTexture = OpenCvSharp.Unity.MatToTexture(_processor.Image, _outputTexture);   // if output is valid texture it's buffer will be re-used, otherwise it will be re-created
            _imageOutput.texture = _outputTexture;
        }

    }
}
