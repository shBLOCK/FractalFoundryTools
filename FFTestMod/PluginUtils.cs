using System.Text;
using MAX.Data;

namespace FFTestMod;

public class PluginUtils {
    public static string prettyPrintItem(ItemData item) {
        switch (item) {
            case UnitItemData unitItem:
                return unitItem.Type switch {
                    EItemType.Empty => "E",
                    EItemType.Void => "V",
                    EItemType.Blue => "B",
                    EItemType.White => "W",
                    EItemType.Yellow => "Y",
                    EItemType.Green => "G",
                    _ => $"<{unitItem.Type}>",
                };
            case ListItemData listItem:
                var str = new StringBuilder($"[{listItem.AxisDimension};");
                foreach (var oneItem in listItem.Items) {
                    str.Append(prettyPrintItem(oneItem));
                }
                str.Append(']');
                return str.ToString();
            default:
                return $"<Unknown item ({item})>";
        }
    }

    public static string prettyPrintItemDetailed(ItemData item, int indent = 0) {
        var str = new StringBuilder();
        switch (item) {
            case UnitItemData unitItem:
                str.Append("Unit(");
                str.Append(prettyPrintItem(unitItem));
                str.Append(", ");
                break;
            case ListItemData listItem:
                str.Append("List([\n");
                foreach (var oneItem in listItem.Items) {
                    for (int i = 0; i <= indent; i++) {
                        str.Append("    ");
                    }
                    str.Append(prettyPrintItemDetailed(oneItem, indent + 1));
                    str.Append(",\n");
                }
                str.Remove(str.Length - 2, 2);
                str.Append("\n");
                for (int i = 0; i < indent; i++) {
                    str.Append("    ");
                }
                str.Append("], ");
                break;
        }

        str.Append($"OffsetUnit={item.OffsetUnit}, ");
        str.Append($"LocalPosition={item.LocalPosition}");
        
        str.Append(")");
        return str.ToString();
    }
}