using ECommons;
using ECommons.DalamudServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AutoDuty.Helpers
{
    using Lumina.Data;
    using Lumina.Excel.GeneratedSheets2;
    using Lumina.Text;

    internal static class ContentHelper
    {
        internal static Dictionary<uint, Content> DictionaryContent { get; set; } = [];

        private static List<uint> ListGCArmyContent { get; set; } = [162, 1039, 1041, 1042, 171, 172, 159, 160, 349, 362, 188, 1064, 1066, 430, 510];
        
        private static List<uint> ListVVDContent { get; set; } = [1069, 1137, 1176]; //[1069, 1075, 1076, 1137, 1155, 1156, 1176, 1179, 1180]; *Criterions

        internal class Content
        {
            internal string? Name { get; set; }

            internal string? DisplayName { get; set; }

            internal uint TerritoryType { get; set; }

            internal uint ExVersion { get; set; }

            internal byte ClassJobLevelRequired { get; set; }

            internal uint ItemLevelRequired { get; set; }

            internal bool DawnContent { get; set; } = false;

            internal int DawnIndex { get; set; } = -1;

            internal uint ContentFinderCondition { get; set; }

            internal uint ContentType { get; set; }

            internal uint ContentMemberType { get; set; }

            internal bool TrustContent { get; set; } = false;

            internal bool VariantContent { get; set; } = false;

            internal int VVDIndex { get; set; } = -1;

            internal bool GCArmyContent { get; set; } = false;

            internal int GCArmyIndex { get; set; } = -1;
        }

        internal static void PopulateDuties()
        {
            var listContentFinderCondition = Svc.Data.GameData.GetExcelSheet<ContentFinderCondition>(Language.English);
            var listContentFinderConditionDisplay =
                Svc.Data.GameData.Options.DefaultExcelLanguage == Language.English ?
                                                        listContentFinderCondition :
                                                        Svc.Data.GameData.GetExcelSheet<ContentFinderCondition>() ?? listContentFinderCondition;

            var listDawnContent = Svc.Data.GameData.GetExcelSheet<DawnContent>(Language.English);


            if (listContentFinderCondition == null || listDawnContent == null) return;


            foreach (var contentFinderCondition in listContentFinderCondition)
            {

                if (contentFinderCondition.ContentType.Value == null || contentFinderCondition.TerritoryType.Value == null || contentFinderCondition.TerritoryType.Value.ExVersion.Value == null || (contentFinderCondition.ContentType.Value.RowId != 2 && contentFinderCondition.ContentType.Value.RowId != 4 && contentFinderCondition.ContentType.Value.RowId != 5 && contentFinderCondition.ContentType.Value.RowId != 30) || contentFinderCondition.Name.ToString().IsNullOrEmpty())
                    continue;

                string CleanName(string name)
                {
                    string result = name;
                    if (result[.. 3].Equals("the"))
                        result = result.ReplaceFirst("the", "The");
                    return result.Replace("--", "-").Replace("<italic(0)>", "").Replace("<italic(1)>", "");
                }


                var content = new Content
                {
                    Name = CleanName(contentFinderCondition.Name.ToString()),
                    TerritoryType = contentFinderCondition.TerritoryType.Value.RowId,
                    ContentType = contentFinderCondition.ContentType.Value.RowId,
                    ContentMemberType = contentFinderCondition.ContentMemberType.Value?.RowId ?? 0,
                    ContentFinderCondition = contentFinderCondition.RowId,
                    ExVersion = contentFinderCondition.TerritoryType.Value.ExVersion.Value.RowId,
                    ClassJobLevelRequired = contentFinderCondition.ClassJobLevelRequired,
                    ItemLevelRequired = contentFinderCondition.ItemLevelRequired,
                    DawnContent = listDawnContent.Any(dawnContent => dawnContent.Content.Value == contentFinderCondition),
                    TrustContent = listDawnContent.Any(dawnContent => dawnContent.Content.Value == contentFinderCondition) && contentFinderCondition.TerritoryType.Value.ExVersion.Value.RowId > 2,
                    VariantContent = ListVVDContent.Any(variantContent => variantContent == contentFinderCondition.TerritoryType.Value.RowId),
                    VVDIndex = ListVVDContent.FindIndex(variantContent => variantContent == contentFinderCondition.TerritoryType.Value.RowId),
                    GCArmyContent = ListGCArmyContent.Any(gcArmyContent => gcArmyContent == contentFinderCondition.TerritoryType.Value.RowId),
                    GCArmyIndex = ListGCArmyContent.FindIndex(gcArmyContent => gcArmyContent == contentFinderCondition.TerritoryType.Value.RowId)
                };

                SeString? displayName = listContentFinderConditionDisplay?.GetRow(contentFinderCondition.RowId)?.Name;
                content.DisplayName = displayName != null ? CleanName(displayName) : content.Name;

                if (content.DawnContent && listDawnContent.Where(dawnContent => dawnContent.Content.Value == contentFinderCondition).Any())
                    content.DawnIndex = listDawnContent.Where(dawnContent => dawnContent.Content.Value == contentFinderCondition).First().RowId < 32 ? (int)listDawnContent.Where(dawnContent => dawnContent.Content.Value == contentFinderCondition).First().RowId : (int)listDawnContent.Where(dawnContent => dawnContent.Content.Value == contentFinderCondition).First().RowId - 200;
                
                DictionaryContent.Add(contentFinderCondition.TerritoryType.Value.RowId, content);
            }

            DictionaryContent = DictionaryContent.OrderBy(content => content.Value.ExVersion).ThenBy(content => content.Value.ClassJobLevelRequired).ThenBy(content => content.Value.TerritoryType).ToDictionary();
        }

        internal static void DebugOutputDictionaryContent()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("DictionaryContent Contents:");
            sb.AppendLine("TerritoryType | Name | DisplayName | GCArmyContent | GCArmyIndex");
            sb.AppendLine("------------------------------------------------------------");

            foreach (var content in DictionaryContent.Values.OrderBy(c => c.TerritoryType))
            {
                sb.AppendLine($"{content.TerritoryType} | {content.Name} | {content.DisplayName} | {content.GCArmyContent} | {content.GCArmyIndex}");
            }

            // Output to log
            Svc.Log.Information(sb.ToString());

            // Optionally, write to a file
            string filePath = Path.Combine(Svc.PluginInterface.GetPluginConfigDirectory(), "ContentDebug.txt");
            File.WriteAllText(filePath, sb.ToString());
            Svc.Log.Information($"Content debug information written to: {filePath}");
        }
    }
}
