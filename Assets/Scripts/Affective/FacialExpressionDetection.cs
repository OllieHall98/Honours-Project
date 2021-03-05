using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DlibDotNet;
using Microsoft.ML;
using Microsoft.ML.Data;
using UnityEngine;
using DlibDotNet.Extensions;

namespace Affective
{
    public class FacialExpressionDetection : MonoBehaviour
    {
        private FacialDetection _facialDetection;
        private AffectiveManager _affectiveManager = null;

        private Thread _thread;

        private string _emotion;

        public Array2D<RgbPixel> ImageData;

        private void Awake()
        {
            _affectiveManager = GetComponent<AffectiveManager>();
            
            _facialDetection = new FacialDetection();
        }

        private void Start()
        {
            _facialDetection.Init();
            
            _thread = new Thread(Yeet) {Name = "fea thread"};
        }

        public void DetectFacialExpression(Array2D<RgbPixel> imageData)
        {
            ImageData = imageData;
            
            const int interval = 100;
            var timer = new System.Timers.Timer(interval) { AutoReset = false };

            timer.Elapsed += ((sender, eventArgs) =>
            {
                //Debug.Log("Joined thread");
                _thread.Join();
           
            });

            if (_thread != null && !_thread.IsAlive)
            {
                // Debug.Log("Started thread");
                _thread = new Thread(Yeet);
                _thread.Start();
                timer.Start();
            }

            _affectiveManager.SetCurrentEmotion(_emotion);
        }

        private void Yeet()
        {
            _emotion = _facialDetection.TestCustomImage(ImageData);
        }

        private void OnApplicationQuit()
        {
            if(_thread != null && _thread.IsAlive) _thread.Abort();
        }
    }
    
    public class FacialDetection
    {
        private enum Datatype { Training, Testing };
        Datatype currentDataType = Datatype.Training;
        private System.Drawing.Bitmap _bmp;

        private MLContext _mlContext;
        private PredictionEngine<FeatureInputData, ExpressionPrediction> _predictionEngine;
        private ShapePredictor _sp;
        private FrontalFaceDetector _fd;
        public ExpressionPrediction Prediction;

        // file paths
        private string currentFilePath = "";
        private string trainingOutput = @"training_feature_vectors.csv";
        private string testingOutput = @"testing_feature_vectors.csv";

        public void Init()
        {
            _mlContext = new MLContext();

            OpenPredictionPipeline();

            _sp = ShapePredictor.Deserialize(Application.streamingAssetsPath + "/shape_predictor_68_face_landmarks.dat");
            _fd = Dlib.GetFrontalFaceDetector();
        }

        private void GenerateMetrics(IDataView dataView)
        {
            ITransformer model = _mlContext.Model.Load("model.zip", out _);

            var testMetrics = _mlContext.MulticlassClassification.Evaluate(model.Transform(dataView));

            string metrics = ($"* Metrics for Multi-class Classification model - Test Data\n") +
                             ($"* MicroAccuracy:    {testMetrics.MicroAccuracy:0.###}\n") +
                             ($"* MacroAccuracy:    {testMetrics.MacroAccuracy:0.###}\n") +
                             ($"* LogLoss:          {testMetrics.LogLoss:#.###}\n") +
                             ($"* LogLossReduction: {testMetrics.LogLossReduction:#.###}\n") +
                             ($"* {testMetrics.ConfusionMatrix.GetFormattedConfusionTable():#.###}");

            File.WriteAllText("MetricData.txt", metrics);

            //Form1.DisplayMetrics(metrics);

        }

        public void TrainModel()
        {
            _mlContext = new MLContext();

            IDataView dataView = _mlContext.Data.LoadFromTextFile<FeatureInputData>(trainingOutput, hasHeader: true, separatorChar: ',');

            //var trainTestData = RearrangeData(dataView);

            var featureVectorName = "Features";
            var labelColumnName = "Label";
            var pipeline = _mlContext
                .Transforms.Conversion.MapValueToKey(inputColumnName: nameof(FeatureInputData.label), outputColumnName: labelColumnName).
                Append(_mlContext.Transforms.Concatenate(featureVectorName,
                    nameof(FeatureInputData.leftEyebrow),
                    nameof(FeatureInputData.rightEyebrow),
                    nameof(FeatureInputData.leftLip),
                    nameof(FeatureInputData.rightLip),
                    nameof(FeatureInputData.lipWidth),
                    nameof(FeatureInputData.lipHeight)))
                .Append(_mlContext.Transforms.NormalizeMinMax(featureVectorName, featureVectorName))
                .AppendCacheCheckpoint(_mlContext)
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName, featureVectorName))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = pipeline.Fit(dataView);

            using var fileStream = new FileStream("model.zip", FileMode.Create, FileAccess.Write, FileShare.Write);
            _mlContext.Model.Save(model, dataView.Schema, fileStream);

            //GenerateMetrics(trainTestData.TestSet);


        }

        //static private DataOperationsCatalog.TrainTestData RearrangeData(IDataView testDataView)
        //{
        //    //IDataView shuffledData = mlContext.Data.ShuffleRows(testDataView);

        //    // split the data 
        //    //return (mlContext.Data.TrainTestSplit(testDataView, testFraction: 0.2));
        //}

        public void CreateFeatureVectors()
        {
            string output = currentDataType == Datatype.Testing ? testingOutput : trainingOutput;

            string[] dirs = Directory.GetFiles(currentFilePath, "*.*", SearchOption.AllDirectories);

            // ReSharper disable once CommentTypo
            // Set up Dlib Face Detector
            using var fd = Dlib.GetFrontalFaceDetector();
            using var sp = ShapePredictor.Deserialize("shape_predictor_68_face_landmarks.dat");
            const string header = "LeftEyebrow,RightEyebrow,LeftLip,RightLip,LipWidth,LipHeight,Label\n";

            // Create the CSV file and fill in the first line with the header
            File.WriteAllText(output, header);

            foreach (string dir in dirs)
            {
                // call function that sets the label based on what the filename contains
                string label = DetermineLabel(dir);

                // load input image
                if (!(dir.EndsWith("png") || dir.EndsWith("jpg")))
                    continue;

                var img = Dlib.LoadImage<RgbPixel>(dir);

                // find all faces in the image
                var faces = this._fd.Operator(img);

                // for each face draw over the facial landmarks
                foreach (var face in faces)
                {
                    // Write to the console displaying the progress and current emotion
                    //Form1.SetProgress(faceCount, dirs.Length - 1);

                    // find the landmark points for this face
                    var shape = sp.Detect(img, face);

                    for (var i = 0; i < shape.Parts; i++)
                    {
                        RgbPixel colour = new RgbPixel(255, 255, 255);
                        var point = shape.GetPart((uint)i);
                        var rect = new Rectangle(point);
                        Dlib.DrawRectangle(img, rect, color: colour, thickness: 2);
                    }

                    //SetFormImage(img);

                    float leftEyebrow = CalculateLeftEyebrow(shape);
                    float rightEyebrow = CalculateRightEyebrow(shape);
                    float leftLip = CalculateLeftLip(shape);
                    float rightLip = CalculateRightLip(shape);
                    float lipWidth = CalculateLipWidth(shape);
                    float lipHeight = CalculateLipHeight(shape);

                    using var file = new StreamWriter(output, true);
                    file.WriteLine(leftEyebrow + "," + rightEyebrow + "," + leftLip + "," + rightLip + "," + lipWidth + "," + lipHeight + "," + label);

                    // Increment count used for console output
                }
            }

            if (currentDataType == Datatype.Testing)
            {
                var testDataView = _mlContext.Data.LoadFromTextFile<FeatureInputData>(output, hasHeader: true, separatorChar: ',');
                GenerateMetrics(testDataView);
            }

            //Form1.HideImage();
        }
        
        public string TestCustomImage(Array2D<RgbPixel> image)
        {
            // // convert byte data into bitmap for DlibDotNet to process
            // using (var memoryStream = new MemoryStream((byte[])data))
            // {
            //     _bmp = new System.Drawing.Bitmap(memoryStream);
            // }

            // find all faces in the image
            var faces = _fd.Operator(image);

            if (!faces.Any())
                return "N/A";

            // find the landmark points for this face
            var shape = _sp.Detect(image, faces[0]);

            FeatureInputData inputData = new FeatureInputData
            {
                leftEyebrow = CalculateLeftEyebrow(shape),
                rightEyebrow = CalculateRightEyebrow(shape),
                leftLip = CalculateLeftLip(shape),
                rightLip = CalculateRightLip(shape),
                lipWidth = CalculateLipWidth(shape),
                lipHeight = CalculateLipHeight(shape)
            };

            return _predictionEngine.Predict(inputData).Expression;
        }


        private Array2D<byte> ToArray2D(System.Drawing.Bitmap bitmap)
        {
            Int32 stride;
            Byte[] data;
            // Removes unnecessary getter calls.
            Int32 width = bitmap.Width;
            Int32 height = bitmap.Height;
            // 'using' block to properly dispose temp image.
            using (System.Drawing.Bitmap grayImage = MakeGrayscale(bitmap))
            {
                System.Drawing.Imaging.BitmapData bits = grayImage.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                stride = bits.Stride;
                Int32 length = stride * height;
                data = new Byte[length];
                Marshal.Copy(bits.Scan0, data, 0, length);
                grayImage.UnlockBits(bits);
            }
            // Constructor is (rows, columns), so (height, width)
            Array2D<Byte> array = new Array2D<Byte>(height, width);
            Int32 offset = 0;
            for (Int32 y = 0; y < height; y++)
            {
                // Offset variable for processing one line
                Int32 curOffset = offset;
                // Get row in advance
                Array2D<Byte>.Row<Byte> curRow = array[y];
                for (Int32 x = 0; x < width; x++)
                {
                    curRow[x] = data[curOffset]; // Should be the Blue component.
                    curOffset += 4;
                }
                // Stride is the actual data length of one line. No need to calculate that;
                // not only is it already given by the BitmapData object, but in some situations
                // it may differ from the actual data length. This also saves processing time
                // by avoiding multiplications inside each loop.
                offset += stride;
            }
            return array;
        }

        private System.Drawing.Bitmap MakeGrayscale(System.Drawing.Bitmap original)
        {
            //create a blank bitmap the same size as original
            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(original);

            //newBitmap.SetResolution(original.Width, original.Height);

            //get a graphics object from the new image
            using System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBitmap);
            //create the grayscale ColorMatrix
            System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix(
                new[]
                {
                    new[] {.3f, .3f, .3f, 0, 0},
                    new[] {.59f, .59f, .59f, 0, 0},
                    new[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

            //create some image attributes
            using System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes();
            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix

            g.DrawImage(original, new System.Drawing.Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, System.Drawing.GraphicsUnit.Pixel, attributes);

            return newBitmap;
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

        static float CalculateLeftEyebrow(FullObjectDetection shape)
        {
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 21, 39);

            for (uint i = 18; i <= 21; i++) result += CalculateDistance(shape, i, 39) / normalisationDistance;

            return result;
        }

        static float CalculateRightEyebrow(FullObjectDetection shape)
        {
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 22, 42);

            for (uint i = 22; i <= 25; i++) result += CalculateDistance(shape, i, 42) / normalisationDistance;

            return result;
        }

        static float CalculateLeftLip(FullObjectDetection shape)
        {
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 33, 51);

            for (uint i = 48; i <= 50; i++) result += CalculateDistance(shape, i, 33) / normalisationDistance;

            return result;
        }

        static float CalculateRightLip(FullObjectDetection shape)
        {
            float result = 0;
            float normalisationDistance = CalculateDistance(shape, 33, 51);

            for (uint i = 52; i <= 54; i++) result += CalculateDistance(shape, i, 33) / normalisationDistance;

            return result;
        }

        static float CalculateLipWidth(FullObjectDetection shape)
        {
            return CalculateDistance(shape, 48, 54) / CalculateDistance(shape, 33, 51);
        }

        static float CalculateLipHeight(FullObjectDetection shape)
        {
            return CalculateDistance(shape, 51, 57) / CalculateDistance(shape, 33, 51);
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