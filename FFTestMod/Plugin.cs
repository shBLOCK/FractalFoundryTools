using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MAX.Game;
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
        harmony.PatchAll(typeof(CustomFactoryManagerPatch));
        // harmony.PatchAll(typeof(ItemDataPatch));
        Logger.LogInfo("Patched");
        Logger.LogInfo(GraphicsSettings.currentRenderPipeline);
    }

    private void Update() {
        // AccessTools.Method(typeof(GridManager), "CacheMaskAndItemTypes").Invoke(Manager<GridManager>.Instance, []);
    }

    private void OnDestroy() {
        harmony.UnpatchSelf();
    }
}