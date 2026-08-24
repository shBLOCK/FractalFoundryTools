using System;
using HarmonyLib;
using MAX.Data;
using MAX.Game;
using UnityEngine;

namespace FFTestMod;

public class ItemRendererPatch {
    public static void Patch(Harmony harmony) {
        harmony.PatchAll(typeof(ItemRendererPatch));
        harmony.Patch(
            AccessTools.Method(
                AccessTools.Inner(typeof(ItemRenderer), "PopulateBufferParallelFor"),
                "CreateDrawData"
            ),
            prefix: new HarmonyMethod(typeof(ItemRendererPatch), nameof(CreateDrawData_Prefix))
        );
        // harmony.Patch(
        //     AccessTools.Method(
        //         AccessTools.Inner(typeof(ItemRenderer), "PopulateBufferParallelFor"),
        //         "UpdateUnitDrawData"
        //     ),
        //     prefix: new HarmonyMethod(typeof(ItemRendererPatch), nameof(UpdateUnitDrawData_Prefix))
        // );
    }

    static string vec3ToStringNoRounding(Vector3 vec3) => $"({vec3.x}, {vec3.y}, {vec3.z})";

    static bool CreateDrawData_Prefix(int parentIdx, Vector3 pos, Vector3 scale, EItemType type, ref UnitDrawData __result) {
        var pos2i = Vector3Int.RoundToInt(pos * 2f);
        int data1 = 0;
        data1 |= ((pos2i.x + 256) >> 1) & 0xFF;
        data1 |= (((pos2i.y + 256) >> 1) & 0xFF) << 8;
        data1 |= (((pos2i.z + 256) >> 1) & 0xFF) << 16;
        data1 |= (~((pos2i.x & 0b1) | ((pos2i.y & 0b1) << 1) | ((pos2i.z & 0b1) << 2)) & 0b111) << 24;
        data1 |= (int)type << 28;
        __result = new UnitDrawData {
            PackedData1 = (uint)data1,
            PackedData2 =
                (uint)((byte)(scale.x - 1.0) | (byte)(scale.y - 1.0) << 8 | (byte)(scale.z - 1.0) << 16 /*0x10*/)
        };
        return false;
    }

    // static bool UpdateUnitDrawData_Prefix() {
    //     return false;
    // }
}