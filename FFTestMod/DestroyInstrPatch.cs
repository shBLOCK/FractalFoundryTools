using HarmonyLib;
using MAX.Data;

namespace FFTestMod;

public class DestroyInstrPatch {
    [HarmonyPatch(typeof(DestroyInstructionData), "ApplyInstruction")]
    [HarmonyPrefix]
    static void DestroyInstructionData_ApplyInstruction(DestroyInstructionData __instance) {
        var item = __instance.ItemInput.ItemInTransition;
        Plugin.Logger.LogDebug($"Destroying: {PluginUtils.prettyPrintItem(item)}\n{PluginUtils.prettyPrintItemDetailed(item)}");
    }
}