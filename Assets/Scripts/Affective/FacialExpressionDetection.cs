using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DlibDotNet;
using Microsoft.ML;
using Microsoft.ML.Data;
using UnityEngine;

using OpenCvSharp;
using ShapePredictor = DlibDotNet.ShapePredictor;

namespace Affective
{
    public class FacialDetectionOld
    {
        private MLContext _mlContext;
        private PredictionEngine<FeatureInputData, ExpressionPrediction> _predictionEngine;
        private ShapePredictor _shapePredictor;
        private FrontalFaceDetector _frontalFaceDetector;

        private static float _leftEyebrow;
        private static float _rightEyebrow;
        private static float _leftLip;
        private static float _rightLip;
        private static float _lipWidth;
        private static float _lipHeight;

        private Thread _leftEyebrowThread;
        private Thread _rightEyebrowThread;
        private Thread _leftLipThread;
        private Thread _rightLipThread;
        private Thread _lipWidthThread;
        private Thread _lipHeightThread;

        private Rectangle[] faces;
        private Array2D<BgrPixel> cimg;

        public void Init()
        {
            faces = new Rectangle[0];
            cimg = new Array2D<BgrPixel>();
            
            _leftEyebrowThread = new Thread(CalculateLeftEyebrow);
            _rightEyebrowThread = new Thread(CalculateRightEyebrow);
            _leftLipThread = new Thread(CalculateLeftLip);
            _rightLipThread = new Thread(CalculateRightLip);
            _lipWidthThread = new Thread(CalculateLipWidth);
            _lipHeightThread = new Thread(CalculateLipHeight);
            
            _mlContext = new MLContext();

            OpenPredictionPipeline();

            _shapePredictor = ShapePredictor.Deserialize(Application.streamingAssetsPath + "/shape_predictor_68_face_landmarks.dat");
            _frontalFaceDetector = Dlib.GetFrontalFaceDetector();
            
        }

        public void FindFaces()
        {
            // find all faces in the image
            faces = _frontalFaceDetector.Operator(cimg);
        }
        
        public string DetectFacialLandmarks(OpenCvSharp.Mat image)
        {
            cimg = GenerateDlibArray2D(image);
            
            if (!faces.Any())
            {
                FindFaces();
                return "N/A";
            }

            // find the landmark points for this face
            var shape = _shapePredictor.Detect(cimg, faces[0]);
            
            new Thread(() => CalculateLeftEyebrow(shape)).Start();
            new Thread(() => CalculateRightEyebrow(shape)).Start();
            new Thread(() => CalculateLeftLip(shape)).Start();
            new Thread(() => CalculateRightLip(shape)).Start();
            new Thread(() => CalculateLipWidth(shape)).Start();
            new Thread(() => CalculateLipHeight(shape)).Start();

            FeatureInputData inputData = new FeatureInputData
            {
                leftEyebrow = _leftEyebrow,
                rightEyebrow = _rightEyebrow,
                leftLip = _leftLip,
                rightLip = _rightLip,
                lipWidth = _lipWidth,
                lipHeight = _lipHeight
            };
            return _predictionEngine.Predict(inputData).Expression;
        }
        
        
        Array2D<BgrPixel> GenerateDlibArray2D(OpenCvSharp.Mat image)
        {
            var array = new byte[image.Width * image.Height * image.ElemSize()];
            
            Marshal.Copy(image.Data, array, 0 , array.Length);
            
            return  Dlib.LoadImageData<BgrPixel>(array, (uint) image.Height, (uint) image.Width,
                (uint) (image.Width * image.ElemSize()));
            
        }

        private void OpenPredictionPipeline()
        {
            ITransformer predictionPipeline = _mlContext.Model.Load(Application.streamingAssetsPath + "/model.zip", out _);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<FeatureInputData, ExpressionPrediction>(predictionPipeline);
        }

        string DetermineLabel(string dir)
        {
            if (dir.Contains("neutral")) return "neutral";
            if (dir.Contains("surprise")) return "surprise";
            if (dir.Contains("sadness")) return "sadness";
            if (dir.Contains("fear")) return "fear";
            if (dir.Contains("disgust")) return "disgust";
            if (dir.Contains("anger")) return "anger";
            if (dir.Contains("joy")) return "joy";
            else return "N/A";
        }

        public void CalculateLeftEyebrow(object data)
        {
            var shape = (FullObjectDetection) data;

            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 21, 39);

            for (uint i = 18; i <= 21; i++) result += CalculateDistance(shape, i, 39) / normalisationDistance;

            _leftEyebrow = result;
            }

        static void CalculateRightEyebrow(object data)
        {
            var shape = (FullObjectDetection) data;
            
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 22, 42);

            for (uint i = 22; i <= 25; i++) result += CalculateDistance(shape, i, 42) / normalisationDistance;

            _rightEyebrow = result;
        }

        static void CalculateLeftLip(object data)
        {
            var shape = (FullObjectDetection) data;
            
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 33, 51);

            for (uint i = 48; i <= 50; i++) result += CalculateDistance(shape, i, 33) / normalisationDistance;

            _leftLip = result;
        }

        static void CalculateRightLip(object data)
        {
            var shape = (FullObjectDetection) data;
            
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 33, 51);

            for (uint i = 52; i <= 54; i++) result += CalculateDistance(shape, i, 33) / normalisationDistance;

            _rightLip = result;
        }

        static void CalculateLipWidth(object data)
        {
            var shape = (FullObjectDetection) data;
            _lipWidth = CalculateDistance(shape, 48, 54) / CalculateDistance(shape, 33, 51);
        }

        static void CalculateLipHeight(object data)
        {
            var shape = (FullObjectDetection) data;
            _lipHeight = CalculateDistance(shape, 51, 57) / CalculateDistance(shape, 33, 51);
        }

        static float CalculateDistance(FullObjectDetection shape, uint point1, uint point2)
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
            public string label { get; set; }
        }

        public class ExpressionPrediction
        {
            [ColumnName("PredictedLabel")]
            public string Expression { get; set; }
        }
    }
}