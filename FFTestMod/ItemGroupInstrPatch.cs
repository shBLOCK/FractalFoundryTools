using HarmonyLib;
using MAX.Data;

namespace FFTestMod;

public class ItemGroupInstrPatch {
    [HarmonyPatch(typeof(ItemGroupOnAxisInstructionData), "AddToList")]
    [HarmonyPrefix]
    static void ItemGroupOnAxisInstructionData_AddToListPrefix(ItemGroupOnAxisInstructionData __instance,
        ref ListItemData listItem, ref ItemData itemToAdd, ref bool insertAtBegin) {
        Plugin.Logger.LogInfo($"AddToList({listItem}, {itemToAdd}, {insertAtBegin})");
    }
}