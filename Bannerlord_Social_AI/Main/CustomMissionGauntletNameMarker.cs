using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Bannerlord_Social_AI
{
    class CustomMissionGauntletNameMarker : MissionView
    {
        public int ViewOrderPriority { get; }
        public CustomMissionGauntletNameMarker(CiF_CampaignBehavior_Dialogs CBB, Mission _mission) { this.ViewOrderPriorty = 1; CBB_ref = CBB; mission = _mission; }
        private CustomMissionNameMarkerVM _dataSource;
        private CiF_CampaignBehavior_Dialogs CBB_ref;
        private GauntletLayer _gauntletLayer;
        private Mission mission;
        private bool _firstTick = true;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();

            _dataSource = new CustomMissionNameMarkerVM(mission, base.MissionScreen.CombatCamera);
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

                _dataSource.EnableDataSource();

                if (_firstTick)
                {
                    _dataSource.IsEnabled = true;
                    CBB_ref.customAgents = _dataSource.customAgentsList;
                    _firstTick = false;
                }

                CheckIntentionFromNPCToPlayer(dt);

                if (_dataSource.GetCanResetCBB_refVariables())
                {
                    ResetCBB_refVariables();
                    _dataSource.SetCanResetCBB_refVariables(false);
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
            if (_dataSource.intentionRefToCBB != SocialExchangeSE.IntentionEnum.Undefined && _dataSource.customCharacterReftoCampaignBehaviorBase != null)
            {
                CBB_ref.characterRef = _dataSource.customCharacterReftoCampaignBehaviorBase;
                switch (_dataSource.intentionRefToCBB)
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
                CheckIfThereIsAnyChange(CBB_ref.customAgentConversation);
                _dataSource.OnConversationEndWithPlayer(CBB_ref.customAgentConversation);
            }

            //if (_dataSource != null && CBB_ref.AskWhatsGoinOn)
            //{
            //    Random rnd = new Random();
            //    CustomAgent custom = _dataSource.customAgentsList[rnd.Next(_dataSource.customAgentsList.Count)];
            //    custom.selfAgent.OnUse(Agent.Main);
            //}
        }
        
        private void CheckIfThereIsAnyChange(CustomAgent customAgentConversation)
        {
            if (CBB_ref.StartDating)
            {
                Start_Dating(customAgentConversation);
                CBB_ref.StartDating = false;
            }
            else if (CBB_ref.DoBreak)
            {
                DoBreak(customAgentConversation);
                CBB_ref.DoBreak = false;
            }
            else if (CBB_ref.IncreaseFriendshipWithPlayer)
            {
                UpdateRelationWithPlayerChoice(customAgentConversation, "Friends", 1, Agent.Main);
                CBB_ref.IncreaseFriendshipWithPlayer = false;
            }
            else if (CBB_ref.DecreaseFriendshipWithPlayer)
            {
                UpdateRelationWithPlayerChoice(customAgentConversation, "Friends", -1, Agent.Main);
                CBB_ref.DecreaseFriendshipWithPlayer = false;
            }
            else if (CBB_ref.IncreaseDatingWithPlayer)
            {
                UpdateRelationWithPlayerChoice(customAgentConversation, "Dating", 1, Agent.Main);
                CBB_ref.IncreaseDatingWithPlayer = false;
            }
            else if (CBB_ref.DecreaseDatingWithPlayer)
            {
                UpdateRelationWithPlayerChoice(customAgentConversation, "Dating", -1, Agent.Main);
                CBB_ref.DecreaseDatingWithPlayer = false;
            }
            else if (CBB_ref.giveCourage)
            {
                GiveCourageToCharacter(customAgentConversation);
                CBB_ref.giveCourage = false;
            }


        }

        private void DoBreak(CustomAgent customAgentConversation)
        {
            SocialExchangeSE se = InitializeSocialExchange(customAgentConversation);
            se.BreakUpMethod();

            _dataSource.SaveToJson();
        }

        private void Start_Dating(CustomAgent customAgentConversation)
        {
            SocialExchangeSE se = InitializeSocialExchange(customAgentConversation);
            se.AskOutMethod();

            _dataSource.SaveToJson();
        }

        private void UpdateRelationWithPlayerChoice(CustomAgent customAgentConversation, string relation, int value , Agent agent = null)
        {
            SocialExchangeSE se = InitializeSocialExchange(customAgentConversation);
            se.PlayerConversationWithNPC(relation, value);

            _dataSource.SaveToJson();
        }

        private SocialExchangeSE InitializeSocialExchange(CustomAgent customAgentConversation)
        {
            CustomAgent customAgent = _dataSource.customAgentsList.Find(c => c.selfAgent.Name == customAgentConversation.selfAgent.Name && c.Id == customAgentConversation.Id);
            CustomAgent MainCustomAgent = _dataSource.customAgentsList.Find(c => c.selfAgent == Agent.Main);
            MainCustomAgent.customAgentTarget = customAgent;

            SocialExchangeSE se = new SocialExchangeSE("", MainCustomAgent, _dataSource.customAgentsList)
            {
                CustomAgentReceiver = customAgent
            };
            return se;
        }

        private void GiveCourageToCharacter(CustomAgent customAgentConversation)
        {
            CustomAgent customAgent = _dataSource.customAgentsList.Find(c => c.selfAgent.Name == customAgentConversation.selfAgent.Name && c.Id == customAgentConversation.Id);
            customAgent.UpdateAllStatus(0, 0, 1, 0, 0, 0);
        }

        private void ResetCBB_refVariables()
        {
            CBB_ref.FriendlyBool = false;
            CBB_ref.RomanticBool = false;
            CBB_ref.UnFriendlyBool = false;
            CBB_ref.HostileBool = false;
            CBB_ref.SpecialBool = false;
            CBB_ref.StartDating = false;
            CBB_ref.DoBreak = false;
        }
    }
}