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

namespace Bannerlord_Social_AI
{
    public class CustomMissionNameMarkerVM : ViewModel
    {
        public CustomMissionNameMarkerVM(Mission mission, Camera missionCamera)
        {
            this.Targets = new MBBindingList<CustomMissionNameMarkerTargetVM>();
            this._distanceComparer = new CustomMissionNameMarkerVM.MarkerDistanceComparer();
            this._missionCamera = missionCamera;
            this._mission = mission;
        }

        private List<SocialExchangeSE> SocialExchangesList { get; set; }
        private List<mostWantedSE> mostWantedSEList { get; set; }
        private List<NextSE> nextSEList { get; set; }
        private NextSE nextSE { get; set; }

        public List<CustomAgent> customAgentsList { get; set; }
        private SocialExchangeSE socialExchangeSE { get; set; }
        private List<string> StatusList { get; set; }

        private string CurrentSettlement { get; set; }
        public string CurrentLocation { get; set; }
        private bool giveTraitsToNPCs { get; set; }
        private int OnGoingSEs { get; set; }
        private int MaximumSEs { get; set; }

        private string auxSEName;
        private SEs_Enum auxSE;
        private CustomAgent auxInitiatorAgent;
        private CustomAgent auxReceiverAgent;
        private int auxVolition;

        public CustomAgent customAgentInteractingWithPlayer;
        public bool playerStartedASE;
        private int CIF_Range = 0;
        private Random rnd { get; set; }

        public void Tick(float dt)
        {
            MissionMode missionMode = CampaignMission.Current.Mode;
            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null && CampaignMission.Current.Location.StringId != "arena" && missionMode != MissionMode.Battle)
            {
                if (this._firstTick)
                {
                    rnd = new Random();

                    PreInitializeOnSettlement();

                    InitializeOnSettlement(giveTraitsToNPCs);

                    this._firstTick = false;

                    //Hero.MainHero.Clan.ResetClanRenown();
                    //Hero.MainHero.Clan.Renown;
                    //Hero.MainHero.Clan.AddRenown(100);

                    //CIF_Range = 5; // para testar o loop nas conversas
                    int renownRequirement = Hero.MainHero.Clan.RenownRequirementForNextTier;
                    int tier = Hero.MainHero.Clan.Tier;
                    // quando atingir nova tier, dar trigger rule "gratitude" a um NPC
                }

                if (CharacterObject.OneToOneConversationCharacter == null)
                {
                    DecreaseNPCsCountdown(dt);

                    foreach (CustomAgent customAgent in customAgentsList)
                    {
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
                        PreInitializeOnBattle();

                        this._firstTick = false;
                    }

                    if (Mission.Current.Teams.Count != 0 && Mission.Current.Teams[0].TeamAgents.Count != 0 && _secondTick)
                    {
                        InitializeOnBattle(rnd);

                        _secondTick = false;
                    }
                    else
                    {
                        OnBattle();

                        DecreaseCountdownOnBattle(dt);

                        UpdateTargetScreen();
                    }
                }
            }
        }

        private void CheckPlayerTalkingWithAgent()
        {
            if (!playerStartedASE && customAgentInteractingWithPlayer == null)
            {
                customAgentInteractingWithPlayer = customAgentsList.Find(c => c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter && c.customAgentTarget != null);

                if (customAgentInteractingWithPlayer != null)
                {
                    if (customAgentInteractingWithPlayer.customAgentTarget == null)
                    {
                        playerStartedASE = true;
                        OnGoingSEs++;
                    }
                    else
                    {
                        if (customAgentInteractingWithPlayer.customAgentTarget.selfAgent != Agent.Main)
                        {
                            playerStartedASE = true;
                            OnGoingSEs++;
                        }
                    }
                }
                else
                {
                    customAgentInteractingWithPlayer = customAgentsList.Find(c => c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);
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
                foreach (CustomAgent customAgent in customAgentsList)
                {
                    if (customAgent.selfAgent.IsHero && customAgent.selfAgent != Agent.Main)
                    {
                        customAgent.ResetSocialExchangesOptions();
                    }
                }

                ResetSavedSEs();

            }
        }

        private void CustomAgentGoingToSE(float dt, CustomAgent customAgent, string _CurrentLocation)
        {
            customAgent.CustomAgentWithDesire(dt, rnd, DialogsDictionary, _CurrentLocation);
            if (customAgent.EndingSocialExchange)
            {
                OnGoingSEs--;
                customAgent.EndingSocialExchange = false;

                SaveAllInfoToJSON();
            }

            if (customAgent.customAgentTarget != null && customAgent.customAgentTarget.selfAgent == Agent.Main)
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
        public SEs_Enum SE_identifier;

        private void DecreaseNPCsCountdown(float dt)
        {
            if (OnGoingSEs >= MaximumSEs)
            {
                return;
            }

            foreach (CustomAgent customAgent in customAgentsList)
            {
                if (customAgent.RunAI && CustomAgentHasEnoughRest(customAgent) && !customAgent.EnoughRest)
                {
                    if (customAgent.SecsDelay(dt, customAgent.Countdown))
                    {
                        customAgent.EnoughRest = true;
                    }
                }
            }

            DesireFormation();
        }

        private void UpdateStatus(CustomAgent customAgent)
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
                k.nextSE.SEName = "";
                k.nextSE.se = SEs_Enum.Undefined;
                k.nextSE.InitiatorAgent = null;
                k.nextSE.ReceiverAgent = null;
                k.nextSE.Volition = 0;
            }

            nextSE.SEName = "";
            nextSE.se = SEs_Enum.Undefined;
            nextSE.Volition = 0;
            nextSE.InitiatorAgent = null;
            nextSE.ReceiverAgent = null;

            /* Each NPC will check the environment */
            foreach (CustomAgent c1 in customAgentsList)
            {
                if (c1.selfAgent == Agent.Main || c1.Busy || !c1.EnoughRest || OnGoingSEs >= MaximumSEs || !c1.RunAI)
                {
                    continue;
                }

                auxVolition = 0;
                auxSEName = "";
                auxSE = SEs_Enum.Undefined;
                auxInitiatorAgent = null;
                auxReceiverAgent = null;

                nextSEList.Clear();

                /*Calculate Volitions for the NPCs around*/
                foreach (CustomAgent c2 in customAgentsList)
                {
                    if (c1 == c2 || c2.Busy || !c2.RunAI) { continue; } // Player Included because it can be the target for some NPC 

                    /* For each Social Exchange */
                    foreach (SocialExchangeSE se in SocialExchangesList)
                    {
                        se.CustomAgentInitiator = c1;
                        se.CustomAgentReceiver = c2;

                        int initiatorVolition = se.InitiadorVolition();
                        if (initiatorVolition == auxVolition)
                        {
                            nextSEList.Add(new NextSE(se.SEName, se.SE_Enum, c1, c2, initiatorVolition));
                        }

                        else if (initiatorVolition > auxVolition)
                        {
                            auxVolition = se.CustomAgentInitiator.SEVolition;
                            auxSEName = se.SEName;
                            auxSE = se.SE_Enum;
                            auxInitiatorAgent = se.CustomAgentInitiator;
                            auxReceiverAgent = se.CustomAgentReceiver;

                            nextSEList.Clear();
                            nextSEList.Add(new NextSE(auxSEName, auxSE, c1, c2, initiatorVolition));
                        }
                    }
                }

                mostWantedSE mostWanted = mostWantedSEList.Find(mostWantedSE => mostWantedSE.customAgent == c1);
                if (nextSEList.Count > 0)
                {
                    int index = rnd.Next(nextSEList.Count);
                    NextSE sE = nextSEList[index];

                    mostWanted.nextSE.SEName = sE.SEName;
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
                    nextSE.SEName = k.nextSE.SEName;
                    nextSE.se = k.nextSE.se;
                    nextSE.InitiatorAgent = k.nextSE.InitiatorAgent;
                    nextSE.ReceiverAgent = k.nextSE.ReceiverAgent;
                    nextSE.Volition = k.nextSE.Volition;
                }
            }

            if (nextSE.Volition > 0)
            {
                /* Get NPC & Start SE */
                nextSE.InitiatorAgent.StartSE(nextSE.SEName, nextSE.se, nextSE.ReceiverAgent);
                OnGoingSEs++;
            }
        }

        private void PreInitializeOnSettlement()
        {
            CheckIfFileExists();
            CheckIfSavedSEsFileExists();

            CurrentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            CurrentLocation = CampaignMission.Current.Location.StringId;
            if (CheckIfSettlementExistsOnFile(CurrentSettlement, CurrentLocation))
            {
                giveTraitsToNPCs = false;
            }
            else { giveTraitsToNPCs = true; }
        }

        private void InitializeOnSettlement(bool giveTraitsToNPCs)
        {
            InitializeSocialExchanges();

            InitializeStatusList();

            ReadConfigFileToSetCIFRange();

            if (Mission.Current.MainAgent != null)
            {
                if (customAgentsList == null)
                {
                    customAgentsList = new List<CustomAgent>();

                    nextSE = new NextSE("", SEs_Enum.Undefined, null, null, 0);
                    mostWantedSEList = new List<mostWantedSE>();
                    nextSEList = new List<NextSE>();

                    foreach (Agent agent in Mission.Current.Agents)
                    {
                        if (agent.IsHuman && agent.Character != null)
                        {
                            CreateCustomAgent(agent, true);
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

        private void ReadConfigFileToSetCIFRange()
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/player_conversations.json");
            CBB_Root deserializedBattleClass = JsonConvert.DeserializeObject<CBB_Root>(json);
            if (deserializedBattleClass != null)
            {
                CIF_Range = deserializedBattleClass.RangeCIFConversations;
            }
        }

        private int IncreaseCountdownDependingOnHowManyNPCs()
        {
            int increaseCountdown = 0;
            if (customAgentsList.Count < 21) { increaseCountdown = 5; }
            else if (customAgentsList.Count >= 21 && customAgentsList.Count <= 51) { increaseCountdown = 3; }
            else { increaseCountdown = 2; }

            return increaseCountdown;
        }

        private void InitializeCountdownToAgents(int _increaseCountdown)
        {
            foreach (CustomAgent customAgent in customAgentsList)
            {
                customAgent.Countdown += _increaseCountdown;
                customAgent.Countdown += customAgent.CheckCountdownWithCurrentTraits();
            }
        }

        private void InitializeSocialExchanges()
        {
            OnGoingSEs = 0;
            MaximumSEs = CurrentLocation == "center" ? 3 : 2;

            SocialExchangesList = new List<SocialExchangeSE>();

            foreach (SEs_Enum SocialExchange_E in Enum.GetValues(typeof(SEs_Enum)))
            {
                if (SocialExchange_E != SEs_Enum.Undefined)
                {
                    //int r = ((int)SocialExchange_E);
                    //string seName = SocialExchange_E.ToString();

                    SocialExchangesList.Add(new SocialExchangeSE(SocialExchange_E, null, null));
                }
            }
        }

        private void InitializeStatusList()
        {
            StatusList = new List<string>() { "SocialTalk", "BullyNeed", "Courage", "Anger", "Shame", "Tiredness" };
        }

        private static bool CustomAgentHasEnoughRest(CustomAgent customAgent)
        {
            customAgent.EnoughRest = customAgent.StatusList.Find(_status => _status.Name == "Tiredness").intensity < 0.5;
            return customAgent.EnoughRest;
        }

        public void OnConversationEndWithPlayer(CustomAgent custom)
        {
            if (custom == null)
            {
                return;
            }

            OnGoingSEs--;
            playerStartedASE = false;
            customAgentInteractingWithPlayer = null;

            if (custom.customAgentTarget != null && custom.customAgentTarget.Name == Agent.Main.Name)
            {
                customCharacterReftoCampaignBehaviorBase = null;
                customCharacterIdRefCampaignBehaviorBase = -1;
                SetCanResetCBB_refVariables(true);

                custom.Busy = false;
                custom.IsInitiator = false;
                custom.EndingSocialExchange = false;
                custom.TalkingWithPlayer = false;
                custom.EnoughRest = false;
                custom.UpdateAllStatus(0, 0, 0, 0, 0, 1);
                
                custom.FinalizeSocialExchange();
            }

            custom.EndFollowBehavior();
        }

        private void InitializeEnergyToAgents()
        {
            foreach (CustomAgent customAgent in customAgentsList)
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
                if (customAgent.selfAgent.IsHuman)
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
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/npc_conversations.json");
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

        private void CheckIfFileExists()
        {
            bool fileExists = File.Exists(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/data.json");

            if (!fileExists)
            {
                FileStream file = File.Create(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/data.json");
                file.Close();

                string text = "{ " + "SettlementJson" + ": [] }";
                RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(text);

                File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/data.json", JsonConvert.SerializeObject(myDeserializedClass));
            }
        }

        private void CreateCustomAgent(Agent agent, bool ToPerformSEs, Random rnd = null)
        {
            int id = 0;
            CustomAgent customAgentTemp = new CustomAgent(agent, id, StatusList, SEs_Enum.Undefined);

            foreach (CustomAgent customAgent in customAgentsList)
            {
                if (customAgent.Name == customAgentTemp.Name && customAgent.Id == customAgentTemp.Id)
                {
                    customAgentTemp.Id++;
                }
            }

            if (customAgentTemp.selfAgent.IsHero)
            {
                LoadSavedSEs(customAgentTemp);
            }

            customAgentsList.Add(customAgentTemp);
            AddAgentTarget(agent, customAgentTemp.Id);

            if (ToPerformSEs)
            {
                RandomItem(customAgentTemp);

                mostWantedSE sE = new mostWantedSE(customAgentTemp, new NextSE("", SEs_Enum.Undefined, null, null, 0));
                mostWantedSEList.Add(sE);
            }
            else
            {
                customAgentTemp.Countdown = rnd.Next(1, 3);
            }
        }

        private void ResetSavedSEs()
        {
            File.Delete(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/saved_SEs.json");
            CheckIfSavedSEsFileExists();
        }

        private static void LoadSavedSEs(CustomAgent customAgent)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/saved_SEs.json");
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
                            var key = CustomAgent.Intentions.Hostile;
                            switch (item.SocialExchange)
                            {
                                case "Friendly":
                                    key = CustomAgent.Intentions.Friendly;
                                    break;
                                case "UnFriendly":
                                    key = CustomAgent.Intentions.Unfriendly;
                                    break;
                                case "Romantic":
                                    key = CustomAgent.Intentions.Romantic;
                                    break;
                                case "Hostile":
                                    key = CustomAgent.Intentions.Hostile;
                                    break;
                                case "AskOut":
                                case "Break":
                                    key = CustomAgent.Intentions.Special;
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

        public void SaveSavedSEs(CustomAgent customAgent, string socialExchange)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/saved_SEs.json");
            SEsPerformedToday myDeserializedClass = JsonConvert.DeserializeObject<SEsPerformedToday>(json);

            myDeserializedClass.SEsPerformedList.Add(new SEsPerformed(customAgent.Name, customAgent.Id, socialExchange));

            File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/saved_SEs.json", JsonConvert.SerializeObject(myDeserializedClass));
        }

        private static void CheckIfSavedSEsFileExists()
        {
            bool fileExists = File.Exists(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/saved_SEs.json");

            if (!fileExists)
            {
                FileStream file = File.Create(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/saved_SEs.json");
                file.Close();

                string text = "{ " + "SEsPerformedList" + ": [] }";
                SEsPerformedToday myDeserializedClass = JsonConvert.DeserializeObject<SEsPerformedToday>(text);

                File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/saved_SEs.json", JsonConvert.SerializeObject(myDeserializedClass));
            }
        }

        private void RandomItem(CustomAgent customAgent)
        {
            List<String> listItems = new List<string>() { "gem", "gift" };

            double temp = rnd.NextDouble();
            if (temp < 0.15)
            {
                Random rnd = new Random();
                string item = listItems[rnd.Next(listItems.Count)];

                customAgent.AddItem(item, 1);
            }
        }

        private bool CheckIfSettlementExistsOnFile(string _currentSettlementName, string _currentLocationName)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/data.json");
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

        private void GenerateRandomTraitsForThisNPC(CustomAgent customAgent)
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

        private void SaveNewAgentsInfoToJSON(List<CustomAgent> customAgentsList)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            List<CustomAgentJson> jsonlist = new List<CustomAgentJson>();
            foreach (CustomAgent customAgent in customAgentsList)
            {
                CustomAgentJson json1 = new CustomAgentJson(customAgent.Name, customAgent.Id, customAgent.TraitList, customAgent.ItemList);
                jsonlist.Add(json1);
            }

            myDeserializedClass.SettlementJson.Add(new SettlementJson(CurrentSettlement, CurrentLocation, jsonlist));

            File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/data.json", JsonConvert.SerializeObject(myDeserializedClass));
        }

        private void LoadAllInfoFromJSON()
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            foreach (SettlementJson _settlement in myDeserializedClass.SettlementJson)
            {
                if (_settlement.Name == CurrentSettlement && _settlement.LocationWithId == CurrentLocation)
                {
                    CustomAgent customMain = customAgentsList.Find(c => c.selfAgent == Agent.Main);

                    foreach (CustomAgent customAgent in customAgentsList)
                    {
                        CustomAgentJson _customAgentJson = _settlement.CustomAgentJsonList.Find(c => c.Name == customAgent.Name && c.Id == customAgent.Id);
                        if (_customAgentJson != null)
                        {
                            customAgent.TraitList = _customAgentJson.TraitList;
                            customAgent.SocialNetworkBeliefs = _customAgentJson.SocialNetworkBeliefs;
                            customAgent.ItemList = _customAgentJson.ItemsList;
                            customAgent.MemorySEs = _customAgentJson.MemoriesList;

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

                            File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/data.json", JsonConvert.SerializeObject(myDeserializedClass));
                        }
                        File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/data.json", JsonConvert.SerializeObject(myDeserializedClass));

                        foreach (Trait trait in customAgent.TraitList)
                        {
                            trait.SetCountdownToIncreaseDecrease(trait.traitName);
                        }
                    }
                    break;
                }
            }
        }

        private static void CheckInGameRelationBetweenHeroes(CustomAgent customMain, CustomAgent customAgent)
        {
            int indexHero = Hero.AllAliveHeroes.FindIndex(h => h.CharacterObject == customAgent.selfAgent.Character);
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

        private static SocialNetworkBelief IfBeliefIsNullCreateANewOne(CustomAgent customMain, CustomAgent customAgent, SocialNetworkBelief belief)
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
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            foreach (SettlementJson item in myDeserializedClass.SettlementJson)
            {
                if (item.Name == CurrentSettlement && item.LocationWithId == CurrentLocation)
                {
                    foreach (CustomAgentJson _customAgentJson in item.CustomAgentJsonList)
                    {
                        CustomAgent x = customAgentsList.Find(c => c.Name == _customAgentJson.Name && c.Id == _customAgentJson.Id);
                        if (x != null)
                        {
                            _customAgentJson.TraitList = x.TraitList;
                            _customAgentJson.SocialNetworkBeliefs = x.SocialNetworkBeliefs;
                            _customAgentJson.ItemsList = x.ItemList;
                            _customAgentJson.MemoriesList = x.MemorySEs;
                        }
                    }
                    break;
                }
            }

            File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/data.json", JsonConvert.SerializeObject(myDeserializedClass));
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

        public CustomAgent customCharacterReftoCampaignBehaviorBase { get; set; }
        public int customCharacterIdRefCampaignBehaviorBase { get; set; }

        public SEs_Enum SocialExchange_E { get; set; }

        public enum SEs_Enum
        {
            Undefined = -1, Compliment, GiveGift, Gratitude, Jealous, FriendSabotage, Flirt, Bully, RomanticSabotage, AskOut, Break, HaveAChild
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
            foreach (CustomAgent customAgent in customAgentsList)
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
            float teamOpponentPercentage = 0;
            float teamPlayerPercentage = 0;
            int auxCountPlayerTeam = 0;

            CheckDeadAgentsFromTeams(ref teamOpponentPercentage, ref teamPlayerPercentage, ref auxCountPlayerTeam);

            if (OtherTeamCurrentSpeakers >= OtherTeamLimitSpeakers || PlayerTeamCurrentSpeakers >= PlayerTeamLimitSpeakers || customAgentsList.Count == 0)
            {
                return;
            }

            int index = rnd.Next(customAgentsList.Count);
            CustomAgent customAgent = customAgentsList[index];


            if (!CustomAgentInsideRangeFromPlayer(customAgent))
            {
                return;
            }

            if (customAgent.IsDead)
            {
                customAgent.Message = "";
            }
            else
            {
                if (customAgent.IsPlayerTeam)
                {
                    customAgent.MarkerTyperRef = 1;
                    PlayerTeamCurrentSpeakers++;
                }
                else
                {
                    customAgent.MarkerTyperRef = 2;
                    OtherTeamCurrentSpeakers++;
                }

                GetBattleSentences(customAgent, customAgent.IsPlayerTeam, teamPlayerPercentage, teamOpponentPercentage, rnd);
            }
        }

        private void CheckDeadAgentsFromTeams(ref float teamOpponentCount, ref float teamPlayerCount, ref int auxCountOpponentTeam)
        {
            foreach (Team team in Mission.Current.Teams)
            {
                if (!team.IsPlayerTeam)
                {
                    teamOpponentCount = CheckDeadAgentFromThisTeam(team);
                    auxCountOpponentTeam = team.TeamAgents.Count;
                }
                else
                {
                    teamPlayerCount = CheckDeadAgentFromThisTeam(team, auxCountOpponentTeam);
                }
            }
        }

        private float CheckDeadAgentFromThisTeam(Team team, int auxInt = 0)
        {
            for (int i = 0; i < team.TeamAgents.Count; i++)
            {
                Agent item = team.TeamAgents[i];
                if (!item.IsActive() || item.Health <= 0)
                {
                    customAgentsList[auxInt + i].IsDead = true;
                }
            }

            if (team.TeamAgents.Count == 0)
            {
                return 0;
            }
            else
            {
                return (team.ActiveAgents.Count / team.TeamAgents.Count);
            }
        }

        private void GetBattleSentences(CustomAgent customAgent, bool isPlayerTeam, float teamPlayerCount, float teamOpponentCount, Random rnd)
        {
            if (teamPlayerCount < teamOpponentCount)
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
            else if (teamPlayerCount > teamOpponentCount)
            {
                if (isPlayerTeam)
                {
                    GetBattleSingleSentence(customAgent, BattleDictionary.Winning, rnd);
                }
                else
                {
                    GetBattleSingleSentence(customAgent, BattleDictionary.Losing, rnd);
                }
            }
            else
            {
                GetBattleSingleSentence(customAgent, BattleDictionary.Neutral, rnd);
            }
        }

        private void GetBattleSingleSentence(CustomAgent customAgent, BattleDictionary key, Random rnd)
        {
            battleDictionarySentences.TryGetValue(key, out List<string> BattleSentences);

            int index = rnd.Next(BattleSentences.Count);
            customAgent.Message = BattleSentences[index];
        }

        private void InitializeOnBattle(Random rnd)
        {
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
            rnd = new Random();
            customAgentsList = new List<CustomAgent>();
            battleDictionarySentences = new Dictionary<BattleDictionary, List<string>>();

            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/battle_conversations.json");
            BattleConversationsJson deserializedBattleClass = JsonConvert.DeserializeObject<BattleConversationsJson>(json);
            if (deserializedBattleClass != null)
            {
                battleDictionarySentences.Add(BattleDictionary.Winning, deserializedBattleClass.Winning);
                battleDictionarySentences.Add(BattleDictionary.Neutral, deserializedBattleClass.Neutral);
                battleDictionarySentences.Add(BattleDictionary.Losing, deserializedBattleClass.Losing);
                OtherTeamLimitSpeakers = deserializedBattleClass.OtherTeamLimitSpeakers;
                PlayerTeamLimitSpeakers = deserializedBattleClass.PlayerTeamLimitSpeakers;
                CIF_Range = deserializedBattleClass.RangeConversations;
            }
        }

        private Dictionary<BattleDictionary, List<string>> battleDictionarySentences { get; set; }
        private enum BattleDictionary { Losing, Winning, Neutral }
        private int OtherTeamLimitSpeakers { get; set; }
        private int PlayerTeamLimitSpeakers { get; set; }
        private int OtherTeamCurrentSpeakers { get; set; }
        private int PlayerTeamCurrentSpeakers { get; set; }

        public Dictionary<Enum, CustomAgent.Intentions> dictionaryWithSEsToGauntlet = new Dictionary<Enum, CustomAgent.Intentions>()
        {
            { SocialExchangeSE.IntentionEnum.Undefined, CustomAgent.Intentions.Undefined  },
            { SocialExchangeSE.IntentionEnum.Positive , CustomAgent.Intentions.Friendly   },
            { SocialExchangeSE.IntentionEnum.Negative , CustomAgent.Intentions.Unfriendly },
            { SocialExchangeSE.IntentionEnum.Romantic , CustomAgent.Intentions.Romantic   },
            { SocialExchangeSE.IntentionEnum.Hostile  , CustomAgent.Intentions.Hostile    },
            { SocialExchangeSE.IntentionEnum.Special  , CustomAgent.Intentions.Special    }
        };

        #endregion

        private bool CustomAgentIsNearToPlayer(CustomAgent customAgent)
        {
            return Agent.Main.Position.Distance(customAgent.selfAgent.Position) < 3 && Agent.Main.CanInteractWithAgent(customAgent.selfAgent, 0);
        }

        private bool CustomAgentInsideRangeFromPlayer(CustomAgent customAgent)
        {
            if (Agent.Main != null)
            {
                if (Agent.Main.Position.Distance(customAgent.selfAgent.Position) <= CIF_Range)
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
            if (agent != Agent.Main && agent.Character != null && agent.IsActive() && !this.Targets.Any((CustomMissionNameMarkerTargetVM t) => t.TargetAgent == agent))
            {
                if (agent.IsHuman)
                {
                    CustomMissionNameMarkerTargetVM item = new CustomMissionNameMarkerTargetVM(agent, id);
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
            foreach (CustomMissionNameMarkerTargetVM missionNameMarkerTargetVM in this.Targets)
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
            foreach (CustomMissionNameMarkerTargetVM missionNameMarkerTargetVM in this.Targets)
            {
                missionNameMarkerTargetVM.IsEnabled = state;
            }
        }

        public void EnableDataSource()
        {
            foreach (CustomMissionNameMarkerTargetVM item in this.Targets)
            {
                CustomAgent customAgent = customAgentsList.Find(c => c.Name == item.Name && c.Id == item.Id);

                if (customAgent != null)
                {
                    if (customAgent.Message != "")
                    {
                        item.MarkerType = customAgent.MarkerTyperRef;
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
        public MBBindingList<CustomMissionNameMarkerTargetVM> Targets
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
        private readonly CustomMissionNameMarkerVM.MarkerDistanceComparer _distanceComparer;
        private MBBindingList<CustomMissionNameMarkerTargetVM> _targets;
        private bool _isEnabled;

        private class MarkerDistanceComparer : IComparer<CustomMissionNameMarkerTargetVM>
        {
            public int Compare(CustomMissionNameMarkerTargetVM x, CustomMissionNameMarkerTargetVM y)
            {
                return y.Distance.CompareTo(x.Distance);
            }
        }
    }
}