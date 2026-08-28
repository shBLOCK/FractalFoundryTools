using System;
using System.Collections.Generic;
using HarmonyLib;
using MAX.Data;

namespace FFTestMod;

public class VariableLinkPatch {
    public static Dictionary<ExpressionData, object> EXPR_CACHE = new();

    [HarmonyPatch(typeof(VariableLink<float>), "Data", MethodType.Getter)]
    [HarmonyPrefix]
    static bool VariableLink_get_Data_float_Prefix(VariableLink<float> __instance, float ___m_Data,
        ref float __result) {
        object result = null;
        if (!VariableLink_get_Data_generic_Prefix(__instance, ___m_Data, ref result)) {
            __result = Convert.ToSingle(result);
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(VariableLink<object>), "Data", MethodType.Getter)]
    [HarmonyPrefix]
    static bool VariableLink_get_Data_object_Prefix(VariableLink<object> __instance, object ___m_Data,
        ref object __result)
        => VariableLink_get_Data_generic_Prefix(__instance, ___m_Data, ref __result);

    static bool VariableLink_get_Data_generic_Prefix<T>(VariableLink<T> __instance, T ___m_Data, ref object __result) {
        if (__instance.VariableDefinition != null) return true;
        if (__instance.Expression != null) {
            if (EXPR_CACHE.TryGetValue(__instance.Expression, out object cached)) {
                __result = cached;
            } else {
                Plugin.Logger.LogInfo($"Eval ({typeof(T)}) start: {__instance.Expression.Expression}");
                __result = __instance.Expression.Evaluate(___m_Data);
                Plugin.Logger.LogInfo($"Eval ({typeof(T)}) end: {__instance.Expression.Expression} -> {__result}");
                EXPR_CACHE.Add(__instance.Expression, __result);
            }

            return false;
        }

        return true;
    }

    // [HarmonyPatch(typeof(VariableLink<float>), "Data", MethodType.Getter)]
    // [HarmonyPostfix]
    // static void VariableLink_get_Data_Postfix(VariableLink __instance, ref float __result) {
    //     if (__instance.Expression != null) {
    //         Plugin.Logger.LogInfo($"Eval end: {__instance.Expression.Expression} -> {__result}");
    //     }
    // }
}