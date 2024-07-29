using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ECommons.Automation;
using ECommons.Automation.LegacyTaskManager;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
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
        }
        
        internal static void Stop() 
        {
            if (AutoEquipRunning)
                Svc.Log.Info($"AutoEquip Finished");
            SchedulerHelper.DescheduleAction("AutoEquipTimeout");
            Svc.Framework.Update -= AutoEquipRecommendedGear;
            AutoEquipRunning = false;
            AutoDuty.Plugin.Action = "";
        }

        internal static bool AutoEquipRunning = false;

        internal static unsafe void AutoEquipRecommendedGear(IFramework framework)
        {

            _taskManager.Insert(() =>
            {
                if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat] || 
                    Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas])
                {
                    Svc.Log.Debug("Cannot equip gear: player is in combat or between areas.");
                    AutoEquipRunning = false;
                    return false;
                }
                Stop();
                return true;
            }, "CheckConditions");

            

            var mod = RecommendEquipModule.Instance();

            _taskManager.EnqueueImmediate(() => 
            {
                Svc.Log.Debug("Set up recommended gear for current class/job.");
                mod->SetupForClassJob((byte)Svc.ClientState.LocalPlayer!.ClassJob.Id);
                mod->EquipRecommendedGear();
            });

            _taskManager.EnqueueImmediate(() => 
            {
                Svc.Log.Info("Attempted to equip recommended gear.");
                mod->EquipRecommendedGear();
            });

            
            _taskManager.EnqueueImmediate(() => 
            {
                mod->EquipRecommendedGear();
                var id = RaptureGearsetModule.Instance()->CurrentGearsetIndex;
                RaptureGearsetModule.Instance()->UpdateGearset(id);
                Svc.Log.Info($"Attempted to update gearset {id}.");
                AutoEquipRunning = false;
                Stop();
                
            }, "UpdateGearset");

            // Force prevention of additional calls per queue
            Svc.Framework.Update -= AutoEquipRecommendedGear;
        }
    }
}