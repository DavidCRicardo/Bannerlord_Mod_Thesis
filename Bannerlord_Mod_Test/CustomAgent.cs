using Newtonsoft.Json;
using SandBox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord_Mod_Test
{
    public class CustomAgent : CampaignBehaviorBase
    {
        public Agent selfAgent;
        public Agent targetAgent; 
        public string Name { get; set; } // ID
        public string message { get; set; } // Output Message
        public string[] FullMessage { get; set; } // Output Full Message
        public string SocialMove { get; set; }
        public bool SE_Accepted { get; set; }
        public int SEVolition { get; set; }
        public List<Status> StatusList { get; set; }
        public List<CustomAgent> customAgentsList { get; set; }
        public List<Trait> TraitList { get; set; }
        public List<Goal> GoalsList { get; set; }
        public List<SocialNetworkBelief> SocialNetworkBeliefs { get; set; }
        public List<Item> ItemList { get; set; }
        public List<MemorySE> MemorySEs { get; set; }
        public List<TriggerRule> TriggerRuleList { get; set; }
        public int Countdown { get; set; }
        public bool NearEnoughToStartConversation { get; set; }
        public bool busy { get; set; } // Has Target to start a social exchange when close? // or it's interacting?
        public bool cooldown { get; set; }
        public bool EnoughRest { get; set; }

        public bool EndingSocialExchange { get; set; }
        public bool IsInitiator { get; set; }
        public SocialExchangeSE socialExchangeSE { get; set; }
        public SocialExchangeSE.IntentionEnum SEIntention { get; set; }
        public CustomAgent(Agent agent, List<string> auxStatusList = null)
        {
            this.selfAgent = agent; // reference to self
            this.Name = "";
            this.message = "";
            this.SocialMove = "";
            this.targetAgent = null;
            this.customAgentsList = new List<CustomAgent>(); // reference to NPCs around 
            this.TraitList = new List<Trait>();
            this.GoalsList = new List<Goal>();
            this.SocialNetworkBeliefs = new List<SocialNetworkBelief>();
            this.ItemList = new List<Item>();
            this.MemorySEs = new List<MemorySE>();
            this.TriggerRuleList = new List<TriggerRule>();
            this.StatusList = new List<Status>();
            this.IsInitiator = false;

            AddStatusToCustomAgent(auxStatusList);
            this.Countdown = SetCountdownToCustomAgent();

            this.busy = false;
            this.cooldown = true;
        }

        private void AddStatusToCustomAgent(List<string> auxStatusList)
        {
            if (auxStatusList != null)
            {
                StatusList.AddRange(auxStatusList.Select(statusName => new Status(statusName)));
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
        public void startSE(string _SEName, string _ReceiverName, int _Volition)
        {       
            UpdateTarget(_ReceiverName);
            //this.selfAgent.SetLookAgent(targetAgent);
            GetCustomAgentByName(targetAgent.Name).busy = true;

            IsInitiator = true;
            SocialMove = _SEName;

            busy = true;
        }
        internal void CustomAgentHasDesire(float dt, string SEName, CustomAgent customAgent, RootMessageJson rootMessageJson, Random rnd, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> megaDictionary)
        {
            if (NearEnoughToStartConversation)
            {
                ConversationBetweenCustomAgents(dt, rootMessageJson, rnd, megaDictionary);
            }
            else
            {
                CheckDistanceBetweenAgentsToSocialExchange(dt, SEName, customAgent, rnd);
            }
        }
        internal void CheckDistanceBetweenAgentsToSocialExchange(float dt, string SEName, CustomAgent customAgent, Random rnd)
        {
            if (customAgent != null && customAgent.Name != Agent.Main.Name && customAgent.targetAgent != null)
            {
                if (customAgent.selfAgent.Position.Distance(customAgent.targetAgent.Position) < 3)
                {
                    /* Social Exchange */
                    socialExchangeSE = new SocialExchangeSE(SEName, customAgent, customAgentsList);
                    socialExchangeSE.OnInitialize(rnd);

                    NearEnoughToStartConversation = true;
                }
            }
        }
        internal void ConversationBetweenCustomAgents(float dt, RootMessageJson rootMessageJson, Random rnd, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> megaDictionary)
        {
            int seconds = socialExchangeSE.ReduceDelay ? 0 : 3;

            if (SecsDelay(dt, seconds) || socialExchangeSE.ReceptorIsPlayer)
            {
                socialExchangeSE.OnGoingSocialExchange(rootMessageJson, megaDictionary);

                if (socialExchangeSE.SocialExchangeDoneAndReacted) 
                {
                    if (!socialExchangeSE.ReceptorIsPlayer)
                    {
                        FinalizeSocialExchange();
                    }
                }
            }
        }

        internal void FinalizeSocialExchange()
        {
            socialExchangeSE.OnFinalize();
            socialExchangeSE = null;

            NearEnoughToStartConversation = false;
            EndingSocialExchange = true;

            IsInitiator = false;
            EnoughRest = false;     
        }
        internal void InitiatorToSocialMove(CustomAgent customAgentInitiator, CustomAgent customAgentReceptor, RootMessageJson rootMessageJson, Random rnd, int _index, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> megaDictionary)
        {
            if (FullMessage == null)
            {
                CultureCode cultureCode = customAgentInitiator.selfAgent.Character.Culture.GetCultureCode();
                CustomMessageNPC messageNPC = new CustomMessageNPC(customAgentInitiator.socialExchangeSE, rnd, true, cultureCode, megaDictionary);

                FullMessage = messageNPC.MainSocialMove();
            }

            message = FullMessage.ElementAtOrDefault(_index);
            message = (message == null) ? message = "" : message;
        }
        internal void ReceiverToSocialMove(CustomAgent customAgentInitiator, CustomAgent customAgentReceptor, RootMessageJson rootMessageJson, Random rnd, int _index, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> megaDictionary)
        {
            if (FullMessage == null)
            {
                CultureCode cultureCode = customAgentReceptor.selfAgent.Character.Culture.GetCultureCode();
                CustomMessageNPC messageNPC = new CustomMessageNPC(customAgentInitiator.socialExchangeSE, rnd, false, cultureCode, megaDictionary, customAgentReceptor.SEVolition);
                FullMessage = messageNPC.MainSocialMove();

                SE_Accepted = messageNPC.IsAccepted;
            }

            message = FullMessage.ElementAtOrDefault(_index);
            message = (message == null) ? message = "" : message;
        }

        public void UpdateBeliefWithPlayer(SocialNetworkBelief _belief, bool FromCampaing, CustomAgent _customAgent)
        {
            if (FromCampaing)
            {
                SocialNetworkBelief localBelief = _belief;
                LoadDataFromJsonToAgent(Hero.MainHero.CurrentSettlement.Name.ToString(), CampaignMission.Current.Location.StringId);

                SocialNetworkBelief belief = GetBelief(_belief.relationship, _customAgent);
                UpdateBelief(localBelief, localBelief.value);
                SaveDataFromAgentToJson(Hero.MainHero.CurrentSettlement.Name.ToString(), CampaignMission.Current.Location.StringId);
            }
        }

        public void SaveDataFromAgentToJson(string _currentSettlement, string _currentLocation)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);
            SettlementJson settlementJson = myDeserializedClass.SettlementJson.Find(s => s.Name == _currentSettlement && s.LocationWithId == _currentLocation);

            if (settlementJson != null)
            {
                CustomAgentJson _customAgentJson = settlementJson.CustomAgentJsonList.Find(c => c.Name == selfAgent.Name);

                if (_customAgentJson != null)
                {
                    _customAgentJson.TraitList = TraitList;
                    _customAgentJson.SocialNetworkBeliefs = SocialNetworkBeliefs;
                    _customAgentJson.GoalsList = GoalsList;
                    _customAgentJson.ItemsList = ItemList;
                }
            }           

            File.WriteAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json", JsonConvert.SerializeObject(myDeserializedClass));
        }
        public void LoadDataFromJsonToAgent(string _currentSettlement, string _currentLocation)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Mod_Test/data.json");
            RootJsonData myDeserializedClass = JsonConvert.DeserializeObject<RootJsonData>(json);

            SettlementJson settlementJson = myDeserializedClass.SettlementJson.Find(s => s.Name == _currentSettlement && s.LocationWithId == _currentLocation);

            if (settlementJson != null)
            {
                CustomAgentJson _customAgentJson = settlementJson.CustomAgentJsonList.Find(c => c.Name == selfAgent.Name);

                if (_customAgentJson != null)
                {
                    TraitList = _customAgentJson.TraitList;
                    SocialNetworkBeliefs = _customAgentJson.SocialNetworkBeliefs;
                    GoalsList = _customAgentJson.GoalsList;
                    ItemList = _customAgentJson.ItemsList;
                }
            }
        }
        internal void AbortSocialExchange()
        {
            targetAgent = null;
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

        private Agent GetAgentByName(string name)
        {
            Agent agent = null;
            foreach (Agent item in Mission.Current.Agents)
            {
                if (item.Name == name) { agent = item; break; }
            }
            return agent;
        }     
        private CustomAgent GetCustomAgentByName(string name)
        {
            CustomAgent customAgent = null;
            foreach (CustomAgent item in customAgentsList)
            {
                if (item.Name == name) { customAgent = item; break; }
            }
            return customAgent;
        }
        public void UpdateTarget(string _targetName)
        {
            busy = true;
            targetAgent = GetAgentByName(_targetName);
            StartFollowBehavior(selfAgent, targetAgent);
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
        #region /* Add / Update Beliefs */ 
        public void AddBelief(SocialNetworkBelief belief)
        {
            SocialNetworkBeliefs.Add(new SocialNetworkBelief(belief.relationship, belief.agents, belief.value));
        }
        public void UpdateBelief(SocialNetworkBelief belief, int _value)
        {
            SocialNetworkBelief _belief = SocialNetworkBeliefs.Find(b => b.relationship == belief.relationship && belief.agents.Contains(b.agents[0]) && belief.agents.Contains(b.agents[1]));

                if (_belief == null)
                {
                    AddBelief(belief);
                    _belief = belief;
                }
                else
                {
                    _belief.value += _value;

                    if (_belief.value >= 10)
                    {
                        _belief.value = 10;
                    }
                    if (_belief.value <= 0)
                    {
                        _belief.value = 0;
                    }
                }
        }
        public SocialNetworkBelief GetBelief(string relation, CustomAgent _otherCustomAgent)
        {
            return this.SocialNetworkBeliefs.Find
                (b => b.relationship == relation 
                && b.agents.Contains(Name)
                && b.agents.Contains(_otherCustomAgent.Name)
                );
        }
        public SocialNetworkBelief GetBeliefFrom(CustomAgent customAgent1, CustomAgent customAgent2, string relation)
        {
            return this.SocialNetworkBeliefs.Find
                (b => b.relationship == relation 
                && b.agents.Contains(customAgent1.Name)
                && b.agents.Contains(customAgent2.Name)
                );
        }

        public SocialNetworkBelief CheckIfAgentIsDatingWithAnyone(CustomAgent customAgentToCheck)
        {
            return this.SocialNetworkBeliefs.Find
                (b => b.relationship == "Dating"
                && b.agents.Contains(customAgentToCheck.Name)
                && b.value > 0);
        }

        public void Change(string _newRelation, SocialNetworkBelief belief)
        {
            belief.relationship = _newRelation;
        }

        #endregion
        #region /* Add / Update / Remove Goals */
        public void AddGoal(string _relationship, string _target, int _value)
        {
            GoalsList.Add(new Goal(_relationship, _target, _value));
        }
        public void UpdateGoal(string _relationship, string _target, int _value)
        {
            var a = GoalsList.Find(g => g.relationship == _relationship && g.targetName == _target);
            if (a != null) { a.value += _value; }
        }
        public void RemoveGoal(string _relationship, string _target)
        {
            var r = GoalsList.Find(g => g.relationship == _relationship && g.targetName == _target);
            GoalsList.Remove(r);
        }
        #endregion
        #region /* Update Status */
        public void UpdateStatus(string statusName, double _value)
        {
            Status status = StatusList.Find(s => s.statusName == statusName);
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

        public bool HasRelationWith(string relation, CustomAgent customAgentReceiver)
        {
            return this.SocialNetworkBeliefs.Exists
                (b => b.relationship == relation
                && b.agents.Contains(Name)
                && b.agents.Contains(customAgentReceiver.Name)
                && b.value > 0);
        }

        public void AddToMemory(MemorySE _newMemory)
        {
            MemorySEs.Clear();
            MemorySEs.Add(_newMemory);
        }
        public void AddToTriggerRulesList(TriggerRule triggerRule)
        {
            TriggerRuleList.Add(triggerRule);
        }
        public void RunTriggerRules()
        {
            foreach (TriggerRule TRule in TriggerRuleList)
            {
                CheckRule(TRule);
                break;
            }
        }
        private void CheckRule(TriggerRule TRule)
        {
            foreach (SocialNetworkBelief belief in SocialNetworkBeliefs)
            {
                if (belief.relationship == TRule.RelationshipName
                    && TRule.NPCsOnRule == belief.agents
                    && TRule.Value <= belief.value)
                {
                    InformationManager.DisplayMessage(new InformationMessage("."));

                    break;
                }
            }
        }
    }
}