using System;
using HarmonyLib;
using MAX.Data;
using UnityEngine;

namespace FFTestMod;

public class ProcessDataPatch {
    [HarmonyPatch(typeof(ProcessDataManager), "ProcessAllData")]
    [HarmonyPrefix]
    static bool ProcessAllData_Prefix(float dt) {
        const float MAX_DT = 1f / 30f;
        dt = Mathf.Min(dt, 5f);
        while (!Mathf.Approximately(dt, 0f)) {
            var ddt = Mathf.Min(MAX_DT, dt);
            ProcessAllData_Original(ddt);
            dt -= ddt;
        }
        return false;
    }

    [HarmonyPatch(typeof(ProcessDataManager), "ProcessAllData")]
    [HarmonyReversePatch]
    static void ProcessAllData_Original(float dt) => throw new NotImplementedException();
}