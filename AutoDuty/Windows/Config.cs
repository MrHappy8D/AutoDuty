﻿using AutoDuty.IPC;
using Dalamud.Configuration;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using ECommons.ExcelServices;
using AutoDuty.Helpers;
using ECommons.GameHelpers;
using ECommons.DalamudServices;
using Lumina.Excel.GeneratedSheets;
using ECommons;
using Dalamud.Interface.Utility;

namespace AutoDuty.Windows;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public HashSet<string> DoNotUpdatePathFiles { get; set; } = [];

    public int Version { get; set; } = 95;
    public int AutoRepairPct { get; set; } = 50;
    public int AutoGCTurninSlotsLeft { get; set; } = 5;
    public int LoopTimes { get; set; } = 1;
    public int TreasureCofferScanDistance { get; set; } = 25;
    public bool AutoEquipRecommendedGear { get; set; } = false;
    public bool AutoDungeonSelect { get; set; } = false;
    public bool UseSquadronsIfPossible { get; set; } = false;
    public bool PreferTrust { get; set; } = false;
    public bool AutoManageBossModAISettings { get; set; } = true;
    public bool AutoManageRSRState { get; set; } = true;
    public bool AutoExitDuty { get; set; } = true;
    public bool AutoKillClient { get; set; } = false;
    public bool AutoLogout { get; set; } = false;
    public bool AutoARMultiEnable { get; set; } = false;
    public bool LootTreasure { get; set; } = true;
    public bool LootBossTreasureOnly { get; set; } = true;
    public bool AutoRepair { get; set; } = false;
    public bool AutoRepairSelf { get; set; } = false;
    public bool AutoRepairCity { get; set; } = true;
    public bool RetireToInnBeforeLoops { get; set; } = true;
    public bool RetireToBarracksBeforeLoops { get; set; } = false;
    public bool AutoExtract { get; set; } = false;
    public bool AutoExtractEquipped { get; set; } = true;
    public bool AutoExtractAll { get; set; } = false;
    public bool AutoDesynth { get; set; } = false;
    public bool AutoGCTurnin { get; set; } = false;
    public bool Support { get; set; } = false;
    public bool Trust { get; set; } = false;
    public bool Squadron { get; set; } = false;
    public bool Regular { get; set; } = false;
    public bool Trial { get; set; } = false;
    public bool Raid { get; set; } = false;
    public bool Variant { get; set; } = false;
    public bool Unsynced { get; set; } = false;
    public bool HideUnavailableDuties { get; set; } = false;
    public bool HideBossModAIConfig { get; set; } = false;
    public bool FollowDuringCombat { get; set; } = true;
    public bool FollowDuringActiveBossModule { get; set; } = true;
    public bool FollowOutOfCombat { get; set; } = false;
    public bool FollowTarget { get; set; } = true;
    public bool FollowSelf { get; set; } = true;
    public bool FollowSlot { get; set; } = false;
    public int FollowSlotInt { get; set; } = 1;
    public bool FollowRole { get; set; } = false;
    public string FollowRoleStr { get; set; } = "Healer";
    public bool MaxDistanceToTargetRoleRange { get; set; } = true;
    public int MaxDistanceToTarget { get; set; } = 3;
    public int MaxDistanceToTargetAoE { get; set; } = 12;
    public int MaxDistanceToSlot { get; set; } = 1;
    public bool PositionalRoleBased { get; set; } = true;
    public string PositionalCustom { get; set; } = "Any";
    public bool StopLevel { get; set; } = false;
    public int StopLevelInt { get; set; } = 0;
    public bool StopNoRestedXP { get; set; } = false;
    public bool StopItemQty { get; set; } = false;
    public Dictionary<uint, KeyValuePair<string, int>> StopItemQtyItemDictionary { get; set; } = [];
    public int StopItemQtyInt { get; set; } = 1;

    public Dictionary<uint, Dictionary<Job, int>> PathSelections { get; set; } = [];
    

    [NonSerialized]
    private IDalamudPluginInterface? PluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
    }

    public void Save()
    {
        PluginInterface!.SavePluginConfig(this);
    }
}

public static class ConfigTab
{
    private static Configuration Configuration = AutoDuty.Plugin.Configuration;

    private static Dictionary<uint, string> Items { get; set; } = Svc.Data.GetExcelSheet<Item>()?.Where(x => !x.Name.RawString.IsNullOrEmpty()).ToDictionary(x => x.RowId, x => x.Name.RawString)!;
    private static string stopItemQtyItemNameInput = "";
    private static KeyValuePair<uint, string> selectedItem = new(0, "");

    public static void Draw()
    {
        if (MainWindow.CurrentTabName != "Config")
            MainWindow.CurrentTabName = "Config";
        var autoEquipRecommendedGear = Configuration.AutoEquipRecommendedGear;
        var autoManageRSRState = Configuration.AutoManageRSRState;
        var autoManageBossModAISettings = Configuration.AutoManageBossModAISettings;
        var autoExitDuty = Configuration.AutoExitDuty;
        var autoKillClient = Configuration.AutoKillClient;
        var autoLogout = Configuration.AutoLogout;
        var autoARMultiEnable = Configuration.AutoARMultiEnable;
        var lootTreasure = Configuration.LootTreasure;
        var treasureCofferScanDistance = Configuration.TreasureCofferScanDistance;
        var lootBossTreasureOnly = Configuration.LootBossTreasureOnly;
        var autoRepair = Configuration.AutoRepair;
        var autoRepairSelf = Configuration.AutoRepairSelf;
        var autoRepairCity = Configuration.AutoRepairCity;
        var autoRepairPct = Configuration.AutoRepairPct;
        var retireToInnBeforeLoops = Configuration.RetireToInnBeforeLoops;
        var retireToBarracksBeforeLoops = Configuration.RetireToBarracksBeforeLoops;
        var autoExtract = Configuration.AutoExtract;
        var autoExtractEquipped = Configuration.AutoExtractEquipped;
        var autoExtractAll = Configuration.AutoExtractAll;
        var autoDesynth = Configuration.AutoDesynth;
        var autoGCTurnin = Configuration.AutoGCTurnin;
        var hideBossModAIConfig = Configuration.HideBossModAIConfig;
        var stopLevel = Configuration.StopLevel;
        var stopLevelInt = Configuration.StopLevelInt;
        var stopNoRestedXP = Configuration.StopNoRestedXP;
        var stopItemQty = Configuration.StopItemQty;
        var stopItemQtyItemDictionary = Configuration.StopItemQtyItemDictionary;
        var stopItemQtyInt = Configuration.StopItemQtyInt;
        if (ImGui.Checkbox("Auto Manage Rotation Solver State", ref autoManageRSRState))
        {
            Configuration.AutoManageRSRState = autoManageRSRState;
            Configuration.Save();
        }
        if (ImGui.Checkbox("Auto Manage BossMod AI Settings", ref autoManageBossModAISettings))
        {
            Configuration.AutoManageBossModAISettings = autoManageBossModAISettings;
            hideBossModAIConfig = !autoManageBossModAISettings;
            Configuration.HideBossModAIConfig = hideBossModAIConfig;
            Configuration.Save();
        }
        ImGui.SameLine(0, 5);
        
        using (var autoManageBossModAISettingsDisable = ImRaii.Disabled(!autoManageBossModAISettings))
        {
            if (ImGui.Button(hideBossModAIConfig ? "Show" : "Hide"))
            {
                hideBossModAIConfig = !hideBossModAIConfig;
                Configuration.HideBossModAIConfig = hideBossModAIConfig;
                Configuration.Save();
            }
        }
        if (ImGui.Checkbox("Auto Kill Client on Completion of Looping", ref autoKillClient))
        {
            Configuration.AutoKillClient = autoKillClient;
            Configuration.AutoLogout = false;
            Configuration.Save();
        }
        if (ImGui.Checkbox("Auto Logout on Completion of Looping", ref autoLogout))
        {
            Configuration.AutoLogout = autoLogout;
            Configuration.AutoKillClient = false;
            Configuration.Save();
        }
        if (ImGui.Checkbox("AutoRetainer Multi on Completion of Looping", ref autoARMultiEnable))
        {
            Configuration.AutoARMultiEnable = autoARMultiEnable;
            Configuration.Save();
        }
        if (ImGui.Checkbox("Auto Exit Duty on Completion of Dungeon", ref autoExitDuty))
        {
            Configuration.AutoExitDuty = autoExitDuty;
            Configuration.Save();
        }

        if (ImGui.Checkbox("Loot Treasure Coffers", ref lootTreasure))
        {
            Configuration.LootTreasure = lootTreasure;
            Configuration.Save();
        }
        /*/disabled for now
        using (var d0 = ImRaii.Disabled(true))
        {
            if (ImGui.SliderInt("Scan Distance", ref treasureCofferScanDistance, 1, 100))
            {
                Configuration.TreasureCofferScanDistance = treasureCofferScanDistance;
                Configuration.Save();
            }
        }*/
        using (var d1 = ImRaii.Disabled(!lootTreasure))
        {
            if (ImGui.Checkbox("Loot Boss Treasure Only", ref lootBossTreasureOnly))
            {
                Configuration.LootBossTreasureOnly = lootBossTreasureOnly;
                Configuration.Save();
            }
        }
        if (ImGui.Checkbox("Retire to Inn before each Loop", ref retireToInnBeforeLoops))
        {
            Configuration.RetireToInnBeforeLoops = retireToInnBeforeLoops;
            Configuration.RetireToBarracksBeforeLoops = false;
            retireToBarracksBeforeLoops = false;
            Configuration.Save();
        }
        if (ImGui.Checkbox("Retire to Barracks before each Loop", ref retireToBarracksBeforeLoops))
        {
            Configuration.RetireToBarracksBeforeLoops = retireToBarracksBeforeLoops;
            Configuration.RetireToInnBeforeLoops = false;
            retireToInnBeforeLoops = false;
            Configuration.Save();
        }
        ImGui.Separator();
        if (ImGui.Checkbox("AutoRepair Enabled @", ref autoRepair))
        {
            Configuration.AutoRepair = autoRepair;
            Configuration.Save();
        }

        using (var d1 = ImRaii.Disabled(!autoRepair))
        {
            ImGui.SameLine(0, 15);
            ImGui.PushItemWidth(150 * ImGuiHelpers.GlobalScale);
            if (ImGui.SliderInt("##Repair@", ref autoRepairPct, 1, 100, "%d%%"))
            {
                Configuration.AutoRepairPct = autoRepairPct;
                Configuration.Save();
            }
            ImGui.PopItemWidth();
            if (ImGui.Checkbox("Self AutoRepair", ref autoRepairSelf))
            {
                Configuration.AutoRepairSelf = autoRepairSelf;
                Configuration.AutoRepairCity = false;
                autoRepairCity = false;
                Configuration.Save();
            }
            ImGui.SameLine(0, 5);
            if (ImGui.Checkbox("AutoRepair at City", ref autoRepairCity))
            {
                Configuration.AutoRepairCity = autoRepairCity;
                Configuration.AutoRepairSelf = false;
                autoRepairSelf = false;
                Configuration.Save();
            }
        }
        if (ImGui.Checkbox("Auto Equip Recommended Gear", ref autoEquipRecommendedGear))
        {
            Configuration.AutoEquipRecommendedGear = autoEquipRecommendedGear;
            Configuration.Save();
        }
        ImGui.Separator();
        if (ImGui.Checkbox("Auto Extract", ref autoExtract))
        {
            Configuration.AutoExtract = autoExtract;
            Configuration.Save();
        }
        ImGui.SameLine(0, 5);
        if (ImGui.Checkbox("Extract Equipped", ref autoExtractEquipped))
        {
            if (autoExtractEquipped)
            {
                Configuration.AutoExtractEquipped = true;
                Configuration.AutoExtractAll = false;
                autoExtractAll = false;
            }
            else
            {
                Configuration.AutoExtractAll = true;
                Configuration.AutoExtractEquipped = false;
                autoExtractAll = true;
            }
            Configuration.Save();
        }
        ImGui.SameLine(0, 5);
        if (ImGui.Checkbox("Extract All", ref autoExtractAll))
        {
            if (autoExtractAll)
            {
                Configuration.AutoExtractAll = true;
                Configuration.AutoExtractEquipped = false;
                autoExtractEquipped = false;
            }
            else
            {
                Configuration.AutoExtractAll = false;
                Configuration.AutoExtractEquipped = true;
                autoExtractEquipped = true;
            }
            Configuration.Save();
        }
        if (ImGui.Checkbox("Auto Desynth", ref autoDesynth))
        {
            Configuration.AutoDesynth = autoDesynth;
            Configuration.AutoGCTurnin = false;
            autoGCTurnin = false;
            Configuration.Save();
        }
        ImGui.SameLine(0, 5);
        using (var autoGcTurninDisabled = ImRaii.Disabled(!Deliveroo_IPCSubscriber.IsEnabled))
        {
            if (ImGui.Checkbox("Auto GC Turnin", ref autoGCTurnin))
            {
                Configuration.AutoGCTurnin = autoGCTurnin;
                Configuration.AutoDesynth = false;
                autoDesynth = false;
                Configuration.Save();
            }
            if (!Deliveroo_IPCSubscriber.IsEnabled)
            {
                Configuration.AutoGCTurnin = false;
                autoGCTurnin = false;
                Configuration.Save();
                ImGui.Text("* Auto GC Turnin Requires Deliveroo plugin");
                ImGui.Text("Get @ https://git.carvel.li/liza/plugin-repo");
            }
        }
        ImGui.Separator();
        if (ImGui.Checkbox("Stop Looping @ Level", ref stopLevel))
        {
            Configuration.StopLevel = stopLevel;
            Configuration.Save();
        }
        using (var d1 = ImRaii.Disabled(!stopLevel))
        {
            ImGui.SameLine(0,15);
            ImGui.PushItemWidth(155 * ImGuiHelpers.GlobalScale);
            if (ImGui.SliderInt("##Level", ref stopLevelInt, 1, 100))
            {
                Configuration.StopLevelInt = stopLevelInt;
                Configuration.Save();
            }
            ImGui.PopItemWidth();
        }
        if (ImGui.Checkbox("Stop When No Rested XP", ref stopNoRestedXP))
        {
            Configuration.StopNoRestedXP = stopNoRestedXP;
            Configuration.Save();
        }
        if (ImGui.Checkbox("Stop Looping When Reach Item Qty", ref stopItemQty))
        {
            Configuration.StopItemQty = stopItemQty;
            Configuration.Save();
        }
        using (var d1 = ImRaii.Disabled(!stopItemQty))
        {
            ImGui.PushItemWidth(250 * ImGuiHelpers.GlobalScale);
            if (ImGui.BeginCombo("Select Item", selectedItem.Value))
            {
                ImGui.InputTextWithHint("Item Name", "Start typing item name to search", ref stopItemQtyItemNameInput, 1000);
                foreach (var item in Items.Where(x => x.Value.Contains(stopItemQtyItemNameInput, StringComparison.InvariantCultureIgnoreCase))!)
                {
                    if (ImGui.Selectable($"{item.Value}"))
                        selectedItem = item;
                }
                ImGui.EndCombo();
            }
            ImGui.PopItemWidth();
            ImGui.PushItemWidth(190 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputInt("Quantity", ref stopItemQtyInt))
            {
                Configuration.StopItemQtyInt = stopItemQtyInt;
                Configuration.Save();
            }
            ImGui.SameLine(0, 5);
            using (var addDisabled = ImRaii.Disabled(selectedItem.Value.IsNullOrEmpty()))
            {
                if (ImGui.Button("Add Item"))
                {
                    if (!stopItemQtyItemDictionary.TryAdd(selectedItem.Key, new(selectedItem.Value, stopItemQtyInt)))
                    {
                        stopItemQtyItemDictionary.Remove(selectedItem.Key);
                        stopItemQtyItemDictionary.Add(selectedItem.Key, new(selectedItem.Value, stopItemQtyInt));
                    }
                    Configuration.Save();
                }
            }
            ImGui.PopItemWidth();
            if (!ImGui.BeginListBox("##ItemList", new System.Numerics.Vector2(325 * ImGuiHelpers.GlobalScale, 40 * ImGuiHelpers.GlobalScale))) return;

            foreach (var item in stopItemQtyItemDictionary)
            {
                ImGui.Selectable($"{item.Value.Key} (Qty: {item.Value.Value})");
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    stopItemQtyItemDictionary.Remove(item);
                    Configuration.Save();
                }
            }

            ImGui.EndListBox();
        }
        ImGui.Separator();
        using (var savedPathsDisabled = ImRaii.Disabled(!Configuration.PathSelections.Any(kvp => kvp.Value.Any())))
        {
            if (ImGui.Button("Clear all saved path selections"))
            {
                Configuration.PathSelections.Clear();
                Configuration.Save();
            }
        }

    }

    public static class BossModConfigTab
    {
        private static Configuration Configuration = AutoDuty.Plugin.Configuration;

        public static void Draw()
        {
            if (MainWindow.CurrentTabName != "BossModConfig")
                MainWindow.CurrentTabName = "BossModConfig";
            var followDuringCombat = Configuration.FollowDuringCombat;
            var followDuringActiveBossModule = Configuration.FollowDuringActiveBossModule;
            var followOutOfCombat = Configuration.FollowOutOfCombat;
            var followTarget = Configuration.FollowTarget;
            var followSelf = Configuration.FollowSelf;
            var followSlot = Configuration.FollowSlot;
            var followSlotInt = Configuration.FollowSlotInt;
            var followRole = Configuration.FollowRole;
            var followRoleStr = Configuration.FollowRoleStr;
            var maxDistanceToTargetRoleBased = Configuration.MaxDistanceToTargetRoleRange;
            var maxDistanceToTarget = Configuration.MaxDistanceToTarget;
            var maxDistanceToTargetAoE = Configuration.MaxDistanceToTargetAoE;
            var maxDistanceToSlot = Configuration.MaxDistanceToSlot;
            var positionalRoleBased = Configuration.PositionalRoleBased;
            var positionalCustom = Configuration.PositionalCustom;

            if (ImGui.Checkbox("Follow During Combat", ref followDuringCombat))
            {
                Configuration.FollowDuringCombat = followDuringCombat;
                Configuration.Save();
            }
            if (ImGui.Checkbox("Follow During Active BossModule", ref followDuringActiveBossModule))
            {
                Configuration.FollowDuringActiveBossModule = followDuringActiveBossModule;
                Configuration.Save();
            }
            if (ImGui.Checkbox("Follow Out Of Combat (Not Recommended)", ref followOutOfCombat))
            {
                Configuration.FollowOutOfCombat = followOutOfCombat;
                Configuration.Save();
            }
            if (ImGui.Checkbox("Follow Target", ref followTarget))
            {
                Configuration.FollowTarget = followTarget;
                Configuration.Save();
            }

            if (ImGui.Checkbox("Follow Self", ref followSelf))
            {
                Configuration.FollowSelf = followSelf;
                Configuration.FollowSlot = false;
                Configuration.FollowRole = false;
                followRole = false;
                Configuration.Save();
            }
            if (ImGui.Checkbox("Follow Slot", ref followSlot))
            {
                Configuration.FollowSelf = false;
                followSelf = false;
                Configuration.FollowSlot = followSlot;
                followSlot = false;
                Configuration.FollowRole = false;
                followRole = false;
                Configuration.Save();
            }
            using (var d1 = ImRaii.Disabled(!followSlot))
            {
                ImGui.PushItemWidth(300);
                if (ImGui.SliderInt("Follow Slot #", ref followSlotInt, 1, 4))
                {
                    Configuration.FollowSlotInt = followSlotInt;
                    Configuration.Save();
                }
                ImGui.PopItemWidth();
            }
            if (ImGui.Checkbox("Follow Role", ref followRole))
            {
                Configuration.FollowSelf = false;
                followSelf = false;
                Configuration.FollowSlot = false;
                followSlot = false;
                Configuration.FollowRole = followRole;
                Configuration.Save();
            }
            using (var d1 = ImRaii.Disabled(!followRole))
            {
                ImGui.SameLine(0, 10);
                if (ImGui.Button(followRoleStr))
                {
                    ImGui.OpenPopup("RolePopup");
                }
                if (ImGui.BeginPopup("RolePopup"))
                {
                    if (ImGui.Selectable("Tank"))
                    {
                        Configuration.FollowRoleStr = "Tank";
                        followRoleStr = "Tank";
                        Configuration.Save();
                    }
                    if (ImGui.Selectable("Melee"))
                    {
                        Configuration.FollowRoleStr = "Melee";
                        followRoleStr = "Melee";
                        Configuration.Save();
                    }
                    if (ImGui.Selectable("Ranged"))
                    {
                        Configuration.FollowRoleStr = "Ranged";
                        followRoleStr = "Ranged";
                        Configuration.Save();
                    }
                    if (ImGui.Selectable("Healer"))
                    {
                        Configuration.FollowRoleStr = "Healer";
                        followRoleStr = "Healer";
                        Configuration.Save();
                    }
                    ImGui.EndPopup();
                }
            }

            if (ImGui.Checkbox("Set Max Distance To Target Based on Role", ref maxDistanceToTargetRoleBased))
            {
                Configuration.MaxDistanceToTargetRoleRange = maxDistanceToTargetRoleBased;
                Configuration.Save();
            }

            if (ObjectHelper.IsValid && maxDistanceToTargetRoleBased && maxDistanceToTarget != (ObjectHelper.GetJobRole(Player.Object.ClassJob.GameData!) == ObjectHelper.JobRole.Melee || ObjectHelper.GetJobRole(Player.Object.ClassJob.GameData!) == ObjectHelper.JobRole.Tank ? 3 : 10))
            {
                maxDistanceToTarget = (ObjectHelper.GetJobRole(Player.Object.ClassJob.GameData!) == ObjectHelper.JobRole.Melee || ObjectHelper.GetJobRole(Player.Object.ClassJob.GameData!) == ObjectHelper.JobRole.Tank ? 3 : 10);
                Configuration.MaxDistanceToTarget = maxDistanceToTarget;
                Configuration.Save();
            }

            if (ObjectHelper.IsValid && maxDistanceToTargetRoleBased && maxDistanceToTargetAoE != (ObjectHelper.GetJobRole(Player.Object.ClassJob.GameData!) == ObjectHelper.JobRole.Melee || ObjectHelper.GetJobRole(Player.Object.ClassJob.GameData!) == ObjectHelper.JobRole.Tank ? 3 : 10))
            {
                maxDistanceToTargetAoE = (ObjectHelper.GetJobRole(Player.Object.ClassJob.GameData!) == ObjectHelper.JobRole.Melee || ObjectHelper.GetJobRole(Player.Object.ClassJob.GameData!) == ObjectHelper.JobRole.Tank ? 3 : 10);
                Configuration.MaxDistanceToTargetAoE = maxDistanceToTargetAoE;
                Configuration.Save();
            }

            using (var d1 = ImRaii.Disabled(maxDistanceToTargetRoleBased))
            {
                ImGui.PushItemWidth(200);
                if (ImGui.SliderInt("Max Distance To Target", ref maxDistanceToTarget, 1, 30))
                {
                    Configuration.MaxDistanceToTarget = maxDistanceToTarget;
                    Configuration.Save();
                }
                if (ImGui.SliderInt("Max Distance To Target AoE", ref maxDistanceToTargetAoE, 1, 10))
                {
                    Configuration.MaxDistanceToTargetAoE = maxDistanceToTargetAoE;
                    Configuration.Save();
                }
                ImGui.PopItemWidth();
            }
            ImGui.PushItemWidth(200);
            if (ImGui.SliderInt("Max Distance To Slot", ref maxDistanceToSlot, 1, 30))
            {
                Configuration.MaxDistanceToSlot = maxDistanceToSlot;
                Configuration.Save();
            }
            ImGui.PopItemWidth();

            if (ImGui.Checkbox("Set Positional Based on Role", ref positionalRoleBased))
            {
                Configuration.PositionalRoleBased = positionalRoleBased;
                Configuration.Save();
            }

            if (ObjectHelper.IsValid && positionalRoleBased && positionalCustom != (ObjectHelper.GetJobRole(Player.Object.ClassJob.GameData!) == ObjectHelper.JobRole.Melee ? "Rear" : "Any"))
            {
                positionalCustom = (ObjectHelper.GetJobRole(Player.Object.ClassJob.GameData!) == ObjectHelper.JobRole.Melee ? "Rear" : "Any");
                Configuration.PositionalCustom = positionalCustom;
                Configuration.Save();
            }

            using (var d1 = ImRaii.Disabled(positionalRoleBased))
            {
                ImGui.SameLine(0, 10);
                if (ImGui.Button(positionalCustom))
                    ImGui.OpenPopup("PositionalPopup");

                if (ImGui.BeginPopup("PositionalPopup"))
                {
                    if (ImGui.Selectable("Any"))
                    {
                        positionalCustom = "Any";
                        Configuration.PositionalCustom = positionalCustom;
                        Configuration.Save();
                    }
                    if (ImGui.Selectable("Rear"))
                    {
                        positionalCustom = "Rear";
                        Configuration.PositionalCustom = positionalCustom;
                        Configuration.Save();
                    }
                    if (ImGui.Selectable("Flank"))
                    {
                        positionalCustom = "Flank";
                        Configuration.PositionalCustom = positionalCustom;
                        Configuration.Save();
                    }
                    if (ImGui.Selectable("Front"))
                    {
                        positionalCustom = "Front";
                        Configuration.PositionalCustom = positionalCustom;
                        Configuration.Save();
                    }
                    ImGui.EndPopup();
                }
            }
        }
    }
}
