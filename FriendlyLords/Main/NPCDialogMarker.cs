using System;
using System.Collections.Generic;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace FriendlyLords
{
    class CIFDialogMarker : MissionView
    {
        public CIFDialogMarker(CiF_CampaignBehavior_Dialogs CBB, Mission _mission) { CBB_ref = CBB; mission = _mission; }
        private CIFManager _dataSource;
        private CiF_CampaignBehavior_Dialogs CBB_ref;
        private GauntletLayer _gauntletLayer;
        private Mission mission;

        private bool _firstTick = true;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            
            _dataSource = new CIFManager(mission, base.MissionScreen.CombatCamera);
            this._gauntletLayer = new GauntletLayer(1, "GauntletLayer", false);
            this._gauntletLayer.LoadMovie("MessageMarker", this._dataSource);
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

                if (_dataSource.letsUpdate)
                {
                    _dataSource.letsUpdate = false;
                }

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

                if (CBB_ref.ResetSocialExchanges)
                {
                    _dataSource.ResetSocialExchangesAllNPCsOptions();
                    CBB_ref.ResetSocialExchanges = false;            
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
            if (_dataSource.customCharacterReftoCampaignBehaviorBase != null)
            {
                CBB_ref.characterRefWithDesireToPlayer = _dataSource.customCharacterReftoCampaignBehaviorBase;
                CBB_ref.characterIdRefWithDesireToPlayer = _dataSource.customCharacterIdRefCampaignBehaviorBase;

                switch (_dataSource.SocialExchange_E)
                {
                    case CIFManager.SEs_Enum.Compliment: 
                        CBB_ref.FriendlyBool = true;
                        break;
                    case CIFManager.SEs_Enum.GiveGift: 
                        CBB_ref.OfferGift = true;
                        break;
                    case CIFManager.SEs_Enum.Jealous:
                    case CIFManager.SEs_Enum.FriendSabotage:
                        CBB_ref.UnFriendlyBool = true;
                        break;
                    case CIFManager.SEs_Enum.Flirt:
                        CBB_ref.RomanticBool = true;
                        break;
                    case CIFManager.SEs_Enum.Bully:
                    case CIFManager.SEs_Enum.RomanticSabotage:
                        CBB_ref.HostileBool = true;
                        break;
                    case CIFManager.SEs_Enum.AskOut:
                        CBB_ref.AskOutPerformed = true; 
                        break;
                    case CIFManager.SEs_Enum.Break:
                        CBB_ref.BreakBool = true;
                        break;
                    case CIFManager.SEs_Enum.Admiration:
                        CBB_ref.GratitudeBool = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private void OnConversationEnd(CharacterObject characterObject)
        {
            if (_dataSource != null && _dataSource.customAgentsList != null)
            {
                //if (CBB_ref.customAgentConversation == null) 
                //{
                    foreach (CIF_Character custom in _dataSource.customAgentsList)
                    {
                        if (custom.AgentReference.Character == characterObject && custom == _dataSource.customAgentInteractingWithPlayer)
                        {
                            //CBB_ref.customAgentConversation = custom;
                            break;
                        }
                    }
                //}         
     
                CBB_ref.FriendlyOptionExists = false;
                CBB_ref.UnFriendlyOptionExists = false;
                CBB_ref.RomanticOptionExists = false;
                CBB_ref.HostileOptionExists = false;
                CBB_ref.auxBool = false;

                if (CBB_ref.customAgentConversation != null)
                {
                    CheckIfThereIsAnyChange(CBB_ref.customAgentConversation);
                    _dataSource.OnConversationEndWithPlayer(CBB_ref.customAgentConversation);
                }
            }
        }

        private void CheckIfThereIsAnyChange(CIF_Character customAgentConversation)
        {
            if (CBB_ref.AskOutPerformed)
            {
                CBB_ref.AskOutPerformed = false;
                if (_dataSource.playerStartedASE)
                {
                    CheckOptionToLock(customAgentConversation, "AskOut");
                }
            }
            else if (CBB_ref.HaveAChildInitialMovePerformed)
            {
                CBB_ref.HaveAChildInitialMovePerformed = false;
                if (_dataSource.playerStartedASE)
                {
                    CheckOptionToLock(customAgentConversation, "HaveAChild");
                }
            }

            if (CBB_ref.StartDating)
            {
                Start_Dating(customAgentConversation);

                CBB_ref.StartDating = false;

                if (_dataSource.playerStartedASE)
                {
                    CheckOptionToLock(customAgentConversation, "AskOut");
                }

                InformationManager.DisplayMessage(new InformationMessage(Agent.Main.Name + " is now Dating with " + customAgentConversation.Name));
            }
            else if (CBB_ref.DoBreak)
            {
                DoBreak(customAgentConversation);
                CBB_ref.DoBreak = false;

                if (_dataSource.playerStartedASE)
{
                    CheckOptionToLock(customAgentConversation, "Break");
                }

                InformationManager.DisplayMessage(new InformationMessage(Agent.Main.Name + " is broke up with " + customAgentConversation.Name));
            }
            else if (CBB_ref.IncreaseRelationshipWithPlayer && CBB_ref.customAgentConversation != null)
            {
                string localRelation = GetRelationshipBetweenPlayerAndNPC();
                int value = 1;

                if (_dataSource.playerStartedASE)
                {
                    CheckOptionToLock(customAgentConversation, localRelation, value);
                }

                RelationInGameChanges(customAgentConversation, value);
                UpdateRelationWithPlayerChoice(customAgentConversation, localRelation, value, se_enum);          

                CBB_ref.IncreaseRelationshipWithPlayer = false;
            }
            else if (CBB_ref.DecreaseRelationshipWithPlayer && CBB_ref.customAgentConversation != null)
            {
                string localRelation = GetRelationshipBetweenPlayerAndNPC();
                int value = -1;

                if (_dataSource.playerStartedASE)
                {
                    CheckOptionToLock(customAgentConversation, localRelation, value);
                }

                RelationInGameChanges(customAgentConversation, value);
                UpdateRelationWithPlayerChoice(customAgentConversation, localRelation, value, se_enum);

                CBB_ref.DecreaseRelationshipWithPlayer = false;
            }
        }
        CIFManager.SEs_Enum se_enum { get; set; }

        private void CheckOptionToLock(CIF_Character customAgentConversation, string localRelation, int value = 0)
        {
            if (localRelation == "AskOut" )
            {
                SetOptionAsUnavailable(customAgentConversation, CIF_Character.Intentions.Special, true);
                se_enum = CIFManager.SEs_Enum.AskOut;
            }
            else if (localRelation == "Break")
            {
                SetOptionAsUnavailable(customAgentConversation, CIF_Character.Intentions.Special, true);
                se_enum = CIFManager.SEs_Enum.Break;
            }
            else if (localRelation == "HaveAChild")
            {
                SetOptionAsUnavailable(customAgentConversation, CIF_Character.Intentions.Special, true);
                se_enum = CIFManager.SEs_Enum.HaveAChild;
            }
            else
            {
                if (localRelation == "Friends")
                {
                    if (value > 0)
                    {
                        SetOptionAsUnavailable(customAgentConversation, CIF_Character.Intentions.Friendly, true);
                        se_enum = CIFManager.SEs_Enum.Compliment;
                    }
                    else
                    {
                        SetOptionAsUnavailable(customAgentConversation, CIF_Character.Intentions.Unfriendly, true);
                        se_enum = CIFManager.SEs_Enum.Jealous;
                    }
                }
                else
                {
                    if (value > 0)
                    {
                        SetOptionAsUnavailable(customAgentConversation, CIF_Character.Intentions.Romantic, true);
                        se_enum = CIFManager.SEs_Enum.Flirt;
                    }
                    else
                    {
                        SetOptionAsUnavailable(customAgentConversation, CIF_Character.Intentions.Hostile, true);
                        se_enum = CIFManager.SEs_Enum.Bully;
                    }
                }
            }

            //Player fez uma SE com um NPC e vai guardar a info 
            //Save information from dictionary and variables to File
            //UpdateUserInfo(ConvertCustomAgentSEToDictionaryEnum(se_enum), 1);

            _dataSource.SaveSavedSEs(customAgentConversation, se_enum.ToString());
        }

        private void SetOptionAsUnavailable(CIF_Character customAgent, CIF_Character.Intentions intention, bool value)
        {
            customAgent.keyValuePairsSEs[intention] = value;
        }

        private static void RelationInGameChanges(CIF_Character customAgentConversation, int value)
        {
            Hero hero = Hero.FindFirst(h => h.CharacterObject == customAgentConversation.AgentReference.Character);
            if (hero != null && hero != Hero.MainHero)
            {
                float relationWithPlayer = hero.GetRelationWithPlayer();
                int newValue = (int)(relationWithPlayer + value);
                if (value > 0)
                {
                    if (newValue <= 100)
                    {
                        //InformationManager.AddQuickInformation(new TextObject("Your relation is increased by " + value + " to " + newValue + " with " + hero.Name + "."), 0, hero.CharacterObject);
                        Hero.MainHero.SetPersonalRelation(hero, newValue);
                    }              
                }
                else if (value < 0) 
                {
                    //InformationManager.AddQuickInformation(new TextObject("Your relation is decreased by " + value + " to " + newValue + " with " + hero.Name + "."), 0, hero.CharacterObject);
                    Hero.MainHero.SetPersonalRelation(hero, newValue);
                }
            }
        }

        private string GetRelationshipBetweenPlayerAndNPC()
        {
            CIF_Character AgentPlayer = _dataSource.customAgentsList.Find(c => c.AgentReference == Agent.Main);
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

        private void DoBreak(CIF_Character customAgentConversation)
        {
            CIF_SocialExchange se = InitializeSocialExchange(customAgentConversation, CIFManager.SEs_Enum.Break);
            se.BreakUpMethod();

            _dataSource.SaveToJson();
        }

        private void Start_Dating(CIF_Character customAgentConversation)
        {
            CIF_SocialExchange se = InitializeSocialExchange(customAgentConversation, CIFManager.SEs_Enum.AskOut);
            se.AskOutMethod(true);

            _dataSource.SaveToJson();
        }

        private void UpdateRelationWithPlayerChoice(CIF_Character customAgentConversation, string relation, int value, CIFManager.SEs_Enum seEnum)
        {
            CIF_SocialExchange se = InitializeSocialExchange(customAgentConversation, seEnum);
            se.PlayerConversationWithNPC(relation, value, customAgentConversation.AgentReference.IsHero);

            _dataSource.SaveToJson();
        }

        private CIF_SocialExchange InitializeSocialExchange(CIF_Character customAgentConversation, CIFManager.SEs_Enum seEnum)
        {
            CIF_Character customAgent = _dataSource.customAgentsList.Find(c => c.AgentReference.Name == customAgentConversation.AgentReference.Name && c.Id == customAgentConversation.Id);
            CIF_Character MainCustomAgent = _dataSource.customAgentsList.Find(c => c.AgentReference == Agent.Main);
            MainCustomAgent.customAgentTarget = customAgent;

            CIF_SocialExchange se = new CIF_SocialExchange(seEnum, MainCustomAgent, _dataSource.customAgentsList)
            {
                CustomAgentReceiver = customAgent
            };
            return se;
        }

        private void ResetCBB_refVariables()
        {
            CBB_ref.FriendlyBool = false;
            CBB_ref.OfferGift = false;
            CBB_ref.RomanticBool = false;
            CBB_ref.UnFriendlyBool = false;
            CBB_ref.HostileBool = false;
            CBB_ref.BreakBool = false;
            CBB_ref.StartDating = false;
            CBB_ref.DoBreak = false;
            CBB_ref.GratitudeBool = false;
            CBB_ref.IncreaseRelationshipWithPlayer = false;
            CBB_ref.DecreaseRelationshipWithPlayer = false;

            _dataSource.SocialExchange_E = CIFManager.SEs_Enum.Undefined;
            _dataSource.customCharacterReftoCampaignBehaviorBase = null;
        }       
    }
}