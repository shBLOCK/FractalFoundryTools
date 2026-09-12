using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using MAX.Data;
using MAX.Game;
using UnityEngine;

namespace FFAccessibilityMod;

public class ItemTypeColorsPatch {
    private static Dictionary<EItemType, Color> typeToColorsOverride = new();
    private static Dictionary<EItemType, Color> typeToLightColorsOverride = new();

    [HarmonyPatch(typeof(ItemTypeColors), nameof(ItemTypeColors.GetColor))]
    [HarmonyPrefix]
    static bool GetColor_Prefix(EItemType itemType, ref Color __result) {
        if (typeToColorsOverride.TryGetValue(itemType, out var color)) {
            __result = color;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(ItemTypeColors), nameof(ItemTypeColors.GetLightScreenColor))]
    [HarmonyPrefix]
    static bool GetLightScreenColor_Prefix(EItemType itemType, ref Color __result) {
        if (typeToLightColorsOverride.TryGetValue(itemType, out var color)) {
            __result = color;
            return false;
        }

        return true;
    }

    public static void reloadConfig(ConfigFile config) {
        const string COLOR_OVERRIDE_SECTION = "ColorOverride";
        var itemRenderers = Manager<ItemRendererManager>.InstanceWithFindFallback;
        var colors = itemRenderers.MainRenderer.ItemTypeColors;

        typeToColorsOverride.Clear();
        typeToLightColorsOverride.Clear();

        for (int i = 0; i < (int)EItemType.Count; i++) {
            var type = (EItemType)i;
            var name = type.ToString();
            var section = $"{COLOR_OVERRIDE_SECTION}.{name}";
            var cfgEnable = config.Bind(section, "Enable", false, "Set to true to override this color.");
            var cfgColor = config.Bind(section, "Color", ColorUtility.ToHtmlStringRGB(colors.GetColor(type)));
            var cfgLightColor = config.Bind(section, "LightColor",
                ColorUtility.ToHtmlStringRGB(colors.GetLightScreenColor(type)),
                "The color used for the \"light\" on top of the extractor.");
            if (cfgEnable.Value) {
                if (ColorUtility.TryParseHexColor(cfgColor.Value, out Color color)) {
                    typeToColorsOverride[type] = color;
                } else {
                    Plugin.Logger.LogError($"Failed to parse {type} color {cfgColor.Value}");
                }

                if (ColorUtility.TryParseHexColor(cfgLightColor.Value, out Color lightColor)) {
                    typeToLightColorsOverride[type] = lightColor;
                } else {
                    Plugin.Logger.LogError($"Failed to parse {type} light color {cfgLightColor.Value}");
                }
            }
        }

        var UpdateItemTypeColorsBuffer = AccessTools.Method(typeof(ItemRenderer), "UpdateItemTypeColorsBuffer");
        UpdateItemTypeColorsBuffer.Invoke(itemRenderers.MainRenderer, []);
        Plugin.Logger.LogInfo("Updated MainRenderer");
        try {
            UpdateItemTypeColorsBuffer.Invoke(itemRenderers.PreviewRenderer, []);
            Plugin.Logger.LogInfo("Updated PreviewRenderer");
        } catch (TargetInvocationException e) {
            Plugin.Logger.LogError($"Failed to update PreviewRenderer: {e}");
        }

        foreach (var mat in Resources.FindObjectsOfTypeAll<Material>()) {
            if (mat.name == "MinableFloorMat") {
                Plugin.Logger.LogInfo($"Found MinableFloorMat: {mat}");
                for (int i = 0; i < (int)EItemType.Count; i++) {
                    var type = (EItemType)i;
                    var name = $"_Color{type.ToString()}";
                    if (mat.HasColor(name)) {
                        mat.SetColor(name, typeToColorsOverride[type]);
                    }
                }
            }
        }
    }
}