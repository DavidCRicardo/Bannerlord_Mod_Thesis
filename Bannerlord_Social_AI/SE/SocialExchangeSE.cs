using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace Bannerlord_Social_AI
{
    public class SocialExchangeSE
    {
        public SocialExchangeSE(string _SEName, CustomAgent _customAgentinitiator, List<CustomAgent> customAgents)
        {
            this.SEName = _SEName;

            if (_customAgentinitiator != null)
            {
                this.AgentInitiator = _customAgentinitiator.selfAgent;
                this.CustomAgentInitiator = _customAgentinitiator;

                this.CustomAgentReceiver = CustomAgentInitiator.customAgentTarget;
                this.AgentReceiver = CustomAgentReceiver.selfAgent;

                this.CustomAgentList = customAgents;
                this.index = -1;
            }  
        }

        public void OnInitialize(Random _rnd)
        {
            Rnd = _rnd;

            switch (SEName)
            {
                case "Compliment":
                    Intention = IntentionEnum.Positive;
                    CustomAgentInitiator.MarkerTyperRef = 0;
                    break;
                case "FriendSabotage":
                case "Jealous":
                    Intention = IntentionEnum.Negative;
                    CustomAgentInitiator.MarkerTyperRef = 1;
                    break;
                case "AskOut":
                case "Flirt":
                    Intention = IntentionEnum.Romantic;
                    CustomAgentInitiator.MarkerTyperRef = 0;
                    break;
                case "RomanticSabotage":
                    Intention = IntentionEnum.Hostile;
                    CustomAgentInitiator.MarkerTyperRef = 1;
                    break;
                case "Bully":
                    Intention = IntentionEnum.Hostile;
                    CustomAgentInitiator.MarkerTyperRef = 2;
                    break;
                case "Break":
                    Intention = IntentionEnum.Special;
                    CustomAgentInitiator.MarkerTyperRef = 0;
                    break;
                default:
                    Intention = IntentionEnum.Undefined;
                    CustomAgentInitiator.MarkerTyperRef = 0;
                    break;
            }

            SocialExchangeDoneAndReacted = false;

            ReceptorIsPlayer = AgentReceiver.Name == Agent.Main.Name;

            CustomAgentReceiver.Busy = true;
            CustomAgentReceiver.SocialMove = SEName;
            CustomAgentReceiver.selfAgent.SetLookAgent(AgentInitiator);
        }

        public void OnGoingSocialExchange(Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dialogsDictionary)
        {
            if (auxToCheckWhoIsSpeaking % 2 == 0)
            {
                index++;

                CustomAgentInitiator.AgentGetMessage(true, CustomAgentInitiator, CustomAgentReceiver, Rnd, index, _dialogsDictionary);
                if (CustomAgentInitiator.Message != "")
                {
                    CustomAgentReceiver.Message = "";
                    ReduceDelay = false;
                }
                else { ReduceDelay = true; }

                auxToCheckWhoIsSpeaking++;
            }
            else
            {
                CustomAgentReceiver.SEVolition = ReceiverVolition();
                CustomAgentReceiver.AgentGetMessage(false, CustomAgentInitiator, CustomAgentReceiver, Rnd, index, _dialogsDictionary);

                if (CustomAgentReceiver.Message != "")
                {
                    CustomAgentInitiator.Message = "";
                    ReduceDelay = false;
                }
                else { ReduceDelay = true; }

                auxToCheckWhoIsSpeaking = 0;
            }

            if ((CustomAgentInitiator.Message == "" && CustomAgentReceiver.Message == "") || ReceptorIsPlayer)
            {
                if (ReceptorIsPlayer) { AgentInitiator.OnUse(AgentReceiver); }
                SocialExchangeDoneAndReacted = true; 
            }
        }

        public void OnFinalize()
        {
            AgentInitiator.OnUseStopped(AgentReceiver, true, 0);

            CustomAgentInitiator.AddToMemory(new MemorySE(CustomAgentReceiver.selfAgent.Name, CustomAgentReceiver.Id, SEName));
            CustomAgentReceiver.AddToMemory(new MemorySE(CustomAgentInitiator.selfAgent.Name, CustomAgentInitiator.Id, SEName));

            ResetCustomAgentVariables(CustomAgentInitiator);
            if (!ReceptorIsPlayer)
            {
                ResetCustomAgentVariables(CustomAgentReceiver);
                UpdateBeliefsAndStatus();
            }
        }

        private void UpdateBeliefsAndStatus()
        {
            switch (Intention)
            {
                case IntentionEnum.Positive:
                    ConsequencesFromPositiveIntention();
                    break;

                case IntentionEnum.Romantic:
                    ConsequencesFromRomanticIntention();
                    break;

                case IntentionEnum.Negative:
                    ConsequencesFromNegativeIntention();
                    break;

                case IntentionEnum.Hostile:
                    ConsequencesFromHostileIntention();
                    break;

                case IntentionEnum.Special:
                    ConsequencesFromSpecialIntention();
                    break;

                default:
                    break;
            }
        }

        private void ConsequencesFromSpecialIntention()
        {
            if (true)
            {
                CustomAgentInitiator.UpdateAllStatus(0, -1, -1, 0, 0);

                BreakUpMethod();
            }
        }

        private void ConsequencesFromHostileIntention()
        {
            if (CustomAgentReceiver.SE_Accepted)
            {
                //Bully or RomanticSabotage
                CustomAgentInitiator.UpdateAllStatus(0, 0, -0.3, 1, 0);
                CustomAgentReceiver.UpdateAllStatus(0, -0.2, 0, 0, 0);
            }
            else
            {
                CustomAgentInitiator.UpdateAllStatus(0, 0, -0.3, 0, 0);

                SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", -1);
                UpdateThirdNPCsBeliefs("Friends", belief, -1);
            }
            NPCsNearFriendSocialMove();
        }

        private void ConsequencesFromNegativeIntention()
        {
            if (CustomAgentReceiver.SE_Accepted)
            {
                //Decreases relation with Initiator
                if (SEName == "Jealous")
                {
                    

                    CustomAgentInitiator.UpdateAllStatus(0, 0, -0.3, 0, 0);
                }
                else if (SEName == "FriendSabotage")
                {

                }

                SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", -1);
                UpdateThirdNPCsBeliefs("Friends", belief, -1);
            }
            else
            {
                if (SEName == "Jealous")
                {
                    CustomAgentInitiator.UpdateAllStatus(0, 0, -0.3, 1, 0);
                    CustomAgentReceiver.UpdateAllStatus(0, -0.2, 0, 0, 0);
                }

                else if (SEName == "FriendSabotage")
                {
                    //Decreases relation 
                    CustomAgent CAtoDecrease = CustomAgentReceiver.GetCustomAgentByName(CustomAgentInitiator.thirdAgent, CustomAgentInitiator.thirdAgentId);
                    SocialNetworkBelief belief = CustomAgentReceiver.SelfGetBeliefWithAgent(CAtoDecrease);

                    CustomAgentReceiver.UpdateBeliefWithNewValue(belief, -1);
                }
            }
            NPCsNearFriendSocialMove();
        }

        private void ConsequencesFromRomanticIntention()
        {
            if (CustomAgentReceiver.SE_Accepted)
            {
                //Increases Relationship for both
                if (SEName == "AskOut")
                { 
                    AskOutMethod();
                }
                else if (SEName == "Flirt")
                {
                    SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Dating", 1);
                    UpdateThirdNPCsBeliefs("Dating", belief, 1);
                }
            }
            else
            {
                CustomAgentInitiator.UpdateAllStatus(0, 0, 1, 0, 0);
            }

            NPCsNearRomanticSocialMove();
        }

        private void ConsequencesFromPositiveIntention()
        {
            if (CustomAgentReceiver.SE_Accepted)
            {
                if (SEName == "Compliment")
                {
                    SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", 1);
                    UpdateThirdNPCsBeliefs("Friends", belief, 1);

                    CustomAgentInitiator.UpdateAllStatus(-1, 0, 0, 0, 0);
                }
            }
            else
            {
                CustomAgentInitiator.UpdateAllStatus(-1, 0, 0, 1, 0);
            }
        }

        private void NPCsNearFriendSocialMove()
        {
            foreach (CustomAgent customAgent in CustomAgentList)
            {
                if (customAgent != CustomAgentInitiator && customAgent != CustomAgentReceiver)
                {
                    SocialNetworkBelief beliefWithInitiator = customAgent.SelfGetBeliefWithAgent(CustomAgentInitiator);
                    if (beliefWithInitiator != null && beliefWithInitiator.relationship == "Friends" && beliefWithInitiator.value < 0)
                    {
                        customAgent.UpdateBeliefWithNewValue(beliefWithInitiator, -1);
                    }

                    if (beliefWithInitiator != null && beliefWithInitiator.relationship == "Friends" && beliefWithInitiator.value > 0)
                    {
                        customAgent.UpdateBeliefWithNewValue(beliefWithInitiator, 1);
                    } 
                }
            }
        }

        private void NPCsNearRomanticSocialMove()
        {
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
                        TriggerRule triggerRule = new TriggerRule("RomanticSabotage", CustomAgentInitiator.selfAgent.Name, CustomAgentInitiator.Id);
                        customAgent.AddToTriggerRulesList(triggerRule);
                    }
                }
            }
        }

        public void BreakUpMethod()
        {
            SocialNetworkBelief _belief = UpdateParticipantNPCBeliefs("Dating", -1);
            UpdateThirdNPCsBeliefs("Dating", _belief, -1);

            foreach (CustomAgent customAgent in CustomAgentList)
            {
                customAgent.UpdateBeliefWithNewRelation("Friends", _belief);
            }
        }

        public void AskOutMethod()
        {
            SocialNetworkBelief _belief = UpdateParticipantNPCBeliefs("Friends", 1);
            UpdateThirdNPCsBeliefs("Friends", _belief, 1);

            foreach (CustomAgent customAgent in CustomAgentList)
            {
                customAgent.UpdateBeliefWithNewRelation("Dating", _belief);
            }

            NPCsNearRomanticSocialMove();
        }

        private SocialNetworkBelief UpdateParticipantNPCBeliefs(string _relationName = "", int _value = 0)
        {
            SocialNetworkBelief belief = CustomAgentInitiator.SelfGetBeliefWithAgent(CustomAgentReceiver);
            if (belief == null)
            {
                List<string> agents = new List<string>() { CustomAgentInitiator.selfAgent.Name, CustomAgentReceiver.selfAgent.Name };
                List<int> _ids = new List<int>() { CustomAgentInitiator.Id, CustomAgentReceiver.Id };

                SocialNetworkBelief newBelief = new SocialNetworkBelief(_relationName, agents, _ids, _value);

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
                    b.agents.Contains(CustomAgentInitiator.selfAgent.Name) && b.agents.Contains(CustomAgentReceiver.selfAgent.Name));

                    if (belief == null)
                    {
                        customAgent.AddBelief(_belief);
                        belief = _belief;
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
                            if (belief.agents.Contains(CustomAgentInitiator.selfAgent.Name) || belief.agents.Contains(CustomAgentReceiver.selfAgent.Name))
                            {
                                customAgent.UpdateBeliefWithNewValue(belief, _value * -1);
                            }
                        }
                    }
                }
            }
        }
        
        public int InitiadorVolition()
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

            return CustomAgentInitiator.SEVolition;
        }
        
        private int CheckMemory(int finalVolition, int multiplyToDecrease)
        {
            int howManyTimes = CustomAgentInitiator.MemorySEs.Count(
                memorySlot => memorySlot.NPC_Name == CustomAgentReceiver.selfAgent.Name && memorySlot.SE_Name == SEName);

            if (howManyTimes > 0)
            {
                finalVolition -= howManyTimes * multiplyToDecrease;
            }

            return finalVolition;
        }
        
        public int ReceiverVolition()
        {
            int initialValue = 0;

            InfluenceRule IR = new InfluenceRule(CustomAgentInitiator, CustomAgentReceiver, true, initialValue)
            {
                RelationName = SEName,
                RelationType = Intention
            };
            int finalVolition = ComputeVolitionWithInfluenceRule(IR, CustomAgentReceiver, CustomAgentInitiator);

            CustomAgentReceiver.SEVolition = finalVolition;

            return CustomAgentReceiver.SEVolition;
        }
        
        private int ComputeVolitionWithInfluenceRule(InfluenceRule IR, CustomAgent agentWhoWillCheck, CustomAgent agentChecked)
        {
            IR.InitialValue += (agentWhoWillCheck == CustomAgentInitiator) ? IR.CheckInitiatorTriggerRules(agentWhoWillCheck, agentChecked, IR.RelationName) : 0;

            IR.InitialValue += IR.GetValueParticipantsRelation(agentWhoWillCheck, agentChecked);
            IR.InitialValue += IR.SRunRules();

            return IR.InitialValue;
        }

        public void PlayerConversationWithNPC(string relation, int value)
        {
            SocialNetworkBelief belief = UpdateParticipantNPCBeliefs(relation, value);
            UpdateThirdNPCsBeliefs(relation, belief, value);

            NPCsNearFriendSocialMove();
        }

        private void ResetCustomAgentVariables(CustomAgent customAgent)
        {
            customAgent.SocialMove = "";
            customAgent.IsInitiator = false;
            customAgent.FullMessage = null;
            customAgent.Busy = false;
            customAgent.Message = "";
            customAgent.EnoughRest = false;
            customAgent.customAgentTarget = null;
            customAgent.MarkerTyperRef = 0;
            customAgent.StopAnimation();
            customAgent.EndFollowBehavior();
        }

        public IntentionEnum Intention { get; private set; }
        public enum IntentionEnum
        {
            Undefined = -1,
            Positive,
            Romantic,
            Negative,
            Hostile,
            Special
        }

        public bool SocialExchangeDoneAndReacted { get; set; }
        public bool ReceptorIsPlayer { get; set; }
        public bool ReduceDelay { get; set; }

        public string SEName { get; set; }
        private Random Rnd { get; set; }
        private int auxToCheckWhoIsSpeaking { get; set; }
        private int index { get; set; }

        private List<CustomAgent> CustomAgentList { get; set; }
        public CustomAgent CustomAgentInitiator { get; set; }
        public CustomAgent CustomAgentReceiver { get; set; }
        private Agent AgentInitiator { get; set; }
        private Agent AgentReceiver { get; set; }

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
    }
}