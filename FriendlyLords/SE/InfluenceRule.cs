using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace FriendlyLords
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
            var Dictionary = GetDictionaryToCheckTraitsValues(RelationIntention);

            switch (SE_Enum_Name)
            {
                case CustomMissionNameMarkerVM.SEs_Enum.Compliment:
                    return RunRules(Dictionary, true, false, true, false, false, false, false, false, false);
                case CustomMissionNameMarkerVM.SEs_Enum.GiveGift:
                    return RunRules(Dictionary, true, true, true, false, false, false, false, false, false);
                case CustomMissionNameMarkerVM.SEs_Enum.Jealous:
                    return RunRules(Dictionary, false, false, true, false, false, false, false, false, false);
                case CustomMissionNameMarkerVM.SEs_Enum.FriendSabotage:
                    return RunRules(Dictionary, false, false, false, false, false, false, true, false, false);
                case CustomMissionNameMarkerVM.SEs_Enum.Flirt:
                    return RunRules(Dictionary, true, false, false, true, true, true, false, false, false);
                case CustomMissionNameMarkerVM.SEs_Enum.Bully:
                    return RunRules(Dictionary, false, false, true, true, false, false, false, false, false);
                case CustomMissionNameMarkerVM.SEs_Enum.RomanticSabotage:
                    return RunRules(Dictionary, false, false, false, false, false, false, false, false, true);
                case CustomMissionNameMarkerVM.SEs_Enum.AskOut:
                    return RunRules(Dictionary, true, false, true, false, true, true, false, false, false);
                case CustomMissionNameMarkerVM.SEs_Enum.Break:
                    return RunRules(Dictionary, false, false, false, true, false, false, false, true, false);
                case CustomMissionNameMarkerVM.SEs_Enum.Admiration:
                    return RunRules(Dictionary, true, false, false, false, false, false, false, false, true);
                case CustomMissionNameMarkerVM.SEs_Enum.HaveAChild:
                    return -100;
                default: 
                    return 0;
            }
        }


        private int RunRules(Dictionary<String, Func<CustomAgent, int>> Dictionary, bool IsPositiveOrRomanticSE, bool NeedsItem,
             bool NeedsToBeFriendsOrNull, bool NeedsToBeDating, bool MustHaveDifferentGenderBool, bool NeedsToBeOlderThan18,
             bool GetNPCToSabotageBool, bool BreakUpRuleBool, bool NeedsTriggerRule)
        {
            int sum = 0;
            //sum += (InitialValue > 0) ? InitialValue : InitialValue * -1;

            sum += IsPositiveOrRomanticSE ? InitialValue : InitialValue * -1;

            
            if (!IsReacting)
            {
                sum += Initiator.TraitList.AsParallel().Aggregate(InitialValue, (acc, t) =>
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

            sum += CheckMemoryForPreviousSEs(SE_Enum_Name.ToString(), Initiator, Receiver);

            if (NeedsToBeOlderThan18)
            {
                sum += CheckInitiatorAge();
            }
            if (NeedsItem)
            {
                sum += CheckItemForGiveGiftSE();
            }
            if (NeedsToBeFriendsOrNull)
            {
                sum += CheckNeedsToBeFriendsOrNull();
            }
            if (NeedsToBeDating)
            {
                sum += CheckNeedsToBeDating();
            }
            if (MustHaveDifferentGenderBool)
            {
                sum += MustHaveDifferentGender();
            }
            if (GetNPCToSabotageBool)
            {
                sum += GetNPCToSabotage();
            }

            if (BreakUpRuleBool)
            {
                sum += BreakUpRule();
            }

            if (NeedsTriggerRule)
            {
                sum -= 100;
            }

            return sum;
        }

        //Needs Item or it will decrease 100
        private int CheckItemForGiveGiftSE()
        {
            int localSum = 0;

            if (!Initiator.ItemList.IsEmpty())
            {
                if (Initiator.selfAgent.IsHero)
                {
                    Hero hero = Hero.FindFirst(h => h.Name.ToString() == Initiator.Name);
                    if (hero != null && hero.IsPlayerCompanion && Receiver.selfAgent == Agent.Main && Initiator.ItemList.Count > 0)
                    {
                        localSum += 2; // se for companion, só incrementar se o receiver for o player
                    }
                    else { localSum -= 100; }
                }
                else { localSum += 2; } // se nao for companion pode incrementar pra qualquer 1
            }
            else { localSum -= 100; }

            return localSum;
        }

        // -100 if Dating
        private int CheckNeedsToBeFriendsOrNull()
        {
            SocialNetworkBelief socialNetworkBelief = Initiator.SelfGetBeliefWithAgent(Receiver);

            if (socialNetworkBelief != null && socialNetworkBelief.relationship == "Dating")
            {
                return -100;
            }

            return 0;
        }

        // it must be dating
        private int CheckNeedsToBeDating()
        {
            SocialNetworkBelief socialNetworkBelief = Initiator.SelfGetBeliefWithAgent(Receiver);

            if (socialNetworkBelief == null || socialNetworkBelief.relationship == "Friends")
            {
                return -100;
            }

            return 2;
        }

        // different genders
        private int MustHaveDifferentGender()
        {
            if ((Initiator.selfAgent.IsFemale && Receiver.selfAgent.IsFemale)
                 ||
                (!Initiator.selfAgent.IsFemale && !Receiver.selfAgent.IsFemale))
            {
                return -100;
            }

            return 0;
        }

        // must have someone to sabotage
        private int GetNPCToSabotage()
        {
            int localSum = 0;
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

                        localSum += 2;
                    }
                }
            }

            if (Initiator.thirdAgent != "" && Receiver.Name != Initiator.thirdAgent && Receiver.Id != Initiator.thirdAgentId)
            {
                return localSum;
            }
            else
            {
                localSum -= 100;
                return localSum;
            }       
        }

        // must have belief = Dating && value < 1
        private int BreakUpRule()
        {
            SocialNetworkBelief socialNetworkBelief = Initiator.SelfGetBeliefWithAgent(Receiver);

            if (socialNetworkBelief != null && socialNetworkBelief.relationship == "Dating" && socialNetworkBelief.value <= -1)
            {
                return 10;
            }
            else 
            { 
                return -200; 
            }
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
                        localsum += 5;
                    }
                    else
                    {
                        localsum -= 5;
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
                        localsum -= 5;
                    }
                    else
                    {
                        localsum += 5;
                    }
                }
            }

            return localsum;
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
            if (RelationIntention == SocialExchangeSE.IntentionEnum.Positive)
            {
                if (status.intensity > 0.5 && status.intensity < 1.5)
                {
                    localSum += 2;
                }
                else if (status.intensity >= 1.5 && status.intensity < 3)
                {
                    localSum += 4;
                }
                else if (status.intensity >= 3)
                {
                    localSum += 6;
                }
            }

            /* Anger Status */
            status = CheckStatusIntensity(customAgent, "Anger");
            if (RelationIntention == SocialExchangeSE.IntentionEnum.Positive || RelationIntention == SocialExchangeSE.IntentionEnum.Romantic)
            {
                if (status.intensity > 0.5 && status.intensity < 1.5)
                {
                    localSum -= 2;
                }
                else if (status.intensity >= 1.5 && status.intensity < 3)
                {
                    localSum -= 4;
                }
                else if (status.intensity >= 3)
                {
                    localSum -= 6;
                }
            }
            else if (RelationIntention == SocialExchangeSE.IntentionEnum.Negative || RelationIntention == SocialExchangeSE.IntentionEnum.Hostile)
            {
                if (status.intensity > 0.5 && status.intensity < 1.5)
                {
                    localSum += 2;
                }
                else if(status.intensity >= 1.5 && status.intensity < 3)
                {
                    localSum += 4;
                }
                else if(status.intensity >= 3)
                {
                    localSum += 6;
                }
            }

            status = CheckStatusIntensity(customAgent, "BullyNeed");
            if (RelationIntention == SocialExchangeSE.IntentionEnum.Positive || RelationIntention == SocialExchangeSE.IntentionEnum.Romantic)
            {
                if (status.intensity > 0.5 && status.intensity < 1.5)
                {
                    localSum -= 2;
                }
                else if (status.intensity >= 1.5 && status.intensity < 3)
                {
                    localSum -= 5;
                }
                else if (status.intensity >= 3)
                {
                    localSum -= 10;
                }
            }
            else if (RelationIntention == SocialExchangeSE.IntentionEnum.Hostile)
            {
                if (status.intensity > 0.5 && status.intensity < 1.5)
                {
                    localSum += 2;
                }
                else if (status.intensity >= 1.5 && status.intensity < 3)
                {
                    localSum += 4;
                }
                else if (status.intensity >= 3)
                {
                    localSum += 6;
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
                return (int)belief.value;
            }

            return 0;
        }

        public int CheckInitiatorTriggerRules(CustomAgent agentInitiator, CustomAgent agentReceiver, string relationName)
        {
            if (!agentInitiator.TriggerRuleList.IsEmpty())
            {
                TriggerRule triggerRule = agentInitiator.TriggerRuleList.Find(
                    rule => rule.NPC_OnRule == agentReceiver.Name && rule.NPC_ID == agentReceiver.Id && rule.SocialExchangeToDo == relationName);
                
                if (triggerRule != null)
                {
                    agentInitiator.RemoveTriggerRule(triggerRule);
                    return 300;
                }
            }

            return 0;
        }

        public int CheckInitiatorAge()
        {
            if (
                (Initiator.selfAgent.Age < 18 && Receiver.selfAgent.Age > 18)
                ||
                (Initiator.selfAgent.Age > 18 && Receiver.selfAgent.Age < 18)
                )
            {
                return -100;
            }

            return 0;
        }

        public CustomAgent Initiator { get; }
        public CustomAgent Receiver { get; }
        public bool IsReacting { get; set; }
        public int InitialValue { get; set; }
        public SocialExchangeSE.IntentionEnum RelationIntention { get; set; }
        public CustomMissionNameMarkerVM.SEs_Enum SE_Enum_Name { get; internal set; }

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

        private int CheckMemoryForPreviousSEs(string SEName, CustomAgent c1, CustomAgent c2)
        {
            int localSum = 0;
            MemorySE memory = GetMemory("Break", c1, c2);
            if (memory != null && SEName == "AskOut")
            {
                localSum -= 2;
            }
            else if (memory != null && (SocialExchangeSE.IntentionEnum.Hostile == RelationIntention))
            {
                localSum += 2;
            }
            
            int howMany = CountMemory("RomanticSabotage", c1, c2);
            if (howMany > 0 && SEName == "Bully")
            {
                localSum += howMany * 2;
            }

            howMany = CountMemory("Jealous", c1, c2);
            if (howMany > 0 && SEName == "Bully")
            {
                localSum += howMany * 2;
            }

            howMany = CountMemory("AskOut", c1, c2);
            if (howMany > 0 && SEName == "Bully")
            {
                localSum += howMany * 2;
            }

            return localSum;
        }

        private MemorySE GetMemory(string _SEName, CustomAgent c1, CustomAgent c2)
        {
            MemorySE _memory = c1.MemorySEs.Find(
                            memorySlot =>
                            memorySlot.SE_Name == _SEName &&
                            memorySlot.agents.Contains(c1.Name) &&
                            memorySlot.agents.Contains(c2.Name) &&
                            memorySlot.IDs.Contains(c1.Id) &&
                            memorySlot.IDs.Contains(c2.Id)
                            );
            return _memory;
        }

        private int CountMemory(string _SEName, CustomAgent c1, CustomAgent c2)
        {
            int _howManyTimes = c1.MemorySEs.Count(
                            memorySlot =>
                            memorySlot.SE_Name == _SEName &&
                            memorySlot.agents.Contains(c1.Name) &&
                            memorySlot.agents.Contains(c2.Name) &&
                            memorySlot.IDs.Contains(c1.Id) &&
                            memorySlot.IDs.Contains(c2.Id)
                            );
            return _howManyTimes;
        }
    }
}