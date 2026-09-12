using System;
using BepInEx;
using BepInEx.Logging;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace FFAccessibilityMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Fractal Foundry.exe")]
public class Plugin : BaseUnityPlugin {
    internal static new ManualLogSource Logger;

    private Harmony harmony;

    private void Awake() {
        Logger = base.Logger;
        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
    }

    private void Start() {
        harmony.PatchAll(typeof(ItemTypeColorsPatch));
        ItemTypeColorsPatch.reloadConfig(Config);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            Config.Reload();
            ItemTypeColorsPatch.reloadConfig(Config);
        }
    }
}