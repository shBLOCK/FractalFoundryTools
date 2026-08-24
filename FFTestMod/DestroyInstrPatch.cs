using HarmonyLib;
using MAX.Data;

namespace FFTestMod;

public class DestroyInstrPatch {
    [HarmonyPatch(typeof(DestroyInstructionData), "ApplyInstruction")]
    [HarmonyPrefix]
    static bool DestroyInstructionData_ApplyInstruction(DestroyInstructionData __instance) {
        var item = __instance.ItemInput.CollectItemInTransition();
        // Plugin.Logger.LogDebug($"Destroying: {PluginUtils.prettyPrintItem(item)}\n{PluginUtils.prettyPrintItemDetailed(item)}");
        item.Destroy();
        return false;
    }
}