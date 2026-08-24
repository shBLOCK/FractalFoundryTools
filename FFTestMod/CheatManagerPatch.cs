using System.Linq;
using HarmonyLib;
using MAX.Data;
using MAX.Game;
using UnityEngine;

namespace FFTestMod;

public class CheatManagerPatch {
    [HarmonyPatch(typeof(CheatManager), "HandleCheatNestedScreenView")]
    [HarmonyPostfix]
    static void HandleCheatNestedScreenView_Prefix(ref float? ___m_InitialViewportRatioProximityThreshold) {
        if (___m_InitialViewportRatioProximityThreshold.HasValue) {
            Manager<NestedScreenManager>.Instance.ViewportRatioProximityThreshold = Input.GetKey(KeyCode.LeftShift) ? -1f : 0.0001f;
        }
    }
}