﻿using Newtonsoft.Json;
using SandBox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord_Social_AI
{
    public class CustomAgent : CampaignBehaviorBase
    {
        public Agent selfAgent; // reference to self
        public int Id { get; set; }

        public CultureCode cultureCode { get; set; }
        public List<CultureCode> CulturesFriendly { get; private set; }
        public List<CultureCode> CulturesUnFriendly { get; private set; }

        public CustomAgent customAgentTarget;
        public int IdTarget { get; set; }

        public string[] FullMessage { get; set; } // Output Full Message
        public string Message { get; set; } // Output Message
        public string SocialMove { get; set; }
        public bool SE_Accepted { get; set; }
        public int SEVolition { get; set; }
        public List<CustomAgent> CustomAgentsList { get; set; } // reference to NPCs around 

        public SocialExchangeSE socialExchangeSE { get; set; }

        public List<Trait> TraitList { get; set; }
        public List<SocialNetworkBelief> SocialNetworkBeliefs { get; set; }
        public List<Item> ItemList { get; set; }
        public List<MemorySE> MemorySEs { get; set; }
        public List<TriggerRule> TriggerRuleList { get; set; }
        public List<Status> StatusList { get; set; }

        public bool NearEnoughToStartConversation { get; set; }
        public bool EnoughRest { get; set; } // is not in cooldown? // enough rest?
        public bool Busy { get; set; } // Has Target to start a social exchange when close? // or it's interacting?
        public int Countdown { get; set; } // How much NPC needs to wait to set cooldown as false

        public bool EndingSocialExchange { get; set; }
        public bool IsInitiator { get; set; }
        public bool NearPlayer { get; set; }
        public int MarkerTyperRef { get; set; }

        private readonly int memorySize = 3;
        public string thirdAgent;
        public int thirdAgentId;
        public string Name { get; set; }

        public CustomAgent(Agent _agent, int _id, List<string> _statusList = null)
        {
            this.selfAgent = _agent;
            this.Name = _agent.Name;
            this.Id = _id;

            SetCultureCodeInfo(_agent);

            this.customAgentTarget = null;
            this.IdTarget = -1;

            this.Message = "";
            this.SocialMove = "";
            this.CustomAgentsList = new List<CustomAgent>();

            this.TraitList = new List<Trait>();
            this.SocialNetworkBeliefs = new List<SocialNetworkBelief>();
            this.ItemList = new List<Item>();
            this.MemorySEs = new List<MemorySE>();
            this.TriggerRuleList = new List<TriggerRule>();
            this.StatusList = new List<Status>();

            this.IsInitiator = false;
            this.NearPlayer = false;
            this.MarkerTyperRef = 1;

            AddStatusToCustomAgent(_statusList);
            this.Countdown = SetCountdownToCustomAgent();

            this.Busy = false;
            this.EnoughRest = false;
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

        public void StartSE(string _SEName, CustomAgent _Receiver)
        {
            UpdateTarget(_Receiver.Name, _Receiver.Id);
            //this.selfAgent.SetLookAgent(targetAgent);

            customAgentTarget.Busy = true;

            IsInitiator = true;
            SocialMove = _SEName;

            Busy = true;
        }
        public void CustomAgentWithDesire(float dt, Random rnd, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dialogsDictionary)
        {
            if (NearEnoughToStartConversation)
            {
                ConversationBetweenCustomAgents(dt, _dialogsDictionary);
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
                    socialExchangeSE = new SocialExchangeSE(SocialMove, this, CustomAgentsList);
                    socialExchangeSE.OnInitialize(rnd);

                    NearEnoughToStartConversation = true;
                }
            }
        }

        public void ConversationBetweenCustomAgents(float dt, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dialogsDictionary)
        {
            int seconds = socialExchangeSE.ReduceDelay ? 0 : 3;

            if (SecsDelay(dt, seconds) || socialExchangeSE.ReceptorIsPlayer)
            {
                socialExchangeSE.OnGoingSocialExchange(_dialogsDictionary);

                if (socialExchangeSE.SocialExchangeDoneAndReacted)
                {
                    if (!socialExchangeSE.ReceptorIsPlayer)
                    {
                        FinalizeSocialExchange();
                    }
                }
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
            EndingSocialExchange = true;

            EnoughRest = false;

        }

        public void AgentGetMessage(bool _isInitiator, CustomAgent customAgentInitiator, CustomAgent customAgentReceptor, Random rnd, int _index, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dialogsDictionary)
        {
            if (_isInitiator)
            {
                if (FullMessage == null)
                {
                    CustomMessageNPC messageNPC = new CustomMessageNPC(customAgentInitiator.socialExchangeSE, rnd, _isInitiator, this.cultureCode, _dialogsDictionary);

                    FullMessage = messageNPC.MainSocialMove();
                }
            }
            else
            {
                if (FullMessage == null)
                {
                    CustomMessageNPC messageNPC = new CustomMessageNPC(customAgentInitiator.socialExchangeSE, rnd, _isInitiator, this.cultureCode, _dialogsDictionary, customAgentReceptor.SEVolition);
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
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/data.json");
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

            File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/data.json", JsonConvert.SerializeObject(myDeserializedClass));
        }

        public void LoadDataFromJsonToAgent(string _currentSettlement, string _currentLocation)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/data.json");
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
            SocialMove = "";
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

            StartFollowBehavior(selfAgent, customAgentTarget.selfAgent);
        }

        public void StartFollowBehavior(Agent _agent, Agent _agentTarget)
        {
            _agent.SetLookAgent(_agentTarget);

            DailyBehaviorGroup behaviorGroup = _agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
            behaviorGroup.AddBehavior<FollowAgentBehavior>().SetTargetAgent(_agentTarget);
            behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
        }

        public void EndFollowBehavior()
        {
            selfAgent.ResetLookAgent();

            DailyBehaviorGroup behaviorGroup = selfAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
            behaviorGroup.RemoveBehavior<FollowAgentBehavior>();
        }

        #region /* Add / Update Beliefs  / Get Beliefs */ 
        public void AddBelief(SocialNetworkBelief belief)
        {
            SocialNetworkBeliefs.Add(new SocialNetworkBelief(belief.relationship, belief.agents, belief.IDs, belief.value));
        }

        public void UpdateBeliefWithNewValue(SocialNetworkBelief belief, int _value)
        {
            SocialNetworkBelief _belief = belief;
            if (belief != null)
            {
                _belief = SocialNetworkBeliefs.Find(b => b.relationship == belief.relationship
               && belief.agents.Contains(b.agents[0]) && belief.agents.Contains(b.agents[1])
               && belief.IDs.Contains(b.IDs[0]) && belief.IDs.Contains(b.IDs[1]));
            }

            if (_belief == null)
            {
                AddBelief(belief);
                _belief = belief;
            }
            else
            {
                _belief.value += _value;

                if (_belief.value >= 5)
                {
                    _belief.value = 5;
                }
                if (_belief.value <= -5)
                {
                    _belief.value = -5;
                }
            }
        }

        public void UpdateBeliefWithNewRelation(string _newRelation, SocialNetworkBelief belief)
        {
            //It will check if have that belief
            SocialNetworkBelief _belief = SocialNetworkBeliefs.Find(
                b => belief.agents.Contains(b.agents[0])
                && belief.agents.Contains(b.agents[1]));

            // If belief not found so add it to prevent error
            if (_belief == null)
            {
                AddBelief(belief);
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
            return this.SocialNetworkBeliefs.Find(belief
                => belief.agents.Contains(Name)
                && belief.agents.Contains(_otherCustomAgent.Name)
                && belief.IDs.Contains(Id)
                && belief.IDs.Contains(_otherCustomAgent.Id)
                );
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

        #endregion

        #region /* Update Status */
        private void UpdateStatus(string statusName, double _value)
        {
            Status status = StatusList.Find(_status => _status.Name == statusName);
            if (status != null)
            {
                status.intensity += _value;
                if (status.intensity < 0)
                {
                    status.intensity = 0;
                }
                else if (status.intensity > 1)
                {
                    status.intensity = 1;
                }
            }
        }

        public void UpdateAllStatus(double _SocialTalk = 0, double _Courage = 0, double _Anger = 0, double _Shame = 0, double _Tiredness = 0)
        {
            List<double> tempList = new List<double>() { _SocialTalk, _Courage, _Anger, _Shame, _Tiredness };

            for (int i = 0; i < StatusList.Count; i++)
            {
                Status status = StatusList[i];
                double value = tempList[i];

                UpdateStatus(status.Name, value);
            }
        }
        #endregion

        #region /* Add Item */
        public void AddItem(string _itemName, int _quantity)
        {
            Item r = new Item(_itemName, _quantity);
            ItemList.Add(r);
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

    }
}