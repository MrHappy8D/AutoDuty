using Dalamud.Game;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using AutoDuty.Managers; 
using AutoDuty.Helpers;
using ECommons.Automation.LegacyTaskManager;

namespace AutoDuty.Helpers
{
    public enum DungeonManagerType
    {
        Regular,
        Support,
        Squadron,
        Trust
    }

    public static class AutoDungeonHelper
    {
        public static (uint?, DungeonManagerType) GetAppropriateDungeon(IClientState clientState, bool useSquadronsIfPossible, bool preferTrust)
        {
            Svc.Log.Info("GetAppropriateDungeon called");

            var playerCharacter = clientState.LocalPlayer;
            if (playerCharacter == null)
            {
                Svc.Log.Info("LocalPlayer is null");
                return (null, DungeonManagerType.Regular);
            }

            var currentLevel = playerCharacter.Level;
            Svc.Log.Info($"Player level: {currentLevel}");

            // Switch case based on level
            var (dungeonId, managerType) = currentLevel switch
            {
                >= 16 and < 24 => (1036u, DungeonManagerType.Support), // TamTara Deepcroft
                >= 24 and < 32 => (1039u, DetermineManagerType(useSquadronsIfPossible, false, true)), // The Thousand Maws of Toto-Rak
                >= 32 and < 41 => (1041u, DetermineManagerType(useSquadronsIfPossible, false, true)), // Brayflox's Longstop
                >= 41 and < 53 => (1042u, DetermineManagerType(useSquadronsIfPossible, false, true)), // Stone Vigil
                >= 53 and < 61 => (1064u, DetermineManagerType(useSquadronsIfPossible, false, true)), // Sohm Al
                >= 61 and < 67 => (1142u, DetermineManagerType(false, false, false)), // Sirenson Sea
                >= 67 and < 71 => (1144u, DetermineManagerType(false, false, false)), // Doma Castle
                >= 71 and < 75 => (837u, DetermineManagerType(false, preferTrust, false)), // Holminster
                >= 75 and < 81 => (823u, DetermineManagerType(false, preferTrust, false)), // Qitana
                >= 81 and < 87 => (952u, DetermineManagerType(false, preferTrust, false)), // Tower of Zot
                >= 87 and < 89 => (974u, DetermineManagerType(false, preferTrust, false)), // Ktisis Hyperboreia
                >= 89 and < 91 => (978u, DetermineManagerType(false, preferTrust, false)), // The Aitiascope
                >= 91 and < 93 => (1167u, DetermineManagerType(false, preferTrust, false)), // Ihuykatumu
                >= 93 and < 95 => (1193u, DetermineManagerType(false, preferTrust, false)), // Worqor Zormor
                >= 95 and < 97 => (11944u, DetermineManagerType(false, preferTrust, false)), // The Skydeep Cenote
                >= 97 and < 99 => (1198u, DetermineManagerType(false, preferTrust, false)), // Vanguard
                >= 99 and < 100 => (1208u, DetermineManagerType(false, preferTrust, false)), // Origenics
                _ => ((uint?)null, DungeonManagerType.Support)
            };


            Svc.Log.Info($"Selected dungeon ID: {dungeonId}, Manager Type: {managerType}");
            return (dungeonId, managerType);
        }

        private static DungeonManagerType DetermineManagerType(bool useSquadronsIfPossible, bool preferTrust, bool squadronAvailable)
        {
            if (preferTrust)
            {
                return DungeonManagerType.Trust;
            }
            else if (useSquadronsIfPossible && squadronAvailable)
            {
                return DungeonManagerType.Squadron;
            }
            else
            {
                return DungeonManagerType.Support;
            }
        }

        internal static void RegisterDungeonBasedOnType(uint? dungeonId, DungeonManagerType managerType, 
            DutySupportManager dutySupportManager, SquadronManager squadronManager, TrustManager trustManager,
            TaskManager taskManager)
        {
            if (!dungeonId.HasValue)
            {
                Svc.Log.Error("No dungeon ID provided.");
                return;
            }

            if (ContentHelper.DictionaryContent.TryGetValue(dungeonId.Value, out var content))
            {
                switch (managerType)
                {
                    case DungeonManagerType.Support:
                        dutySupportManager.RegisterDutySupport(content);
                        break;
                    case DungeonManagerType.Squadron:
                        taskManager.Enqueue(() => GotoBarracksHelper.Invoke(), "Run-GotoBarracksInvoke");
                        taskManager.DelayNext("Run-Delay50", 50);
                        taskManager.Enqueue(() => !GotoBarracksHelper.GotoBarracksRunning && !GotoInnHelper.GotoInnRunning, int.MaxValue, "Run-WaitGotoComplete");
                        squadronManager.RegisterSquadron(content);
                        break;
                    case DungeonManagerType.Trust:
                        trustManager.RegisterTrust(content);
                        break;
                    default:
                        Svc.Log.Error("Unsupported dungeon manager type. Defaulting to DutySupport.");
                        dutySupportManager.RegisterDutySupport(content);
                        break;
                }
            }
            else
            {
                Svc.Log.Error($"No content found for TerritoryType: {dungeonId.Value}");
            }
        }
    }
}