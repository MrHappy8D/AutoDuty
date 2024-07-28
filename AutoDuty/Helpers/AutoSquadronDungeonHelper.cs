using Dalamud.Game;
using Dalamud.Plugin.Services;
using Dalamud.Logging;
using ECommons.DalamudServices;

namespace AutoDuty.Helpers
{
    public static class AutoSquadronDungeonHelper
    {
        public static uint? GetAppropriateSquadronDungeonId(IClientState clientState)
        {
            Svc.Log.Info("GetAppropriateSquadronDungeonId called");

            var playerCharacter = clientState.LocalPlayer;
            if (playerCharacter == null)
            {
                Svc.Log.Info("LocalPlayer is null");
                return null;
            }

            var currentLevel = playerCharacter.Level;
            Svc.Log.Info($"Player level: {currentLevel}");

            // Switch case based on level
            var dungeonId = currentLevel switch
            {
                >= 20 and < 24 => 162u,
                >= 24 and < 32 => 1039u,
                >= 32 and < 41 => 1041u,
                >= 41 and < 53 => 1042u,
                >= 53 and < 61 => 1064u,
                _ => (uint?)null
            };

            Svc.Log.Info($"Selected dungeon ID: {dungeonId}");
            return dungeonId;
        }
    }
}