using HarmonyLib;
using UnityEngine;

namespace TreysThirdPersonCamera
{
    [HarmonyPatch(typeof(OVRCameraRig), "UpdateAnchors")]
    class UpdateAnchorsPatch
    {
        static void Prefix(OVRCameraRig __instance)
        {
            Plugin.PluginLogger.LogInfo("CameraRenderer Ran");
            // Ensure the third-person camera is updated to follow the VR camera
            Plugin.HandleInput();
            Plugin.UpdateCamPosition();
        }
    }
}
