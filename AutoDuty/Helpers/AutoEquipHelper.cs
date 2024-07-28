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
    internal unsafe class AutoEquipHelper
    {
        
        private static TaskManager _taskManager;

        internal static void Invoke(TaskManager taskManager)
        {
            if (!AutoEquipRunning)
            {
                Svc.Log.Info($"Equipping Started");
                AutoEquipRunning = true;
                _taskManager = taskManager;
                if (AutoDuty.Plugin.Configuration.AutoEquipRecommendedGear)
                    SchedulerHelper.ScheduleAction("AutoEquipTimeout", Stop, 300000);
                else
                    SchedulerHelper.ScheduleAction("AutoEquipTimeout", Stop, 600000);
                Svc.Framework.Update += AutoEquipRecommendedGear;
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
                Svc.Framework.Update -= AutoEquipRecommendedGear;
                AutoEquipRunning = false;
                AutoDuty.Plugin.Action = "";
            }
            else
            {
                Svc.Log.Info("AutoEquip was not running");
            }
        }

        internal static bool AutoEquipRunning = false;
        
        internal static unsafe void AutoEquipRecommendedGear(IFramework framework)
        {

            if (AutoDuty.Plugin.Started)
                Stop();

            if (!EzThrottler.Check("AutoEquipRecommended"))
                return;

            EzThrottler.Throttle("AutoEquipRecommended", 250);

            if (Svc.ClientState.LocalPlayer == null)
                return;
            
            AutoDuty.Plugin.Action = "Repairing";

            if (_taskManager == null)
            {
                Svc.Log.Error("TaskManager is null");
                Stop();
                return;
            }
            
            _taskManager.Enqueue(() =>
            {
                if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat] || 
                    Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas])
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
                Stop(); // Call Stop() here to ensure the process is terminated
                return true;
            }, "UpdateGearset");
        }
    }
}