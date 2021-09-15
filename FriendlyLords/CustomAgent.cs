using Newtonsoft.Json;
using SandBox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace FriendlyLords
{
    public class CustomAgent : CampaignBehaviorBase
    {
        public Agent selfAgent; 
        public int Id { get; set; }
        public string Name { get; set; }

        public CultureCode cultureCode { get; set; }
        public List<CultureCode> CulturesFriendly { get; private set; }
        public List<CultureCode> CulturesUnFriendly { get; private set; }

        public CustomAgent customAgentTarget;
        public int IdTarget { get; set; }

        public string[] FullMessage { get; set; } 
        public string Message { get; set; } 
        public bool SE_Accepted { get; set; }
        public int SEVolition { get; set; }
        public List<CustomAgent> CustomAgentsList { get; set; }

        public List<mostWantedSE> mostWantedSEList { get; set; }
        public SocialExchangeSE socialExchangeSE { get; set; }

        public List<Trait> TraitList { get; set; }
        public List<Status> StatusList { get; set; }
        public List<SocialNetworkBelief> SocialNetworkBeliefs { get; set; }
        public List<Item> ItemList { get; set; }
        public List<TriggerRule> TriggerRuleList { get; set; }
        public List<MemorySE> MemorySEs { get; set; }

        public bool NearEnoughToStartConversation { get; set; }
        public bool EnoughRest { get; set; } // is not in cooldown? // enough rest?
        public bool Busy { get; set; } // Has Target to start a social exchange when close? // or it's interacting?
        public float Countdown { get; set; } // How much NPC needs to wait to set cooldown as false

        public bool EndingSocialExchange { get; set; }
        public bool IsInitiator { get; set; }
        public bool NearPlayer { get; set; }
        public int MarkerTypeRef { get; set; }

        private readonly int memorySize = 5;
        public string thirdAgent;
        public int thirdAgentId;
        public bool TalkingWithPlayer { get; set; }
        public bool CompanionFollowingPlayer { get; set; }

        public bool IsPlayerTeam { get; set; }
        public bool IsDead { get; set; }
        public bool RunAI { get; internal set; }

        public enum Intentions { Undefined, Friendly, Unfriendly, Romantic, Hostile, Special }
        public Dictionary<Intentions, bool> keyValuePairsSEs { get; set; }

        public CustomMissionNameMarkerVM.SEs_Enum SocialMove_SE { get; set; }

        public CustomAgent(Agent _agent, int _id, List<string> _statusList = null, CustomMissionNameMarkerVM.SEs_Enum se_enum = default, float _NPCCountdownMultiplier = 1)
        {
            this.selfAgent = _agent;
            this.Name = _agent.Name;
            this.Id = _id;

            SetCultureCodeInfo(_agent);

            this.customAgentTarget = null;
            this.IdTarget = -1;

            this.Message = "";
            this.SocialMove_SE = se_enum;

            this.CustomAgentsList = new List<CustomAgent>();

            this.TraitList = new List<Trait>();
            this.SocialNetworkBeliefs = new List<SocialNetworkBelief>();
            this.ItemList = new List<Item>();
            this.MemorySEs = new List<MemorySE>();
            this.TriggerRuleList = new List<TriggerRule>();
            this.StatusList = new List<Status>();

            this.IsInitiator = false;
            this.NearPlayer = false;
            this.MarkerTypeRef = 1;
            this.CompanionFollowingPlayer = false;

            AddStatusToCustomAgent(_statusList);
            this.Countdown = SetCountdownToCustomAgent();

            this.Busy = false;
            this.EnoughRest = false;
            
            if (_agent == Agent.Main)
            {
                this.RunAI = true;
                Countdown += 5;  
            }
            else
            {
                Countdown += 1;
                this.RunAI = false;
            }

            Countdown *= _NPCCountdownMultiplier;

            InitializeSEsOptionsAvailability();
            ResetSocialExchangesOptions();
        }

        internal void ResetSocialExchangesOptions()
        {
            foreach (Intentions item in Enum.GetValues(typeof(Intentions)))
            {
                keyValuePairsSEs[item] = false;
            }
        }

        private void InitializeSEsOptionsAvailability()
        {
            keyValuePairsSEs = new Dictionary<Intentions, bool>();
            keyValuePairsSEs.Add(Intentions.Friendly, false);
            keyValuePairsSEs.Add(Intentions.Unfriendly, false);
            keyValuePairsSEs.Add(Intentions.Romantic, false);
            keyValuePairsSEs.Add(Intentions.Hostile, false);
            keyValuePairsSEs.Add(Intentions.Special, false);
        }

        private void SetCultureCodeInfo(Agent _agent)
        {
            cultureCode = _agent.Character.Culture.GetCultureCode();

            switch (cultureCode)
            {
                case CultureCode.Empire:
                    CulturesFriendly = new List<CultureCode>() { CultureCode.Sturgia };
                    CulturesUnFriendly = new List<CultureCode>() { CultureCode.Aserai };
                    break;
                case CultureCode.Sturgia:
                    CulturesFriendly = new List<CultureCode>() { CultureCode.Empire };
                    CulturesUnFriendly = new List<CultureCode>() { CultureCode.Vlandia };
                    break;
                case CultureCode.Aserai:
                    CulturesFriendly = new List<CultureCode>() { CultureCode.Vlandia };
                    CulturesUnFriendly = new List<CultureCode>() { CultureCode.Empire };
                    break;
                case CultureCode.Vlandia:
                    CulturesFriendly = new List<CultureCode>() { CultureCode.Khuzait };
                    CulturesUnFriendly = null;
                    break;
                case CultureCode.Khuzait:
                    CulturesFriendly = new List<CultureCode>() { CultureCode.Vlandia };
                    CulturesUnFriendly = null;
                    break;
                case CultureCode.Vakken:
                    CulturesFriendly = new List<CultureCode>() { CultureCode.Battania };
                    CulturesUnFriendly = new List<CultureCode>() { CultureCode.Darshi };
                    break;
                case CultureCode.Battania:
                    CulturesFriendly = new List<CultureCode>() { CultureCode.Vakken };
                    CulturesUnFriendly = new List<CultureCode>() { CultureCode.Nord };
                    break;
                case CultureCode.Nord:
                    CulturesFriendly = new List<CultureCode>() { CultureCode.Darshi };
                    CulturesUnFriendly = new List<CultureCode>() { CultureCode.Battania };
                    break;
                case CultureCode.Darshi:
                    CulturesFriendly = new List<CultureCode>() { CultureCode.Nord };
                    CulturesUnFriendly = new List<CultureCode>() { CultureCode.Vakken };
                    break;
                case CultureCode.Invalid:
                case CultureCode.AnyOtherCulture:
                default:
                    CulturesFriendly = null;
                    CulturesUnFriendly = null;
                    break;
            }
        }

        private void AddStatusToCustomAgent(List<string> auxStatusList)
        {
            if (auxStatusList != null)
            {
                foreach (string statusName in auxStatusList)
                {
                    StatusList.Add(new Status(statusName));
                }
            }
        }

        private int SetCountdownToCustomAgent()
        {
            int _countdown = 5;
            if (TraitList.Exists(t => t.traitName == "Shy"))
            {
                _countdown += 2;
            }
            if (TraitList.Exists(t => t.traitName == "Brave"))
            {
                _countdown -= 2;
            }

            return _countdown;
        }

        public override void RegisterEvents() { }
        public override void SyncData(IDataStore dataStore) { }

        public void StartSE(CustomMissionNameMarkerVM.SEs_Enum _seEnum, CustomAgent _Receiver)
        {
            //CompanionDebug();

            UpdateTarget(_Receiver.Name, _Receiver.Id);

            customAgentTarget.Busy = true;

            this.EndingSocialExchange = false;
            IsInitiator = true;

            SocialMove_SE = _seEnum;

            Busy = true;
        }

        private void CompanionDebug()
        {
            foreach (Hero hero in Clan.PlayerClan.Companions)
            {
                if (selfAgent.Character == hero.CharacterObject)
                {
                    DailyBehaviorGroup behaviorGroup = selfAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();

                    var behavior = behaviorGroup.GetBehavior<FollowAgentBehavior>();
                    if (behavior != null)
                    {
                        Vec2 heroVec2Position = Agent.Main.Position.AsVec2 - behavior.Navigator.TargetPosition.AsVec2;
                        Vec2 rangeVec2 = new Vec2(5f, 5f);

                        if (heroVec2Position.X < rangeVec2.X && heroVec2Position.Y < rangeVec2.Y)
                        {
                            this.CompanionFollowingPlayer = true;
                        }
                        else { this.CompanionFollowingPlayer = false; }
                    }
                }
            }
        }

        public void CustomAgentWithDesire(float dt, int _conversationDelay, Random rnd, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dialogsDictionary, string _CurrentLocation)
        {
            if (NearEnoughToStartConversation)
            {
                ConversationBetweenCustomAgents(dt, _conversationDelay, _dialogsDictionary, _CurrentLocation);
            }
            else
            {
                CheckDistanceBetweenAgentsToSocialExchange(rnd);
            }
        }

        public void CheckDistanceBetweenAgentsToSocialExchange(Random rnd)
        {
            if (this.Name != Agent.Main.Name && this.customAgentTarget != null)
            {
                if (this.selfAgent.Position.Distance(this.customAgentTarget.selfAgent.Position) < 3)
                {
                    /* Social Exchange */
                    socialExchangeSE = new SocialExchangeSE(SocialMove_SE, this, CustomAgentsList);
                    socialExchangeSE.OnInitialize(rnd);

                    if (socialExchangeSE.ReceptorIsPlayer)
                    {
                        SetBooleanNumber(false, true, false);
                    }
                    else
                    {
                        SetBooleanNumber(false, false, true);
                    }

                    NearEnoughToStartConversation = true;
                }
            }
        }

        public void ConversationBetweenCustomAgents(float dt, int _conversationDelay, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dialogsDictionary, string _CurrentLocation)
        {
            int seconds = socialExchangeSE.ReduceDelay ? 0 : _conversationDelay;

            if (SecsDelay(dt, seconds) || socialExchangeSE.ReceptorIsPlayer)
            {
                socialExchangeSE.OnGoingSocialExchange(_dialogsDictionary, _CurrentLocation);

                if (socialExchangeSE.IsCompleted)
                {
                    if (!socialExchangeSE.ReceptorIsPlayer)
                    {
                        FinalizeSocialExchange();
                        EndingSocialExchange = true;
                    }
                }
            }
        }

        public int booleanNumber;
        public void SetBooleanNumber(bool PlayerToNPC, bool NPCToPlayer, bool NPCToNPC)
        {
            StartingASocialExchange = true;

            booleanNumber = 0;
            if (NPCToPlayer)
            {
                booleanNumber = -1;
            }
            else if (NPCToNPC)
            {
                booleanNumber = 0;
            }
            else if (PlayerToNPC)
            {
                booleanNumber = 1;
            }
        }

        public void FinalizeSocialExchange()
        {
            if (socialExchangeSE != null)
            {
                socialExchangeSE.OnFinalize();
            }
            else
            {
                socialExchangeSE = null;
            }

            NearEnoughToStartConversation = false;

            EnoughRest = false;
        }

        public void AgentGetMessage(bool _isInitiator, CustomAgent customAgentInitiator, CustomAgent customAgentReceptor, Random rnd, int _index, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dialogsDictionary, string _CurrentLocation)
        {
            if (_isInitiator)
            {
                if (FullMessage == null)
                {
                    CustomMessageNPC messageNPC = new CustomMessageNPC(customAgentInitiator.socialExchangeSE, rnd, _isInitiator, this.cultureCode, _dialogsDictionary, _CurrentLocation);

                    FullMessage = messageNPC.MainSocialMove();
                }
            }
            else
            {
                if (FullMessage == null)
                {
                    CustomMessageNPC messageNPC = new CustomMessageNPC(customAgentInitiator.socialExchangeSE, rnd, _isInitiator, this.cultureCode, _dialogsDictionary, _CurrentLocation, customAgentReceptor.SEVolition);
                    FullMessage = messageNPC.MainSocialMove();

                    SE_Accepted = messageNPC.IsAccepted;
                }
            }

            Message = FullMessage.ElementAtOrDefault(_index);
            Message = (Message == null) ? Message = "" : Message;

            MessageBuilderCheck(customAgentInitiator);
        }

        private void MessageBuilderCheck(CustomAgent customAgentInitiator)
        {
            if (Message.Contains("{PERSON}"))
            {
                StringBuilder builder = new StringBuilder(Message);
                builder.Replace("{PERSON}", customAgentInitiator.thirdAgent);
                Message = builder.ToString();
            }

            if (Message.Contains("{PARTNER}"))
            {
                StringBuilder builder = new StringBuilder(Message);
                if (customAgentInitiator.selfAgent.IsFemale)
                {
                    builder.Replace("{PARTNER}", "husband");
                }
                else
                {
                    builder.Replace("{PARTNER}", "wife");
                }
                Message = builder.ToString();
            }

            if (Message.Contains("{GENDER}"))
            {
                StringBuilder builder = new StringBuilder(Message);
                if (customAgentInitiator.customAgentTarget.selfAgent.IsFemale)
                {
                    builder.Replace("{GENDER}", "she");
                }
                else
                {
                    builder.Replace("{GENDER}", "he");
                }
                Message = builder.ToString();
            }

            if (Message.Contains("{SETTLEMENT}"))
            {
                StringBuilder builder = new StringBuilder(Message);
                builder.Replace("{SETTLEMENT}", Hero.MainHero.CurrentSettlement.Name.ToString());
                Message = builder.ToString();
            }

            if (Message.Contains("{ITEM}"))
            {
                StringBuilder builder = new StringBuilder(Message);
                builder.Replace("{ITEM}", customAgentInitiator.ItemList[0].itemName);
                Message = builder.ToString();
            }
        }

        public void UpdateBeliefWithPlayer(SocialNetworkBelief _belief, bool FromCampaing, CustomAgent _customAgent)
        {
            if (FromCampaing)
            {
                SocialNetworkBelief localBelief = _belief;
                LoadDataFromJsonToAgent(Hero.MainHero.CurrentSettlement.Name.ToString(), CampaignMission.Current.Location.StringId);

                UpdateBeliefWithNewValue(localBelief, localBelief.value);
                SaveDataFromAgentToJson(Hero.MainHero.CurrentSettlement.Name.ToString(), CampaignMission.Current.Location.StringId);
            }
        }

        public void SaveDataFromAgentToJson(string _currentSettlement, string _currentLocation)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);
            SettlementJson settlementJson = myDeserializedClass.SettlementJson.Find(s => s.Name == _currentSettlement && s.LocationWithId == _currentLocation);

            if (settlementJson != null)
            {
                CustomAgentJson _customAgentJson = settlementJson.CustomAgentJsonList.Find(c => c.Name == Name && c.Id == Id);

                if (_customAgentJson != null)
                {
                    _customAgentJson.TraitList = TraitList;
                    _customAgentJson.SocialNetworkBeliefs = SocialNetworkBeliefs;
                    _customAgentJson.ItemsList = ItemList;
                }
            }

            File.WriteAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json", JsonConvert.SerializeObject(myDeserializedClass));
        }

        public void LoadDataFromJsonToAgent(string _currentSettlement, string _currentLocation)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Saved/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            SettlementJson settlementJson = myDeserializedClass.SettlementJson.Find(s => s.Name == _currentSettlement && s.LocationWithId == _currentLocation);

            if (settlementJson != null)
            {
                CustomAgentJson _customAgentJson = settlementJson.CustomAgentJsonList.Find(c => c.Name == Name && c.Id == Id);

                if (_customAgentJson != null)
                {
                    TraitList = _customAgentJson.TraitList;
                    SocialNetworkBeliefs = _customAgentJson.SocialNetworkBeliefs;
                    ItemList = _customAgentJson.ItemsList;
                }
            }
        }

        internal void AbortSocialExchange()
        {
            IsInitiator = false;
            SocialMove_SE = CustomMissionNameMarkerVM.SEs_Enum.Undefined;

            if (selfAgent != Agent.Main)
            {
                EndFollowBehavior();
            }
            EndingSocialExchange = true;
        }

        internal int CheckCountdownWithCurrentTraits()
        {
            int countdownChangesTemp = 0;
            foreach (Trait trait in TraitList)
            {
                if (trait.GetincreaseCountdown())
                {
                    countdownChangesTemp += 2;
                }
                if (trait.GetdecreaseCountdown())
                {
                    countdownChangesTemp -= 2;
                }
            }
            return countdownChangesTemp;
        }

        public bool SecsDelay(float dt, float seconds)
        {
            dtControl += dt;
            if (dtControl >= seconds)
            {
                dtControl = 0;
                return true;
            }
            return false;
        }
        private float dtControl;

        public bool StartingASocialExchange;

        public CustomAgent GetCustomAgentByName(string _name, int _id)
        {
            CustomAgent customAgent = null;
            foreach (CustomAgent item in CustomAgentsList)
            {
                if (item.Name == _name && item.Id == _id)
                {
                    customAgent = item;
                    break;
                }
            }

            customAgent = null;
            foreach (var item in CustomAgentsList.Where(item => item.Name == _name && item.Id == _id))
            {
                customAgent = item;
                break;
            }

            return customAgent;
        }

        public void UpdateTarget(string _targetName, int _id)
        {
            Busy = true;

            customAgentTarget = GetCustomAgentByName(_targetName, _id);
            IdTarget = _id;

            StartFollowBehavior(selfAgent, customAgentTarget.selfAgent);
        }

        public void StartFollowBehavior(Agent _agent, Agent _agentTarget)
        {
            _agent.SetLookAgent(_agentTarget);

            if (_agent.GetComponent<CampaignAgentComponent>().AgentNavigator == null)
            {
                return;
            }

            DailyBehaviorGroup behaviorGroup = _agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
            behaviorGroup.AddBehavior<FollowAgentBehavior>().SetTargetAgent(_agentTarget);
            behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
        }

        public void EndFollowBehavior()
        {
            if (selfAgent != Agent.Main)
            {
                selfAgent.ResetLookAgent();

                if (selfAgent.GetComponent<CampaignAgentComponent>().AgentNavigator == null)
                {
                    return;
                }

                DailyBehaviorGroup behaviorGroup = selfAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
                behaviorGroup.RemoveBehavior<FollowAgentBehavior>();
            }
        }

        #region /* Add / Update Beliefs  / Get Beliefs */ 
        public void AddBelief(SocialNetworkBelief belief)
        {
            if (belief != null)
            {
                SocialNetworkBeliefs.Add(new SocialNetworkBelief(belief.relationship, belief.agents, belief.IDs, belief.value));
            }
        }

        public void UpdateBeliefWithNewValue(SocialNetworkBelief belief, float _value)
        {
            int minRange = 0;
            int maxRange = 0;

            if (belief != null)
            {
                SocialNetworkBelief _belief = SocialNetworkBeliefs.Find(b =>
                b.relationship == belief.relationship &&
                belief.agents.Contains(b.agents[0]) &&
                belief.agents.Contains(b.agents[1]) &&
                belief.IDs.Contains(b.IDs[0]) &&
                belief.IDs.Contains(b.IDs[1]));


                if (_belief == null)
                {
                    AddBelief(belief);
                }
                else
                {
                    _belief.value += _value;

                    minRange = -100;
                    maxRange = 100;

                    if (_belief.value >= maxRange)
                    {
                        _belief.value = maxRange;
                    }
                    if (_belief.value <= minRange)
                    {
                        _belief.value = minRange;
                    }
                }
            }
        }

        public void UpdateBeliefWithNewRelation(string _newRelation, SocialNetworkBelief belief)
        {
            //It will check if have that belief
            SocialNetworkBelief _belief = SocialNetworkBeliefs.Find(b =>
            belief.agents.Contains(b.agents[0]) &&
            belief.agents.Contains(b.agents[1]));

            // If belief not found so add it to prevent error
            if (_belief == null)
            {
                AddBelief(belief);
                _belief = belief;
            }

            _belief.relationship = _newRelation;
        }

        public List<SocialNetworkBelief> SelfGetNegativeRelations(string _relation = "")
        {
            return _relation == ""
                ? SocialNetworkBeliefs.FindAll(belief => belief.value < 0)
                : SocialNetworkBeliefs.FindAll(belief => belief.relationship == _relation && belief.value < 0);
        }

        //Get Belief from itself with other
        public SocialNetworkBelief SelfGetBeliefWithAgent(CustomAgent _otherCustomAgent)
        {
            if (SocialNetworkBeliefs == null)
            {
                return null;
            }
            else
            {

                SocialNetworkBelief belief = SocialNetworkBeliefs.Find(b
                    => b.agents.Contains(Name)
                    && b.agents.Contains(_otherCustomAgent.Name)
                    && b.IDs.Contains(Id)
                    && b.IDs.Contains(_otherCustomAgent.Id)
                    );

                return belief;
            }
        }

        //Get Belief between 2 other NPCs
        public SocialNetworkBelief GetBeliefBetween(CustomAgent customAgent1, CustomAgent customAgent2)
        {
            return this.SocialNetworkBeliefs.Find(belief
                => belief.agents.Contains(customAgent1.Name)
                && belief.agents.Contains(customAgent2.Name)
                && belief.IDs.Contains(customAgent1.Id)
                && belief.IDs.Contains(customAgent2.Id)
                );
        }

        public int CheckHowManyTheAgentIsDating(CustomAgent customAgent)
        {
            return customAgent.SocialNetworkBeliefs.Count(
                belief => belief.relationship == "Dating"
                && belief.agents.Contains(customAgent.Name)
                && belief.IDs.Contains(customAgent.Id)
                );
        }

        public void SetBeliefWithNewValue(SocialNetworkBelief belief, float _value)
        {
            if (belief != null)
            {
                if (SocialNetworkBeliefs == null)
                {
                    AddBelief(belief);

                }
                else
                {
                    SocialNetworkBelief _belief = SocialNetworkBeliefs.Find(b =>
                b.relationship == belief.relationship &&
                belief.agents.Contains(b.agents[0]) &&
                belief.agents.Contains(b.agents[1]) &&
                belief.IDs.Contains(b.IDs[0]) &&
                belief.IDs.Contains(b.IDs[1]));

                    if (_belief == null)
                    {
                        AddBelief(belief);
                        belief.value = _value;
                    }
                    else
                    {
                        _belief.value = _value;
                    }
                }
            }
        }
        #endregion

        #region /* Update Status */
        private void UpdateStatus(string statusName, double _value)
        {
            Status status = StatusList.Find(_status => _status.Name == statusName);
            if (status != null)
            {
                status.intensity += _value;
                if (status.intensity <= 0)
                {
                    status.intensity = 0;
                }
                else if (status.intensity >= 10)
                {
                    status.intensity = 10;
                }
            }
        }

        public void UpdateAllStatus(double _SocialTalk = 0, double _BullyNeed = 0, double _Courage = 0, double _Anger = 0, double _Shame = 0, double _Tiredness = 0)
        {
            List<double> tempList = new List<double>() { _SocialTalk, _BullyNeed, _Courage, _Anger, _Shame, _Tiredness };

            for (int i = 0; i < StatusList.Count; i++)
            {
                Status status = StatusList[i];
                double value = tempList[i];

                UpdateStatus(status.Name, value);
            }
        }
        #endregion

        #region /* Add Item / Remove Item */
        public void AddItem(string _itemName, int _quantity = 0)
        {
            Item item = ItemList.Find(i => i.itemName == _itemName);

            if (item == null)
            {
                Item newItem = new Item(_itemName, _quantity);
                ItemList.Add(newItem);
            }
            else
            {
                item.quantity += _quantity;
            }
        }

        public void RemoveItem(string _itemName, int _quantity = 0)
        {
            Item item = ItemList.Find(i => i.itemName == _itemName);

            item.quantity += _quantity;

            if (item.quantity <= 0)
            {
                ItemList.Remove(item);
            }
        }

        public Item GetItem()
        {
            Item item = ItemList[0];
            return item;
        }

        #endregion

        #region /* Play Animation / Stop Animation */
        public void PlayAnimation(string _animation)
        {
            selfAgent.SetActionChannel(0, ActionIndexCache.Create(_animation), true);
        }
        public void StopAnimation()
        {
            selfAgent.SetActionChannel(0, ActionIndexCache.act_none, true);
        }
        #endregion

        public void AddToMemory(MemorySE _newMemory)
        {
            if (MemorySEs.Count >= memorySize)
            {
                MemorySEs.RemoveAt(0);
            }

            MemorySEs.Add(_newMemory);
        }

        public void AddToTriggerRulesList(TriggerRule triggerRule)
        {
            TriggerRuleList.Add(triggerRule);
        }

        public void RemoveTriggerRule(TriggerRule triggerRule)
        {
            TriggerRuleList.Remove(triggerRule);
        }
    }
}