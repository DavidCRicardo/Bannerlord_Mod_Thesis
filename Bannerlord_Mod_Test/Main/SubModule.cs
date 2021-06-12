using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade.View.Screen;

namespace Bannerlord_Mod_Test
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
        }
        protected override void OnApplicationTick(float dt)
        {
        }
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (game.GameType is Campaign && gameStarterObject is CampaignGameStarter gameInitializer)
            {
                OnCampaignStart(game, gameStarterObject);
            }
        }
        public override void OnCampaignStart(Game game, object starterObject)
        {
            Campaign campaign = game.GameType as Campaign;
             
            if (campaign != null && Campaign.Current != null)
            {
                CampaignGameStarter gameInitializer = (CampaignGameStarter)starterObject;

                CBBref = new CustomCampaignBehaviorBase();
                gameInitializer.AddBehavior(CBBref);
            }
        }
        public override void OnGameInitializationFinished(Game game)
        {
        }
        private CustomCampaignBehaviorBase CBBref;

        public override void OnMissionBehaviourInitialize(Mission mission)
        {
            base.OnMissionBehaviourInitialize(mission);

            mission.MissionBehaviours.Add(new CustomMissionGauntletNameMarker(CBBref));           
        }
    }
}