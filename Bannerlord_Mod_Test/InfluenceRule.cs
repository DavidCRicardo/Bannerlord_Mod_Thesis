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
        public int SRunRules()
        {
            // Is Receiver || Is Initiator
            if (IsReacting || !IsReacting)
            {
                switch (RelationType)
                {
                    case SocialExchangeSE.IntentionEnum.Positive:
                        if (RelationName == "Compliment")
                        {
                            return RunRulesFriendship();
                        }
                        else { return 0; }

                    case SocialExchangeSE.IntentionEnum.Negative:
                        if (RelationName == "Jealous")
                        {
                            return RunRulesJealous();
                        }
                        else if (RelationName == "Sabotage")
                        {
                            return RunRulesSabotage();
                        }
                        else { return 0; }

                    case SocialExchangeSE.IntentionEnum.Romantic:
                        if (RelationName == "Flirt")
                        {
                            return RunRulesRomantic();
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
                        else { return 0; }
                    case SocialExchangeSE.IntentionEnum.Special:
                        if (RelationName == "Break")
                        {
                            return RunRulesSpecial();
                        }
                        else { return 0; }
                    default:
                        return 0;
                }
            }
            else
            {
                 return 0;
            }
        }

        //Positive SE
        private int RunRulesFriendship()
        {
            int sum = InitialValue;

            /* Check Traits */
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

            sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Initiator);
                }

                return acc;
            });

            sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Receiver);
                }

                return acc;
            });
            /* End Check Traits */
            
            /* Check Status */
            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);
            /* End Check Status */

            return sum;
        }
        //Romantic SE
        private int RunRulesRomantic()
        {
            int sum = InitialValue;

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
                { "Faithful"  , agent => (agent == Initiator) ? CheckFaithful(agent, Initiator) : CheckFaithful(agent, Receiver) },
                { "Unfaithful", agent =>  2 }
            };
            sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Initiator);
                }

                return acc;
            });

            sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Receiver);
                }

                return acc;
            });

            /* Check Status */
            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);
            /* End Check Status */

            if ((Initiator.selfAgent.IsFemale && Receiver.selfAgent.IsFemale)
                ||
                (!Initiator.selfAgent.IsFemale && !Receiver.selfAgent.IsFemale))
            {
                sum -= 2;
            }


            //If not dating with the receiver, so decrease drastically the sum to not Flirt 
            SocialNetworkBelief socialNetworkBelief = Initiator.GetBelief("Dating", Receiver);
            if (socialNetworkBelief == null)
            {
                sum -= 100;
            }

            return sum;
        }
        private int RunRulesAskOut()
        {
            int sum = InitialValue;

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
                { "Faithful"  , agent => (agent == Initiator) ? CheckFaithful(agent, Initiator) : CheckFaithful(agent, Receiver) },
                { "Unfaithful", agent =>  2 }
            };
            sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Initiator);
                }

                return acc;
            });

            sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Receiver);
                }

                return acc;
            });

            /* Check Status */
            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);
            /* End Check Status */

            if ((Initiator.selfAgent.IsFemale && Receiver.selfAgent.IsFemale)
                ||
                (!Initiator.selfAgent.IsFemale && !Receiver.selfAgent.IsFemale))
            {
                sum -= 2;
            }

            #region Check Condition to AskOut
            //If dating already with the receiver, so decrease drastically the sum to not AskOut again
            SocialNetworkBelief socialNetworkBelief = Initiator.GetBelief("Dating", Receiver);
            if (socialNetworkBelief != null)
            {
                sum -= 100;
            }
            #endregion

            return sum;
        }
        //Negative SE
        private int RunRulesJealous()
        {
            int sum = InitialValue;

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

            sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Initiator);
                }

                return acc;
            });

            sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Receiver);
                }

                return acc;
            });

            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);

            return sum;
        }
        private int RunRulesSabotage()
        {
            int sum = InitialValue;

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

            sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Initiator);
                }

                return acc;
            });

            sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Receiver);
                }

                return acc;
            });

            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);

            #region Check Condition To Sabotage
            /* Check if there is anyone dating and sabotage */
            SocialNetworkBelief belief = Initiator.CheckIfAgentIsDatingWithAnyone(Receiver);
            /* If noone is dating, there is no need to sabotage */
            if (belief != null)
            {
                sum += 2;
            }
            else
            {
                sum -= 2;
            }
            #endregion

            return sum;
        }
        //Hostile SE
        private int RunRulesHostile()
        {
            int sum = InitialValue;

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
            sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Initiator);
                }

                return acc;
            });

            sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Receiver);
                }

                return acc;
            });

            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);

            return sum;
        }
        //Special SE
        private int RunRulesSpecial()
        {
            int sum = InitialValue;

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
            sum = Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Initiator);
                }

                return acc;
            });

            sum += Receiver.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
            {
                Func<CustomAgent, int> TraitFunc;
                if (TraitFunc_Dictionary.TryGetValue(t.traitName, out TraitFunc))
                {
                    acc += TraitFunc(Receiver);
                }

                return acc;
            });

            sum += IsReacting ? CheckStatus(Receiver) : CheckStatus(Initiator);
            if (Initiator.HasRelationWith("Dating", Receiver)) { }

            return sum;
        }

        internal int CheckGoals(string _relation)
        {
            if (!Initiator.GoalsList.IsEmpty())
            {
                foreach (var _goal in Initiator.GoalsList)
                {
                    if (_goal.relationship == _relation && _goal.targetName == Receiver.Name)
                    {
                        /* Add Belief to check if belief value < goal value */
                        if (!Initiator.HasRelationWith(_relation, Receiver))
                        {
                            SocialNetworkBelief belief = Initiator.GetBelief(_relation, Receiver);
                            if (belief == null)
                            {
                                List<string> a = new List<string>() { Initiator.Name, Receiver.Name };
                                SocialNetworkBelief newBelief = new SocialNetworkBelief(_relation, a, 0);
                                Initiator.AddBelief(newBelief);
                            }
                        }
                        /* */
                        foreach (var _belief in Initiator.SocialNetworkBeliefs)
                        {
                            if (_belief.agents.Contains(Initiator.Name) && _belief.agents.Contains(Receiver.Name))
                            {
                                if (_belief.value < _goal.value)
                                {
                                    return 3;
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
                localSum += 2;
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
        internal int GetValueParticipantsRelation()
        {
            SocialNetworkBelief localBelief = GetParticipantsRelation();
            if (localBelief != null)
            {
                return localBelief.value;
            }

            return 0;
        }
        private SocialNetworkBelief GetParticipantsRelation()
        {
            // Relation between the Initiator and the Receiver
            return Initiator.SocialNetworkBeliefs.Find(b => b.agents.Contains(Initiator.Name) && b.agents.Contains(Receiver.Name));
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
