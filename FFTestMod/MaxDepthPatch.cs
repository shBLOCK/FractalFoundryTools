using HarmonyLib;
using MAX.Data;
using MAX.Game;
using UnityEngine;

namespace FFTestMod;

public class MaxDepthPatch {
    public const int MAX_DEPTH = 30;

    [HarmonyPatch(typeof(DataManager), nameof(DataManager.IsStackOverflow), MethodType.Getter)]
    [HarmonyPrefix]
    static bool DataManager_IsStackOverflow(DataManager __instance, ref bool __result) {
        __result = __instance.StackDepth > MAX_DEPTH;
        return false;
    }

    [HarmonyPatch(typeof(DataManager), nameof(DataManager.StackDepthRest), MethodType.Getter)]
    [HarmonyPrefix]
    static bool DataManager_StackDepthRest(DataManager __instance, ref int __result) {
        __result = MAX_DEPTH - __instance.StackDepth;
        return false;
    }

    [HarmonyPatch(typeof(DataManager), nameof(DataManager.MaxDepth), MethodType.Getter)]
    [HarmonyPrefix]
    static bool DataManager_MaxDepth(DataManager __instance, ref int __result) {
        if (__instance.GhostMaxDepth >= 0) {
            __result = Mathf.Min(__instance.GhostMaxDepth, MAX_DEPTH);
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(CustomInstruction), "DetectRercursionError")]
    [HarmonyPostfix]
    static void CustomInstruction_DetectRecursionError(CustomInstruction __instance, DataManager owner, ref ERecursionErrorType __result) {
        if (__result == ERecursionErrorType.StackOverflow) {
            if (owner.StackDepth < MAX_DEPTH) {
                __result = ERecursionErrorType.None;
            }
        }
    }
}