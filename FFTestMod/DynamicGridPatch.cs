using System;
using HarmonyLib;
using MAX.Data;
using MAX.Game;
using UnityEngine;
using ZLinq;

namespace FFTestMod;

public class DynamicGridPatch {
    static EItemType floorShader(Vector2 pos) {
        var z = pos * 1.5f;
        float t = Time.time * 1f;
        var c = new Vector2(0f, 0.75f);
        c += new Vector2(Mathf.Cos(t), Mathf.Sin(t)) * 0.05f;
        int i = 1;
        int max_iter = 35;
        for (; i <= max_iter; i++) {
            z = new Vector2(z.x * z.x - z.y * z.y, 2f * z.x * z.y) + c;
            if (z.sqrMagnitude > 4f) break;
        }

        if (i <= 25) return EItemType.Blue;
        if (i <= 30) return EItemType.Green;
        if (i <= 35) return EItemType.White;
        return EItemType.Yellow;
    }

    [HarmonyPatch(typeof(GridManager), "CacheMaskAndItemTypes")]
    [HarmonyPrefix]
    static bool GridManager_CacheMaskAndItemTypesPrefix(GridManager __instance, ref Texture2D ___m_MinableMask,
        ref EItemType[] ___m_CachedItemTypes) {
        var size = new Vector2Int(___m_MinableMask.width, ___m_MinableMask.height);
        ___m_CachedItemTypes = new EItemType[size.x * size.y];
        for (int index = 0; index < ___m_CachedItemTypes.Length; ++index) {
            // ReSharper disable once PossibleLossOfFraction
            var pos = new Vector2(index % size.x, index / size.x) / size;
            pos = pos * 2f - Vector2.one;
            ___m_CachedItemTypes[index] = floorShader(pos);
        }

        AccessTools.Method(typeof(GridManager), "CreateMineableTexture").Invoke(__instance, []);
        return false;
    }

    class MinerUpdater : MonoBehaviour {
        private ItemMinerInstruction miner;

        private void Awake() {
            miner = GetComponent<ItemMinerInstruction>();
        }

        private void Update() {
            var probs = AccessTools.Method(typeof(ItemMinerInstruction), "UpdateExtractionWeights")
                .Invoke(miner, [null, true]);
            miner.Data.ExtractionTypeProbs = (float[])probs;
        }
    }

    [HarmonyPatch(typeof(ItemMinerInstruction), "Start")]
    [HarmonyPostfix]
    static void ItemMinerInstruction_Start(ItemMinerInstruction __instance) {
        __instance.gameObject.AddComponent<MinerUpdater>();
    }
}