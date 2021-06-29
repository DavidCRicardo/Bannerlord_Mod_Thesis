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

        private List<SocialExchangeSE> SocialExchangesList { get; set; }
        private List<mostWantedSE> mostWantedSEList { get; set; }
        private SocialExchangeSE socialExchangeSE { get; set; }
        public List<CustomAgent> customAgentsList { get; set; }
        private List<string> StatusList { get; set; }

        private int onGoingSEs { get; set; }
        private int maximumSEs { get; set; }
        private bool giveTraitsToNPCs { get; set; }
        private string _currentSettlement { get; set; }
        private string _currentLocation { get; set; } // tavern, center, prison, lordshall
        private NextSE nextSE { get; set; }

        private string auxSEName;
        private CustomAgent auxInitiatorAgent;
        private CustomAgent auxReceiverAgent;
        private int auxVolition;

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

                if (CharacterObject.OneToOneConversationCharacter == null)
                {
                    _firstTick2 = true;

                    foreach (CustomAgent customAgent in customAgentsList)
                    {
                        CustomAgentsNearPlayer(customAgent);
                        
                        CustomAgentGoingToSE(dt, customAgent);
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
                    }
                }

                UpdateTargetScreen();
            }
        }

        private void CustomAgentGoingToSE(float dt, CustomAgent customAgent)
        {
            if (customAgent.Busy && customAgent.IsInitiator)
            {
                if (customAgent.customAgentTarget != null && customAgent.customAgentTarget.selfAgent == Agent.Main)
                {
                    customCharacterReftoCampaignBehaviorBase = customAgent;
                    intentionRefToCBB = GetIntentionToCBB(customAgent);
                }

                customAgent.CustomAgentWithDesire(dt, rnd, MegaDictionary);
                if (customAgent.EndingSocialExchange)
                {
                    onGoingSEs--;
                    customAgent.EndingSocialExchange = false;

                    SaveAllInfoToJSON();
                }
            }
            else
            {
                customAgent.CustomAgentWithoutDesire(dt);
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
                    if (customAgent.SecsDelay(dt, customAgent.Countdown))
                    {
                        customAgent.EnoughRest = true;
                    }
                }
            }

            DesireFormation();
        }

        private void UpdateStatus()
        {
            foreach (CustomAgent customAgent in customAgentsList)
            {
                if (customAgent.customAgentTarget != null) // if he has a target and it's going to it 
                {
                    customAgent.UpdateAllStatus(0.05, -0.05, -0.05, -0.05, 0.05);
                }
                else // else is resting
                {
                    customAgent.UpdateAllStatus(0.05, -0.05, -0.05, -0.05, -0.05);
                }
            }
        }

        private void DesireFormation()
        {
            /* Set mostWantedSE & nextSE to default values */
            foreach (var k in mostWantedSEList)
            {
                k.nextSE.SEName = "";
                k.nextSE.InitiatorAgent = null;
                k.nextSE.ReceiverAgent = null;
                k.nextSE.Volition = 0;
            }
            nextSE.SEName = "";
            nextSE.Volition = 0;
            nextSE.InitiatorAgent = null;
            nextSE.ReceiverAgent = null;

            /* Each NPC will check the environment */
            foreach (var c1 in customAgentsList)
            {
                if (c1.selfAgent == Agent.Main || c1.Busy || !c1.EnoughRest)
                {
                    continue;
                }

                auxVolition = 0;
                auxSEName = "";
                auxInitiatorAgent = null;
                auxReceiverAgent = null;

                /*Calculate Volitions for the NPCs around*/
                foreach (var c2 in customAgentsList)
                {
                    if (c1 == c2 || c2.Busy) { continue; } // Player Included because it can be the target for some NPC 

                    /* For each Social Exchange */
                    foreach (SocialExchangeSE se in SocialExchangesList)
                    {
                        se.CustomAgentInitiator = c1;
                        se.CustomAgentReceiver = c2;
                        
                        if (se.InitiadorVolition() > auxVolition)
                        {
                            auxVolition = se.CustomAgentInitiator.SEVolition;
                            auxSEName = se.SEName;
                            auxInitiatorAgent = se.CustomAgentInitiator;
                            auxReceiverAgent = se.CustomAgentReceiver;
                        }
                    }
                }

                mostWantedSE mostWanted = mostWantedSEList.Find(mostWantedSE => mostWantedSE.customAgent == c1);
                if (auxVolition > mostWanted.nextSE.Volition)
                {
                    mostWanted.nextSE.SEName = auxSEName;
                    mostWanted.nextSE.InitiatorAgent = auxInitiatorAgent;
                    mostWanted.nextSE.ReceiverAgent = auxReceiverAgent;
                    mostWanted.nextSE.Volition = auxVolition;
                }
            }

            /* Calculate Next SE */
            foreach (var k in mostWantedSEList)
            {
                if (k.nextSE.Volition > nextSE.Volition)
                {
                    nextSE.SEName = k.nextSE.SEName;
                    nextSE.InitiatorAgent = k.nextSE.InitiatorAgent;
                    nextSE.ReceiverAgent = k.nextSE.ReceiverAgent;
                    nextSE.Volition = k.nextSE.Volition;
                }
            }

            if (nextSE.Volition > 0)
            {
                /* Get NPC & Start SE */
                nextSE.InitiatorAgent.StartSE(nextSE.SEName, nextSE.ReceiverAgent);
                onGoingSEs++;
            }
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

        private void Initialize(bool giveTraitsToNPCs)
        {
            InitializeSocialExchanges();

            InitializeStatusList();

            if (Mission.Current.MainAgent != null)
            {
                if (customAgentsList == null)
                {
                    /* Reference to Next SE & Most Wanted SE */
                    nextSE = new NextSE("", null, null, 0);
                    mostWantedSEList = new List<mostWantedSE>();

                    /* Reference to CustomAgent */
                    customAgentsList = new List<CustomAgent>();

                    foreach (Agent agent in Mission.Current.Agents)
                    {
                        if (agent.IsHuman && agent.Character != null) //to allow all the NPCs
                        {
                            CreateCustomAgent(agent);
                        }
                    }

                    if (giveTraitsToNPCs)
                    {
                        GiveRandomTraitsToAgents();
                        SaveNewAgentsInfoToJSON(customAgentsList);
                    }

                    LoadAllInfoFromJSON();

                    // Set CustomAgent countdown depending of the traits
                    foreach (CustomAgent customAgent in customAgentsList)
                    {
                        customAgent.Countdown += customAgent.CheckCountdownWithCurrentTraits();
                    }

                    customAgentsList.ForEach(c => c.CustomAgentsList = customAgentsList);
                    GiveRandomEnergyToAgents();

                    LoadDialogsFromJSON();
                }
            }
        }

        private void InitializeSocialExchanges()
        {
            onGoingSEs = 0;
            maximumSEs = _currentLocation == "center" ? 4 : 2;

            SocialExchangesList = new List<SocialExchangeSE>();

            List<string> Temp_SEs = new List<string>() { "Compliment", "Jealous", "FriendSabotage", "AskOut", "Flirt", "RomanticSabotage", "Bully", "Break" };
            foreach (string SE_Name in Temp_SEs)
            {
                SocialExchangesList.Add(new SocialExchangeSE(SE_Name, null, null));
            }
        }
        
        private void InitializeStatusList()
        {
            StatusList = new List<string>() { "SocialTalk", "Courage", "Anger", "Shame", "Tiredness" };
        }

        private static bool CustomAgentHasEnoughRest(CustomAgent customAgent)
        {
            customAgent.EnoughRest = customAgent.StatusList.Find(_status => _status.Name == "Tiredness").intensity <= 0.5;
            return customAgent.EnoughRest;
        }

        public void OnConversationEndWithPlayer(CustomAgent custom)
        {
            CustomAgent customAgent = customAgentsList.Find(c => c.selfAgent == custom.selfAgent && c.Id == custom.Id);
            if (customAgent != null)
            {
                if (customAgent.customAgentTarget != null && customAgent.customAgentTarget.selfAgent.Name == Agent.Main.Name)
                {
                    intentionRefToCBB = SocialExchangeSE.IntentionEnum.Undefined;
                    customCharacterReftoCampaignBehaviorBase = null;
                    SetCanResetCBB_refVariables(true);

                    onGoingSEs--;
                    customAgent.EndingSocialExchange = false;
                    customAgent.FinalizeSocialExchange();
                    customAgent.customAgentTarget = null;

                }
            }
        }

        private void GiveRandomEnergyToAgents()
        {
            foreach (CustomAgent customAgent in customAgentsList)
            {
                customAgent.UpdateAllStatus(0, 0, 0, 0, rnd.NextDouble());
            }
        }

        private void GiveRandomTraitsToAgents()
        {
            List<Trait> ListWithAllTraits = InitializeListWithAllTraits();

            foreach (var customAgent in customAgentsList)
            {
                if (customAgent.selfAgent.IsHuman)
                {
                    for (int i = 0; i < ListWithAllTraits.Count; i++)
                    {
                        double tempDouble = rnd.NextDouble();

                        if (tempDouble >= 0.5)
                        {
                            int traitToAdd;
                            if (tempDouble <= 0.75)
                            {
                                traitToAdd = i;
                            }
                            else
                            {
                                traitToAdd = i++;
                            }

                            customAgent.TraitList.Add(ListWithAllTraits[traitToAdd]);
                        }

                        i++;
                    }

                    /*for (int i = 0; i < ListWithAllTraits.Count; i++)
                    {
                        if (rnd.NextDouble() > 0.5)
                        {
                            if (i % 2 == 0)
                            {
                                customAgent.TraitList.Add(ListWithAllTraits[i]);
                            }
                            else
                            {
                                bool agentHasTrait = !customAgent.TraitList.Contains(
                                    ListWithAllTraits[i - 1]);
                                if (agentHasTrait)
                                {
                                    customAgent.TraitList.Add(ListWithAllTraits[i]);
                                }
                            }
                        }
                    }*/
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
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/npc_conversations.json");
            RootMessageJson myDeserializedClassConversations = JsonConvert.DeserializeObject<RootMessageJson>(json);

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
                        else if (_npcDialog.id == "accept")
                        {
                            fromIDGetListMessages.Add(_npcDialog.id, _npcDialog.messages);
                        }
                        else if (_npcDialog.id == "reject")
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

        private void CreateCustomAgent(Agent agent)
        {
            int id = 0;
            CustomAgent customAgentTemp = new CustomAgent(agent, id, StatusList);

            foreach (CustomAgent customAgent in customAgentsList)
            {
                if (customAgent.selfAgent.Name == customAgentTemp.selfAgent.Name && customAgent.Id == customAgentTemp.Id)
                {
                    customAgentTemp.Id++;
                }
            }

            customAgentsList.Add(customAgentTemp);
            AddAgentTarget(agent, customAgentTemp.Id);

            mostWantedSE sE = new mostWantedSE(customAgentTemp, new NextSE("", null, null, 0));
            mostWantedSEList.Add(sE);
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
                CustomAgentJson json1 = new CustomAgentJson(customAgent.selfAgent.Name, customAgent.Id, customAgent.TraitList);
                jsonlist.Add(json1);
            }

            myDeserializedClass.SettlementJson.Add(new SettlementJson(_currentSettlement, _currentLocation, jsonlist));

            File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json", JsonConvert.SerializeObject(myDeserializedClass));
        }

        private void LoadAllInfoFromJSON()
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            foreach (SettlementJson _settlement in myDeserializedClass.SettlementJson)
            {
                if (_settlement.Name == _currentSettlement && _settlement.LocationWithId == _currentLocation)
                {
                    foreach (CustomAgent customAgent in customAgentsList)
                    {
                        CustomAgentJson _customAgentJson = _settlement.CustomAgentJsonList.Find(c => c.Name == customAgent.selfAgent.Name && c.Id == customAgent.Id);
                        if (_customAgentJson != null)
                        {
                            customAgent.TraitList = _customAgentJson.TraitList;
                            customAgent.SocialNetworkBeliefs = _customAgentJson.SocialNetworkBeliefs;
                            customAgent.ItemList = _customAgentJson.ItemsList;
                            customAgent.MemorySEs = _customAgentJson.MemoriesList;
                            customAgent.TriggerRuleList = _customAgentJson.TriggerRulesList;
                        }
                        else
                        {
                            //Checking if there is any NPC new on the town who hasn't on the 1st time when the file was generated
                            GenerateRandomTraitsForThisNPC(customAgent);
                            _settlement.CustomAgentJsonList.Add(new CustomAgentJson(customAgent.selfAgent.Name, customAgent.Id, customAgent.TraitList));

                            File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json", JsonConvert.SerializeObject(myDeserializedClass));
                        }

                        foreach (Trait trait in customAgent.TraitList)
                        {
                            trait.SetCountdownToIncreaseDecrease(trait.traitName);
                        }
                    }
                    break;
                }
            }
        }

        private void GenerateRandomTraitsForThisNPC(CustomAgent customAgent)
        {
            List<Trait> ListWithAllTraits = InitializeListWithAllTraits();

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
                        CustomAgent x = customAgentsList.Find(c => c.selfAgent.Name == _customAgentJson.Name && c.Id == _customAgentJson.Id);
                        if (x != null)
                        {
                            _customAgentJson.TraitList = x.TraitList;
                            _customAgentJson.SocialNetworkBeliefs = x.SocialNetworkBeliefs;
                            _customAgentJson.ItemsList = x.ItemList;
                            _customAgentJson.MemoriesList = x.MemorySEs;
                            _customAgentJson.TriggerRulesList = x.TriggerRuleList;
                        }
                    }
                    break;
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

        public bool CanResetCBB_refVariables()
        {
            return resetVariables;
        }

        public void SetCanResetCBB_refVariables(bool value)
        {
            resetVariables = value;
        }

        public CustomAgent customCharacterReftoCampaignBehaviorBase { get; set; }

        public SocialExchangeSE.IntentionEnum intentionRefToCBB { get; set; }
        private SocialExchangeSE.IntentionEnum GetIntentionToCBB(CustomAgent customAgent)
        {
            if (customAgent.SocialMove != "")
            {
                if (customAgent.SocialMove == "Compliment")
                {
                    return SocialExchangeSE.IntentionEnum.Positive;
                }
                else if (customAgent.SocialMove == "Flirt" || customAgent.SocialMove == "AskOut")
                {
                    return SocialExchangeSE.IntentionEnum.Romantic;
                }
                else if (customAgent.SocialMove == "Bully" || customAgent.SocialMove == "RomanticSabotage")
                {
                    return SocialExchangeSE.IntentionEnum.Hostile;
                }
                else if (customAgent.SocialMove == "Jealous" || customAgent.SocialMove == "FriendSabotage")
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

        private void CustomAgentsNearPlayer(CustomAgent customAgent)
        {
            if (customAgent.selfAgent != Agent.Main && Agent.Main.Position.Distance(customAgent.selfAgent.Position) < 3 && Agent.Main.CanInteractWithAgent(customAgent.selfAgent, 0))
            {
                customAgent.NearPlayer = true;
            }
            else
            {
                customAgent.NearPlayer = false;
            }
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
                CustomAgent customAgent = customAgentsList.Find(c => c.selfAgent.Name == item.Name && c.Id == item.Id);

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

        private void AbortOnGoingSE(CustomAgent customAgent)
        {
            // se o player ou o NPC interagido pelo player formos os targets ou tiver o target em alguem.. então é tudo abortado
            // o player e o NPC interagido sao considerados como busy
            // se algum outro NPC viria para interagir com o player ou o npc, então aborta a social exchange 

            if (customAgent != null && customAgent.customAgentTarget != null && customAgent.customAgentTarget.selfAgent != Agent.Main)
            {
                if (customAgent.Busy) // busy // target or not
                {
                    customAgent.AbortSocialExchange();

                    if (customAgent.customAgentTarget != null) // if has target // its the initiator
                    {
                        if (customAgent.IsInitiator) { customAgent.IsInitiator = false; }
                        if (socialExchangeSE != null) { socialExchangeSE.OnFinalize(); onGoingSEs--; }
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

        public Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> MegaDictionary { get; private set; }

        private readonly Camera _missionCamera;
        private bool _firstTick = true;
        private bool _firstTick2 = true;
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