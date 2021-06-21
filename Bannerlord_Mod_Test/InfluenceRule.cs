using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;

namespace Bannerlord_Mod_Test
{
    class InfluenceRule
    {
        public InfluenceRule(CustomAgent c1, CustomAgent c2, bool reacting, int initialValue = 0)
        {
            Initiator = c1;
            Receiver = c2;
            InitialValue = initialValue;
            IsReacting = reacting;
        }
        public int SRunRules() // (IsReacting || !IsReacting)
        {
                switch (RelationType)
                {
                    case SocialExchangeSE.IntentionEnum.Positive:
                        if (RelationName == "Compliment")
                        {
                            return RunRulesCompliment();
                        }
                        else { return 0; }

                    case SocialExchangeSE.IntentionEnum.Negative:
                        if (RelationName == "Jealous")
                        {//Insult
                            return RunRulesJealous();
                        }
                        else if (RelationName == "FriendSabotage")
                        {
                            return RunRulesFriendSabotage();
                        }
                        else { return 0; }

                    case SocialExchangeSE.IntentionEnum.Romantic:
                        if (RelationName == "Flirt")
                        {
                            return RunRulesFlirt();
                        }
                        else if (RelationName == "AskOut")
                        {
                            return RunRulesAskOut();
                        }
                        else { return 0; }

                    case SocialExchangeSE.IntentionEnum.Hostile:
                        if (RelationName == "Bully")
                        {
                            return RunRulesHostile();
                        }
                        else if (RelationName == "RomanticSabotage")
                        {//Jealous
                            return RunRulesRomanticSabotage();
                        }
                        else { return 0; }
                    case SocialExchangeSE.IntentionEnum.Special:
                        if (RelationName == "Break")
                        {
                            return RunRulesBreakUp();
                        }
                        else { return 0; }
                    default:
                        return 0;
                }
        }

        //Positive SE
        private int RunRulesCompliment()
        {
            int sum = 0;
            sum += (InitialValue > 0) ? InitialValue : InitialValue * -1;

            #region /* Check Traits */
            Dictionary<String, Func<CustomAgent, int>> TraitFunc_Dictionary = new Dictionary<string, Func<CustomAgent, int>>{
                { "Friendly"  , agent =>  2 },
                { "Hostile"   , agent => -2 },
                { "Charming"  , agent =>  0 },
                { "UnCharming", agent =>  0 },
                { "Shy"       , agent => -2 },
                { "Brave"     , agent =>  2 },
                { "Calm"      , agent =>  2 },
                { "Aggressive", agent => -2 },
                { "Faithful"  , agent =>  0 },
                { "Unfaithful", agent =>  0 }
            };

            if (!IsReacting)
            {
                sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Initiator);
                    }

                    return acc;
                });
            }
            else
            {
                sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Receiver);
                    }

                    return acc;
                });
            }
            #endregion /* End Check Traits */

            /* Check Status */
            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);

            return sum;
        }
        //Romantic SE
        private int RunRulesFlirt()
        {
            int sum = 0;
            sum += (InitialValue > 0) ? InitialValue : InitialValue * -1;

            #region /* Check Traits */
            Dictionary<String, Func<CustomAgent, int>> TraitFunc_Dictionary = new Dictionary<string, Func<CustomAgent, int>>{
                { "Friendly"  , agent =>  0 },
                { "Hostile"   , agent =>  0 },
                { "Charming"  , agent =>  2 },
                { "UnCharming", agent => -2 },
                { "Shy"       , agent => -2 },
                { "Brave"     , agent =>  2 },
                { "Calm"      , agent =>  0 },
                { "Aggressive", agent =>  0 },
                { "Faithful"  , agent => (agent == Initiator) ? CheckFaithful(Initiator, Receiver) : CheckFaithful(Initiator, Receiver) },
                { "Unfaithful", agent =>  2 }
            };
            if (!IsReacting)
            {
                sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Initiator);
                    }

                    return acc;
                });
            }
            else
            {
                sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Receiver);
                    }

                    return acc;
                });
            }
            #endregion

            /* Check Status */
            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);
            /* End Check Status */

            /* Check Extra Rules */
            if ((Initiator.selfAgent.IsFemale && Receiver.selfAgent.IsFemale)
                ||
                (!Initiator.selfAgent.IsFemale && !Receiver.selfAgent.IsFemale))
            {
                sum -= 10;
            }

            //If not dating with the receiver, so decrease drastically the sum to not Flirt because it has noone to flirt
            SocialNetworkBelief socialNetworkBelief = Initiator.SelfGetBeliefWithAgent(Receiver);
            if (socialNetworkBelief == null || socialNetworkBelief.relationship != "Dating")
            {
                sum -= 100;
            }
            /* Extra Rules */

            return sum;
        }
        private int RunRulesAskOut()
        {
            int sum = 0;
            sum += (InitialValue > 0) ? InitialValue : InitialValue * -1;

            /* Check Initiator & Receiver Traits */
            Dictionary<String, Func<CustomAgent, int>> TraitFunc_Dictionary = new Dictionary<string, Func<CustomAgent, int>>{
                { "Friendly"  , agent =>  0 },
                { "Hostile"   , agent =>  0 },
                { "Charming"  , agent =>  2 },
                { "UnCharming", agent => -2 },
                { "Shy"       , agent => -2 },
                { "Brave"     , agent =>  2 },
                { "Calm"      , agent =>  0 },
                { "Aggressive", agent =>  0 },
                { "Faithful"  , agent => (agent == Initiator) ? CheckFaithful(Initiator, Receiver) : CheckFaithful(Receiver, Initiator) },
                { "Unfaithful", agent =>  2 }
            };
            if (!IsReacting)
            {
                sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Initiator);
                    }

                    return acc;
                });
            }
            else
            {
                sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Receiver);
                    }

                    return acc;
                });
            }

            /* Check Status */
            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);
            /* End Check Status */

            if ((Initiator.selfAgent.IsFemale && Receiver.selfAgent.IsFemale)
                ||
                (!Initiator.selfAgent.IsFemale && !Receiver.selfAgent.IsFemale))
            {
                sum -= 10;
            }

            #region Check Condition to AskOut
            //If dating already with the receiver, so decrease drastically the sum to not AskOut again
            SocialNetworkBelief socialNetworkBelief = Initiator.SelfGetBeliefWithAgent(Receiver);
            if (socialNetworkBelief != null && socialNetworkBelief.relationship == "Dating")
            {
                sum -= 100;
            }
            #endregion

            return sum;
        }
        //Negative SE
        private int RunRulesJealous()
        {
            int sum = 0;
            sum += (InitialValue > 0) ? InitialValue : InitialValue * -1;

            /* Check Traits */
            Dictionary<String, Func<CustomAgent, int>> TraitFunc_Dictionary = new Dictionary<string, Func<CustomAgent, int>>{
                { "Friendly"  , agent => -2 },
                { "Hostile"   , agent =>  2 },
                { "Charming"  , agent =>  0 },
                { "UnCharming", agent =>  0 },
                { "Shy"       , agent => -2 },
                { "Brave"     , agent =>  2 },
                { "Calm"      , agent => -2 },
                { "Aggressive", agent =>  2 },
                { "Faithful"  , agent =>  0 },
                { "Unfaithful", agent =>  0 }
            };

            if (!IsReacting)
            {
                sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Initiator);
                    }

                    return acc;
                });
            }
            else
            {
                sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Receiver);
                    }

                    return acc;
                });
            }

            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);

            return sum;
        }
        private int RunRulesFriendSabotage()
        {
            int sum = 0;
            sum += (InitialValue > 0) ? InitialValue : InitialValue * -1; 

            /* Check Traits */
            Dictionary<String, Func<CustomAgent, int>> TraitFunc_Dictionary = new Dictionary<string, Func<CustomAgent, int>>{
                { "Friendly"  , agent => -2 },
                { "Hostile"   , agent =>  2 },
                { "Charming"  , agent =>  0 },
                { "UnCharming", agent =>  0 },
                { "Shy"       , agent => -2 },
                { "Brave"     , agent =>  2 },
                { "Calm"      , agent => -2 },
                { "Aggressive", agent =>  2 },
                { "Faithful"  , agent =>  0 },
                { "Unfaithful", agent =>  0 }
            };

            if (!IsReacting)
            {
                sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Initiator);
                    }

                    return acc;
                });
            }
            else
            {
                sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Receiver);
                    }

                    return acc;
                });
            }

            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);

            List<SocialNetworkBelief> tempList = Initiator.SelfGetNegativeRelations();
            if (tempList != null && tempList.Count > 0)
            {
                Random rnd = new Random();
                int index = rnd.Next(tempList.Count);
                // get one randomly to sabotage
                // get the name of the agent with the negative relation
                // it will skip if the agent will be the same as Initiator
                // it will catch the other agent who the initiator has the negative relation
                List<string> agentsOnRelation = tempList[index].agents;
                //List<int> agentsIdOnRelation = tempList[index].agents;

                if (agentsOnRelation.Contains(Initiator.Name))
                {
                    foreach (var agent in agentsOnRelation.Where(agent => agent != Initiator.Name))
                    {
                        //char delimiterChar = ' ';
                        //string[] sentences = agent.Split(delimiterChar);
                        //Initiator.thirdAgent = sentences[0];

                        Initiator.thirdAgent = agent;
                        Initiator.thirdAgentId = index;

                        sum += 2;
                    }
                }
            }
            else
            {
                sum -= 100;
            }

            return sum;
        }
        // Go talk with someone if that someone talked with his wife/husband
        private int RunRulesRomanticSabotage()
        {
            int sum = 0;
            sum += (InitialValue > 0) ? InitialValue : InitialValue * -1;

            /* Check Traits */
            Dictionary<String, Func<CustomAgent, int>> TraitFunc_Dictionary = new Dictionary<string, Func<CustomAgent, int>>{
                { "Friendly"  , agent => -2 },
                { "Hostile"   , agent =>  2 },
                { "Charming"  , agent =>  0 },
                { "UnCharming", agent =>  0 },
                { "Shy"       , agent => -2 },
                { "Brave"     , agent =>  2 },
                { "Calm"      , agent => -2 },
                { "Aggressive", agent =>  2 },
                { "Faithful"  , agent =>  0 },
                { "Unfaithful", agent =>  0 }
            };

            if (!IsReacting)
            {
                sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Initiator);
                    }

                    return acc;
                });
            }
            else
            {
                sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Receiver);
                    }

                    return acc;
                });
            }

            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);

            return sum;
        }
        //Hostile SE
        private int RunRulesHostile()
        {
            int sum = 0;
            sum += (InitialValue > 0) ? InitialValue : InitialValue * -1;

            /* Check Traits */
            Dictionary<String, Func<CustomAgent, int>> TraitFunc_Dictionary = new Dictionary<string, Func<CustomAgent, int>>{
                { "Friendly"  , agent => -2 },
                { "Hostile"   , agent =>  2 },
                { "Charming"  , agent =>  0 },
                { "UnCharming", agent =>  0 },
                { "Shy"       , agent => -2 },
                { "Brave"     , agent =>  2 },
                { "Calm"      , agent => -2 },
                { "Aggressive", agent =>  2 },
                { "Faithful"  , agent =>  0 },
                { "Unfaithful", agent =>  0 }
            };
            if (!IsReacting)
            {
                sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Initiator);
                    }

                    return acc;
                });
            }
            else
            {
                sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Receiver);
                    }

                    return acc;
                });
            }

            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);
           
            return sum;
        }
        //Special SE
        private int RunRulesBreakUp()
        {
            int sum = 0;
            sum += (InitialValue > 0) ? InitialValue : InitialValue * -1;

            /* Check Traits */
            Dictionary<String, Func<CustomAgent, int>> TraitFunc_Dictionary = new Dictionary<string, Func<CustomAgent, int>>{
                { "Friendly"  , agent => -2 },
                { "Hostile"   , agent =>  2 },
                { "Charming"  , agent =>  0 },
                { "UnCharming", agent =>  0 },
                { "Shy"       , agent => -2 },
                { "Brave"     , agent =>  2 },
                { "Calm"      , agent => -2 },
                { "Aggressive", agent =>  2 },
                { "Faithful"  , agent =>  0 },
                { "Unfaithful", agent =>  0 }
            };
            if (!IsReacting)
            {
                sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Initiator);
                    }

                    return acc;
                });
            }
            else
            {
                sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Receiver);
                    }

                    return acc;
                });
            }

            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);

            //It will check when the value is 0 about dating with the receiver before break up
            SocialNetworkBelief belief = Initiator.SelfGetBeliefWithAgent(Receiver);
            if (belief != null && belief.relationship == "Dating" && belief.value < 1)
            {
                sum += 100;
            }

            return sum;
        }

        internal int CheckInitiatorTriggerRules(CustomAgent agentWhoWillCheck, CustomAgent agentChecked, string relationName)
        {
            if (!agentWhoWillCheck.TriggerRuleList.IsEmpty())
            {
                TriggerRule triggerRule = agentWhoWillCheck.TriggerRuleList.Find(t => t.NPC_OnRule == agentChecked.Name && t.SocialExchangeToDo == relationName);
                if (triggerRule != null)
                {
                    return 100;
                }
            }
            
            return 0;
        }
        internal int CheckGoals(string _relation)
        {
            if (!Initiator.GoalsList.IsEmpty())
            {
                foreach (var _goal in Initiator.GoalsList)
                {
                    if (_goal.relationship == _relation && _goal.targetName == Receiver.Name)
                    {
                        /* Belief = Null? So Add Belief to check if belief value < goal value */
                        SocialNetworkBelief belief = Initiator.SelfGetBeliefWithAgent(Receiver);
                        if (belief == null)
                        {
                            List<string> a = new List<string>() { Initiator.Name, Receiver.Name };
                            List<int> b = new List<int>() { Initiator.Id, Receiver.Id };
                            SocialNetworkBelief newBelief = new SocialNetworkBelief(_relation, a, b, 0);
                            Initiator.AddBelief(newBelief);
                        }

                        foreach (var _belief in Initiator.SocialNetworkBeliefs)
                        {
                            if (_belief.agents.Contains(Initiator.Name) && _belief.agents.Contains(Receiver.Name) 
                                && _belief.IDs.Contains(Initiator.Id) && _belief.IDs.Contains(Receiver.Id))
                            {
                                if (_belief.value < _goal.value)
                                {
                                    return 100;
                                }
                            }
                        }
                        break;
                    }
                }
            }

            return 0;
        }
        private int CheckStatus(CustomAgent customAgent)
        {
            int localSum = 0;

            /* Shame Status */
            Status status = CheckStatusIntensity(customAgent, "Shame");
            if (status.intensity > 0.5)
            {
                localSum -= 2;
            }

            /* Courage Status */
            status = CheckStatusIntensity(customAgent, "Courage");
            if (status.intensity > 0.5)
            {
                localSum += 2;
            }

            /* SocialTalk Status */
            status = CheckStatusIntensity(customAgent, "SocialTalk");
            if (status.intensity > 0.5)
            {
                if (RelationType == SocialExchangeSE.IntentionEnum.Positive)
                {
                    localSum += 2;
                }
            }
            
            /* Anger Status */
            status = CheckStatusIntensity(customAgent, "Anger");
            if (status.intensity > 0.5)
            {
                if (RelationType == SocialExchangeSE.IntentionEnum.Positive || RelationType == SocialExchangeSE.IntentionEnum.Romantic)
                {
                    localSum -= 2;
                }
                else if (RelationType == SocialExchangeSE.IntentionEnum.Negative || RelationType == SocialExchangeSE.IntentionEnum.Hostile)
                {
                    localSum += 2;
                }
            }

            return localSum;
        }
        private Status CheckStatusIntensity(CustomAgent customAgent, string statusName)
        {
            return customAgent.StatusList.Find(s => s.statusName == statusName);
        }
        internal int GetValueParticipantsRelation(CustomAgent agentWhoWillCheck, CustomAgent agentChecked)
        {
            SocialNetworkBelief belief = agentWhoWillCheck.SelfGetBeliefWithAgent(agentChecked); // Relation between the Initiator and the Receiver
            if (belief != null)
            {
                return belief.value;
            }

            return 0;
        }
        private int CheckFaithful(CustomAgent agent, CustomAgent otherAgent)
        {
            SocialNetworkBelief belief = agent.SocialNetworkBeliefs.Find(b => b.relationship == "Dating");
            if (belief != null)
            {
                if (belief.agents.Contains(agent.Name) && belief.agents.Contains(otherAgent.Name))
                {
                    if (belief.value > 0)
                    {
                        return 2; // dating with that specific NPC
                    }
                    else 
                    { 
                        return 2; // Contains but not dating anymore
                    }
                }
                else
                {
                    return -2; // dating with other NPC
                }
            }
            else
            {
                return 2; //not dating with anyone
            }
        }
        public CustomAgent Initiator { get; }
        public CustomAgent Receiver { get; }
        public int InitialValue { get; set; }
        public string RelationName { get; set; }
        public bool IsReacting { get; set; }
        public SocialExchangeSE.IntentionEnum RelationType { get; internal set; }
    }
}
