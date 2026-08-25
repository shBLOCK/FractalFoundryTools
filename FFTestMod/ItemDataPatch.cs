// using System.Collections.Generic;
// using System.Linq;
// using HarmonyLib;
// using MAX.Data;
//
// namespace FFTestMod;
//
// public class ItemDataPatch {
//     [HarmonyPatch(typeof(ReferenceListItemData), nameof(ReferenceListItemData.CreatePointer))]
//     [HarmonyPrefix]
//     static bool ReferenceListItemData_CreatePointer_Prefix(ReferenceListItemData __instance, DataManager owner, bool registerToDataManager, ref ItemData __result) {
//         __result = __instance;
//         if (registerToDataManager) {
//             __instance.RegisterToOwner(owner);
//         }
//         return false;
//     }
//     
//     [HarmonyPatch(typeof(UnitItemData), nameof(UnitItemData.CreatePointer))]
//     [HarmonyPrefix]
//     static bool UnitItemData_CreatePointer_Prefix(UnitItemData __instance, DataManager owner, bool registerToDataManager, ref ItemData __result) {
//         __result = __instance;
//         if (registerToDataManager) {
//             __instance.RegisterToOwner(owner);
//         }
//         return false;
//     }
//
//     [HarmonyPatch(typeof(ItemDataReferences), nameof(ItemDataReferences.CreateUnitData))]
//     [HarmonyPrefix]
//     static bool CreateUnitData_Prefix(DataManager owner,
//         EItemType itemType,
//         bool registerToDataManager,
//         out bool wasCreated, ref ItemData __result) {
//         __result = new UnitItemData(owner, itemType, registerToDataManager);
//         wasCreated = true;
//         return false;
//     }
//     
//     [HarmonyPatch(typeof(ItemDataReferences), nameof(ItemDataReferences.CreateListData))]
//     [HarmonyPrefix]
//     static bool CreateListData_Prefix(DataManager owner,
//         EAxisDimension axisDimension,
//         bool registerToDataManager,
//         out bool wasCreated,
//         IEnumerable<ItemData> itemsData,
//         bool registerReference, ref ItemData __result) {
//         __result = new ReferenceListItemData(owner, axisDimension, registerToDataManager, itemsData.ToArray());
//         __result.UpdateLocalPos();
//         wasCreated = true;
//         return false;
//     }
//
//     [HarmonyPatch(typeof(ItemDataRecipeReferences), nameof(ItemDataRecipeReferences.Merge))]
//     [HarmonyPrefix]
//     static bool Merge_Prefix(ref ListItemData __result) {
//         __result = null;
//         return false;
//     }
//
//     [HarmonyPatch(typeof(ItemDataRecipeReferences), nameof(ItemDataRecipeReferences.Split))]
//     [HarmonyPrefix]
//     static bool Split_Prefix(ref SplitResultData<ItemData>? __result) {
//         __result = null;
//         return false;
//     }
//
//     [HarmonyPatch(typeof(ItemDataRecipeReferences), nameof(ItemDataRecipeReferences.RegisterRecipe))]
//     [HarmonyPrefix]
//     static bool RegisterRecipe_Prefix() {
//         return false;
//     }
//
//     [HarmonyPatch(typeof(ItemDataReferences), nameof(ItemDataReferences.CreateData))]
//     [HarmonyPrefix]
//     static bool CreateData_Prefix(ref ItemData __result) {
//         __result = null;
//         return false;
//     }
//
//     [HarmonyPatch(typeof(ItemMinerInstructionData), "ApplyInstruction")]
//     [HarmonyPrefix]
//     static void ItemMinerInstructionData_ApplyInstruction_Prefix(ItemMinerInstructionData __instance) {
//         __instance.ResetCachedData();
//     }
// }