using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Bannerlord_Mod_Test
{
    public class SocialExchangeSE
    {
        public SocialExchangeSE(string _SEname, CustomAgent _customAgentinitiator, List<CustomAgent> customAgents)
        {
            this.SEName = _SEname;

            if (_customAgentinitiator != null)
            {
                this.AgentInitiator = _customAgentinitiator.selfAgent;
                this.CustomAgentInitiator = _customAgentinitiator;

                //this.AgentReceiver = _customAgentinitiator.targetAgent;
                //this.CustomAgentReceiver = CustomAgentList.Single(item => item.Name == AgentReceiver.Name);
                this.CustomAgentList = customAgents;
                this.CustomAgentReceiver = CustomAgentInitiator.customTargetAgent;
                this.AgentReceiver = CustomAgentReceiver.selfAgent;
                this.index = -1;
            }

            switch (SEName)
            {
                case "Compliment": Intention = IntentionEnum.Positive; break;
                case "FriendSabotage":
                case "Jealous": Intention = IntentionEnum.Negative; break;
                case "AskOut":
                case "Flirt": Intention = IntentionEnum.Romantic; break;
                case "RomanticSabotage":
                case "Bully": Intention = IntentionEnum.Hostile; break;
                case "Break": Intention = IntentionEnum.Special; break;
                default: Intention = IntentionEnum.Undefined; break;
            }
        }

        internal void OnInitialize(Random _rnd)
        {
            Rnd = _rnd;
            SocialExchangeDoneAndReacted = false;

            ReceptorIsPlayer = AgentReceiver.Name == Agent.Main.Name;

            CustomAgentInitiator.SEIntention = Intention;

            CustomAgentReceiver.busy = true;
            CustomAgentReceiver.SEIntention = Intention;
            CustomAgentReceiver.SocialMove = SEName;
            CustomAgentReceiver.selfAgent.SetLookAgent(AgentInitiator);
        }
        public bool ReduceDelay { get; set; }
        internal void OnGoingSocialExchange(Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> megaDictionary)
        {
            if (auxToCheckWhoIsSpeaking % 2 == 0)
            {
                index++;

                CustomAgentInitiator.AgentGetMessage(true, CustomAgentInitiator, CustomAgentReceiver, Rnd, index, megaDictionary);
                if (CustomAgentInitiator.message != "")
                {
                    CustomAgentReceiver.message = "";
                    ReduceDelay = false;
                }
                else { ReduceDelay = true; }

                auxToCheckWhoIsSpeaking++;
            }
            else
            {
                CustomAgentReceiver.SEVolition = ReceiverVolition();
                CustomAgentReceiver.AgentGetMessage(false, CustomAgentInitiator, CustomAgentReceiver, Rnd, index, megaDictionary);

                if (CustomAgentReceiver.message != "")
                {
                    CustomAgentInitiator.message = "";
                    ReduceDelay = false;
                }
                else { ReduceDelay = true; }

                auxToCheckWhoIsSpeaking = 0;
            }

            if ((CustomAgentInitiator.message == "" && CustomAgentReceiver.message == "") || ReceptorIsPlayer)
            {
                if (ReceptorIsPlayer) { AgentInitiator.OnUse(AgentReceiver); }
                SocialExchangeDoneAndReacted = true; 
            }
        }
        internal void OnFinalize()
        {
            AgentInitiator.OnUseStopped(AgentReceiver, true, 0);

            CustomAgentInitiator.AddToMemory(new MemorySE(CustomAgentReceiver.Name, SEName));
            CustomAgentReceiver.AddToMemory(new MemorySE(CustomAgentInitiator.Name, SEName));

            ResetCustomAgentVariables(CustomAgentInitiator);
            if (!ReceptorIsPlayer)
            {
                ResetCustomAgentVariables(CustomAgentReceiver);
                UpdateBeliefsAndStatus();
            }
        }

        private void ResetCustomAgentVariables(CustomAgent customAgent)
        {
            customAgent.SocialMove = "";
            customAgent.IsInitiator = false;
            customAgent.FullMessage = null;
            customAgent.busy = false;
            customAgent.message = "";
            customAgent.cooldown = true;
            customAgent.targetAgent = null;
            customAgent.customTargetAgent = null;
            customAgent.StopAnimation();
            customAgent.EndFollowBehavior();
        }

        private void UpdateBeliefsAndStatus()
        {
            switch (Intention)
            {
                case IntentionEnum.Positive:
                    if (CustomAgentReceiver.SE_Accepted)
                    {
                        if (SEName == "Compliment")
                        {
                            SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", 1);
                            UpdateThirdNPCsBeliefs("Friends", belief, 1);
                            CustomAgentInitiator.UpdateStatus("SocialTalk", -1);
                        }
                    }
                    else
                    {
                        CustomAgentInitiator.UpdateStatus("SocialTalk", -1);
                        CustomAgentInitiator.UpdateStatus("Shame", 1);
                    }

                    break;
                case IntentionEnum.Romantic:
                    if (CustomAgentReceiver.SE_Accepted)
                    {
                        //Increases Relationship for both
                        if (SEName == "AskOut")
                        {
                            //if they are not friends so start dating with a new belief
                            ////If they are already friends, it updates for dating while keeping the same value 
                            SocialNetworkBelief _belief = UpdateParticipantNPCBeliefs("Friends", 1);
                            UpdateThirdNPCsBeliefs("Friends", _belief, 1);

                            //CustomAgentInitiator.UpdateBeliefWithNewRelation("Dating", _belief);
                            //CustomAgentReceiver.UpdateBeliefWithNewRelation("Dating", _belief);

                            foreach (CustomAgent customAgent in CustomAgentList)
                            {
                                customAgent.UpdateBeliefWithNewRelation("Dating", _belief);
                            }

                            InformationManager.DisplayMessage(new InformationMessage(CustomAgentReceiver.Name + " is now dating " + CustomAgentInitiator.Name));
                        }
                        else if (SEName == "Flirt")
                        {
                            SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Dating", 1);
                            UpdateThirdNPCsBeliefs("Dating", belief, 1);
                        }
                    }
                    else
                    {
                        CustomAgentInitiator.UpdateStatus("Anger", 1);
                        InformationManager.DisplayMessage(new InformationMessage(CustomAgentReceiver.Name + " rejected " + CustomAgentInitiator.Name + " " + SEName));
                    }

                    //Independentemente se aceitou ou nao.. 
                    foreach (CustomAgent customAgent in CustomAgentList)
                    {
                        // verificar para todos menos para aqueles que estavam envolvidos na SE
                        if (customAgent != CustomAgentInitiator && customAgent != CustomAgentReceiver)
                        {
                            //se o initiator é o seu parceiro (dating) que começou a Romantic SE // nao vai gostar da SE e vai decrementar 1 ponto
                            SocialNetworkBelief beliefWithInitiator = customAgent.SelfGetBeliefWithAgent(CustomAgentInitiator);
                            if (beliefWithInitiator != null && beliefWithInitiator.relationship == "Dating")
                            {
                                customAgent.UpdateBeliefWithNewValue(beliefWithInitiator, -1);
                            }

                            //se o receiver é o seu parceiro (dating) de que foi alvo de Romantic SE // vai ter ciumes do Initiator 
                            SocialNetworkBelief beliefWithReceiver = customAgent.SelfGetBeliefWithAgent(CustomAgentReceiver);
                            if (beliefWithReceiver != null && beliefWithReceiver.relationship == "Dating")
                            {
                                //tem relaçao com o receiver e essa relação é dating? então ganha o goal de ciumes para a SE
                                TriggerRule triggerRule = new TriggerRule("RomanticSabotage", CustomAgentInitiator.Name);
                                customAgent.AddToTriggerRulesList(triggerRule);
                            }
                        }
                    }

                    break;
                case IntentionEnum.Negative:
                    if (CustomAgentReceiver.SE_Accepted)
                    {
                        if (SEName == "Jealous")
                        {
                            //Decreases relation with Initiator
                            SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", -1);
                            UpdateThirdNPCsBeliefs("Friends", belief, -1);

                            CustomAgentInitiator.UpdateStatus("Anger", -0.3);
                        }

                        if (SEName == "FriendSabotage")
                        {
                            //Decreases relation with Initiator
                            SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", -1);
                            UpdateThirdNPCsBeliefs("Friends", belief, -1);
                        }
                    }
                    else
                    {
                        if (SEName == "Jealous")
                        {
                            CustomAgentInitiator.UpdateStatus("Anger", -0.3);
                            CustomAgentInitiator.UpdateStatus("Shame", 1);
                            CustomAgentReceiver.UpdateStatus("Courage", -0.2);
                        }
                        
                        if (SEName == "FriendSabotage")
                        {
                            //Decreases relation dating
                            InformationManager.DisplayMessage(new InformationMessage(CustomAgentInitiator.Name + " sabotaged " + CustomAgentReceiver.Name));
                            CustomAgent CAtoDecrease = CustomAgentReceiver.GetCustomAgentByName(CustomAgentInitiator.thirdAgent);
                            SocialNetworkBelief belief = CustomAgentReceiver.SelfGetBeliefWithAgent(CAtoDecrease);

                            CustomAgentReceiver.UpdateBeliefWithNewValue(belief, -1);
                        }
                    }
                    break;
                case IntentionEnum.Hostile:
                    if (CustomAgentReceiver.SE_Accepted)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(CustomAgentReceiver.Name + " rejected " + CustomAgentInitiator.Name + " " + SEName));
                        InformationManager.DisplayMessage(new InformationMessage(CustomAgentInitiator.Name + " is embarrassed."));

                        CustomAgentInitiator.UpdateStatus("Anger", -0.3);
                        CustomAgentInitiator.UpdateStatus("Shame", 1);
                        CustomAgentReceiver.UpdateStatus("Courage", -0.2);

                        CustomAgentInitiator.StopAnimation();
                        CustomAgentReceiver.StopAnimation();
                    }
                    else
                    {
                        InformationManager.DisplayMessage(new InformationMessage(CustomAgentReceiver.Name + " bullied by " + CustomAgentInitiator.Name));
                        CustomAgentInitiator.UpdateStatus("Anger", -0.3);

                        SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", -1);
                        UpdateThirdNPCsBeliefs("Friends", belief, -1);
                    }
                    break;
                case IntentionEnum.Special:
                    if (true)
                    {
                        CustomAgentInitiator.UpdateStatus("Anger", -1);
                        CustomAgentInitiator.UpdateStatus("Courage", -1);

                        SocialNetworkBelief _belief = UpdateParticipantNPCBeliefs("Dating", -1);
                        UpdateThirdNPCsBeliefs("Dating", _belief, -1);

                        foreach (CustomAgent customAgent in CustomAgentList)
                        {
                            customAgent.UpdateBeliefWithNewRelation("Friends", _belief);
                        }

                        InformationManager.DisplayMessage(new InformationMessage(CustomAgentInitiator.Name + " broke up with " + CustomAgentReceiver.Name));
                    }
                    break;
                default:
                    break;
            }
        }
        private SocialNetworkBelief UpdateParticipantNPCBeliefs(string _relationName = "", int _value = 0)
        {
            SocialNetworkBelief belief = CustomAgentInitiator.SelfGetBeliefWithAgent(CustomAgentReceiver);
            if (belief == null)
            {
                List<string> agents = new List<string>() { CustomAgentInitiator.Name, CustomAgentReceiver.Name };
                SocialNetworkBelief newBelief = new SocialNetworkBelief(_relationName, agents, _value);

                CustomAgentInitiator.AddBelief(newBelief);
                CustomAgentReceiver.AddBelief(newBelief);

                belief = newBelief;
            }
            else
            {
                CustomAgentInitiator.UpdateBeliefWithNewValue(belief, _value);
                CustomAgentReceiver.UpdateBeliefWithNewValue(belief, _value);
            }

            return belief;
        }
        private void UpdateThirdNPCsBeliefs(string _relationName, SocialNetworkBelief _belief, int _value)
        {
            foreach (CustomAgent customAgent in CustomAgentList)
            {
                if (customAgent != CustomAgentInitiator && customAgent != CustomAgentReceiver)
                {
                    SocialNetworkBelief belief = customAgent.SocialNetworkBeliefs.Find(b => 
                    b.agents.Contains(CustomAgentInitiator.Name)
                    && b.agents.Contains(CustomAgentReceiver.Name));

                    if (belief == null)
                    {
                        customAgent.AddBelief(_belief);
                    }
                    else
                    {
                        customAgent.UpdateBeliefWithNewValue(_belief, _value);
                    }

                    //Decrease Dating relationship if my partner accepted romantic intentions from other
                    //Otherwise it will increase
                    if (_relationName == "Dating")
                    {
                        int datingHowMany = customAgent.CheckHowManyTheAgentIsDating(customAgent);
                        if (datingHowMany > 0)
                        {
                            if (belief.agents.Contains(CustomAgentInitiator.Name) || belief.agents.Contains(CustomAgentReceiver.Name))
                            {
                                customAgent.UpdateBeliefWithNewValue(belief, _value * -1);
                            }
                        }
                    }
                }
            }
        }
        internal int InitiadorVolition()
        {
            int initialValue = 0;

            InfluenceRule IR = new InfluenceRule(CustomAgentInitiator, CustomAgentReceiver, false, initialValue)
            {
                RelationName = SEName,
                RelationType = Intention
            };
            int finalVolition = ComputeVolitionWithInfluenceRule(IR, CustomAgentInitiator, CustomAgentReceiver);
            finalVolition = CheckMemory(finalVolition, 2);

            CustomAgentInitiator.SEVolition = finalVolition;

            //InformationManager.DisplayMessage(new InformationMessage(SEName + " > " + finalVolition.ToString()));

            return CustomAgentInitiator.SEVolition;
        }
        private int CheckMemory(int finalVolition, int multiplyToDecrease)
        {
            int howManyTimes = CustomAgentInitiator.MemorySEs.Count(m => m.NPC_Name == CustomAgentReceiver.Name && m.SE_Name == SEName);
            if (howManyTimes > 0)
            {
                finalVolition -= howManyTimes * multiplyToDecrease;
            }

            return finalVolition;
        }
        internal int ReceiverVolition()
        {
            int initialValue = 0;

            InfluenceRule IR = new InfluenceRule(CustomAgentInitiator, CustomAgentReceiver, true, initialValue)
            {
                RelationName = SEName,
                RelationType = Intention
            };
            int finalVolition = ComputeVolitionWithInfluenceRule(IR, CustomAgentReceiver, CustomAgentInitiator);

            CustomAgentReceiver.SEVolition = finalVolition;

            //InformationManager.DisplayMessage(new InformationMessage(SEName + " > " + finalVolition.ToString()));

            return CustomAgentReceiver.SEVolition;
        }
        private int ComputeVolitionWithInfluenceRule(InfluenceRule IR, CustomAgent agentWhoWillCheck, CustomAgent agentChecked)
        {
            string relation = GetRelationType(IR);

            IR.InitialValue = IR.CheckGoals(relation);

            IR.InitialValue += (agentWhoWillCheck == CustomAgentInitiator) ? IR.CheckInitiatorTriggerRules(agentWhoWillCheck, agentChecked, IR.RelationName) : 0;

            IR.InitialValue += IR.GetValueParticipantsRelation(agentWhoWillCheck, agentChecked);
            IR.InitialValue += IR.SRunRules();

            return IR.InitialValue;
        }


        public void PlayerConversationWithNPC(string relation, int value)
        {
            SocialNetworkBelief belief = UpdateParticipantNPCBeliefs(relation, value);
            UpdateThirdNPCsBeliefs(relation, belief, value);
        }
        private static string GetRelationType(InfluenceRule IR)
        {
            string relation = "";
            switch (IR.RelationType)
            {
                case IntentionEnum.Positive:
                case IntentionEnum.Negative:
                    relation = "Friends";
                    break;
                case IntentionEnum.Romantic:
                    relation = "Dating";
                    break;
                case IntentionEnum.Hostile:
                    break;
                case IntentionEnum.Special:
                    break;
                default:
                    break;
            }

            return relation;
        }

        public enum IntentionEnum
        {
            Undefined = -1,
            Positive,
            Romantic,
            Negative,
            Hostile,
            Special,
            AllTypes
        }
        public IntentionEnum Intention { get; private set; }
        public bool SocialExchangeDoneAndReacted { get; set; }
        public bool ReceptorIsPlayer { get; set; }
        public string SEName { get; set; }
        private Random Rnd { get; set; }
        private int auxToCheckWhoIsSpeaking;
        private int index;

        private List<CustomAgent> CustomAgentList { get; set; }
        public CustomAgent CustomAgentInitiator { get; set; }
        public CustomAgent CustomAgentReceiver { get; set; }
        private Agent AgentInitiator { get; set; }
        private Agent AgentReceiver { get; set; }

        //*
        private void ComputeOutcome(int _SEVolition, int minThreshold, int maxThreshold)
        {
            if (_SEVolition > maxThreshold)
            {
                //Accepted
            }
            else if (_SEVolition < maxThreshold && _SEVolition > minThreshold)
            {
                //Neutral
            }
            else if (_SEVolition < minThreshold)
            {
                //Rejected
            }
        }
        //private void RunTriggerRulesForEveryone()
        //{
        //    foreach (CustomAgent _customAgent in CustomAgentList)
        //    {
        //        _customAgent.RunTriggerRules();
        //    }
        //}
        //*
    }
}