using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Demo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Threading;
using DlibDotNet;
using Microsoft.ML;
using Microsoft.ML.Data;
using UI;
using Rect = OpenCvSharp.Rect;

namespace Affective
{
    public class ProcessWebcamInput : MonoBehaviour
    {
        //private FacialExpressionDetection _facialExpressionDetection = null;
        public AffectiveManager affectiveManager = null;
        private FacialDetection _facialDetection;
        private GameObject _webcamNotFoundImage = null;

        public WebCamTexture _webcamTex;

        [SerializeField] private TMPro.TextMeshProUGUI hotkeyText;
        
        [Header("Haar Cascade files")]
        public TextAsset faces;
        public TextAsset eyes;
        public TextAsset shapes;

        private FaceProcessorLive<WebCamTexture> _processor;

        private RawImage _imageOutput;

        private WebCamDevice[] _webcams;
        public TMP_Dropdown webcamDropdown;
        public Toggle showFaceToggle;

        public AspectRatioFitter imageFitter;
        
        private bool _checkFaceStarted;
        private bool _findFaceStarted;

        public bool affectiveOn;

        private Rectangle faceRect;
        
        //private CascadeClassifier _faceCascade;

        private bool scanningFace = false;
        

        private void Awake()
        {
            _facialDetection = new FacialDetection();
            _facialDetection.Init();
            
            _imageOutput = GetComponent<RawImage>();
            
            //_faceCascade = new CascadeClassifier();
            //_faceCascade.Load(faces.text);
            
            InitializeProcessor();
            
            if (affectiveOn)
            {
                SearchForWebcam();
            }
        }

        private void InitializeProcessor()
        {
            _processor = new FaceProcessorLive<WebCamTexture>();
            _processor.Initialize(faces.text, eyes.text, shapes.bytes);

            // data stabilizer - affects face rects, face landmarks etc.
            _processor.DataStabilizer.Enabled = true;        // enable stabilizer
            _processor.DataStabilizer.Threshold = 4.0;       // threshold value in pixels
            _processor.DataStabilizer.SamplesCount = 1;      // how many samples do we need to compute stable data

            // performance data - some tricks to make it work faster
            //_processor.Performance.Downscale = 1024;          // processed image is pre-scaled down to N px by long side
            //_processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)
        }

        private void Start()
        {
            if (affectiveOn)
            {
                hotkeyText.enabled = false;
                //SearchForWebcam();
            }
            else
            {
                hotkeyText.enabled = true;
            }
            
            showFaceToggle.onValueChanged.AddListener(delegate {SetWebcamVisibility();});
            webcamDropdown.onValueChanged.AddListener(delegate { PickWebcamFromDropdown(); });

            scanningFace = false;
        }


        private void PickWebcamFromDropdown()
        {
            foreach (var webcam in _webcams)
            {
                if (webcam.name != webcamDropdown.options[webcamDropdown.value].text) continue;
                SelectWebcam(webcam);
                return;
            }
        }

        private void SearchForWebcam()
        {
            _webcamNotFoundImage = transform.GetChild(0).gameObject;
            _webcamNotFoundImage.SetActive(true);
            
            _webcams = WebCamTexture.devices;

            if (_webcams.Length == 0)
            {
                _webcamNotFoundImage.SetActive(true);
                Debug.LogError("No camera detected!");
                return;
            }

            // Populate dropdown
            var webcamList = _webcams.Select(webcam => webcam.name).ToList();
            webcamDropdown.ClearOptions();
            webcamDropdown.AddOptions(webcamList);
            
            SelectWebcam(_webcams[0]);
            _webcamNotFoundImage.SetActive(false);

            float videoRatio = _webcamTex.width / (float)_webcamTex.height;
            imageFitter.aspectRatio = videoRatio;
        }


        private void SelectWebcam(WebCamDevice cam)
        {
            Debug.Log("Using " + cam.name);

            _webcamTex = new WebCamTexture(cam.name);
            _webcamTex.Play();
        }
    

        private void Update()
        {
            if (!affectiveOn)
            {
                if(Input.GetKey(KeyCode.Alpha1)) affectiveManager.SetCurrentEmotion("neutral");
                if(Input.GetKey(KeyCode.Alpha2)) affectiveManager.SetCurrentEmotion("joy");
                if(Input.GetKey(KeyCode.Alpha3)) affectiveManager.SetCurrentEmotion("anger");
                if(Input.GetKey(KeyCode.Alpha4)) affectiveManager.SetCurrentEmotion("sadness");
                if(Input.GetKey(KeyCode.Alpha5)) affectiveManager.SetCurrentEmotion("surprise");
                if(Input.GetKey(KeyCode.Alpha6)) affectiveManager.SetCurrentEmotion("disgust");
                if(Input.GetKey(KeyCode.Alpha7)) affectiveManager.SetCurrentEmotion("fear");
                return;
            }
            
            OutputProcessedImage();

             if (!_facialDetection.faceCoroutineRunning)
             {
                 StartCoroutine(_facialDetection.CheckForFaces());
             }

             if (!_webcamTex || scanningFace) return;
             
             if (_facialDetection.coroutineRunning) return;
             
             _processor.ProcessTexture(_webcamTex, new OpenCvSharp.Unity.TextureConversionParams(), false);
             StartCoroutine(_facialDetection.DetectFacialExpression(_processor.Image));
             //StartCoroutine(CheckFace());
        }

        private IEnumerator FindFace()
        {
            _findFaceStarted = true;
            
            _facialDetection.FaceFound();
            
            yield return new WaitForSecondsRealtime(2);
            
            _findFaceStarted = false;
        }
    

        private Mat PreprocessedImage()
        {
            // detect everything we're interested in
            _processor.ProcessTexture(_webcamTex, new OpenCvSharp.Unity.TextureConversionParams(), false);
           
            return _processor.Image;
        }

        private void OutputProcessedImage()
        {
            if (!_imageOutput.enabled)
                return;
    
            _imageOutput.texture = _webcamTex;
        }

        private void SetWebcamVisibility() => _imageOutput.enabled = showFaceToggle.isOn;

    }
    
    
     [SuppressMessage("ReSharper", "InconsistentNaming")]
     public class FacialDetection
    {
        private MLContext _mlContext;
        private PredictionEngine<FeatureInputData, ExpressionPrediction> _predictionEngine;
        private DlibDotNet.ShapePredictor _shapePredictor;
        private FrontalFaceDetector _frontalFaceDetector;

        private AffectiveManager _affectiveManager;

        private static float leftEyebrow;
        private static float rightEyebrow;
        private static float leftLip;
        private static float rightLip;
        private static float lipWidth;
        private static float lipHeight;
        
        private Rectangle[] _faces;
        private Array2D<BgrPixel> _cimg;
        
        private static FullObjectDetection shape;

        public bool coroutineRunning = false;
        public bool faceCoroutineRunning = false;

        private float calculationInterval = 0.02f;

        public void Init()
        {
            _faces = new Rectangle[0];
            _cimg = new Array2D<BgrPixel>();

            _mlContext = new MLContext();

            OpenPredictionPipeline();
            
            _shapePredictor = DlibDotNet.ShapePredictor.Deserialize(Application.streamingAssetsPath + "/shape_predictor_68_face_landmarks.dat");
            _frontalFaceDetector = Dlib.GetFrontalFaceDetector();
            
            coroutineRunning = false;
            faceCoroutineRunning = false;
            
            _affectiveManager = AffectiveManager.Instance;
            
            //threads = new Thread[6];
        }

        public void FaceFound()
        {
            // find all faces in the image
            _faces = _frontalFaceDetector.Operator(_cimg);
        }
        
       
         public IEnumerator CheckForFaces()
         {
             faceCoroutineRunning = true;

             _faces = _frontalFaceDetector.Operator(_cimg);

             yield return new WaitForSecondsRealtime(5.0f);
             
             faceCoroutineRunning = false;
         }
         
         public IEnumerator DetectFacialExpression(Mat image)
         {
             coroutineRunning = true;
             
             _cimg = GenerateArray2D(image);

              if (!_faces.Any())
              {
                  _faces = _frontalFaceDetector.Operator(_cimg);
                  coroutineRunning = false;
                  goto here;
              }
             
             yield return new WaitForSecondsRealtime(calculationInterval);
             shape = _shapePredictor.Detect(_cimg, _faces[0]);

             if (shape == null)
             {
                 _faces = _frontalFaceDetector.Operator(_cimg);
                 coroutineRunning = false;
                 goto here;
             }
             
             CalculateLeftEyebrow();
             yield return new WaitForSecondsRealtime(calculationInterval);
             CalculateRightEyebrow();
             yield return new WaitForSecondsRealtime(calculationInterval);
             CalculateLeftLip();
             yield return new WaitForSecondsRealtime(calculationInterval);
             CalculateRightLip();
             yield return new WaitForSecondsRealtime(calculationInterval);
             CalculateLipWidth();
             yield return new WaitForSecondsRealtime(calculationInterval);
             CalculateLipHeight();
             yield return new WaitForSecondsRealtime(calculationInterval);
             
             var inputData = new FeatureInputData
             {
                 leftEyebrow = leftEyebrow,
                 rightEyebrow = rightEyebrow,
                 leftLip = leftLip,
                 rightLip = rightLip,
                 lipWidth = lipWidth,
                 lipHeight = lipHeight
             };
             
             AffectiveManager.Instance.SetCurrentEmotion(_predictionEngine.Predict(inputData).Expression);
             
             here:
             
             coroutineRunning = false;
         }
         
        
        
        Array2D<BgrPixel> GenerateArray2D(Mat image)
        {
            var array = new byte[image.Width * image.Height * image.ElemSize()];
            
            Marshal.Copy(image.Data, array, 0 , array.Length);
            
            return  Dlib.LoadImageData<BgrPixel>(array, (uint) image.Height, (uint) image.Width,
                (uint) (image.Width * image.ElemSize()));
            
        }

        private void OpenPredictionPipeline()
        {
            var predictionPipeline = _mlContext.Model.Load(Application.streamingAssetsPath + "/model.zip", out _);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<FeatureInputData, ExpressionPrediction>(predictionPipeline);
        }

        private static void CalculateLeftEyebrow()
        {
            //var shape = (FullObjectDetection) data;

            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 21, 39);

            for (uint i = 18; i <= 21; i++) result += CalculateDistance(shape, i, 39) / normalisationDistance;

            leftEyebrow = result;
            }

        private static void CalculateRightEyebrow()
        {
            //var shape = (FullObjectDetection) data;
            
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 22, 42);

            for (uint i = 22; i <= 25; i++) result += CalculateDistance(shape, i, 42) / normalisationDistance;

            rightEyebrow = result;
        }

        private static void CalculateLeftLip()
        {
            //var shape = (FullObjectDetection) data;
            
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 33, 51);

            for (uint i = 48; i <= 50; i++) result += CalculateDistance(shape, i, 33) / normalisationDistance;

            leftLip = result;
        }

        private static void CalculateRightLip()
        {
            //var shape = (FullObjectDetection) data;
            
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 33, 51);

            for (uint i = 52; i <= 54; i++) result += CalculateDistance(shape, i, 33) / normalisationDistance;

            rightLip = result;
        }

        private static void CalculateLipWidth()
        {
            //var shape = (FullObjectDetection) data;
            lipWidth = CalculateDistance(shape, 48, 54) / CalculateDistance(shape, 33, 51);
        }

        private static void CalculateLipHeight()
        {
            lipHeight = CalculateDistance(shape, 51, 57) / CalculateDistance(shape, 33, 51);
        }

        private static float CalculateDistance(FullObjectDetection shape, uint point1, uint point2)
        {
            return Mathf.Sqrt(Mathf.Pow(shape.GetPart(point1).X - shape.GetPart(point2).X, 2) + Mathf.Pow(shape.GetPart(point1).Y - shape.GetPart(point2).Y, 2));
        }

        private class FeatureInputData
        {
            [LoadColumn(0)]
            public float leftEyebrow { get; set; }

            [LoadColumn(1)]
            public float rightEyebrow { get; set; }

            [LoadColumn(2)]
            public float leftLip { get; set; }

            [LoadColumn(3)]
            public float rightLip { get; set; }

            [LoadColumn(4)]
            public float lipWidth { get; set; }

            [LoadColumn(5)]
            public float lipHeight { get; set; }

            [LoadColumn(6)]
            public string label;
        }

        private class ExpressionPrediction
        {
            [ColumnName("PredictedLabel")]
            public string Expression { get; set; }
        }
    }
}
