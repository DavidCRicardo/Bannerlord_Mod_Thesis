using System;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using SandBox.ViewModelCollection;
using Newtonsoft.Json;
using System.IO;

namespace Bannerlord_Mod_Test
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
        public List<CustomAgent> customAgentsList { get; set; }
        private List<string> StatusListString { get; set; }
        private List<string> TraitsListString { get; set; }
        private List<string> SocialExchangeListString { get; set; }
        private List<SocialExchangeSE> SocialExchangeSEList { get; set; }
        private SocialExchangeSE socialExchangeSE { get; set; }
        private List<mostWantedSE> mostWantedSEList { get; set; }
        private int onGoingSEs { get; set; }
        private int maximumSEs { get; set; }
        private bool giveTraitsToNPCs { get; set; }
        private string _currentSettlement { get; set; }
        private string _currentLocation { get; set; } // tavern, center, prison, lordshall
        private NextSE nextSE { get; set; }
        private int auxVolition;
        private string auxSEName;
        private string auxInitiatorName;
        private string auxReceiverName;
        private Random rnd { get; set; }
        public void Tick(float dt)
        {
            if (Hero.MainHero.CurrentSettlement != null)
            {
                if (this._firstTick)
                {
                    rnd = new Random();

                    PreInitialize();
                    Initialize(giveTraitsToNPCs);

                    this._firstTick = false;
                }

                InformationManager.DisplayMessage(new InformationMessage(onGoingSEs.ToString()));

                /*if (onGoingSEs > 1 || onGoingSEs < 0)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Message"));
                }*/

                if (CharacterObject.OneToOneConversationCharacter == null)
                {
                    _firstTick2 = true;

                    foreach (CustomAgent customAgent in customAgentsList)
                    {
                        if (customAgent.busy && customAgent.IsInitiator)
                        {
                            if (customAgent.targetAgent == Agent.Main)
                            {
                                characterReftoCampaignBehaviorBase = customAgent.selfAgent.Character;
                                intentionReftoCampaignBehaviorBase = GetIntentionToCampaignBehaviorBase(customAgent);
                            }

                            customAgent.CustomAgentHasDesire(dt, customAgent.SocialMove, customAgent, rnd, MegaDictionary);
                            if (customAgent.EndingSocialExchange)
                            {
                                onGoingSEs--;
                                customAgent.EndingSocialExchange = false;

                                SaveAllInfoToJSON();
                            }
                        }
                    }

                    DecreaseNPCsCountdown(dt);

                    if (SecsDelay(dt, 1))
                    {
                        UpdateStatus();
                    }
                }
                else
                {
                    if (_firstTick2)
                    {
                        _firstTick2 = false;

                        //
                    }
                }
                UpdateTargetScreen(dt);
            }
        }

        private void DecreaseNPCsCountdown(float dt)
        {
            if (onGoingSEs >= maximumSEs)
            {
                return;
            }

            foreach (CustomAgent customAgent in customAgentsList)
            {
                if (CustomAgentHasEnoughRest(customAgent))
                {
                    if (customAgent.cooldown)
                    {
                        if (customAgent.SecsDelay(dt, customAgent.Countdown))
                        {
                            customAgent.cooldown = false;
                        }
                    }
                }
            }

            DesireFormation();
        }
        private void PreInitialize()
        {
            CheckIfFileExists();

            _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            _currentLocation = CampaignMission.Current.Location.StringId;
            if (CheckIfSettlementExistsOnFile(_currentSettlement, _currentLocation))
            {
                giveTraitsToNPCs = false;
            }
            else { giveTraitsToNPCs = true; }
        }
        private void Initialize(bool letsRandom)
        {
            onGoingSEs = 0;
            maximumSEs = 1;
            /* Initialize the Social Exchanges */
            StatusListString = new List<string>() { "SocialTalk", "Courage", "Anger", "Shame", "Tiredness" };
            SocialExchangeListString = new List<string>() { "Compliment", "AskOut", "Flirt", "Bully", "Sabotage", "Insult", "Jealous", "Break" };

            SocialExchangeSEList = new List<SocialExchangeSE>();
            foreach (String seName in SocialExchangeListString)
            {
                SocialExchangeSEList.Add(new SocialExchangeSE(seName, null, null));
            }
            //SocialExchangeSEList.Add(new SocialExchangeSE("Insult", null, null));

            if (Mission.Current.MainAgent != null)
            {
                if (customAgentsList == null)
                {
                    /* Reference to Next SE & Most Wanted SE*/
                    nextSE = new NextSE("", "", "", 0);
                    mostWantedSEList = new List<mostWantedSE>();

                    /* Reference to CustomAgent */
                    customAgentsList = new List<CustomAgent>();
                    foreach (Agent agent in Mission.Current.Agents)
                    {
                        //if (agent.IsHuman) //to allow all the NPCs
                        if (agent.IsHuman && agent.IsHero)
                        {                            
                            //InformationManager.DisplayMessage(new InformationMessage(agent.Character.Id.ToString()));
                            CustomAgent customAgent = new CustomAgent(agent, StatusListString) { Name = agent.Name };
                            customAgentsList.Add(customAgent);

                            mostWantedSE sE = new mostWantedSE(customAgent.Name, new NextSE("", "", "", 0));
                            mostWantedSEList.Add(sE);

                            AddAgentTarget(agent);
                        }
                    }

                    if (letsRandom)
                    {
                        GiveRandomTraitsToAgents();
                        GiveRandomRulesToAgents();
                    }

                    LoadAllInfoFromJSON();

                    // Increase & Decrease customAgent countdown depending of the traits
                    foreach (CustomAgent customAgent in customAgentsList)
                    {
                        customAgent.Countdown += customAgent.CheckCountdownWithCurrentTraits();
                    }

                    customAgentsList.ForEach(c => c.customAgentsList = customAgentsList);
                    GiveRandomEnergyToAgents();

                    LoadDialogsFromJSON();
                }
            }
        }
        private void UpdateStatus()
        {
            foreach (var customAgent in customAgentsList)
            {
                customAgent.UpdateStatus("SocialTalk", 0.05);
                customAgent.UpdateStatus("Courage", -0.05);
                customAgent.UpdateStatus("Shame", -0.05);

                if (customAgent.targetAgent != null)
                {
                    customAgent.UpdateStatus("Tiredness", 0.1);
                }
                else
                {
                    customAgent.UpdateStatus("Tiredness", -0.05);
                }
            }
        }
        private void DesireFormation()
        {
            /* Set mostWantedSE & nextSE to default values */
            foreach (var k in mostWantedSEList)
            {
                k.nextSE.SEName = "";
                k.nextSE.InitiatorName = "";
                k.nextSE.ReceiverName = "";
                k.nextSE.Volition = 0;
            }
            nextSE.SEName = "";
            nextSE.InitiatorName = "";
            nextSE.ReceiverName = "";
            nextSE.Volition = 0;

            /* Each NPC will check the environment */
            foreach (var c1 in customAgentsList)
            {
                if (c1.selfAgent == Agent.Main || c1.busy || c1.cooldown || !c1.EnoughRest)
                {
                    continue;
                }

                auxVolition = 0;
                auxSEName = "";
                auxInitiatorName = "";
                auxReceiverName = "";

                /*Calculate Volitions for the NPCs around*/
                foreach (var c2 in customAgentsList)
                {
                    if (c1 == c2 || c2.busy) { continue; } // Player Included because it can be the target for some NPC 

                    /* For each Social Exchange */
                    foreach (var se in SocialExchangeSEList)
                    {
                        se.CustomAgentInitiator = c1;
                        se.CustomAgentReceiver = c2;

                        if (se.InitiadorVolition() > auxVolition)
                        {
                            auxVolition = se.CustomAgentInitiator.SEVolition;
                            auxSEName = se.SEName;
                            auxInitiatorName = se.CustomAgentInitiator.Name;
                            auxReceiverName = se.CustomAgentReceiver.Name;
                        }
                    }
                }

                mostWantedSE mostWanted = mostWantedSEList.Find(mostWantedSE => mostWantedSE.CustomAgentName == c1.Name);
                if (auxVolition > mostWanted.nextSE.Volition)
                {
                    mostWanted.nextSE.SEName = auxSEName;
                    mostWanted.nextSE.InitiatorName = auxInitiatorName;
                    mostWanted.nextSE.ReceiverName = auxReceiverName;
                    mostWanted.nextSE.Volition = auxVolition;
                }
            }

            /* Calculate Next SE */
            foreach (var k in mostWantedSEList)
            {
                if (k.nextSE.Volition > nextSE.Volition)
                {
                    nextSE.SEName = k.nextSE.SEName;
                    nextSE.InitiatorName = k.nextSE.InitiatorName;
                    nextSE.ReceiverName = k.nextSE.ReceiverName;
                    nextSE.Volition = k.nextSE.Volition;
                }
            }

            if (nextSE.Volition > 0)
            {
                /*Get Custom Agent | Get NPC*/
                GetCustomAgent(nextSE.InitiatorName).startSE(nextSE.SEName, nextSE.ReceiverName, nextSE.Volition);
                onGoingSEs++;
            }
        }
        private void AbortOnGoingSE(CustomAgent customAgent)
        {
            // se o player ou o NPC interagido pelo player formos os targets ou tiver o target em alguem.. então é tudo abortado
            // o player e o NPC interagido sao considerados como busy
            // se algum outro NPC viria para interagir com o player ou o npc, então aborta a social exchange 
            if (customAgent != null && customAgent.targetAgent != null && customAgent.targetAgent != Agent.Main)
            {
                if (customAgent.busy) // busy // target or not
                {
                    customAgent.AbortSocialExchange();

                    if (customAgent.targetAgent != null) // if has target // its the initiator
                    {
                        if (customAgent.IsInitiator) { customAgent.IsInitiator = false; }
                        if (socialExchangeSE != null) { socialExchangeSE.OnFinalize(); onGoingSEs--; }
                    }
                }
                //customAgent.AbortSocialExchange();
                //customAgent.EndingSocialExchange = false;
            }
        }

        private CustomAgent GetCustomAgent(string _name)
        {
            return customAgentsList.Find(a => a.Name == _name);
        }
        private static bool CustomAgentHasEnoughRest(CustomAgent customAgent)
        {
            customAgent.EnoughRest = customAgent.StatusList.Find(s => s.statusName == "Tiredness").intensity <= 0.5;
            return customAgent.EnoughRest;
        }
        internal void OnConversationEnd2()
        {
            CustomAgent customAgent = customAgentsList.Find(c => c.Name == CharacterObject.OneToOneConversationCharacter.Name.ToString());
            if (customAgent != null)
            {
                if (customAgent.targetAgent == Agent.Main)
                {
                    intentionReftoCampaignBehaviorBase = SocialExchangeSE.IntentionEnum.Undefined;
                    characterReftoCampaignBehaviorBase = null;
                    SetResetOtherVariables(true);

                    onGoingSEs--;
                    customAgent.EndingSocialExchange = false;
                    customAgent.FinalizeSocialExchange();
                    customAgent.targetAgent = null;
                }
            }
        }
        private void GiveRandomRulesToAgents()
        {
            //Random rnd = new Random();
            foreach (var customAgent in customAgentsList)
            {
                string randomRule = SocialExchangeListString[rnd.Next(SocialExchangeListString.Count)];
                List<string> a = new List<string>();
                int v = rnd.Next(-1, 4);

                int i = 0;
                do
                {
                    CustomAgent c = (customAgentsList[rnd.Next(customAgentsList.Count)]);
                    if (c != customAgent)
                    {
                        a.Add(c.Name);

                        i++;
                    }
                } while (i < 2);

                if (customAgent.selfAgent.IsHero)
                {
                    customAgent.TriggerRuleList.Add(new TriggerRule(randomRule, a, v));
                }
            }
            SaveNewAgentsInfoToJSON(customAgentsList);
        }
        private void GiveRandomEnergyToAgents()
        {
            //Random rnd = new Random();
            foreach (var customAgent in customAgentsList)
            {
                //Status status = customAgent.StatusList.Find(s => s.statusName == "Tiredness");
                if (customAgent.selfAgent.IsHero)
                {
                    customAgent.UpdateStatus("Tiredness", rnd.NextDouble());
                }
            }
        }
        private void GiveRandomTraitsToAgents()
        {
            //Random rnd = new Random();
            List<Trait> ListWithAllTraits = InitializeListWithAllTraits();

            foreach (var customAgent in customAgentsList)
            {
                if (customAgent.selfAgent.IsHero && customAgent.selfAgent.IsHuman)
                {
                    for (int i = 0; i < ListWithAllTraits.Count; i++)
                    {
                        if (rnd.NextDouble() > 0.5)
                        {
                            if (i % 2 == 0)
                            {
                                customAgent.TraitList.Add(ListWithAllTraits[i]);
                            }
                            else
                            {
                                bool agentHasTrait = !customAgent.TraitList.Contains(ListWithAllTraits[i - 1]);
                                if (agentHasTrait)
                                {
                                    customAgent.TraitList.Add(ListWithAllTraits[i]);
                                }
                            }
                        }
                    }
                }
            }

            SaveNewAgentsInfoToJSON(customAgentsList);
        }
        private List<Trait> InitializeListWithAllTraits()
        {
            TraitsListString = new List<string>() { "Friendly", "Hostile", "Charming", "UnCharming", "Shy", "Brave", "Calm", "Aggressive", "Faithful", "Unfaithful"};
            List<Trait> AllTraitList = new List<Trait>();

            foreach (var traitName in TraitsListString)
            {
                Trait newTrait = new Trait(traitName);
                AllTraitList.Add(newTrait);
            }

            return AllTraitList;
        }
        private void LoadDialogsFromJSON()
        {
            string myJsonResponse = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/npc_conversations.json");
            RootMessageJson myDeserializedClassConversations = JsonConvert.DeserializeObject<RootMessageJson>(myJsonResponse);

            Dictionary<string, List<string>> fromIDGetListMessages = new Dictionary<string, List<string>>();
            Dictionary<string, Dictionary<string, List<string>>> fromCultureGetID = new Dictionary<string, Dictionary<string, List<string>>>();
            Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> fromIntentionGetCulture = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();

            fromIntentionGetCulture = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
            //Only 1 DialogsRoot
            foreach (DialogsRoot _socialExchange in myDeserializedClassConversations.SocialExchangeListFromJson)
            {
                fromCultureGetID = new Dictionary<string, Dictionary<string, List<string>>>();
                foreach (Culture _culture in _socialExchange.CultureList)
                {
                    fromIDGetListMessages = new Dictionary<string, List<string>>();

                    foreach (NPCDialog _npcDialog in _culture.NPCDialogs)
                    {
                        if (_npcDialog.id == "start")
                        {
                            fromIDGetListMessages.Add(_npcDialog.id, _npcDialog.messages);
                        }
                        if (_npcDialog.id == "accept")
                        {
                            fromIDGetListMessages.Add(_npcDialog.id, _npcDialog.messages);
                        }
                        if (_npcDialog.id == "reject")
                        {
                            fromIDGetListMessages.Add(_npcDialog.id, _npcDialog.messages);
                        }
                    }
                    fromCultureGetID.Add(_culture.CultureCode, fromIDGetListMessages);

                }
                fromIntentionGetCulture.Add(_socialExchange.SocialExchange, fromCultureGetID);
            }

            MegaDictionary = fromIntentionGetCulture;
        }
        private void CheckIfFileExists()
        {
            try
            {
                File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json");
            }
            catch
            {
                FileStream file = File.Create(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json");
                file.Close();

                string text = "{ " + "SettlementJson" + ": [] }";
                RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(text);

                File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json", JsonConvert.SerializeObject(myDeserializedClass));
            }
        }
        private bool CheckIfSettlementExistsOnFile(string _currentSettlementName, string _currentLocationName)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            if (myDeserializedClass == null)
            {
                return false;
            }
            if (myDeserializedClass.SettlementJson.Any(s => s.Name == _currentSettlementName && s.LocationWithId == _currentLocationName)) { return true; }
            else
            {
                return false;
            }
        }
        private void SaveNewAgentsInfoToJSON(List<CustomAgent> customAgentsList)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            List<CustomAgentJson> jsonlist = new List<CustomAgentJson>();
            foreach (CustomAgent customAgent in customAgentsList)
            {
                if (customAgent.selfAgent.IsHero)
                {
                    CustomAgentJson json1 = new CustomAgentJson(customAgent.Name, customAgent.TraitList, customAgent.TriggerRuleList);
                    jsonlist.Add(json1);
                }
            }

            myDeserializedClass.SettlementJson.Add(new SettlementJson(_currentSettlement, _currentLocation, jsonlist));

            File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json", JsonConvert.SerializeObject(myDeserializedClass));
        }
        private void LoadAllInfoFromJSON()
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            foreach (SettlementJson item in myDeserializedClass.SettlementJson)
            {
                if (item.Name == _currentSettlement && item.LocationWithId == _currentLocation)
                {
                    foreach (CustomAgentJson _customAgentJson in item.CustomAgentJsonList)
                    {
                        CustomAgent x = customAgentsList.Find(c => c.Name == _customAgentJson.Name);
                        if (x != null)
                        {
                            x.TraitList = _customAgentJson.TraitList;
                            x.GoalsList = _customAgentJson.GoalsList;
                            x.SocialNetworkBeliefs = _customAgentJson.SocialNetworkBeliefs;
                            x.ItemList = _customAgentJson.ItemsList;
                            x.MemorySEs = _customAgentJson.MemoriesList;
                            x.TriggerRuleList = _customAgentJson.TriggerRulesList;

                            foreach (Trait trait in x.TraitList)
                            {
                                trait.SetCountdownToIncreaseDecrease(trait.traitName);
                            }
                        }
                    }
                    break;
                }
            }
        }
        private void SaveAllInfoToJSON()
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            foreach (SettlementJson item in myDeserializedClass.SettlementJson)
            {
                if (item.Name == _currentSettlement && item.LocationWithId == _currentLocation)
                {
                    foreach (CustomAgentJson _customAgentJson in item.CustomAgentJsonList)
                    {
                        var x = customAgentsList.Find(c => c.Name == _customAgentJson.Name);
                        if (x != null)
                        {
                            _customAgentJson.TraitList = x.TraitList;
                            _customAgentJson.GoalsList = x.GoalsList;
                            _customAgentJson.SocialNetworkBeliefs = x.SocialNetworkBeliefs;
                            _customAgentJson.ItemsList = x.ItemList;
                            _customAgentJson.MemoriesList = x.MemorySEs;
                            _customAgentJson.TriggerRulesList = x.TriggerRuleList;
                        }
                    }
                    //break;
                }
            }

            File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json", JsonConvert.SerializeObject(myDeserializedClass));
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

        private static bool resetVariables { get; set; }
        public bool GetResetOtherVariables()
        {
            return resetVariables;
        }
        public void SetResetOtherVariables(bool value)
        {
            resetVariables = value;
        }

        internal BasicCharacterObject characterReftoCampaignBehaviorBase { get; set; }
        internal SocialExchangeSE.IntentionEnum intentionReftoCampaignBehaviorBase { get; set; }
        private SocialExchangeSE.IntentionEnum GetIntentionToCampaignBehaviorBase(CustomAgent customAgent)
        {
            if (customAgent.SocialMove != "")
            {
                if (customAgent.SocialMove == "Compliment")
                {
                    return SocialExchangeSE.IntentionEnum.Positive;
                }
                else if (customAgent.SocialMove == "Flirt")
                {
                    return SocialExchangeSE.IntentionEnum.Romantic;
                }
                else if (customAgent.SocialMove == "Bully")
                {
                    return SocialExchangeSE.IntentionEnum.Hostile;
                }
                else if (customAgent.SocialMove == "Jealous")
                {
                    return SocialExchangeSE.IntentionEnum.Negative;
                }
                else if (customAgent.SocialMove == "Break")
                {
                    return SocialExchangeSE.IntentionEnum.Special;
                }
                else
                {
                    return SocialExchangeSE.IntentionEnum.Undefined;
                }
            }
            else
            {
                return SocialExchangeSE.IntentionEnum.Undefined;
            }
        }

        private void AddAgentTarget(Agent agent)
        {
            if (agent != Agent.Main && agent.Character != null && agent.IsActive() && !this.Targets.Any((CustomMissionNameMarkerTargetVM t) => t.TargetAgent == agent))
            {
                if (agent.IsHuman/* agent.Character.IsHero*/)
                {
                    CustomMissionNameMarkerTargetVM item = new CustomMissionNameMarkerTargetVM(agent);
                    this.Targets.Add(item);
                    return;
                }
                Settlement currentSettlement = Settlement.CurrentSettlement;
                bool flag;
                if (currentSettlement == null)
                {
                    flag = false;
                }
                else
                {
                    LocationCharacter locationCharacter = currentSettlement.LocationComplex.FindCharacter(agent);
                    bool? flag2 = (locationCharacter != null) ? new bool?(locationCharacter.IsVisualTracked) : null;
                    bool flag3 = true;
                    flag = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
                }
                if (flag)
                {
                    CustomMissionNameMarkerTargetVM item2 = new CustomMissionNameMarkerTargetVM(agent);
                    this.Targets.Add(item2);
                }
            }
        }
        private void UpdateTargetScreen(float dt)
        {
            if (this.IsEnabled)
            {
                this.UpdateTargetScreenPositions();
                this._fadeOutTimerStarted = false;
                this._fadeOutTimer = 0f;
                this._prevEnabledState = this.IsEnabled;
            }
            else
            {
                if (this._prevEnabledState)
                {
                    this._fadeOutTimerStarted = true;
                }
                if (this._fadeOutTimerStarted)
                {
                    this._fadeOutTimer += dt;
                }
                if (this._fadeOutTimer < 2f)
                {
                    this.UpdateTargetScreenPositions();
                }
                else
                {
                    this._fadeOutTimerStarted = false;
                }
            }
            this._prevEnabledState = this.IsEnabled;
        }
        private void UpdateTargetScreenPositions()
        {
            foreach (CustomMissionNameMarkerTargetVM missionNameMarkerTargetVM in this.Targets)
            {
                //missionNameMarkerTargetVM.IsEnabled = true;
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

        public void EnableDataSource(CustomMissionNameMarkerVM _dataSource)
        {
            foreach (CustomMissionNameMarkerTargetVM item in _dataSource.Targets)
            {
                //code here
            
                CustomAgent customAgent = customAgentsList.Find(c => c.Name == item.TargetAgent.Name);
                if (customAgent.message != "")
                {
                    item.Name = customAgent.message;
                    item.IsEnabled = true;
                }
                else
                {
                    item.IsEnabled = false;
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

        internal Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> MegaDictionary { get; private set; }

        private readonly Camera _missionCamera;
        private bool _firstTick = true;
        private bool _firstTick2 = true;
        private readonly Mission _mission;
        private Vec3 _heightOffset = new Vec3(0f, 0f, 2f, -1f);
        private bool _prevEnabledState;
        private bool _fadeOutTimerStarted;
        private float _fadeOutTimer;
        private readonly CustomMissionNameMarkerVM.MarkerDistanceComparer _distanceComparer;

        private readonly List<string> PassagePointFilter = new List<string>
        {
            "Empty Shop"
        };

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

//private void RemoveAgentTarget(Agent agent)
//{
//    if (this.Targets.SingleOrDefault((CustomMissionNameMarkerTargetVM t) => t.TargetAgent == agent) != null)
//    {
//        this.Targets.Remove(this.Targets.Single((CustomMissionNameMarkerTargetVM t) => t.TargetAgent == agent));
//    }
//}
//public override void RefreshValues()
//{
//    base.RefreshValues();
//    this.Targets.ApplyActionOnAllItems(delegate (CustomMissionNameMarkerTargetVM x)
//    {
//        x.RefreshValues();
//    });
//}