using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MAX.Data;
using MAX.Game;
using UnityEngine;
using UnityEngine.Rendering;

namespace FFTestMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Fractal Foundry.exe")]
public class Plugin : BaseUnityPlugin {
    internal static new ManualLogSource Logger;

    private Harmony harmony;

    private void Awake() {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!!!");

        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        // harmony.PatchAll(typeof(DynamicGridPatch));
        // harmony.PatchAll(typeof(DestroyInstrPatch));
        harmony.PatchAll(typeof(ProcessDataPatch));
        harmony.PatchAll(typeof(CheatManagerPatch));
        harmony.PatchAll(typeof(CustomInstructionPatch));
        // harmony.PatchAll(typeof(CustomFactoryManagerPatch));
        // harmony.PatchAll(typeof(ItemDataPatch));
        harmony.PatchAll(typeof(MaxDepthPatch));
        harmony.PatchAll(typeof(VariableLinkPatch));
        Logger.LogInfo("Patched");
        Logger.LogInfo(GraphicsSettings.currentRenderPipeline);

        for (int i = 0; i < 31; i++) {
            Logger.LogInfo($"Layer {i}: {LayerMask.LayerToName(i)}");
        }
    }

    private void Update() {
        // AccessTools.Method(typeof(GridManager), "CacheMaskAndItemTypes").Invoke(Manager<GridManager>.Instance, []);
        if (Input.GetKeyDown(KeyCode.N)) {
            CustomInstruction ci = Manager<FactorySelectionManager>.Instance.Selected.FirstOrDefault() as CustomInstruction;
            if (ci) {
                Logger.LogInfo($"Total NFs in this NF: {recursiveCountCustomInstructions(ci.Data)}");                
            }
        }

        if (Input.GetKeyDown(KeyCode.K)) {
            VariableLinkPatch.EXPR_CACHE.Clear();
            Logger.LogInfo("Cleared EXPR_CACHE");
        }
    }

    private int recursiveCountCustomInstructions(CustomInstructionData data) {
        if (data == null) return 0;
        return 1 + data.SubDataManager.GetDatas<CustomInstructionData>().Sum(recursiveCountCustomInstructions);
    }

    private void OnDestroy() {
        harmony.UnpatchSelf();
    }
}