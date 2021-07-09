using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;

namespace Bannerlord_Social_AI
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

                CBBref = new CiF_CampaignBehavior_Dialogs();
                gameInitializer.AddBehavior(CBBref);
            }
        }
        public override void OnGameInitializationFinished(Game game)
        {
        }

        private CiF_CampaignBehavior_Dialogs CBBref;

        public override void OnMissionBehaviourInitialize(Mission mission)
        {
            base.OnMissionBehaviourInitialize(mission);

            mission.MissionBehaviours.Add(new CustomMissionGauntletNameMarker(CBBref, mission));           
        }
    }
}