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
using DlibDotNet;
using Microsoft.ML;
using Microsoft.ML.Data;
using UnityEngine.Windows.WebCam;

namespace Affective
{
    public class ProcessWebcamInput : MonoBehaviour
    {
        //private FacialExpressionDetection _facialExpressionDetection = null;
        public AffectiveManager affectiveManager = null;
        private FacialDetection _facialDetection;
        private WebCamTexture _webcamTex;

        [SerializeField] private TMPro.TextMeshProUGUI hotkeyText;
        
        [Header("Haar Cascade files")]
        public TextAsset faces;
        public TextAsset eyes;
        public TextAsset shapes;

        private FaceProcessorLive<WebCamTexture> _processor;

        [SerializeField] private RawImage imageOutput;

        private WebCamDevice[] _webcams;
        public TMP_Dropdown webcamDropdown;
        public Toggle showFaceToggle;

        public AspectRatioFitter imageFitter;
        
        public bool affectiveOn;
        
        private bool _scanningFace = false;
        

        private void Awake()
        {
            _facialDetection = new FacialDetection();
            _facialDetection.Init();
            
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
            _processor.DataStabilizer.Threshold = 6.0;       // threshold value in pixels
            _processor.DataStabilizer.SamplesCount = 1;      // how many samples do we need to compute stable data

            // performance data - some tricks to make it work faster
            _processor.Performance.Downscale = 512;          // processed image is pre-scaled down to N px by long side
            //_processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)
        }

        private void Start()
        {
            if (affectiveOn)
            {
                hotkeyText.enabled = false;
            }
            else
            {
                hotkeyText.enabled = true;
            }
            
            showFaceToggle.onValueChanged.AddListener(delegate {SetWebcamVisibility();});
            webcamDropdown.onValueChanged.AddListener(delegate { PickWebcamFromDropdown(); });

            _scanningFace = false;
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
            _webcams = WebCamTexture.devices;

            SelectWebcam(_webcams[0]);

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

            if (!_webcamTex || _scanningFace) return;
             
             if (_facialDetection.coroutineRunning) return;
            
             _processor.ProcessTexture(_webcamTex, new OpenCvSharp.Unity.TextureConversionParams(), false);
             StartCoroutine(_facialDetection.DetectFacialExpression(_processor.Image));
             
             if (!_facialDetection.faceCoroutineRunning)  
                 StartCoroutine(_facialDetection.CheckForFaces());
        }
        
        private void OutputProcessedImage()
        {
            if (!imageOutput.enabled)
                return;
    
            imageOutput.texture = _webcamTex;
        }

        private void SetWebcamVisibility() => imageOutput.enabled = showFaceToggle.isOn;

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

        private static FullObjectDetection shape;

        public bool coroutineRunning = false;
        public bool faceCoroutineRunning = false;

        private float calculationInterval = 0.03f;

        private Array2D<RgbPixel> img;

        public void Init()
        {
            _faces = new Rectangle[0];
            _mlContext = new MLContext();

            OpenPredictionPipeline();
            
            _shapePredictor = DlibDotNet.ShapePredictor.Deserialize(Application.streamingAssetsPath + "/shape_predictor_68_face_landmarks.dat");
            _frontalFaceDetector = Dlib.GetFrontalFaceDetector();
            
            coroutineRunning = false;
            faceCoroutineRunning = false;
            
            _affectiveManager = AffectiveManager.Instance;
            
        }
        
       
         public IEnumerator CheckForFaces()
         {
             faceCoroutineRunning = true;
             
             _faces = _frontalFaceDetector .Operator(img);

             yield return new WaitForSecondsRealtime(5.0f);
             
             faceCoroutineRunning = false;
         }

      
         public IEnumerator DetectFacialExpression(Mat mat)
         {
             coroutineRunning = true;
             
             var array = new byte[mat.Width * mat.Height * mat.ElemSize()];
             Marshal.Copy(mat.Data, array, 0, array.Length);

             img = Dlib.LoadImageData<RgbPixel>(array, (uint) mat.Height, (uint) mat.Width,
                 (uint) (mat.Width * mat.ElemSize()));

             if (!_faces.Any())
             {
                 _faces = _frontalFaceDetector.Operator(img);
                 goto here;
             }
             
             shape = _shapePredictor.Detect(img, _faces[0]);
             
             yield return new WaitForSecondsRealtime(calculationInterval);
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
             var inputData = new FeatureInputData
             {
                 leftEyebrow = leftEyebrow,
                 rightEyebrow = rightEyebrow,
                 leftLip = leftLip,
                 rightLip = rightLip,
                 lipWidth = lipWidth,
                 lipHeight = lipHeight
             };
             yield return new WaitForSecondsRealtime(calculationInterval);
             string emotion = _predictionEngine.Predict(inputData).Expression;
             AffectiveManager.Instance.SetCurrentEmotion(emotion);
             
             here:
             coroutineRunning = false;
         }
        
        private void OpenPredictionPipeline()
        {
            var predictionPipeline = _mlContext.Model.Load(Application.streamingAssetsPath + "/model.zip", out _);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<FeatureInputData, ExpressionPrediction>(predictionPipeline);
        }

        private static void CalculateLeftEyebrow()
        {
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 21, 39);

            for (uint i = 18; i <= 21; i++) result += CalculateDistance(shape, i, 39) / normalisationDistance;

            leftEyebrow = result;
            }

        private static void CalculateRightEyebrow()
        {
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 22, 42);

            for (uint i = 22; i <= 25; i++) result += CalculateDistance(shape, i, 42) / normalisationDistance;

            rightEyebrow = result;
        }

        private static void CalculateLeftLip()
        {
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 33, 51);

            for (uint i = 48; i <= 50; i++) result += CalculateDistance(shape, i, 33) / normalisationDistance;

            leftLip = result;
        }

        private static void CalculateRightLip()
        {
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 33, 51);

            for (uint i = 52; i <= 54; i++) result += CalculateDistance(shape, i, 33) / normalisationDistance;

            rightLip = result;
        }

        private static void CalculateLipWidth()
        {
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
