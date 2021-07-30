using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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
        private string fileName = "";
        private string filePath = "";

        private int TotalSEs;
        private int NPCsInteractedWithPlayer;
        private int PlayerInteractedWithNPCs;
        private int NPCsInteractedWithNPCs;
        private int DaysPassed;

        private List<string> list;

        private enum DictionaryEnumWithSEs { Undefined, Compliment, GiveGift, Gratitude, Jealous, FriendSabotage, AskOut, Flirt, Bully, RomanticSabotage, Break, HaveAChild }

        private Dictionary<bool, Dictionary<Enum, int>> PlayerOrNPC_Dictionary =
            new Dictionary<bool, Dictionary<Enum, int>>
            {
                { false , new Dictionary<Enum, int> { 
                    { DictionaryEnumWithSEs.Undefined       , 0 },
                    { DictionaryEnumWithSEs.Compliment      , 0 },
                    { DictionaryEnumWithSEs.GiveGift        , 0 },
                    { DictionaryEnumWithSEs.Gratitude       , 0 },
                    { DictionaryEnumWithSEs.Jealous         , 0 },
                    { DictionaryEnumWithSEs.FriendSabotage  , 0 },
                    { DictionaryEnumWithSEs.AskOut          , 0 },
                    { DictionaryEnumWithSEs.Flirt           , 0 },
                    { DictionaryEnumWithSEs.Bully           , 0 },
                    { DictionaryEnumWithSEs.RomanticSabotage, 0 },
                    { DictionaryEnumWithSEs.Break           , 0 },
                    { DictionaryEnumWithSEs.HaveAChild      , 0 }
                    
                } },
                { true , new Dictionary<Enum, int> {
                    { DictionaryEnumWithSEs.Undefined       , 0 },
                    { DictionaryEnumWithSEs.Compliment      , 0 },
                    { DictionaryEnumWithSEs.GiveGift        , 0 },
                    { DictionaryEnumWithSEs.Gratitude       , 0 },
                    { DictionaryEnumWithSEs.Jealous         , 0 },
                    { DictionaryEnumWithSEs.FriendSabotage  , 0 },
                    { DictionaryEnumWithSEs.AskOut          , 0 },
                    { DictionaryEnumWithSEs.Flirt           , 0 },
                    { DictionaryEnumWithSEs.Bully           , 0 },
                    { DictionaryEnumWithSEs.RomanticSabotage, 0 },
                    { DictionaryEnumWithSEs.Break           , 0 },
                    { DictionaryEnumWithSEs.HaveAChild      , 0 }
                } },
            };

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            
            _dataSource = new CustomMissionNameMarkerVM(mission, base.MissionScreen.CombatCamera);
            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriorty, "GauntletLayer");
            this._gauntletLayer.LoadMovie("NameMarkerMessage", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);

            CheckIfUserFileExists();

            LoadUserInfoFromFile();

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

                    var result = ConvertCustomAgentSEToDictionaryEnum(_dataSource.SocialExchange_E);
                    UpdateUserInfo(result, _dataSource.BooleanNumber);
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
                    DaysPassed++;

                    SaveUserInfoToFile();
                    UploadFileToFTP();
                }
            }
        }

        public override void OnMissionScreenFinalize()
        {
            UploadFileToFTP();

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
                CBB_ref.characterRef = _dataSource.customCharacterReftoCampaignBehaviorBase;
                CBB_ref.characterIdRef = _dataSource.customCharacterIdRefCampaignBehaviorBase;

                switch (_dataSource.SocialExchange_E)
                {
                    case CustomMissionNameMarkerVM.SEs_Enum.Compliment: 
                        CBB_ref.FriendlyBool = true;
                        break;
                    case CustomMissionNameMarkerVM.SEs_Enum.GiveGift: 
                        CBB_ref.OfferGift = true;
                        break;
                    case CustomMissionNameMarkerVM.SEs_Enum.Jealous:
                    case CustomMissionNameMarkerVM.SEs_Enum.FriendSabotage:
                        CBB_ref.UnFriendlyBool = true;
                        break;
                    case CustomMissionNameMarkerVM.SEs_Enum.Flirt:
                        CBB_ref.RomanticBool = true;
                        break;
                    case CustomMissionNameMarkerVM.SEs_Enum.Bully:
                    case CustomMissionNameMarkerVM.SEs_Enum.RomanticSabotage:
                        CBB_ref.HostileBool = true;
                        break;
                    case CustomMissionNameMarkerVM.SEs_Enum.AskOut:
                        CBB_ref.AskOutPerformed = true; //?
                        break;
                    case CustomMissionNameMarkerVM.SEs_Enum.Break:
                        CBB_ref.SpecialBool = true;
                        break;
                    case CustomMissionNameMarkerVM.SEs_Enum.Gratitude:
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
                if (CBB_ref.customAgentConversation == null) // prevent some kind of bug
                {
                    foreach (CustomAgent custom in _dataSource.customAgentsList)
                    {
                        if (custom.selfAgent.Character == characterObject && custom == _dataSource.customAgentInteractingWithPlayer)
                        {
                            CBB_ref.customAgentConversation = custom;
                            break;
                        }
                    }
                }         
     
                CBB_ref.FriendlyOptionExists = false;
                CBB_ref.UnFriendlyOptionExists = false;
                CBB_ref.RomanticOptionExists = false;
                CBB_ref.HostileOptionExists = false;

                CheckIfThereIsAnyChange(CBB_ref.customAgentConversation);
                _dataSource.OnConversationEndWithPlayer(CBB_ref.customAgentConversation);
            }
        }

        private void CheckIfThereIsAnyChange(CustomAgent customAgentConversation)
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

                DictionaryEnumWithSEs key = ConvertCustomAgentSEToDictionaryEnum(CustomMissionNameMarkerVM.SEs_Enum.Break);
                UpdateUserInfo(key, 1);
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
        CustomMissionNameMarkerVM.SEs_Enum se_enum { get; set; }

        private void CheckOptionToLock(CustomAgent customAgentConversation, string localRelation, int value = 0)
        {
            if (localRelation == "AskOut" )
            {
                SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Special, true);
                se_enum = CustomMissionNameMarkerVM.SEs_Enum.AskOut;
            }
            else if (localRelation == "Break")
            {
                SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Special, true);
                se_enum = CustomMissionNameMarkerVM.SEs_Enum.Break;
            }
            else if (localRelation == "HaveAChild")
            {
                SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Special, true);
                se_enum = CustomMissionNameMarkerVM.SEs_Enum.HaveAChild;
            }
            else
            {
                if (localRelation == "Friends")
                {
                    if (value > 0)
                    {
                        SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Friendly, true);
                        se_enum = CustomMissionNameMarkerVM.SEs_Enum.Compliment;
                    }
                    else
                    {
                        SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Unfriendly, true);
                        se_enum = CustomMissionNameMarkerVM.SEs_Enum.Jealous;
                    }
                }
                else
                {
                    if (value > 0)
                    {
                        SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Romantic, true);
                        se_enum = CustomMissionNameMarkerVM.SEs_Enum.Flirt;
                    }
                    else
                    {
                        SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Hostile, true);
                        se_enum = CustomMissionNameMarkerVM.SEs_Enum.Bully;
                    }
                }
            }

            //Player fez uma SE com um NPC e vai guardar a info 
            DictionaryEnumWithSEs SE_Enum = ConvertCustomAgentSEToDictionaryEnum(se_enum);
            // Save information from dictionary and variables to File
            UpdateUserInfo(SE_Enum, 1);

            _dataSource.SaveSavedSEs(customAgentConversation, se_enum.ToString());
        }

        private void SetOptionAsUnavailable(CustomAgent customAgent, CustomAgent.Intentions intention, bool value)
        {
            customAgent.keyValuePairsSEs[intention] = value;
        }

        private static void RelationInGameChanges(CustomAgent customAgentConversation, int value)
        {
            Hero hero = Hero.FindFirst(h => h.CharacterObject == customAgentConversation.selfAgent.Character);
            if (hero != null && hero != Hero.MainHero)
            {
                float relationWithPlayer = hero.GetRelationWithPlayer();
                int newValue = (int)(relationWithPlayer + value);
                if (value > 0)
                {
                    if (newValue <= 100)
                    {
                        InformationManager.AddQuickInformation(new TextObject("Your relation is increased by " + value + " to " + newValue + " with " + hero.Name + "."), 0, hero.CharacterObject);
                        Hero.MainHero.SetPersonalRelation(hero, newValue);
                    }              
                }
                else
                {
                    InformationManager.AddQuickInformation(new TextObject("Your relation is decreased by " + value + " to " + newValue + " with " + hero.Name + "."), 0, hero.CharacterObject);
                    Hero.MainHero.SetPersonalRelation(hero, newValue);
                }
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
            SocialExchangeSE se = InitializeSocialExchange(customAgentConversation, CustomMissionNameMarkerVM.SEs_Enum.Break);
            se.BreakUpMethod();

            _dataSource.SaveToJson();
        }

        private void Start_Dating(CustomAgent customAgentConversation)
        {
            SocialExchangeSE se = InitializeSocialExchange(customAgentConversation, CustomMissionNameMarkerVM.SEs_Enum.AskOut);
            se.AskOutMethod(true);

            _dataSource.SaveToJson();
        }

        private void UpdateRelationWithPlayerChoice(CustomAgent customAgentConversation, string relation, int value, CustomMissionNameMarkerVM.SEs_Enum seEnum)
        {
            SocialExchangeSE se = InitializeSocialExchange(customAgentConversation, seEnum);
            se.PlayerConversationWithNPC(relation, value, customAgentConversation.selfAgent.IsHero);

            _dataSource.SaveToJson();
        }

        private SocialExchangeSE InitializeSocialExchange(CustomAgent customAgentConversation, CustomMissionNameMarkerVM.SEs_Enum seEnum)
        {
            CustomAgent customAgent = _dataSource.customAgentsList.Find(c => c.selfAgent.Name == customAgentConversation.selfAgent.Name && c.Id == customAgentConversation.Id);
            CustomAgent MainCustomAgent = _dataSource.customAgentsList.Find(c => c.selfAgent == Agent.Main);
            MainCustomAgent.customAgentTarget = customAgent;

            SocialExchangeSE se = new SocialExchangeSE(seEnum, MainCustomAgent, _dataSource.customAgentsList)
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
            CBB_ref.SpecialBool = false;
            CBB_ref.StartDating = false;
            CBB_ref.DoBreak = false;
            CBB_ref.IncreaseRelationshipWithPlayer = false;
            CBB_ref.DecreaseRelationshipWithPlayer = false;

            _dataSource.SocialExchange_E = CustomMissionNameMarkerVM.SEs_Enum.Undefined;
            _dataSource.customCharacterReftoCampaignBehaviorBase = null;
        }

        private static DictionaryEnumWithSEs ConvertCustomAgentSEToDictionaryEnum(CustomMissionNameMarkerVM.SEs_Enum se)
        {
            DictionaryEnumWithSEs key;
            switch (se)
            {
                case CustomMissionNameMarkerVM.SEs_Enum.Compliment:
                    key = DictionaryEnumWithSEs.Compliment;
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.GiveGift:
                    key = DictionaryEnumWithSEs.GiveGift;
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.Jealous:
                    key = DictionaryEnumWithSEs.Jealous;
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.FriendSabotage:
                    key = DictionaryEnumWithSEs.FriendSabotage;
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.Flirt:
                    key = DictionaryEnumWithSEs.Flirt;
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.Bully:
                    key = DictionaryEnumWithSEs.Bully;
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.RomanticSabotage:
                    key = DictionaryEnumWithSEs.RomanticSabotage;
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.AskOut:
                    key = DictionaryEnumWithSEs.AskOut;
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.Break:
                    key = DictionaryEnumWithSEs.Break;
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.Gratitude:
                    key = DictionaryEnumWithSEs.Gratitude;
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.HaveAChild:
                    key = DictionaryEnumWithSEs.HaveAChild;
                    break;
                default:
                    key = DictionaryEnumWithSEs.Undefined;
                    break;
            }

            return key;
        }

        private void LoadUserInfoFromFile()
        {
            string json = File.ReadAllText(filePath + fileName);

            UserInfoJson deserializedUserInfoClass = JsonConvert.DeserializeObject<UserInfoJson>(json);
            if (deserializedUserInfoClass != null)
            {
                PlayerOrNPC_Dictionary.TryGetValue(false, out Dictionary<Enum, int> result);
                result[DictionaryEnumWithSEs.Compliment] = deserializedUserInfoClass.NCompliment;
                result[DictionaryEnumWithSEs.GiveGift] = deserializedUserInfoClass.NGiveGift;
                result[DictionaryEnumWithSEs.Jealous] = deserializedUserInfoClass.NJealous;
                result[DictionaryEnumWithSEs.FriendSabotage] = deserializedUserInfoClass.NFriendSabotage;
                result[DictionaryEnumWithSEs.Flirt] = deserializedUserInfoClass.NFlirt;
                result[DictionaryEnumWithSEs.Bully] = deserializedUserInfoClass.NBully;
                result[DictionaryEnumWithSEs.RomanticSabotage] = deserializedUserInfoClass.NRomanticSabotage;
                result[DictionaryEnumWithSEs.Break] = deserializedUserInfoClass.NBreak;
                result[DictionaryEnumWithSEs.AskOut] = deserializedUserInfoClass.NAskOut;
                result[DictionaryEnumWithSEs.Gratitude] = deserializedUserInfoClass.NGratitude;
                result[DictionaryEnumWithSEs.HaveAChild] = deserializedUserInfoClass.NHaveAChild;
                PlayerOrNPC_Dictionary[false] = result;

                PlayerOrNPC_Dictionary.TryGetValue(true, out result);
                result[DictionaryEnumWithSEs.Compliment] = deserializedUserInfoClass.PCompliment;
                result[DictionaryEnumWithSEs.GiveGift] = deserializedUserInfoClass.PGiveGift;
                result[DictionaryEnumWithSEs.Jealous] = deserializedUserInfoClass.PJealous;
                result[DictionaryEnumWithSEs.FriendSabotage] = deserializedUserInfoClass.PFriendSabotage;
                result[DictionaryEnumWithSEs.Flirt] = deserializedUserInfoClass.PFlirt;
                result[DictionaryEnumWithSEs.Bully] = deserializedUserInfoClass.PBully;
                result[DictionaryEnumWithSEs.RomanticSabotage] = deserializedUserInfoClass.PRomanticSabotage;
                result[DictionaryEnumWithSEs.Break] = deserializedUserInfoClass.PBreak;
                result[DictionaryEnumWithSEs.AskOut] = deserializedUserInfoClass.PAskOut;
                result[DictionaryEnumWithSEs.Gratitude] = deserializedUserInfoClass.PGratitude;
                result[DictionaryEnumWithSEs.HaveAChild] = deserializedUserInfoClass.PHaveAChild;
                PlayerOrNPC_Dictionary[true] = result;

                TotalSEs = deserializedUserInfoClass.TotalSocialExchanges;
                NPCsInteractedWithPlayer = deserializedUserInfoClass.NPCInteractedWithPlayer;
                PlayerInteractedWithNPCs = deserializedUserInfoClass.PlayerInteractedWithNPC;
                NPCsInteractedWithNPCs = deserializedUserInfoClass.NPCsInteractedWithNPC;
                DaysPassed = deserializedUserInfoClass.DaysPassedInGame;
            }
        }

        private void UpdateUserInfo(DictionaryEnumWithSEs dictionaryKey, int WhoWasTheInitiator)
        {
            Dictionary<Enum, int> result;
            int value;

            switch (WhoWasTheInitiator)
            {
                case -1:
                    NPCsInteractedWithPlayer++;

                    PlayerOrNPC_Dictionary.TryGetValue(false, out result);
                    result.TryGetValue(dictionaryKey, out value);
                    result[dictionaryKey] = value + 1;
                    break;
                case 0:
                    NPCsInteractedWithNPCs++;

                    PlayerOrNPC_Dictionary.TryGetValue(false, out result);
                    result.TryGetValue(dictionaryKey, out value);
                    result[dictionaryKey] = value + 1;
                    break;
                case 1:
                    PlayerInteractedWithNPCs++;

                    PlayerOrNPC_Dictionary.TryGetValue(true, out result);
                    result.TryGetValue(dictionaryKey, out value);
                    result[dictionaryKey] = value + 1;
                    break;
                default:
                    break;
            }

            TotalSEs = NPCsInteractedWithNPCs + NPCsInteractedWithPlayer + PlayerInteractedWithNPCs;

            SaveUserInfoToFile();
        }

        private void SaveUserInfoToFile()
        {
            string json = File.ReadAllText(filePath + fileName);

            UserInfoJson deserializedUserInfoClass = JsonConvert.DeserializeObject<UserInfoJson>(json);
            if (deserializedUserInfoClass != null)
            {
                deserializedUserInfoClass.NPCInteractedWithPlayer = NPCsInteractedWithPlayer;
                deserializedUserInfoClass.PlayerInteractedWithNPC = PlayerInteractedWithNPCs;
                deserializedUserInfoClass.NPCsInteractedWithNPC = NPCsInteractedWithNPCs;
                deserializedUserInfoClass.TotalSocialExchanges = TotalSEs;
                deserializedUserInfoClass.DaysPassedInGame = DaysPassed;

                PlayerOrNPC_Dictionary.TryGetValue(false, out Dictionary<Enum, int> result);
                result.TryGetValue(DictionaryEnumWithSEs.Compliment, out int value);
                deserializedUserInfoClass.NCompliment = value;
                result.TryGetValue(DictionaryEnumWithSEs.GiveGift, out value);
                deserializedUserInfoClass.NGiveGift = value;
                result.TryGetValue(DictionaryEnumWithSEs.Jealous, out value);
                deserializedUserInfoClass.NJealous = value;
                result.TryGetValue(DictionaryEnumWithSEs.FriendSabotage, out value);
                deserializedUserInfoClass.NFriendSabotage = value;
                result.TryGetValue(DictionaryEnumWithSEs.Flirt, out value);
                deserializedUserInfoClass.NFlirt = value;
                result.TryGetValue(DictionaryEnumWithSEs.Bully, out value);
                deserializedUserInfoClass.NBully = value;
                result.TryGetValue(DictionaryEnumWithSEs.RomanticSabotage, out value);
                deserializedUserInfoClass.NRomanticSabotage = value;
                result.TryGetValue(DictionaryEnumWithSEs.Break, out value);
                deserializedUserInfoClass.NBreak = value;
                result.TryGetValue(DictionaryEnumWithSEs.AskOut, out value);
                deserializedUserInfoClass.NAskOut = value;
                result.TryGetValue(DictionaryEnumWithSEs.Gratitude, out value);
                deserializedUserInfoClass.NGratitude = value;
                result.TryGetValue(DictionaryEnumWithSEs.HaveAChild, out value);
                deserializedUserInfoClass.NHaveAChild = value;

                deserializedUserInfoClass.NFriendlySEs = deserializedUserInfoClass.NCompliment + deserializedUserInfoClass.NGiveGift + deserializedUserInfoClass.NGratitude;
                deserializedUserInfoClass.NUnFriendlySEs = deserializedUserInfoClass.NJealous + deserializedUserInfoClass.NFriendSabotage;
                deserializedUserInfoClass.NRomanticSEs = deserializedUserInfoClass.NFlirt;
                deserializedUserInfoClass.NHostileSEs = deserializedUserInfoClass.NBully + deserializedUserInfoClass.NRomanticSabotage;
                deserializedUserInfoClass.NSpecialSEs = deserializedUserInfoClass.NBreak + deserializedUserInfoClass.NAskOut + deserializedUserInfoClass.NHaveAChild;

                PlayerOrNPC_Dictionary.TryGetValue(true, out result);
                result.TryGetValue(DictionaryEnumWithSEs.Compliment, out value);
                deserializedUserInfoClass.PCompliment = value;
                result.TryGetValue(DictionaryEnumWithSEs.GiveGift, out value);
                deserializedUserInfoClass.PGiveGift = value;
                result.TryGetValue(DictionaryEnumWithSEs.Jealous, out value);
                deserializedUserInfoClass.PJealous = value;
                result.TryGetValue(DictionaryEnumWithSEs.FriendSabotage, out value);
                deserializedUserInfoClass.PFriendSabotage = value;
                result.TryGetValue(DictionaryEnumWithSEs.Flirt, out value);
                deserializedUserInfoClass.PFlirt = value;
                result.TryGetValue(DictionaryEnumWithSEs.Bully, out value);
                deserializedUserInfoClass.PBully = value;
                result.TryGetValue(DictionaryEnumWithSEs.RomanticSabotage, out value);
                deserializedUserInfoClass.PRomanticSabotage = value;
                result.TryGetValue(DictionaryEnumWithSEs.Break, out value);
                deserializedUserInfoClass.PBreak = value;
                result.TryGetValue(DictionaryEnumWithSEs.AskOut, out value);
                deserializedUserInfoClass.PAskOut = value;
                result.TryGetValue(DictionaryEnumWithSEs.Gratitude, out value);
                deserializedUserInfoClass.PGratitude = value;
                result.TryGetValue(DictionaryEnumWithSEs.HaveAChild, out value);
                deserializedUserInfoClass.PHaveAChild = value;

                deserializedUserInfoClass.PFriendlySEs = deserializedUserInfoClass.PCompliment + deserializedUserInfoClass.PGiveGift + deserializedUserInfoClass.PGratitude;
                deserializedUserInfoClass.PUnFriendlySEs = deserializedUserInfoClass.PJealous + deserializedUserInfoClass.PFriendSabotage;
                deserializedUserInfoClass.PRomanticSEs = deserializedUserInfoClass.PFlirt;
                deserializedUserInfoClass.PHostileSEs = deserializedUserInfoClass.PBully + deserializedUserInfoClass.PRomanticSabotage;
                deserializedUserInfoClass.PSpecialSEs = deserializedUserInfoClass.PBreak + deserializedUserInfoClass.PAskOut + deserializedUserInfoClass.PHaveAChild;
            }

            File.WriteAllText(filePath + fileName, JsonConvert.SerializeObject(deserializedUserInfoClass));
        }

        private void UploadFileToFTP()
        {
            string ftpServerIP = "ftp.davidricardo.x10host.com/";
            string ftpUserName = "user@davidricardo.x10host.com";
            string ftpPassword = "P2NVL60v";

            FileInfo objFile = new FileInfo(filePath + fileName);
            FtpWebRequest objFTPRequest;

            // Create FtpWebRequest object 
            objFTPRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + objFile.Name));

            // Set Credintials
            objFTPRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

            // By default KeepAlive is true, where the control connection is 
            // not closed after a command is executed.
            objFTPRequest.KeepAlive = false;

            // Set the data transfer type.
            objFTPRequest.UseBinary = true;

            // Set content length
            objFTPRequest.ContentLength = objFile.Length;

            // Set request method
            objFTPRequest.Method = WebRequestMethods.Ftp.UploadFile;

            // Set buffer size
            int intBufferLength = 16 * 1024;
            byte[] objBuffer = new byte[intBufferLength];

            // Opens a file to read
            FileStream objFileStream = objFile.OpenRead();

            try
            {
                // Get Stream of the file
                Stream objStream = objFTPRequest.GetRequestStream();

                int len = 0;

                while ((len = objFileStream.Read(objBuffer, 0, intBufferLength)) != 0)
                {
                    // Write file Content 
                    objStream.Write(objBuffer, 0, len);

                }

                objStream.Close();
                objFileStream.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CheckIfUserFileExists()
        {
            filePath = BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/";

            string[] files = Directory.GetFiles(filePath);
            if (files.Length != 0)
            {
                foreach (string file in files)
                {
                    if (file.Contains("user"))
                    {
                        fileName = file.Remove(0, filePath.Length);
                        break;
                    }
                }
            }

            if (fileName == "")
            {
                //Create a new file
                fileName = GetListFiles();

                if (fileName != "")
                {
                    System.IO.File.Create(filePath + fileName).Close();

                    string text = "{ }";
                    UserInfoJson myDeserializedClass = JsonConvert.DeserializeObject<UserInfoJson>(text);
                    File.WriteAllText(filePath + fileName, JsonConvert.SerializeObject(myDeserializedClass));

                }
            }
        }

        private string GetListFiles()
        {
            list = new List<string>();

            try
            {
                string ftpServerIP = "ftp.davidricardo.x10host.com/";
                string ftpUserName = "user@davidricardo.x10host.com";
                string ftpPassword = "P2NVL60v";

                FileInfo objFile = new FileInfo(filePath);
                FtpWebRequest request;

                // Create FtpWebRequest object 
                request = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + objFile.Name));
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string names = reader.ReadToEnd();

                reader.Close();
                response.Close();
                responseStream.Close();

                list = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach (string file in list)
                {
                    if (file.Contains("user"))
                    {
                        string number = file.Remove(0, 5);
                        number = number.Replace(".json", "");

                        int id = int.Parse(number) + 1;

                        string newName = "user_" + id.ToString() + ".json";
                        return newName;
                    }
                }

                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}

//private void GiveCourageToCharacter(CustomAgent customAgentConversation)
//{
//    CustomAgent customAgent = _dataSource.customAgentsList.Find(c => c.selfAgent.Name == customAgentConversation.selfAgent.Name && c.Id == customAgentConversation.Id);
//    customAgent.UpdateAllStatus(0, 0, 1, 0, 0, 0);
//}
//else if (CBB_ref.giveCourage)
//{
//    GiveCourageToCharacter(customAgentConversation);
//    CBB_ref.giveCourage = false;
//}