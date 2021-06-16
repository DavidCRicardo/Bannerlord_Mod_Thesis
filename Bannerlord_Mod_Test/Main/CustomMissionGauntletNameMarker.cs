using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Bannerlord_Mod_Test
{
    class CustomMissionGauntletNameMarker : MissionView
    {
        public int ViewOrderPriority { get; }
        public CustomMissionGauntletNameMarker(CustomCampaignBehaviorBase CBB) { this.ViewOrderPriorty = 1; CBB_ref = CBB; }
        private CustomMissionNameMarkerVM _dataSource;
        private CustomCampaignBehaviorBase CBB_ref;
        private GauntletLayer _gauntletLayer;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();

            _dataSource = new CustomMissionNameMarkerVM(base.Mission, base.MissionScreen.CombatCamera);
            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriorty, "GauntletLayer");
            this._gauntletLayer.LoadMovie("NameMarkerMessage", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            CampaignEvents.ConversationEnded.AddNonSerializedListener(this, new Action<CharacterObject>(this.OnConversationEnd));
        }
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);

            if (!MBCommon.IsPaused)
            {
                _dataSource.Tick(dt);

                _dataSource.EnableDataSource(_dataSource);

                CheckIntentionFromNPCToPlayer(dt);

                if (_dataSource.GetResetOtherVariables())
                {
                    CBB_ref.FriendlyBool = false;
                    CBB_ref.RomanticBool = false;
                    CBB_ref.UnFriendlyBool = false;
                    CBB_ref.HostileBool = false;
                    CBB_ref.SpecialBool = false;
                    _dataSource.SetResetOtherVariables(false);
                }
            }
        }
        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            base.MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
            _dataSource.OnFinalize();
            _dataSource = null;
        }

        private void CheckIntentionFromNPCToPlayer(float dt)
        {
            if (_dataSource.intentionReftoCampaignBehaviorBase != SocialExchangeSE.IntentionEnum.Undefined && _dataSource.characterReftoCampaignBehaviorBase != null)
            {
                CBB_ref.characterRef = _dataSource.characterReftoCampaignBehaviorBase;
                switch (_dataSource.intentionReftoCampaignBehaviorBase)
                {
                    case SocialExchangeSE.IntentionEnum.Positive:
                        CBB_ref.FriendlyBool = true;
                        break;
                    case SocialExchangeSE.IntentionEnum.Romantic:
                        CBB_ref.RomanticBool = true;
                        break;
                    case SocialExchangeSE.IntentionEnum.Negative:
                        CBB_ref.UnFriendlyBool = true;
                        break;
                    case SocialExchangeSE.IntentionEnum.Hostile:
                        CBB_ref.HostileBool = true;
                        break;
                    case SocialExchangeSE.IntentionEnum.Special:
                        CBB_ref.SpecialBool = true;
                        break;
                    default:
                        break;
                }
            }
        }
        private void OnConversationEnd(CharacterObject characterObject)
        {
            if (_dataSource != null)
            {
                CheckIfThereIsAnyChange(characterObject);
                _dataSource.OnConversationEnd2();
            }

            if (_dataSource != null && CBB_ref.AskWhatsGoinOn)
            {
                Random rnd = new Random();
                CustomAgent custom = _dataSource.customAgentsList[rnd.Next(_dataSource.customAgentsList.Count)];
                custom.selfAgent.OnUse(Agent.Main);
            }
        }
        private void CheckIfThereIsAnyChange(CharacterObject characterObject)
        {
            if (CBB_ref.giveCourage)
            {
                GiveCourageToCharacter(characterObject);
                CBB_ref.giveCourage = false;
            }

            if (CBB_ref.IncreaseFriendshipWithPlayer)
            {
                UpdateRelationWithPlayerChoice(characterObject, "Friends", 1);
                CBB_ref.IncreaseFriendshipWithPlayer = false;
            }
            if (CBB_ref.DecreaseFriendshipWithPlayer)
            {
                UpdateRelationWithPlayerChoice(characterObject, "Friends", -1);
                CBB_ref.DecreaseFriendshipWithPlayer = false;
            }
        }
        private void UpdateRelationWithPlayerChoice(CharacterObject characterObject, string relation, int value)
        {
            CustomAgent customAgent = _dataSource.customAgentsList.Find(c => c.Name == characterObject.Name.ToString());
            CustomAgent MainCustomAgent = _dataSource.customAgentsList.Find(c => c.Name == Agent.Main.Name);
            MainCustomAgent.targetAgent = customAgent.selfAgent;

            SocialExchangeSE se = new SocialExchangeSE("", MainCustomAgent, _dataSource.customAgentsList)
            {
                CustomAgentReceiver = customAgent
            };
            se.PlayerConversationWithNPC(relation, value);

            _dataSource.SaveToJson();
        }
        private void GiveCourageToCharacter(CharacterObject characterObject)
        {
            CustomAgent customAgent = _dataSource.customAgentsList.Find(c => c.Name == characterObject.Name.ToString());
            customAgent.UpdateStatus("Courage", 1);
        }
    }
}