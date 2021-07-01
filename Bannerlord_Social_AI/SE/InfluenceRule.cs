using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;

namespace Bannerlord_Social_AI
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

        public int SRunRules()
        {
            var Dictionary = GetDictionaryToCheckTraitsValues(RelationType);

            switch (RelationType)
            {
                case SocialExchangeSE.IntentionEnum.Positive:
                    switch (RelationName)
                    {
                        case "Compliment": return RunRules(Dictionary, true, false, false, false, false, true);
                        default: return 0;
                    }
                   
                case SocialExchangeSE.IntentionEnum.Negative:
                    switch (RelationName)
                    {
                        case "Jealous": return RunRules(Dictionary, true, false, false, false, false, false);
                        case "FriendSabotage": return RunRules(Dictionary, false, false, false, true, false, false);
                        default: return 0;
                    }

                case SocialExchangeSE.IntentionEnum.Romantic:
                    switch (RelationName)
                    {
                        case "AskOut": return RunRules(Dictionary, true, false, true, false, false, true);
                        case "Flirt": return RunRules(Dictionary, false, true, false, false, false, true);
                        default: return 0;
                    }

                case SocialExchangeSE.IntentionEnum.Hostile:
                    switch (RelationName)
                    {
                        case "Bully": return RunRules(Dictionary, false, true, false, false, false, false);
                        case "RomanticSabotage": return RunRules(Dictionary, false, false, false, false, false, false);
                        default: return 0;
                    }
                   
                case SocialExchangeSE.IntentionEnum.Special:
                    switch (RelationName)
                    {
                        case "Break": return RunRules(Dictionary, false, true, false, false, true, false);
                        default: return 0;
                    }

                default:
                    return 0;
            }
        }

        private int RunRules(Dictionary<String, Func<CustomAgent, int>> Dictionary,
             bool DecreaseIfDatingBool, bool DecreaseIfNotDatingBool, bool MustHaveDifferentGenderBool,
             bool GetNPCToSabotageBool, bool BreakUpRuleBool, bool IsPositiveOrRomanticSE)
        {
            int sum = 0;
            sum += (InitialValue > 0) ? InitialValue : InitialValue * -1;

            if (!IsReacting)
            {
                sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
                {
                    Func<CustomAgent, int> TraitFunc;
                    if (Dictionary.TryGetValue(t.traitName, out TraitFunc))
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
                    if (Dictionary.TryGetValue(t.traitName, out TraitFunc))
                    {
                        acc += TraitFunc(Receiver);
                    }

                    return acc;
                });
            }

            if (IsReacting)
            {
                sum += CheckStatus(Receiver);
                sum += CheckCulturesRelationships(Receiver, Initiator, IsPositiveOrRomanticSE);
            }
            else
            {
                sum += CheckStatus(Initiator);
                sum += CheckCulturesRelationships(Initiator, Receiver, IsPositiveOrRomanticSE);
            }

            if (DecreaseIfDatingBool)
            {
                sum += DecreaseIfDating(sum);
            }
            if (DecreaseIfNotDatingBool)
            {
                sum += DecreaseIfNotDating(sum);
            }
            if (MustHaveDifferentGenderBool)
            {
                sum += MustHaveDifferentGender(sum);
            }
            if (GetNPCToSabotageBool)
            {
                sum += GetNPCToSabotage(sum);
            }
            if (BreakUpRuleBool)
            {
                sum += BreakUpRule(sum);
            }

            return sum;
        }

        private int CheckCulturesRelationships(CustomAgent agent, CustomAgent otherAgent, bool IsPositiveOrRomanticSE)
        {
            int localsum = 0;
            bool auxBool = false;

            if (agent.CulturesFriendly != null)
            {
                auxBool = agent.CulturesFriendly.Contains(otherAgent.cultureCode);
                if (auxBool)
                {
                    if (IsPositiveOrRomanticSE)
                    {
                        localsum += 2;
                    }
                    else
                    {
                        localsum -= 2;
                    }
                }
            }

            if (agent.CulturesUnFriendly != null)
            {
                auxBool = agent.CulturesUnFriendly.Contains(otherAgent.cultureCode);
                if (auxBool)
                {
                    if (IsPositiveOrRomanticSE)
                    {
                        localsum -= 2;
                    }
                    else
                    {
                        localsum += 2;
                    }
                }
            }
            
            return localsum;
        }

        // -100 if Dating
        private int DecreaseIfDating(int sum)
        {
            SocialNetworkBelief socialNetworkBelief = Initiator.SelfGetBeliefWithAgent(Receiver);

            if (socialNetworkBelief != null && socialNetworkBelief.relationship != "Friends")
            {
                sum -= 100;
            }

            return sum;
        }

        // it must be dating
        private int DecreaseIfNotDating(int sum)
        {
            SocialNetworkBelief socialNetworkBelief = Initiator.SelfGetBeliefWithAgent(Receiver);
            if (socialNetworkBelief == null || socialNetworkBelief.relationship != "Dating")
            {
                sum -= 100;
            }

            return sum;
        }

        // different genders
        private int MustHaveDifferentGender(int sum)
        {
            if ((Initiator.selfAgent.IsFemale && Receiver.selfAgent.IsFemale)
                 ||
                (!Initiator.selfAgent.IsFemale && !Receiver.selfAgent.IsFemale))
            {
                sum -= 100;
            }

            return sum;
        }

        // must have someone to sabotage
        private int GetNPCToSabotage(int sum)
        {
            List<SocialNetworkBelief> tempList = Initiator.SelfGetNegativeRelations();
            if (tempList != null && tempList.Count > 0)
            {
                Random rnd = new Random();
                int index = rnd.Next(tempList.Count);

                List<string> agentsOnRelation = tempList[index].agents;

                if (agentsOnRelation.Contains(Initiator.Name))
                {
                    foreach (string agent in agentsOnRelation.Where(agent => agent != Initiator.Name))
                    {
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

        // must have belief = Dating && value < 1
        private int BreakUpRule(int sum)
        {
            SocialNetworkBelief socialNetworkBelief = Initiator.SelfGetBeliefWithAgent(Receiver);
            if (socialNetworkBelief != null && socialNetworkBelief.relationship == "Dating" && socialNetworkBelief.value <= 0)
            {
                sum += 100;
            }

            return sum;
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

        private int CheckFaithful(CustomAgent customAgent, CustomAgent otherAgent)
        {
            SocialNetworkBelief belief = customAgent.SocialNetworkBeliefs.Find(b => b.relationship == "Dating");
            if (belief != null)
            {
                if (belief.agents.Contains(customAgent.Name) && belief.agents.Contains(otherAgent.Name))
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

        private Status CheckStatusIntensity(CustomAgent customAgent, string statusName)
        {
            return customAgent.StatusList.Find(s => s.Name == statusName);
        }

        public int GetValueParticipantsRelation(CustomAgent agentWhoWillCheck, CustomAgent agentChecked)
        {
            SocialNetworkBelief belief = agentWhoWillCheck.SelfGetBeliefWithAgent(agentChecked); // Relation between the Initiator and the Receiver
            if (belief != null)
            {
                return belief.value;
            }

            return 0;
        }

        public int CheckInitiatorTriggerRules(CustomAgent agentWhoWillCheck, CustomAgent agentChecked, string relationName)
        {
            if (!agentWhoWillCheck.TriggerRuleList.IsEmpty())
            {
                TriggerRule triggerRule = agentWhoWillCheck.TriggerRuleList.Find(
                    rule => rule.NPC_OnRule == agentChecked.Name && rule.NPC_ID == agentChecked.Id && rule.SocialExchangeToDo == relationName);
                
                if (triggerRule != null)
                {
                    return 100;
                }
            }

            return 0;
        }

        public CustomAgent Initiator { get; }
        public CustomAgent Receiver { get; }
        public bool IsReacting { get; set; }
        public int InitialValue { get; set; }
        public string RelationName { get; set; }
        public SocialExchangeSE.IntentionEnum RelationType { get; set; }

        public Dictionary<String, Func<CustomAgent, int>> TraitFunc_Dictionary = new Dictionary<String, Func<CustomAgent, int>>();
        public Dictionary<String, Func<CustomAgent, int>> GetDictionaryToCheckTraitsValues(SocialExchangeSE.IntentionEnum intention)
        {
            switch (intention)
            {
                case SocialExchangeSE.IntentionEnum.Positive:
                case SocialExchangeSE.IntentionEnum.Special:
                    return TraitFunc_Dictionary = new Dictionary<string, Func<CustomAgent, int>>{
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

                //Flirt & AskOut
                case SocialExchangeSE.IntentionEnum.Romantic:
                    return TraitFunc_Dictionary = new Dictionary<string, Func<CustomAgent, int>>{
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

                //Jealous & FriendSabotage & RomanticSabotage & Hostile
                case SocialExchangeSE.IntentionEnum.Negative:
                case SocialExchangeSE.IntentionEnum.Hostile:
                    return TraitFunc_Dictionary = new Dictionary<string, Func<CustomAgent, int>>{
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

                default:
                    return TraitFunc_Dictionary = new Dictionary<string, Func<CustomAgent, int>>{
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
            }
        }
    }
}