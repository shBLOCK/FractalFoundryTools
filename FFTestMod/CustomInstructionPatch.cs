using System;
using System.Text;
using HarmonyLib;
using MAX.Data;
using MAX.Game;
using MAX.UI;

namespace FFTestMod;

public static class CustomInstructionPatch {
    [HarmonyPatch(typeof(StageCustomInstruction), nameof(StageCustomInstruction.WriteParameters))]
    [HarmonyPrefix]
    static bool WriteParameters_Prefix(VariableOverridesData variableData, ref string __result) {
        if (variableData == null || variableData.VariableDefinitions == null ||
            variableData.VariableDefinitions.Count == 0) {
            __result = "";
            return false;
        }

        int totalWidth = 0;
        StringBuilder stringBuilder = new StringBuilder();
        bool flag = true;
        foreach (VariableDefinitionData definitionData in variableData.VariableDefinitions.Values) {
            if (!flag) stringBuilder.AppendLine();
            flag = false;
            variableData.TryGetValue(definitionData.Id, out var overrideData);
            object friendlyValue = EditVariableUI.GetFriendlyValue(definitionData, overrideData);
            stringBuilder.Append($"<u>{definitionData.Name.PadRight(totalWidth)}: </u>");
            if (overrideData != null) {
                stringBuilder.Append(overrideData.Value.VariableMode switch {
                    EVariableMode.None => "",
                    EVariableMode.Variable => $"{overrideData.Value.VariableDefinition.Name} = ",
                    EVariableMode.Expression => $"{overrideData.Value.Expression.Expression} = ",
                    _ => throw new ArgumentOutOfRangeException(),
                });
            } else {
                stringBuilder.Append("<default> = ");
            }

            stringBuilder.Append(friendlyValue);
        }

        __result = stringBuilder.ToString();
        return false;
    }
}