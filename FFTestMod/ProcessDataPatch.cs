using HarmonyLib;
using MAX.Game;

namespace FFTestMod;

public class ProcessDataPatch {
    [HarmonyPatch(typeof(DataManagerBehavior), "ProcessData")]
    [HarmonyPrefix]
    static void DataManagerBehavior_ProcessData_Prefix(DataManagerBehavior __instance) {
        // __instance.UseSafeProcessAllData = true;
    }
}