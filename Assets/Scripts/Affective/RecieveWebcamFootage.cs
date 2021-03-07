using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Demo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Rect = OpenCvSharp.Rect;

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

        public WebCamDevice[] webcams;
        public TMPro.TMP_Dropdown webcamDropdown;
        public Toggle showFaceToggle;
        public Toggle glassesToggle;
        public Toggle markingToggle;

        public AspectRatioFitter imageFitter;
        
        private bool _coroutineExecuting = false;
        

        private void Awake()
        {
            SearchForWebcam();
            
            _facialExpressionDetection = GetComponent<FacialExpressionDetection>();

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

        private void Start()
        {
            showFaceToggle.onValueChanged.AddListener(delegate {SetWebcamVisibility(showFaceToggle);});
            glassesToggle.onValueChanged.AddListener(delegate {SetGlasses(glassesToggle);});
            markingToggle.onValueChanged.AddListener(delegate {SetMarkFace(markingToggle);});
            webcamDropdown.onValueChanged.AddListener(delegate { PickWebcamFromDropdown(webcamDropdown); });
        }


        public void PickWebcamFromDropdown(TMPro.TMP_Dropdown change)
        {
            foreach (var webcam in webcams)
            {
                if (webcam.name != webcamDropdown.options[webcamDropdown.value].text) continue;
                SelectWebcam(webcam);
                return;
            }
        }
        
        void SearchForWebcam()
        {
            _webcamNotFoundImage = transform.GetChild(0).gameObject;
            _webcamNotFoundImage.SetActive(true);
            
            webcams = WebCamTexture.devices;

            if (webcams.Length == 0)
            {
                _webcamNotFoundImage.SetActive(true);
                Debug.LogError("No camera detected!");
                return;
            }

            var webcamList = webcams.Select(webcam => webcam.name).ToList();

            webcamDropdown.ClearOptions();
            webcamDropdown.AddOptions(webcamList);
            
            SelectWebcam(webcams[0]);
            _webcamNotFoundImage.SetActive(false);

            float videoRatio = (float) _webcamTex.width / (float) _webcamTex.height;
            imageFitter.aspectRatio = videoRatio;
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
            OutputProcessedImage();

            if (!_coroutineExecuting && _webcamTex)
            {
                StartCoroutine(CheckFace());
            }
        }
    
        private IEnumerator CheckFace()
        {
            _coroutineExecuting = true;
            
            _facialExpressionDetection.DetectFacialExpression(PreprocessedImage());

            yield return new WaitForSecondsRealtime(0.25f);

            _coroutineExecuting = false;
        }

        Mat PreprocessedImage()
        {
            // detect everything we're interested in
            _processor.ProcessTexture(_webcamTex, new OpenCvSharp.Unity.TextureConversionParams(), false);

            // Shrink the image for easier processing
            Cv2.Resize(_processor.Image, _processor.Image, new Size(_webcamTex.width / 2.5f, _webcamTex.height / 2.5f), interpolation: InterpolationFlags.Linear);
            
            // Apply a gaussian blur to remove distractions/unnecessary details
            Cv2.GaussianBlur(_processor.Image, _processor.Image, new Size(5, 5), 0);
            
            ConvertToGrayscale(_processor.Image);

            return _processor.Image;
        }

        void ConvertToGrayscale(Mat image)
        {
            // Convert to grayscale
            Cv2.CvtColor(image, image, ColorConversionCodes.RGB2GRAY);
            
            Mat[] channels = new Mat[3];
            channels[0] = image;
            channels[1] = image;
            channels[2] = image;
            Cv2.Merge(channels,image);
        }

        void OutputProcessedImage()
        {
            if (!_imageOutput.enabled)
                return;
    
            _imageOutput.texture = _webcamTex;
        }
        
        public void SetWebcamVisibility(Toggle change)
        {
            _imageOutput.enabled = showFaceToggle.isOn;
        }
        
        public void SetGlasses(Toggle change)
        {
            _processor.Initialize(faces.text, glassesToggle.isOn ? eyesGlasses.text : eyes.text, shapes.bytes);
        }

        public void SetMarkFace(Toggle change)
        {
            markFace = markingToggle.isOn;
        }

    }
}
