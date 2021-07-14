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
            
            try
            {
                CampaignEvents.ConversationEnded.AddNonSerializedListener(this, new Action<CharacterObject>(this.OnConversationEnd));
            }
            catch (Exception e) { }
        }
        
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);

            if (!MBCommon.IsPaused && CampaignMission.Current != null)
            {
                _dataSource.Tick(dt);

                _dataSource.EnableDataSource();

                if (_firstTick || CBB_ref.customAgents == null)
                {
                    _dataSource.IsEnabled = true;
                    CBB_ref.customAgents = _dataSource.customAgentsList;
                    _firstTick = false;
                }

                CheckIntentionFromNPCToPlayer();

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

        private void CheckIntentionFromNPCToPlayer()
        {
            if (_dataSource.intentionRefToCBB != SocialExchangeSE.IntentionEnum.Undefined && _dataSource.customCharacterReftoCampaignBehaviorBase != null)
            {
                // check social move from character (offergift e.g) 
                CBB_ref.characterRef = _dataSource.customCharacterReftoCampaignBehaviorBase;
                switch (_dataSource.intentionRefToCBB)
                {
                    case SocialExchangeSE.IntentionEnum.Positive:
                        if (CBB_ref.characterRef.SocialMove == "GiveGift")
                        {
                            CBB_ref.OfferGift = true;
                        }
                        else
                        {
                            CBB_ref.FriendlyBool = true;
                        }
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
        }
        
        private void CheckIfThereIsAnyChange(CustomAgent customAgentConversation)
        {
            if (CBB_ref.StartDating)
            {
                Start_Dating(customAgentConversation);
                CBB_ref.StartDating = false;
                InformationManager.DisplayMessage(new InformationMessage(Agent.Main.Name + " is now Dating with " + customAgentConversation.Name));
            }
            else if (CBB_ref.DoBreak)
            {
                DoBreak(customAgentConversation);
                CBB_ref.DoBreak = false;

                InformationManager.DisplayMessage(new InformationMessage(Agent.Main.Name + " is broke up with " + customAgentConversation.Name));

            }
            else if (CBB_ref.IncreaseRelationshipWithPlayer)
            {
                string localRelation = GetRelationshipBetweenPlayerAndNPC();

                UpdateRelationWithPlayerChoice(customAgentConversation, localRelation, 1);
                CBB_ref.IncreaseRelationshipWithPlayer = false;
            }
            else if (CBB_ref.DecreaseRelationshipWithPlayer)
            {
                string localRelation = GetRelationshipBetweenPlayerAndNPC();

                UpdateRelationWithPlayerChoice(customAgentConversation, localRelation, -1);
                CBB_ref.DecreaseRelationshipWithPlayer = false;
            }
            else if (CBB_ref.giveCourage)
            {
                GiveCourageToCharacter(customAgentConversation);
                CBB_ref.giveCourage = false;
            }
        }

        private string GetRelationshipBetweenPlayerAndNPC()
        {
            CustomAgent AgentPlayer = _dataSource.customAgentsList.Find(c => c.selfAgent == Agent.Main);
            SocialNetworkBelief belief = AgentPlayer.SelfGetBeliefWithAgent(CBB_ref.customAgentConversation);

            string localRelation = "";
            if (belief == null)
            {
                localRelation = "Friends";
            }
            else
            {
                localRelation = belief.relationship;
            }

            return localRelation;
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

        private void UpdateRelationWithPlayerChoice(CustomAgent customAgentConversation, string relation, int value)
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
            CBB_ref.OfferGift = false;
            CBB_ref.RomanticBool = false;
            CBB_ref.UnFriendlyBool = false;
            CBB_ref.HostileBool = false;
            CBB_ref.SpecialBool = false;
            CBB_ref.StartDating = false;
            CBB_ref.DoBreak = false;
            CBB_ref.IncreaseRelationshipWithPlayer = false;
            CBB_ref.DecreaseRelationshipWithPlayer = false;
        }
    }
}