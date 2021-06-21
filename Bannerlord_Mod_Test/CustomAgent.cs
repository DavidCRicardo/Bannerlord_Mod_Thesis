using Newtonsoft.Json;
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

namespace Bannerlord_Mod_Test
{
    public class CustomAgent : CampaignBehaviorBase
    {
        public Agent selfAgent;
        public Agent targetAgent;
        public CustomAgent customTargetAgent;
        public string Name { get; set; } 
        public int Id { get; set; }
        public int idTarget { get; set; }
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
        private int memorySize = 3;
        public CustomAgent(Agent agent, int _id, List<string> auxStatusList = null)
        {
            this.selfAgent = agent; // reference to self
            this.Id = _id;
            this.idTarget = -1;
            this.Name = "";
            this.message = "";
            this.SocialMove = "";
            this.customTargetAgent = null;
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
        public void startSE(string _SEName, CustomAgent _Receiver/* string _ReceiverName, int _ReceiverId*/)
        {
            UpdateTarget(_Receiver.Name, _Receiver.Id);
            //this.selfAgent.SetLookAgent(targetAgent);

            customTargetAgent.busy = true;

            IsInitiator = true;
            SocialMove = _SEName;

            busy = true;
        }
        internal void CustomAgentHasDesire(float dt, string SEName, CustomAgent customAgent, Random rnd, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> megaDictionary)
        {
            if (NearEnoughToStartConversation)
            {
                ConversationBetweenCustomAgents(dt, megaDictionary);
            }
            else
            {
                CheckDistanceBetweenAgentsToSocialExchange(SEName, customAgent, rnd);
            }
        }
        internal void CheckDistanceBetweenAgentsToSocialExchange(string SEName, CustomAgent customAgent, Random rnd)
        {
            if (customAgent != null && customAgent.Name != Agent.Main.Name && customAgent.customTargetAgent != null)
            {
                InformationManager.DisplayMessage(new InformationMessage(customAgent.selfAgent.Position.Distance(customAgent.customTargetAgent.selfAgent.Position).ToString()));
                
                if (customAgent.selfAgent.Position.Distance(customAgent.customTargetAgent.selfAgent.Position) < 3)
                {
                    /* Social Exchange */
                    socialExchangeSE = new SocialExchangeSE(SEName, customAgent, customAgentsList);
                    socialExchangeSE.OnInitialize(rnd);

                    NearEnoughToStartConversation = true;
                }
            }
        }
        internal void ConversationBetweenCustomAgents(float dt, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> megaDictionary)
        {
            int seconds = socialExchangeSE.ReduceDelay ? 0 : 3;

            if (SecsDelay(dt, seconds) || socialExchangeSE.ReceptorIsPlayer)
            {
                socialExchangeSE.OnGoingSocialExchange(megaDictionary);

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

            EnoughRest = false;
        }
        internal void AgentGetMessage(bool _isInitiator, CustomAgent customAgentInitiator, CustomAgent customAgentReceptor, Random rnd, int _index, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> megaDictionary)
        {
            if (_isInitiator)
            {
                if (FullMessage == null)
                {
                    CultureCode cultureCode = customAgentInitiator.selfAgent.Character.Culture.GetCultureCode();
                    CustomMessageNPC messageNPC = new CustomMessageNPC(customAgentInitiator.socialExchangeSE, rnd, _isInitiator, cultureCode, megaDictionary);

                    FullMessage = messageNPC.MainSocialMove();
                }
            }
            else 
            {
                if (FullMessage == null)
                {
                    CultureCode cultureCode = customAgentReceptor.selfAgent.Character.Culture.GetCultureCode();
                    CustomMessageNPC messageNPC = new CustomMessageNPC(customAgentInitiator.socialExchangeSE, rnd, _isInitiator, cultureCode, megaDictionary, customAgentReceptor.SEVolition);
                    FullMessage = messageNPC.MainSocialMove();

                    SE_Accepted = messageNPC.IsAccepted;
                }
            }

            message = FullMessage.ElementAtOrDefault(_index);
            message = (message == null) ? message = "" : message;

            if (message.Contains("{PERSON}"))
            {
                StringBuilder builder = new StringBuilder(message);
                builder.Replace("{PERSON}", customAgentInitiator.thirdAgent);
                message = builder.ToString();
            }

            if (message.Contains("{PARTNER}"))
            {
                StringBuilder builder = new StringBuilder(message);
                if (customAgentInitiator.selfAgent.IsFemale)
                {
                    builder.Replace("{PARTNER}", "husband");
                }
                else
                {
                    builder.Replace("{PARTNER}", "wife");
                }
                message = builder.ToString();
            }
        }

        public void UpdateBeliefWithPlayer(SocialNetworkBelief _belief, bool FromCampaing, CustomAgent _customAgent)
        {
            if (FromCampaing)
            {
                SocialNetworkBelief localBelief = _belief;
                LoadDataFromJsonToAgent(Hero.MainHero.CurrentSettlement.Name.ToString(), CampaignMission.Current.Location.StringId);

                //SocialNetworkBelief belief = SelfGetBeliefWithAgent(_belief.relationship, _customAgent);
                UpdateBeliefWithNewValue(localBelief, localBelief.value);
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
        public string thirdAgent;
        public int thirdAgentId;

        public CustomAgent GetCustomAgentByName(string name, int _id)
        {
            CustomAgent customAgent = null;
            foreach (CustomAgent item in customAgentsList)
            {
                if (item.Name == name && item.Id == _id) { customAgent = item; break; }
            }
            return customAgent;
        }
        public void UpdateTarget(string _targetName, int _id)
        {
            busy = true;
            
            //targetAgent = GetCustomAgentByName(_targetName).selfAgent;
            customTargetAgent = GetCustomAgentByName(_targetName, _id);

            //StartFollowBehavior(selfAgent, targetAgent);
            StartFollowBehavior(selfAgent, customTargetAgent.selfAgent);
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
            SocialNetworkBelief _belief = SocialNetworkBeliefs.Find(
                b => b.relationship == belief.relationship
                && belief.agents.Contains(b.agents[0]) && belief.agents.Contains(b.agents[1])
                && belief.IDs.Contains(b.IDs[0]) && belief.IDs.Contains(b.IDs[1]));

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
                if (_belief.value <= -10)
                {
                    _belief.value = -10;
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

            //Get Belief Index
            //int tempBeliefIndex = SocialNetworkBeliefs.IndexOf(belief);

            //Change RelationName
            _belief.relationship = _newRelation;

            //Add the new belief on the same index to overrride 
            //SocialNetworkBeliefs.Insert(tempBeliefIndex, belief);
        }
        public List<SocialNetworkBelief> SelfGetNegativeRelations(string _relation = "")
        {
            return _relation == ""
                ? SocialNetworkBeliefs.FindAll(b => b.value < 0)
                : SocialNetworkBeliefs.FindAll(b => b.relationship == _relation && b.value < 0);
        }
        //Get Belief from itself with other
        public SocialNetworkBelief SelfGetBeliefWithAgent(CustomAgent _otherCustomAgent)
        {
            return 
                SocialNetworkBeliefs.Find(b => b.agents.Contains(Name) && b.agents.Contains(_otherCustomAgent.Name) 
                                         && b.IDs.Contains(Id) && b.IDs.Contains(_otherCustomAgent.Id));
        }
        //Get Belief between 2 other NPCs
        public SocialNetworkBelief GetBeliefBetween(CustomAgent customAgent1, CustomAgent customAgent2)
        {
            return this.SocialNetworkBeliefs.Find
                (b => b.agents.Contains(customAgent1.Name) && b.agents.Contains(customAgent2.Name)
                && b.IDs.Contains(customAgent1.Id) && b.IDs.Contains(customAgent2.Id)
                );
        }
        public int CheckHowManyTheAgentIsDating(CustomAgent customAgent)
        {
            return customAgent.SocialNetworkBeliefs.Count(
                b => b.relationship == "Dating"
                && b.agents.Contains(customAgent.Name)
                && b.IDs.Contains(customAgent.Id)
                );
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
        //public void RunTriggerRules()
        //{
        //    foreach (TriggerRule TRule in TriggerRuleList)
        //    {
        //        CheckRule(TRule);
        //        break;
        //    }
        //}
        //private void CheckRule(TriggerRule TRule)
        //{
        //    foreach (SocialNetworkBelief belief in SocialNetworkBeliefs)
        //    {
        //        if (belief.relationship == TRule.RelationshipName
        //            && TRule.NPCsOnRule == belief.agents
        //            && TRule.Value <= belief.value)
        //        {
        //            InformationManager.DisplayMessage(new InformationMessage("."));

        //            break;
        //        }
        //    }
        //}
    }
}