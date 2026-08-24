using System.Collections.Generic;
using HarmonyLib;
using MAX.Data;

namespace FFTestMod;

public class ItemGroupOnAxisInstrPatch {
    [HarmonyPatch(typeof(ItemGroupOnAxisInstructionData), "Merge")]
    [HarmonyPrefix]
    static bool Merge_Prefix(
        ItemGroupOnAxisInstructionData __instance,
        ref List<LinkData> ___m_Inputs,
        ref ListItemData __result
    ) {
        ItemData inputA = ___m_Inputs[0].CollectItemInTransition();
        ItemData inputB = ___m_Inputs[1].CollectItemInTransition();
        int hashIdA = inputA.HashId;
        int hashIdB = inputB.HashId;
        ListItemData result =
            ItemDataRecipeReferences.Merge(__instance.Owner, true, __instance.AxisDimension, inputA, inputB);
        if (result != null) {
            __result = result;
            return false;
        }

        if (inputA is UnitItemData && inputB is UnitItemData) {
            result = CreateNewDimension(__instance, inputA, inputB);
        } else {
            ListItemData listItemA = inputA as ListItemData;
            ListItemData listItemB = inputB as ListItemData;
            if (listItemA != null && listItemB != null) {
                if (listItemA.Count > 0 &&
                    !listItemA[0].IsMergeableOnDimension(listItemB, __instance.AxisDimension))
                    result = CreateNewDimension(__instance, inputA, inputB);
                else if (listItemA.AxisDimension != __instance.AxisDimension)
                    result = CreateNewDimension(__instance, inputA, inputB);
                else
                    result = AddToList(__instance, listItemA, inputB, false);
            } else if (listItemA != null) {
                result = listItemA;
                if (inputB != null) {
                    if (result.AxisDimension != __instance.AxisDimension)
                        result = CreateNewDimension(__instance, listItemA, inputB);
                    else
                        result = AddToList(__instance, listItemA, inputB, false);
                }
            } else if (listItemB != null) {
                result = listItemB;
                if (inputA != null) {
                    if (result.AxisDimension != __instance.AxisDimension)
                        result = CreateNewDimension(__instance, inputA, listItemB);
                    else
                        // result = AddToList(__instance, listItemB, inputA, true);
                        result = CreateNewDimension(__instance, listItemB, inputA);
                }
            }
        }

        if (result is PointerListItemData _result)
            ItemDataRecipeReferences.RegisterRecipe(__instance.AxisDimension, hashIdA, hashIdB, _result);

        __result = result;
        return false;
    }

    private delegate ListItemData CreateNewDimensionDelegate(ItemGroupOnAxisInstructionData instance,
        params ItemData[] items);

    private static readonly CreateNewDimensionDelegate CreateNewDimension =
        AccessTools.MethodDelegate<CreateNewDimensionDelegate>(
            AccessTools.Method(typeof(ItemGroupOnAxisInstructionData), "CreateNewDimension")
        );

    private delegate ListItemData AddToListDelegate(ItemGroupOnAxisInstructionData instance, ListItemData listItem,
        ItemData itemToAdd, bool insertAtBegin);

    private static readonly AddToListDelegate AddToList =
        AccessTools.MethodDelegate<AddToListDelegate>(
            AccessTools.Method(typeof(ItemGroupOnAxisInstructionData), "AddToList")
        );

    [HarmonyPatch(typeof(ItemGroupOnAxisInstructionData), "AddToList")]
    [HarmonyPrefix]
    static bool AddToList_Prefix(ItemGroupOnAxisInstructionData __instance, ListItemData listItem, ItemData itemToAdd,
        bool insertAtBegin, ref ListItemData __result) {
        ListItemData list = null;
        switch (listItem)
        {
            case PointerListItemData _:
                List<ItemData> itemsData = new List<ItemData>(listItem.Items);
                if (insertAtBegin)
                    itemsData.Insert(0, itemToAdd);
                else
                    itemsData.Add(itemToAdd);
                list = ItemDataReferences.CreateListData(__instance.Owner, __instance.AxisDimension, listItem.Owner != null, out bool _, itemsData) as ListItemData;
                itemToAdd.Destroy();
                listItem.Destroy();
                break;
            case ReferenceListItemData referenceListItemData:
                referenceListItemData.Add(itemToAdd, insertAtBegin: insertAtBegin);
                itemToAdd.UnregisterFromOwner();
                list = listItem;
                list.UpdateLocalPos();
                break;
        }
        __result = list;
        return false;
    }
}