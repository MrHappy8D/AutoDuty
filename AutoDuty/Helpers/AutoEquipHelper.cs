using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ECommons.Automation;
using ECommons.Automation.LegacyTaskManager;
using Dalamud.Plugin.Services;
using ECommons.Throttlers;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Game.ClientState.Conditions;

namespace AutoDuty.Helpers
{
    internal static class AutoEquipHelper
    {
        internal static bool AutoEquipRunning = false;
        private static TaskManager _taskManager;

        internal static void Invoke(TaskManager taskManager)
        {
            if (!AutoEquipRunning)
            {
                Svc.Log.Info($"Equipping Started");
                AutoEquipRunning = true;
                _taskManager = taskManager;
                SchedulerHelper.ScheduleAction("AutoEquipTimeout", Stop, 300000);
                AutoEquipRecommendedGear();
            }
            else
            {
                Svc.Log.Info("AutoEquip already running");
            }
        }

        internal static void Stop() 
        {
            if (AutoEquipRunning)
            {
                Svc.Log.Info($"AutoEquip Finished");
                SchedulerHelper.DescheduleAction("AutoEquipTimeout");
                AutoEquipRunning = false;
                AutoDuty.Plugin.Action = "";
            }
            else
            {
                Svc.Log.Info("AutoEquip was not running");
            }
        }

        internal static unsafe void AutoEquipRecommendedGear()
        {
            if (!AutoEquipRunning || Svc.ClientState.LocalPlayer == null)
            {
                Stop();
                return;
            }

            AutoDuty.Plugin.Action = "Equipping Gear";

            _taskManager.Enqueue(() =>
            {
                if (Svc.Condition[ConditionFlag.InCombat] || 
                    Svc.Condition[ConditionFlag.BetweenAreas])
                {
                    Svc.Log.Debug("Cannot equip gear: player is in combat or between areas.");
                    Stop();
                    return false;
                }
                return true;
            }, "CheckConditions");

            var mod = RecommendEquipModule.Instance();

            _taskManager.Enqueue(() => 
            {
                mod->SetupForClassJob((byte)Svc.ClientState.LocalPlayer!.ClassJob.Id);
                Svc.Log.Debug("Set up recommended gear for current class/job.");
            }, "SetupRecommendedGear");

            _taskManager.DelayNext("EquipGear", 500);

            _taskManager.Enqueue(() => 
            {
                mod->EquipRecommendedGear();
                Svc.Log.Info("Attempted to equip recommended gear.");
                return true;
            }, "EquipRecommendedGear");

            _taskManager.DelayNext("UpdateGearset", 1000);
            _taskManager.Enqueue(() => 
            {
                var id = RaptureGearsetModule.Instance()->CurrentGearsetIndex;
                RaptureGearsetModule.Instance()->UpdateGearset(id);
                Svc.Log.Info($"Attempted to update gearset {id}.");
                Stop();
                return true;
            }, "UpdateGearset");
        }
    }
}