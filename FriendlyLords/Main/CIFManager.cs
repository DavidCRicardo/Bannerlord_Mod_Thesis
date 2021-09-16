using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using SandBox.ViewModelCollection;
using Newtonsoft.Json;

namespace FriendlyLords
{
    public class CIFManager : ViewModel
    {
        public CIFManager(Mission mission, Camera missionCamera)
        {
            this.Targets = new MBBindingList<CIFManagerTarget>();
            this._distanceComparer = new CIFManager.MarkerDistanceComparer();
            this._missionCamera = missionCamera;
            this._mission = mission;
        }

        private config configReference { get; set; }

        private List<CIF_SocialExchange> SocialExchangesList { get; set; }
        private List<mostWantedSE> mostWantedSEList { get; set; }
        private List<NextSE> nextSEList { get; set; }
        private NextSE nextSE { get; set; }

        public List<CIF_Character> customAgentsList { get; set; }
        private List<string> StatusList { get; set; }

        private string CurrentSettlement { get; set; }
        public string CurrentLocation { get; set; }
        private bool giveTraitsToNPCs { get; set; }
        private int OnGoingSEs { get; set; }
        private int MaximumSEs { get; set; }

        private SEs_Enum auxSE;
        private int auxVolition;

        public CIF_Character customAgentInteractingWithPlayer;
        public bool playerStartedASE;
        private int CIF_Range = 0;
        private Random rnd { get; set; }

        private int nextRequiredRenown { get; set; }
        public bool StopSEs { get; set; }
        
        public int ConversationDelay { get; set; }
        public float NPCCountdownMultiplier { get; set; }

        public void Tick(float dt)
        {
            MissionMode missionMode = CampaignMission.Current.Mode;
            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null && CampaignMission.Current.Location.StringId != "arena" && missionMode != MissionMode.Battle)
            {
                if (this._firstTick)
                {
                    configReference = ReadConfigFile();

                    rnd = new Random();
                    PreInitializeOnSettlement(configReference);

                    InitializeOnSettlement(giveTraitsToNPCs, NPCCountdownMultiplier);

                    this._firstTick = false;
                }

                if (CharacterObject.OneToOneConversationCharacter == null)
                {
                    CheckAdmirationMethod();

                    DecreaseNPCsCountdown(dt);

                    foreach (CIF_Character customAgent in customAgentsList)
                    {
                        if (customAgent == null)
                        {
                            continue;
                        }

                        if (CustomAgentInsideRangeFromPlayer(customAgent) || customAgent.Busy)
                        {
                            customAgent.NearPlayer = CustomAgentIsNearToPlayer(customAgent);

                            if (customAgent.Busy && customAgent.IsInitiator)
                            {
                                CustomAgentGoingToSE(dt, customAgent, CurrentLocation);
                            }

                            if (SecsDelay(dt, 1))
                            {
                                UpdateStatus(customAgent);
                            }
                        }
                    }
                }
                else
                {
                    CheckPlayerTalkingWithAgent();
                }

                UpdateTargetScreen();
            }
            else
            {
                if (missionMode == MissionMode.Battle && PlacesAvailableToSpeak())
                {
                    if (this._firstTick)
                    {
                        configReference = ReadConfigFile();
                        CIF_Range = configReference.RangeConversations;
                        
                        OtherTeamLimitSpeakers = configReference.SpeakersOnBattlePerTeamLimit;
                        PlayerTeamLimitSpeakers = configReference.SpeakersOnBattlePerTeamLimit;

                        PreInitializeOnBattle();

                        this._firstTick = false;
                    }

                    if (configReference.ConversationOnBattle)
                    {
                        if (Mission.Current.Teams.Count != 0 && Mission.Current.Teams[0].TeamAgents.Count != 0 && _secondTick)
                        {
                            InitializeOnBattle(rnd);

                            _secondTick = false;
                        }
                        else
                        {
                            if (SecsDelay(dt, 1))
                            {
                                OnBattle();
                            }

                            DecreaseCountdownOnBattle(dt);
                            UpdateTargetScreen();
                        }
                    }
                }
            }
        }

        private config ReadConfigFile()
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/FriendlyLords/config.json");
            config myDeserializedClass = JsonConvert.DeserializeObject<config>(json);

            return myDeserializedClass;
        }

        private void CheckAdmirationMethod()
        {
            if (nextRequiredRenown != -1 && (Hero.MainHero.Clan.Renown >= nextRequiredRenown || ReadyToGiveTriggerRule))
            {
                CurrentAgentsWhoAreRunningAI = customAgentsList.FindAll(c => c.RunAI && c.agentRef != Agent.Main);
                if (CurrentAgentsWhoAreRunningAI != null && CurrentAgentsWhoAreRunningAI.Count > 0)
                {
                    ReadyToGiveTriggerRule = true;

                    foreach (CIF_Character item in CurrentAgentsWhoAreRunningAI)
                    {
                        if (ReadyToGiveTriggerRule)
                        {
                            if (item.Busy)
                            {
                                continue;
                            }

                            item.AddToTriggerRulesList(new TriggerRule(SEs_Enum.Admiration.ToString(), Agent.Main.Name, 0));
                            ReadyToGiveTriggerRule = false;
                        }
                    }
                    nextRequiredRenown = Hero.MainHero.Clan.RenownRequirementForNextTier;
                }
                else
                {
                    ReadyToGiveTriggerRule = true;
                }
            }
        }
        private List<CIF_Character> CurrentAgentsWhoAreRunningAI { get; set; }
        private bool ReadyToGiveTriggerRule { get; set; }

        private void CheckPlayerTalkingWithAgent()
        {
            if (!playerStartedASE && customAgentInteractingWithPlayer == null)
            {
                customAgentInteractingWithPlayer = customAgentsList.Find(c => c.agentRef.Character == CharacterObject.OneToOneConversationCharacter && c.customAgentTarget != null);

                if (customAgentInteractingWithPlayer != null)
                {
                    if (customAgentInteractingWithPlayer.customAgentTarget == null)
                    {
                        playerStartedASE = true;
                        OnGoingSEs++;
                    }
                    else
                    {
                        if (customAgentInteractingWithPlayer.customAgentTarget.agentRef != Agent.Main)
                        {
                            playerStartedASE = true;
                            OnGoingSEs++;
                        }
                    }
                }
                else
                {
                    playerStartedASE = true;
                    OnGoingSEs++;
                }
            }
        }

        internal void ResetSocialExchangesAllNPCsOptions()
        {
            if (customAgentsList == null)
            {
                return;
            }
            else
            {
                foreach (CIF_Character customAgent in customAgentsList)
                {
                    if (customAgent.agentRef.IsHero && customAgent.agentRef != Agent.Main)
                    {
                        customAgent.ResetSocialExchangesOptions();
                    }
                }

                ResetSavedSEs();
            }
        }

        private void CustomAgentGoingToSE(float dt, CIF_Character customAgent, string _CurrentLocation)
        {
            customAgent.CustomAgentWithDesire(dt, ConversationDelay, rnd, DialogsDictionary, _CurrentLocation);
            if (customAgent.EndingSocialExchange)
            {
                OnGoingSEs--;
                customAgent.EndingSocialExchange = false;

                SaveAllInfoToJSON();
            }

            if (customAgent.customAgentTarget != null && customAgent.customAgentTarget.agentRef == Agent.Main)
            {
                customCharacterReftoCampaignBehaviorBase = customAgent;
                customCharacterIdRefCampaignBehaviorBase = customAgent.Id;

                SocialExchange_E = customAgent.SocialMove_SE;
            }

            if (customAgent.StartingASocialExchange)
            {
                SocialExchange_E = customAgent.SocialMove_SE;
                BooleanNumber = customAgent.booleanNumber;
                letsUpdate = true;
                customAgent.StartingASocialExchange = false;
            }
        }

        public bool letsUpdate;
        public int BooleanNumber;

        private void DecreaseNPCsCountdown(float dt)
        {
            if (OnGoingSEs >= MaximumSEs)
            {
                return;
            }

            foreach (CIF_Character customAgent in customAgentsList)
            {
                if (customAgent.RunAI)
                {
                    if (CustomAgentHasEnoughRest(customAgent))
                    {
                        if (customAgent.SecsDelay(dt, customAgent.Countdown))
                        {
                            customAgent.EnoughRest = true;
                            customAgent.Busy = false;
                        }
                        else if (customAgent.agentRef == Agent.Main)
                        {
                            customAgent.Busy = false;
                        }
                    }
                    else
                    {
                        if (customAgent.agentRef == Agent.Main)
                        {
                            customAgent.Busy = true;
                        }
                    }               
                }            
            }

            DesireFormation();
        }

        private void UpdateStatus(CIF_Character customAgent)
        {
            double socialTalk = 0;
            double bullyNeed = 0;
            double getTired = 0;

            if (customAgent.customAgentTarget != null) // if he has a target and it's going to it 
            {
                getTired = rnd.NextDouble();
            }
            else
            {
                getTired = -1 * rnd.NextDouble();
            }

            if (customAgent.TraitList.Exists(t => t.traitName == "Friendly"))
            {

                socialTalk += 0.1;
            }

            if (customAgent.TraitList.Exists(t => t.traitName == "UnFriendly"))
            {
                bullyNeed += rnd.NextDouble();
            }

            customAgent.UpdateAllStatus(socialTalk, bullyNeed, -0.1, -0.1, -0.1, getTired);
        }

        private void DesireFormation()
        {
            /* Set mostWantedSE & nextSE to default values */
            foreach (var k in mostWantedSEList)
            {
                k.nextSE.se = SEs_Enum.Undefined;
                k.nextSE.InitiatorAgent = null;
                k.nextSE.ReceiverAgent = null;
                k.nextSE.Volition = 0;
            }

            nextSE.se = SEs_Enum.Undefined;
            nextSE.Volition = 0;
            nextSE.InitiatorAgent = null;
            nextSE.ReceiverAgent = null;

            /* Each NPC will check the environment */
            foreach (CIF_Character c1 in customAgentsList)
            {
                if (c1.agentRef == Agent.Main || c1.Busy || !c1.EnoughRest || OnGoingSEs >= MaximumSEs || !c1.RunAI)
                {
                    continue;
                }

                auxVolition = 0;
                auxSE = SEs_Enum.Undefined;

                nextSEList.Clear();

                /*Calculate Volitions for the NPCs around*/
                foreach (CIF_Character c2 in customAgentsList)
                {
                    if (c1 == c2 || c2.Busy || !c2.RunAI) { continue; } // Player Included because it can be the target for some NPC 

                    /* For each Social Exchange */
                    foreach (CIF_SocialExchange se in SocialExchangesList)
                    {
                        se.CustomAgentInitiator = c1;
                        se.CustomAgentReceiver = c2;

                        int initiatorVolition = se.InitiadorVolition();
                        if (initiatorVolition == auxVolition)
                        {
                            nextSEList.Add(new NextSE(se.SE_Enum, c1, c2, initiatorVolition));
                        }

                        else if (initiatorVolition > auxVolition)
                        {
                            auxVolition = se.CustomAgentInitiator.SEVolition;
                            auxSE = se.SE_Enum;

                            nextSEList.Clear();
                            nextSEList.Add(new NextSE(auxSE, c1, c2, initiatorVolition));
                        }
                    }
                }
                
                mostWantedSE mostWanted = mostWantedSEList.Find(mostWantedSE => mostWantedSE.customAgent == c1);
                if (nextSEList.Count > 0)
                {
                    int index = rnd.Next(nextSEList.Count);
                    NextSE sE = nextSEList[index];

                    mostWanted.nextSE.se = sE.se;
                    mostWanted.nextSE.InitiatorAgent = sE.InitiatorAgent;
                    mostWanted.nextSE.ReceiverAgent = sE.ReceiverAgent;
                    mostWanted.nextSE.Volition = sE.Volition;
                }
            }

            /* Calculate Next SE */
            foreach (var k in mostWantedSEList)
            {
                if (k.nextSE.Volition > nextSE.Volition)
                {
                    nextSE.se = k.nextSE.se;
                    nextSE.InitiatorAgent = k.nextSE.InitiatorAgent;
                    nextSE.ReceiverAgent = k.nextSE.ReceiverAgent;
                    nextSE.Volition = k.nextSE.Volition;
                }
            }

            if (nextSE.Volition > 0)
            {
                /* Get NPC & Start SE */
                nextSE.InitiatorAgent.StartSE(nextSE.se, nextSE.ReceiverAgent);
                OnGoingSEs++;
            }
        }

        private void PreInitializeOnSettlement(config configReference)
        {
            CIF_Range = configReference.RangeConversations;
            ConversationDelay = configReference.ConversationDelay;
            NPCCountdownMultiplier = configReference.NPCCountdownMultiplier;

            CheckIfDataFileExists();
            CheckIfSavedSEsFileExists();

            CurrentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            CurrentLocation = CampaignMission.Current.Location.StringId;
            if (CheckIfSettlementExistsOnDataFile(CurrentSettlement, CurrentLocation))
            {
                giveTraitsToNPCs = false;
            }
            else { giveTraitsToNPCs = true; }
        }

        private void InitializeOnSettlement(bool giveTraitsToNPCs, float _NPCCountdownMultiplier = 1)
        {
            InitializeSocialExchanges();

            InitializeStatusList();

            if (Mission.Current.MainAgent != null)
            {
                if (customAgentsList == null)
                {
                    customAgentsList = new List<CIF_Character>();

                    nextSE = new NextSE(SEs_Enum.Undefined, null, null, 0);
                    mostWantedSEList = new List<mostWantedSE>();
                    nextSEList = new List<NextSE>();

                    foreach (Agent agent in Mission.Current.Agents)
                    {
                        if (agent != null && agent.IsHuman && agent.Character != null)
                        {
                            CreateCustomAgent(agent, true, null, _NPCCountdownMultiplier);
                        }
                    }

                    var companions = Clan.PlayerClan.Companions;                 
                    foreach (Hero hero in companions)
                    {
                        CIF_Character custom1 = customAgentsList.Find(c => c.agentRef.Character == hero.CharacterObject);
                        if (custom1 != null)
                        {
                            //custom1.CompanionFollowingPlayer = true;
                        }
                    }

                    if (giveTraitsToNPCs)
                    {
                        InitializeTraitsToAgents();
                        SaveNewAgentsInfoToJSON(customAgentsList);
                    }

                    LoadAllInfoFromJSON();
                    SaveAllInfoToJSON();

                    int increaseCountdown = IncreaseCountdownDependingOnHowManyNPCs();
                    InitializeCountdownToAgents(increaseCountdown); // Set CustomAgent countdown depending of traits

                    customAgentsList.ForEach(c => c.CustomAgentsList = customAgentsList);

                    InitializeEnergyToAgents();

                    LoadDialogsFromJSON();
                }
            }
        }

        private int IncreaseCountdownDependingOnHowManyNPCs()
        {
            int increaseCountdown;
            if (customAgentsList.Count < 21) { increaseCountdown = 5; }
            else if (customAgentsList.Count >= 21 && customAgentsList.Count <= 51) { increaseCountdown = 3; }
            else { increaseCountdown = 2; }

            return increaseCountdown;
        }

        private void InitializeCountdownToAgents(int _increaseCountdown)
        {
            foreach (CIF_Character customAgent in customAgentsList)
            {
                customAgent.Countdown += _increaseCountdown;
                customAgent.Countdown += customAgent.CheckCountdownWithCurrentTraits();
            }
        }

        private void InitializeSocialExchanges()
        {
            OnGoingSEs = 0;
            MaximumSEs = CurrentLocation == "center" ? 3 : 2;

            SocialExchangesList = new List<CIF_SocialExchange>();

            foreach (SEs_Enum SocialExchange_E in Enum.GetValues(typeof(SEs_Enum)))
            {
                if (SocialExchange_E != SEs_Enum.Undefined)
                {
                    SocialExchangesList.Add(new CIF_SocialExchange(SocialExchange_E, null, null));
                }
            }
        }

        private void InitializeStatusList()
        {
            StatusList = new List<string>() { "SocialTalk", "BullyNeed", "Courage", "Anger", "Shame", "Tiredness" };
        }

        private static bool CustomAgentHasEnoughRest(CIF_Character customAgent)
        {
            customAgent.EnoughRest = customAgent.StatusList.Find(_status => _status.Name == "Tiredness").intensity < 0.5;
            return customAgent.EnoughRest;
        }

        public void OnConversationEndWithPlayer(CIF_Character customAgent)
        {
            if (customAgent == null)
            {
                return;
            }

            OnGoingSEs--;
            playerStartedASE = false;
            customAgentInteractingWithPlayer = null;

            if (customAgent.customAgentTarget != null && customAgent.customAgentTarget.Name == Agent.Main.Name)
            {
                customCharacterReftoCampaignBehaviorBase = null;
                customCharacterIdRefCampaignBehaviorBase = -1;
                SetCanResetCBB_refVariables(true);

                customAgent.Busy = false;
                customAgent.IsInitiator = false;
                customAgent.EndingSocialExchange = false;
                customAgent.TalkingWithPlayer = false;
                customAgent.EnoughRest = false;
                customAgent.UpdateAllStatus(0, 0, 0, 0, 0, 1);

                customAgent.FinalizeSocialExchange();
            }

            customAgent.EndFollowBehavior();
            /*if (!customAgent.CompanionFollowingPlayer)
            {
                customAgent.EndFollowBehavior();
            }
            else { customAgent.StartFollowBehavior(customAgent.selfAgent, Agent.Main); }*/
        }

        private void InitializeEnergyToAgents()
        {
            foreach (CIF_Character customAgent in customAgentsList)
            {
                customAgent.UpdateAllStatus(0, 0, 0, 0, 0, rnd.Next(3));
            }
        }

        private void InitializeTraitsToAgents()
        {
            List<Trait> ListWithAllTraits = InitializeListWithAllTraits();

            double auxDouble = 0;

            foreach (var customAgent in customAgentsList)
            {
                if (customAgent.agentRef.IsHuman)
                {
                    for (int i = 0; i < ListWithAllTraits.Count; i++)
                    {
                        double tempDouble = rnd.NextDouble();
                        if (tempDouble > 0.25)
                        {
                            int traitToAddindex;
                            double tempDouble2 = rnd.NextDouble();
                            tempDouble2 += auxDouble;
                            if (tempDouble2 > 0.5)
                            {
                                traitToAddindex = i;
                                auxDouble -= 0.1;
                            }
                            else
                            {
                                auxDouble += 0.1;
                                traitToAddindex = i + 1;
                            }

                            customAgent.TraitList.Add(ListWithAllTraits[traitToAddindex]);
                        }

                        i++;
                    }
                }
            }
        }

        private List<Trait> InitializeListWithAllTraits()
        {
            List<string> TraitsListString = new List<string>()
            {
                "Friendly", "Hostile",
                "Charming", "UnCharming",
                "Shy", "Brave",
                "Calm", "Aggressive",
                "Faithful", "Unfaithful"
            };

            List<Trait> AllTraitList = new List<Trait>();

            foreach (string traitName in TraitsListString)
            {
                Trait newTrait = new Trait(traitName);
                AllTraitList.Add(newTrait);
            }

            return AllTraitList;
        }

        private void LoadDialogsFromJSON()
        {
            string json = ReadJsonDialogs("/npc_conversations.json");
            
            RootMessageJson myDeserializedClassConversations = JsonConvert.DeserializeObject<RootMessageJson>(json);

            Dictionary<string, List<string>> fromIDGetListMessages = new Dictionary<string, List<string>>();
            Dictionary<string, Dictionary<string, List<string>>> fromCultureCodeGetID = new Dictionary<string, Dictionary<string, List<string>>>();
            Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> fromSEGetCulture = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
            Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>> fromLocationGetSE = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>>();

            fromSEGetCulture = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();

            ReadAndGetDialogsFrom(myDeserializedClassConversations, ref fromIDGetListMessages, ref fromCultureCodeGetID, fromSEGetCulture, CurrentLocation);

            if (fromSEGetCulture.IsEmpty())
            {
                ReadAndGetDialogsFrom(myDeserializedClassConversations, ref fromIDGetListMessages, ref fromCultureCodeGetID, fromSEGetCulture, "tavern");
            }

            DialogsDictionary = fromSEGetCulture;
        }

        private void ReadAndGetDialogsFrom(RootMessageJson myDeserializedClassConversations, ref Dictionary<string, List<string>> fromIDGetListMessages, ref Dictionary<string, Dictionary<string, List<string>>> fromCultureCodeGetID, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> fromSEGetCulture, string _currentLocation)
        {
            foreach (DialogsRoot dialogsRoot in myDeserializedClassConversations.SocialExchangeListFromJson)
            {
                if (dialogsRoot.Location == _currentLocation)
                {
                    foreach (GlobalDialog globalDialog in dialogsRoot.GlobalDialogs)
                    {
                        fromCultureCodeGetID = new Dictionary<string, Dictionary<string, List<string>>>();
                        foreach (Culture culture in globalDialog.Culture)
                        {
                            fromIDGetListMessages = new Dictionary<string, List<string>>();
                            foreach (NPCDialog _npcDialog in culture.NPCDialogs)
                            {
                                if (_npcDialog.id == "start")
                                {
                                    fromIDGetListMessages.Add(_npcDialog.id, _npcDialog.messages);
                                }
                                else if (_npcDialog.id == "accept")
                                {
                                    fromIDGetListMessages.Add(_npcDialog.id, _npcDialog.messages);
                                }
                                else if (_npcDialog.id == "reject")
                                {
                                    fromIDGetListMessages.Add(_npcDialog.id, _npcDialog.messages);
                                }
                            }
                            fromCultureCodeGetID.Add(culture.CultureCode, fromIDGetListMessages);
                        }
                        fromSEGetCulture.Add(globalDialog.SocialExchange, fromCultureCodeGetID);
                    }
                    break;
                }
            }
        }

        private void CheckIfDataFileExists()
        {
            bool fileExists = File.Exists(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json");

            if (!fileExists)
            {
                FileStream file = File.Create(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json");
                file.Close();

                string text = "{ " + "SettlementJson" + ": [] }";
                RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(text);

                File.WriteAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json", JsonConvert.SerializeObject(myDeserializedClass));
            }
        }

        private void CreateCustomAgent(Agent agent, bool ToPerformSEs, Random rnd = null, float _NPCCountdownMultiplier = 1)
        {
            int id = 0;
            CIF_Character customAgentTemp = new CIF_Character(agent, id, StatusList, SEs_Enum.Undefined, _NPCCountdownMultiplier);

            foreach (CIF_Character customAgent in customAgentsList)
            {
                if (customAgent.Name == customAgentTemp.Name && customAgent.Id == customAgentTemp.Id)
                {
                    customAgentTemp.Id++;
                }
            }

            customAgentsList.Add(customAgentTemp);
            AddAgentTarget(agent, customAgentTemp.Id);

            if (ToPerformSEs)
            {
                if (customAgentTemp.agentRef.IsHero)
                {
                    LoadSavedSEs(customAgentTemp);
                }

                RandomItem(customAgentTemp);

                mostWantedSE sE = new mostWantedSE(customAgentTemp, new NextSE(SEs_Enum.Undefined, null, null, 0));
                mostWantedSEList.Add(sE);
            }
            else
            {
                customAgentTemp.Countdown = rnd.Next(1, 3);
            }
        }

        private void ResetSavedSEs()
        {
            string text = "{ " + "SEsPerformedList" + ": [] }";
            File.WriteAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/saved_SEs.json", text);
        }

        private static void LoadSavedSEs(CIF_Character customAgent)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/saved_SEs.json");
            SEsPerformedToday myDeserializedClass = JsonConvert.DeserializeObject<SEsPerformedToday>(json);

            if (myDeserializedClass != null)
            {
                if (myDeserializedClass.SEsPerformedList != null)
                {
                    List<SEsPerformed> SESavedList = myDeserializedClass.SEsPerformedList.FindAll(c => c.Hero_Name == customAgent.Name && c.Hero_ID == customAgent.Id);
                    if (SESavedList != null)
                    {
                        foreach (SEsPerformed item in SESavedList)
                        {
                            var key = CIF_Character.Intentions.Hostile;
                            switch (item.SocialExchange)
                            {            
                                case "Friendly":
                                case "Compliment":
                                    key = CIF_Character.Intentions.Friendly;
                                    break;
                                
                                case "UnFriendly":
                                case "Jealous":
                                    key = CIF_Character.Intentions.Unfriendly;
                                    break;
                                case "Romantic":
                                case "Flirt":
                                    key = CIF_Character.Intentions.Romantic;
                                    break;
                                case "Hostile":
                                case "Bully":
                                    key = CIF_Character.Intentions.Hostile;
                                    break;
                                case "AskOut":
                                case "Break":
                                case "HaveAChild":
                                    key = CIF_Character.Intentions.Special;
                                    break;
                                default:
                                    break;
                            }

                            customAgent.keyValuePairsSEs[key] = true;
                        }
                    }
                }
            }
        }

        public void SaveSavedSEs(CIF_Character customAgent, string socialExchange)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/saved_SEs.json");
            SEsPerformedToday myDeserializedClass = JsonConvert.DeserializeObject<SEsPerformedToday>(json);

            myDeserializedClass.SEsPerformedList.Add(new SEsPerformed(customAgent.Name, customAgent.Id, socialExchange));

            File.WriteAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/saved_SEs.json", JsonConvert.SerializeObject(myDeserializedClass));
        }

        private static void CheckIfSavedSEsFileExists()
        {
            bool fileExists = File.Exists(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/saved_SEs.json");
            
            if (!fileExists)
            {
                FileStream file = File.Create(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/saved_SEs.json");
                file.Close();

                string text = "{ " + "SEsPerformedList" + ": [] }";
                SEsPerformedToday myDeserializedClass = JsonConvert.DeserializeObject<SEsPerformedToday>(text);

                File.WriteAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/saved_SEs.json", JsonConvert.SerializeObject(myDeserializedClass));
            }
        }

        private void RandomItem(CIF_Character customAgent)
        {
            List<String> listItems = new List<string>() { "gem", "gift", "item" };

            double temp = rnd.NextDouble();
            if (temp < 0.15)
            {
                Random rnd = new Random();
                string item = listItems[rnd.Next(listItems.Count)];

                customAgent.AddItem(item, 1);
            }
        }

        private bool CheckIfSettlementExistsOnDataFile(string _currentSettlementName, string _currentLocationName)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            if (myDeserializedClass != null)
            {
                if (myDeserializedClass.SettlementJson != null)
                {
                    if (myDeserializedClass.SettlementJson.Any(s => s.Name == _currentSettlementName && s.LocationWithId == _currentLocationName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void GenerateRandomTraitsForThisNPC(CIF_Character customAgent)
        {
            List<Trait> ListWithAllTraits = InitializeListWithAllTraits();

            for (int i = 0; i < ListWithAllTraits.Count; i++)
            {
                double tempDouble = rnd.NextDouble();
                if (tempDouble > 0.5)
                {
                    int traitToAddindex;
                    double tempDouble2 = rnd.NextDouble();
                    if (tempDouble2 > 0.5)
                    {
                        traitToAddindex = i;
                    }
                    else
                    {
                        traitToAddindex = i + 1;
                    }

                    customAgent.TraitList.Add(ListWithAllTraits[traitToAddindex]);
                }

                i++;
            }
        }

        private void SaveNewAgentsInfoToJSON(List<CIF_Character> customAgentsList)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            List<CustomAgentJson> jsonlist = new List<CustomAgentJson>();
            foreach (CIF_Character customAgent in customAgentsList)
            {
                CustomAgentJson json1 = new CustomAgentJson(customAgent.Name, customAgent.Id, customAgent.TraitList, customAgent.ItemList);
                jsonlist.Add(json1);
            }

            myDeserializedClass.SettlementJson.Add(new SettlementJson(CurrentSettlement, CurrentLocation, jsonlist));

            File.WriteAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json", JsonConvert.SerializeObject(myDeserializedClass));
        }

        private void LoadAllInfoFromJSON()
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            try
            {
                nextRequiredRenown = Hero.MainHero.Clan.RenownRequirementForNextTier;
            }
            catch (Exception e)
            {
                //InformationManager.DisplayMessage(new InformationMessage(e.ToString()));
                nextRequiredRenown = -1;
            }
            
            foreach (SettlementJson _settlement in myDeserializedClass.SettlementJson)
            {
                if (_settlement.Name == CurrentSettlement && _settlement.LocationWithId == CurrentLocation)
                {
                    CIF_Character customMain = customAgentsList.Find(c => c.agentRef == Agent.Main);

                    foreach (CIF_Character customAgent in customAgentsList)
                    {
                        CustomAgentJson _customAgentJson = _settlement.CustomAgentJsonList.Find(c => c.Name == customAgent.Name && c.Id == customAgent.Id);
                        if (_customAgentJson != null)
                        {
                            customAgent.TraitList = _customAgentJson.TraitList;
                            customAgent.SocialNetworkBeliefs = _customAgentJson.SocialNetworkBeliefs;
                            customAgent.ItemList = _customAgentJson.ItemsList;
                            customAgent.MemorySEs = _customAgentJson.SocialExchangeMemory;

                            // Set Custom Agent Belief with Relation Value
                            CheckInGameRelationBetweenHeroes(customMain, customAgent);
                        }
                        else
                        {
                            //Checking if there is any NPC new on the town who hasn't on the 1st time when the file was generated
                            GenerateRandomTraitsForThisNPC(customAgent);
                            RandomItem(customAgent);

                            CheckInGameRelationBetweenHeroes(customMain, customAgent);
                            _settlement.CustomAgentJsonList.Add(new CustomAgentJson(customAgent.Name, customAgent.Id, customAgent.TraitList, customAgent.ItemList, customAgent.SocialNetworkBeliefs));

                            File.WriteAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json", JsonConvert.SerializeObject(myDeserializedClass));
                        }
                        File.WriteAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json", JsonConvert.SerializeObject(myDeserializedClass));

                        foreach (Trait trait in customAgent.TraitList)
                        {
                            trait.SetCountdownToIncreaseDecrease(trait.traitName);
                        }
                    }
                    break;
                }
            }
        }

        private static void CheckInGameRelationBetweenHeroes(CIF_Character customMain, CIF_Character customAgent)
        {
            int indexHero = Hero.AllAliveHeroes.FindIndex(h => h.CharacterObject == customAgent.agentRef.Character);
            if (indexHero >= 0)
            {
                Hero hero = Hero.AllAliveHeroes[indexHero];
                if (hero != null && hero != Hero.MainHero)
                {
                    SocialNetworkBelief belief = customAgent.SelfGetBeliefWithAgent(customMain);
                    belief = IfBeliefIsNullCreateANewOne(customMain, customAgent, belief);

                    float RelationWithPlayer = hero.GetRelationWithPlayer();

                    customAgent.SetBeliefWithNewValue(belief, RelationWithPlayer);
                    customMain.SetBeliefWithNewValue(belief, RelationWithPlayer);
                }
            }
        }

        private static SocialNetworkBelief IfBeliefIsNullCreateANewOne(CIF_Character customMain, CIF_Character customAgent, SocialNetworkBelief belief)
        {
            if (belief == null)
            {
                List<string> agents = new List<string>() { customAgent.Name, customMain.Name };
                List<int> ids = new List<int>() { customAgent.Id, customMain.Id };

                belief = new SocialNetworkBelief("Friends", agents, ids, 0);
            }

            return belief;
        }

        private void SaveAllInfoToJSON()
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            try
            {
                myDeserializedClass.requiredRenown = Hero.MainHero.Clan.RenownRequirementForNextTier;
            }
            catch (Exception e)
            {
                //InformationManager.DisplayMessage(new InformationMessage(e.ToString()));
                myDeserializedClass.requiredRenown = -1;
            }
            
            foreach (SettlementJson item in myDeserializedClass.SettlementJson)
            {
                if (item.Name == CurrentSettlement && item.LocationWithId == CurrentLocation)
                {
                    foreach (CustomAgentJson _customAgentJson in item.CustomAgentJsonList)
                    {
                        CIF_Character x = customAgentsList.Find(c => c.Name == _customAgentJson.Name && c.Id == _customAgentJson.Id);
                        if (x != null)
                        {
                            _customAgentJson.TraitList = x.TraitList;
                            _customAgentJson.SocialNetworkBeliefs = x.SocialNetworkBeliefs;
                            _customAgentJson.ItemsList = x.ItemList;
                            _customAgentJson.SocialExchangeMemory = x.MemorySEs;
                        }
                    }
                    break;
                }
            }

            File.WriteAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json", JsonConvert.SerializeObject(myDeserializedClass));
        }

        public void SaveToJson()
        {
            SaveAllInfoToJSON();
        }

        public bool SecsDelay(float dt, int seconds)
        {
            dtControl = dtControl + dt;
            if (dtControl >= seconds)
            {
                dtControl = 0;
                return true;
            }
            return false;
        }
        private float dtControl;

        public void SetCanResetCBB_refVariables(bool value)
        {
            resetVariables = value;
        }
        public bool GetCanResetCBB_refVariables()
        {
            return resetVariables;
        }
        private static bool resetVariables { get; set; }

        public CIF_Character customCharacterReftoCampaignBehaviorBase { get; set; }
        public int customCharacterIdRefCampaignBehaviorBase { get; set; }

        public SEs_Enum SocialExchange_E { get; set; }

        public enum SEs_Enum
        {
            Undefined = -1, Compliment, GiveGift, Admiration, Jealous, FriendSabotage, Flirt, Bully, RomanticSabotage, AskOut, Break, HaveAChild
        }

        #region On Battle
        private static bool PlacesAvailableToSpeak()
        {
            if (CampaignMission.Current.Location == null)
            {
                if (Hero.MainHero.CurrentSettlement == null)
                {
                    return true; // battle open world
                }
                else
                {
                    if (Hero.MainHero.CurrentSettlement.IsHideout())
                    {
                        return false; // hideout
                    }
                    else if (Hero.MainHero.CurrentSettlement.IsVillage)
                    {
                        return true; // raid village
                    }
                    else
                    {
                        return false; // arena - tournament
                    }
                }
            }
            else
            {
                if (CampaignMission.Current.Location.StringId != "arena")
                {
                    return false; // arena - practice fight
                }
                else { return false; }
            }
        }

        private void DecreaseCountdownOnBattle(float dt)
        {
            foreach (CIF_Character customAgent in customAgentsList)
            {
                if (customAgent.Message != "" && customAgent.SecsDelay(dt, customAgent.Countdown))
                {
                    customAgent.Message = "";
                    if (customAgent.IsPlayerTeam)
                    {
                        PlayerTeamCurrentSpeakers--;
                    }
                    else
                    {
                        OtherTeamCurrentSpeakers--;
                    }
                }
            }
        }

        private void OnBattle()
        {
            int auxCountPlayerTeam = 0;

            CheckDeadAgentsFromBothTeams(ref auxCountPlayerTeam);

            if (customAgentsList.Count == 0)
            {
                return;
            }

            int index = rnd.Next(customAgentsList.Count);
            CIF_Character customAgent = customAgentsList[index];

            if (!CustomAgentInsideRangeFromPlayer(customAgent))
            {
                return;
            }

            CheckIfAgentIsAvailableToSpeak(customAgent);
        }

        private void CheckIfAgentIsAvailableToSpeak(CIF_Character customAgent)
        {
            if (customAgent.IsDead)
            {
                customAgent.Message = "";
            }
            else
            {
                if (customAgent.IsPlayerTeam)
                {
                    customAgent.MarkerTypeRef = 1;
                    PlayerTeamCurrentSpeakers++;
                }
                else
                {
                    customAgent.MarkerTypeRef = 2;
                    OtherTeamCurrentSpeakers++;
                }

                GetBattleSentences(customAgent, customAgent.IsPlayerTeam, rnd);
            }
        }

        private void CheckDeadAgentsFromBothTeams(ref int auxCountOpponentTeam)
        {
            bool auxBool;
            foreach (Team team in Mission.Current.Teams)
            {
                if (!team.IsPlayerTeam)
                {
                    auxCountOpponentTeam = team.TeamAgents.Count;
                    auxBool = false;
                }
                else { auxBool = true; }
                
                CheckDeadAgentForThisTeam(team, auxBool, auxCountOpponentTeam);
            }
        }

        private void CheckDeadAgentForThisTeam(Team team, bool IsPlayerTeam, int auxInt = 0)
        {
            CIF_Character customAgentHelper;
            int index;
            for (int i = 0; i < team.TeamAgents.Count; i++)
            {
                Agent agent = team.TeamAgents[i];

                if (!agent.IsActive() || agent.Health <= 15)
                {
                    if (IsPlayerTeam)
                    {
                        index = auxInt + i;
                    }
                    else 
                    { 
                        index = i; 
                    }

                    if (customAgentsList.Count > index)
                    {
                        customAgentsList[index].IsDead = true;
                        customAgentHelper = customAgentsList[index];
                        NormalizeSpeakers(customAgentHelper);
                    }
                }
            }
        }

        private void NormalizeSpeakers(CIF_Character customAgentHelper)
        {
            if (customAgentHelper.Message != "")
            {
                if (customAgentHelper.IsPlayerTeam)
                {
                    PlayerTeamCurrentSpeakers--;
                }
                else
                {
                    OtherTeamCurrentSpeakers--;
                }

                customAgentHelper.Message = "";
            }
        }

        private void GetBattleSentences(CIF_Character customAgent, bool isPlayerTeam, Random rnd)
        {
            foreach (Team team in Mission.Current.Teams)
            {
                if (team.IsPlayerTeam)
                {
                    teamPlayerPower = team.QuerySystem.TeamPower;
                    if (initialTeamPlayerPower == -1)
                    {
                        initialTeamPlayerPower = teamPlayerPower;
                    }
                }
                else
                {
                    teamOpponentPower = team.QuerySystem.TeamPower;
                    if (initialTeamOpponentPower == -1)
                    {
                        initialTeamOpponentPower = teamOpponentPower;
                    }
                } 
            }

            if (teamPlayerPower == initialTeamPlayerPower && teamOpponentPower == initialTeamOpponentPower)
            {
                GetBattleSingleSentence(customAgent, BattleDictionary.Neutral, rnd);
            }
            else
            {
                if (teamPlayerPower < teamOpponentPower)
                {
                    if (isPlayerTeam)
                    {
                        GetBattleSingleSentence(customAgent, BattleDictionary.Losing, rnd);
                    }
                    else
                    {
                        GetBattleSingleSentence(customAgent, BattleDictionary.Winning, rnd);
                    }
                }
                else if (teamPlayerPower > teamOpponentPower)
                {
                    if (isPlayerTeam)
                    {
                        GetBattleSingleSentence(customAgent, BattleDictionary.Winning, rnd);
                    }
                    else
                    {
                        if (teamOpponentPower == 0)
                        {
                            customAgent.Message = "";
                        }
                        else
                        {
                            GetBattleSingleSentence(customAgent, BattleDictionary.Losing, rnd);
                        }
                    }
                }
                else
                {
                    GetBattleSingleSentence(customAgent, BattleDictionary.Neutral, rnd);
                }
            }        
        }

        private void GetBattleSingleSentence(CIF_Character customAgent, BattleDictionary key, Random rnd)
        {
            battleDictionarySentences.TryGetValue(key, out List<string> BattleSentences);

            int index = rnd.Next(BattleSentences.Count);
            customAgent.Message = BattleSentences[index];
        }

        private void InitializeOnBattle(Random rnd)
        {
            CheckIfSavedSEsFileExists();

            int index = 0;
            foreach (Team team in Mission.Current.Teams)
            {
                foreach (Agent agent in team.TeamAgents)
                {
                    if (agent.IsHuman && agent.Character != null)
                    {
                        CreateCustomAgent(agent, false, rnd);

                        customAgentsList[index].IsPlayerTeam = team.IsPlayerTeam;
                        index++;
                    }
                }
            }
        }

        private void PreInitializeOnBattle()
        {
            OtherTeamCurrentSpeakers = 0;
            PlayerTeamCurrentSpeakers = 0;
            initialTeamPlayerPower = -1;
            initialTeamOpponentPower = -1;
            rnd = new Random();
            customAgentsList = new List<CIF_Character>();
            battleDictionarySentences = new Dictionary<BattleDictionary, List<string>>();

            string json = ReadJsonDialogs("/battle_conversations.json");

            BattleConversationsJson deserializedBattleClass = JsonConvert.DeserializeObject<BattleConversationsJson>(json);
            if (deserializedBattleClass != null)
            {
                battleDictionarySentences.Add(BattleDictionary.Winning, deserializedBattleClass.Winning);
                battleDictionarySentences.Add(BattleDictionary.Neutral, deserializedBattleClass.Neutral);
                battleDictionarySentences.Add(BattleDictionary.Losing, deserializedBattleClass.Losing);
            }
        }

        private static string ReadJsonDialogs(string file)
        {
            string json;
            switch (BannerlordConfig.Language)
            {
                case "English":
                default:
                    //InformationManager.DisplayMessage(new InformationMessage(BannerlordConfig.Language));
                    json = File.ReadAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Localization/en" + file);
                    break;
            }

            return json;
        }

        private Dictionary<BattleDictionary, List<string>> battleDictionarySentences { get; set; }
        private enum BattleDictionary { Losing, Winning, Neutral }
        private int OtherTeamLimitSpeakers { get; set; }
        private int PlayerTeamLimitSpeakers { get; set; }
        private int OtherTeamCurrentSpeakers { get; set; }
        private int PlayerTeamCurrentSpeakers { get; set; }
        private float initialTeamPlayerPower { get; set; }
        private float initialTeamOpponentPower { get; set; }
        private float teamPlayerPower { get; set; }
        private float teamOpponentPower { get; set; }

        public Dictionary<Enum, CIF_Character.Intentions> dictionaryWithSEsToGauntlet = new Dictionary<Enum, CIF_Character.Intentions>()
        {
            { CIF_SocialExchange.IntentionEnum.Undefined, CIF_Character.Intentions.Undefined  },
            { CIF_SocialExchange.IntentionEnum.Positive , CIF_Character.Intentions.Friendly   },
            { CIF_SocialExchange.IntentionEnum.Negative , CIF_Character.Intentions.Unfriendly },
            { CIF_SocialExchange.IntentionEnum.Romantic , CIF_Character.Intentions.Romantic   },
            { CIF_SocialExchange.IntentionEnum.Hostile  , CIF_Character.Intentions.Hostile    },
            { CIF_SocialExchange.IntentionEnum.Special  , CIF_Character.Intentions.Special    }
        };

        #endregion

        private bool CustomAgentIsNearToPlayer(CIF_Character customAgent)
        {
            if (customAgent == null || customAgent.agentRef == null)
            {
                return false;
            }
            else
            {
                return Agent.Main.Position.Distance(customAgent.agentRef.Position) < 3 && Agent.Main.CanInteractWithAgent(customAgent.agentRef, 0);
            }
        }

        private bool CustomAgentInsideRangeFromPlayer(CIF_Character customAgent)
        {
            if (Agent.Main != null)
            {
                if (Agent.Main.Position.Distance(customAgent.agentRef.Position) <= CIF_Range)
                {
                    customAgent.RunAI = true;
                    return true;
                }
                else
                {
                    customAgent.RunAI = false;
                    return false;
                }
            }

            return false;
        }

        private void AddAgentTarget(Agent agent, int id)
        {
            if (agent != Agent.Main && agent.Character != null && agent.IsActive() && !this.Targets.Any((CIFManagerTarget t) => t.TargetAgent == agent))
            {
                if (agent.IsHuman)
                {
                    CIFManagerTarget item = new CIFManagerTarget(agent, id);
                    this.Targets.Add(item);
                    return;
                }
            }
        }

        private void UpdateTargetScreen()
        {
            if (this.IsEnabled)
            {
                this.UpdateTargetScreenPositions();
            }
        }

        private void UpdateTargetScreenPositions()
        {
            foreach (CIFManagerTarget missionNameMarkerTargetVM in this.Targets)
            {
                float a = -100f;
                float b = -100f;
                float num = 0f;
                MBWindowManager.WorldToScreen(this._missionCamera, missionNameMarkerTargetVM.WorldPosition + this._heightOffset, ref a, ref b, ref num);
                if (num > 0f)
                {
                    missionNameMarkerTargetVM.ScreenPosition = new Vec2(a, b);
                    missionNameMarkerTargetVM.Distance = (int)(missionNameMarkerTargetVM.WorldPosition - this._missionCamera.Position).Length;
                }
                else
                {
                    missionNameMarkerTargetVM.Distance = -1;
                    missionNameMarkerTargetVM.ScreenPosition = new Vec2(-100f, -100f);
                }
            }
            this.Targets.Sort(this._distanceComparer);
        }

        private void UpdateTargetStates(bool state)
        {
            foreach (CIFManagerTarget missionNameMarkerTargetVM in this.Targets)
            {
                missionNameMarkerTargetVM.IsEnabled = state;
            }
        }

        public void EnableDataSource()
        {
            foreach (CIFManagerTarget item in this.Targets)
            {
                CIF_Character customAgent = customAgentsList.Find(c => c.Name == item.Name && c.Id == item.Id);

                if (customAgent != null)
                {
                    if (customAgent.Message != "")
                    {
                        item.MarkerType = customAgent.MarkerTypeRef;
                        item.Message = customAgent.Message;
                        item.IsEnabled = true;
                    }
                    else
                    {
                        item.IsEnabled = false;
                    }
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CIFManagerTarget> Targets
        {
            get { return this._targets; }
            set
            {
                if (value != this._targets)
                {
                    this._targets = value;
                    base.OnPropertyChangedWithValue(value, "Targets");
                }
            }
        }

        [DataSourceProperty]
        public bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }
            set
            {
                if (value != this._isEnabled)
                {
                    this._isEnabled = value;
                    base.OnPropertyChangedWithValue(value, "IsEnabled");
                    this.UpdateTargetStates(value);
                    Game.Current.EventManager.TriggerEvent<MissionNameMarkerToggleEvent>(new MissionNameMarkerToggleEvent(value));
                }
            }
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> DialogsDictionary { get; private set; }

        private readonly Camera _missionCamera;
        private bool _firstTick = true;
        private bool _secondTick = true;
        private readonly Mission _mission;
        private Vec3 _heightOffset = new Vec3(0f, 0f, 2f, -1f);
        private readonly CIFManager.MarkerDistanceComparer _distanceComparer;
        private MBBindingList<CIFManagerTarget> _targets;
        private bool _isEnabled;

        private class MarkerDistanceComparer : IComparer<CIFManagerTarget>
        {
            public int Compare(CIFManagerTarget x, CIFManagerTarget y)
            {
                return y.Distance.CompareTo(x.Distance);
            }
        }
    }
}