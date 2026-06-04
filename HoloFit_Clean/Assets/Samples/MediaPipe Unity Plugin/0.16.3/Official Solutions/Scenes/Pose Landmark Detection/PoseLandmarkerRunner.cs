using System.Collections;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.Sample.PoseLandmarkDetection
{
    public class PoseLandmarkerRunner : VisionTaskApiRunner<PoseLandmarker>
    {
        [SerializeField] private PoseLandmarkerResultAnnotationController _poseLandmarkerResultAnnotationController;

        private Experimental.TextureFramePool _textureFramePool;

        public readonly PoseLandmarkDetectionConfig config = new PoseLandmarkDetectionConfig();

        public PoseLandmarkerResult? LatestResult { get; private set; }

        public override void Stop()
        {
            base.Stop();
            _textureFramePool?.Dispose();
            _textureFramePool = null;
            LatestResult = null;
        }

        protected override IEnumerator Run()
        {
            // YAPAY ZEKAYI FOTOÐRAF (IMAGE) MODUNA KÝLÝTLÝYORUZ
            config.RunningMode = Tasks.Vision.Core.RunningMode.IMAGE;

            Debug.Log($"Running Mode = {config.RunningMode}");

            yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

            var options = config.GetPoseLandmarkerOptions(
                config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM
                    ? OnPoseLandmarkDetectionOutput
                    : null
            );

            taskApi = PoseLandmarker.CreateFromOptions(options, GpuManager.GpuResources);
            var imageSource = ImageSourceProvider.ImageSource;

            yield return imageSource.Play();

            if (!imageSource.isPrepared)
            {
                Logger.LogError(TAG, "Failed to start ImageSource, exiting...");
                yield break;
            }

            _textureFramePool = new Experimental.TextureFramePool(
                imageSource.textureWidth,
                imageSource.textureHeight,
                TextureFormat.RGBA32,
                10
            );

            screen.Initialize(imageSource);

            SetupAnnotationController(_poseLandmarkerResultAnnotationController, imageSource);
            _poseLandmarkerResultAnnotationController.InitScreen(imageSource.textureWidth, imageSource.textureHeight);

            var transformationOptions = imageSource.GetTransformationOptions();
            var flipHorizontally = transformationOptions.flipHorizontally;
            var flipVertically = transformationOptions.flipVertically;

            var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: 0);

            AsyncGPUReadbackRequest req = default;
            var waitUntilReqDone = new WaitUntil(() => req.done);
            var waitForEndOfFrame = new WaitForEndOfFrame();
            var result = PoseLandmarkerResult.Alloc(options.numPoses, options.outputSegmentationMasks);

            var canUseGpuImage =
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 &&
                GpuManager.GpuResources != null;

            using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

            while (true)
            {
                if (isPaused)
                {
                    yield return new WaitWhile(() => isPaused);
                }

                if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                Mediapipe.Image image;
                switch (config.ImageReadMode)
                {
                    case ImageReadMode.GPU:
                        if (!canUseGpuImage)
                        {
                            throw new System.Exception("ImageReadMode.GPU is not supported");
                        }
                        textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                        image = textureFrame.BuildGPUImage(glContext);
                        yield return waitForEndOfFrame;
                        break;

                    case ImageReadMode.CPU:
                        yield return waitForEndOfFrame;
                        textureFrame.ReadTextureOnCPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                        image = textureFrame.BuildCPUImage();
                        textureFrame.Release();
                        break;

                    case ImageReadMode.CPUAsync:
                    default:
                        req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                        yield return waitUntilReqDone;

                        if (req.hasError)
                        {
                            Debug.LogWarning("Failed to read texture from the image source");
                            continue;
                        }

                        image = textureFrame.BuildCPUImage();
                        textureFrame.Release();
                        break;
                }

                switch (taskApi.runningMode)
                {
                    case Tasks.Vision.Core.RunningMode.IMAGE:
                        if (taskApi.TryDetect(image, imageProcessingOptions, ref result))
                        {
                            LatestResult = result;
                            // ========================================================================
                            // GÖRSEL ÇÝZÝMÝ KAPATTIK (DrawNow yorum satýrý oldu)
                            // ========================================================================
                            // _poseLandmarkerResultAnnotationController.DrawNow(result);
                        }
                        else
                        {
                            LatestResult = null;
                            // _poseLandmarkerResultAnnotationController.DrawNow(default);
                        }
                        DisposeAllMasks(result);
                        break;

                    case Tasks.Vision.Core.RunningMode.VIDEO:
                        if (taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions, ref result))
                        {
                            LatestResult = result;
                            // ========================================================================
                            // GÖRSEL ÇÝZÝMÝ KAPATTIK (DrawNow yorum satýrý oldu)
                            // ========================================================================
                            // _poseLandmarkerResultAnnotationController.DrawNow(result);
                        }
                        else
                        {
                            LatestResult = null;
                            // _poseLandmarkerResultAnnotationController.DrawNow(default);
                        }
                        DisposeAllMasks(result);
                        break;

                    case Tasks.Vision.Core.RunningMode.LIVE_STREAM:
                        taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
                        break;
                }
            }
        }

        private void OnPoseLandmarkDetectionOutput(PoseLandmarkerResult result, Mediapipe.Image image, long timestamp)
        {
            LatestResult = result;
            // ========================================================================
            // GÖRSEL ÇÝZÝMÝ KAPATTIK (DrawLater yorum satýrý oldu)
            // ========================================================================
            // _poseLandmarkerResultAnnotationController.DrawLater(result);
            DisposeAllMasks(result);
        }

        private void DisposeAllMasks(PoseLandmarkerResult result)
        {
            if (result.segmentationMasks == null)
            {
                return;
            }

            foreach (var mask in result.segmentationMasks)
            {
                mask?.Dispose();
            }
        }

        // =========================================================================================
        // KUSURSUZ STATÝK FOTOÐRAF ANALÝZ MOTORU
        // =========================================================================================
        public PoseLandmarkerResult? AnalyzeStaticImage(Texture2D inputTexture)
        {
            if (taskApi == null)
            {
                Debug.LogError("Yapay Zeka Motoru henüz hazýr deðil!");
                return null;
            }

            var textureFrame = new Experimental.TextureFrame(inputTexture.width, inputTexture.height, TextureFormat.RGBA32);
            textureFrame.ReadTextureOnCPU(inputTexture, flipHorizontally: false, flipVertically: false);

            using var image = textureFrame.BuildCPUImage();
            var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: 0);

            var result = PoseLandmarkerResult.Alloc(config.NumPoses, config.OutputSegmentationMasks);

            if (taskApi.TryDetect(image, imageProcessingOptions, ref result))
            {
                textureFrame.Release();
                return result;
            }

            textureFrame.Release();
            return null;
        }
    }
}