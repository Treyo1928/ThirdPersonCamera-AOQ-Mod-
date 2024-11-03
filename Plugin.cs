using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace TreysThirdPersonCamera
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid = "aoq.treysthirdpersoncamera";
        public const string pluginName = "Treys Third Person Camera";
        public const string pluginVersion = "1.0.0";
        internal static BepInEx.Logging.ManualLogSource PluginLogger;

        private static Camera mainCamera;
        private static Camera thirdPersonCamera;
        public static RenderTexture renderTexture;

        private static float cameraDistance = 3f;
        private static float orbitAngle = 0f;
        private static float pitch = 15f; // Initial fixed pitch angle
        private static bool isPitchStatic = false;

        public void Awake()
        {
            PluginLogger = Logger;

            Logger.LogInfo("Plugin TreysThirdPersonCamera is loaded!");

            Harmony harmony = new Harmony("TreysThirdPersonCamera");
            harmony.PatchAll();

            Logger.LogInfo("Harmony patches applied.");
        }

        void Start()
        {
            SetupCameras();
        }

        //void Update()
        //{
        //    HandleInput();
        //    UpdateCamPosition();
        //}

        public static void HandleInput()
        {
            // Adjust camera distance with W and S
            if (Input.GetKey(KeyCode.W)) cameraDistance -= 0.1f;
            if (Input.GetKey(KeyCode.S)) cameraDistance += 0.1f;

            // Toggle pitch lock with P
            if (Input.GetKeyDown(KeyCode.P)) isPitchStatic = !isPitchStatic;

            // Adjust orbit angle with A and D
            if (Input.GetKey(KeyCode.A)) orbitAngle += 1f;
            if (Input.GetKey(KeyCode.D)) orbitAngle -= 1f;

            // Adjust pitch with arrow keys if pitch is static
            if (isPitchStatic)
            {
                if (Input.GetKey(KeyCode.UpArrow)) pitch += 1f;
                if (Input.GetKey(KeyCode.DownArrow)) pitch -= 1f;
            }
        }

        private static void SetupCameras()
        {
            PluginLogger.LogInfo("Setting up cameras");

            mainCamera = Camera.main;
            PluginLogger.LogInfo("Set main camera");

            // Create a new camera for third-person view
            GameObject thirdPersonCameraObject = new GameObject("ThirdPersonCamera");
            thirdPersonCamera = thirdPersonCameraObject.AddComponent<Camera>();

            renderTexture = new RenderTexture(Display.main.systemWidth, Display.main.systemHeight, 24);
            thirdPersonCamera.targetTexture = renderTexture;
            PluginLogger.LogInfo("Set target texture");

            Screen.fullScreen = true;
            SetupMonitorDisplay();
        }

        private static void SetupMonitorDisplay()
        {
            GameObject canvasObject = new GameObject("ThirdPersonCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject rawImageObject = new GameObject("ThirdPersonImage");
            RawImage rawImage = rawImageObject.AddComponent<RawImage>();
            rawImage.texture = renderTexture;

            RectTransform rectTransform = rawImage.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(Display.main.systemWidth, Display.main.systemHeight);
            rawImageObject.transform.SetParent(canvasObject.transform, false);
        }

        public static void UpdateCamPosition()
        {
            try
            {
                Vector3 playerPosition = mainCamera.transform.position;
                Quaternion playerRotation = mainCamera.transform.rotation;

                // Calculate the offset with a fixed pitch if isPitchStatic is enabled
                Vector3 offset;
                if (isPitchStatic)
                {
                    // Calculate the offset based on the static pitch
                    offset = Quaternion.Euler(mainCamera.transform.eulerAngles.x, orbitAngle, 0) * Vector3.back * cameraDistance;
                    thirdPersonCamera.transform.position = playerPosition + offset + new Vector3(0, pitch, 0);
                    thirdPersonCamera.transform.rotation = Quaternion.Euler(pitch, orbitAngle + playerRotation.eulerAngles.y, 0);
                }
                else
                {
                    // Offset with dynamic pitch
                    offset = playerRotation * Quaternion.Euler(mainCamera.transform.eulerAngles.x, orbitAngle, 0) * Vector3.back * cameraDistance;
                    thirdPersonCamera.transform.position = playerPosition + offset;
                    thirdPersonCamera.transform.LookAt(playerPosition);
                }
            }
            catch
            {
                SetupCameras();
            }
        }
    }
}