using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace FriendlyLords
{
    public class CIF_SocialExchange
    {
        public CIF_SocialExchange(CIFManager.SEs_Enum seEnum, CIF_Character _customAgentinitiator, List<CIF_Character> customAgents)
        {
            SE_Enum = seEnum;

            if (_customAgentinitiator != null)
            {
                this.AgentInitiator = _customAgentinitiator.AgentReference;
                this.CustomAgentInitiator = _customAgentinitiator;

                this.CustomAgentReceiver = CustomAgentInitiator.customAgentTarget;
                this.AgentReceiver = CustomAgentReceiver.AgentReference;

                this.CustomAgentList = customAgents;
                this.index = -1;

                if (AgentInitiator == Agent.Main || AgentReceiver == Agent.Main)
                {
                    SetIntention(false, true);
                }
                else 
                {
                    CustomAgentReceiver.UpdateTarget(_customAgentinitiator.Name, _customAgentinitiator.Id);
                    SetIntention(true, true);
                }
                
                CustomAgentInitiator.UpdateTarget(CustomAgentReceiver.Name, CustomAgentReceiver.Id);
            }
        }

        public void OnInitialize(Random _rnd)
        {
            Rnd = _rnd;

            IsCompleted = false;

            ReceptorIsPlayer = AgentReceiver.Name == Agent.Main.Name;

            CustomAgentReceiver.Busy = true;
            CustomAgentReceiver.SocialMove_SE = SE_Enum;
        }

        private void SetIntention(bool setMarkerAndAnimation, bool OnSocialExchange)
        {
            switch (SE_Enum)
            {
                case CIFManager.SEs_Enum.Compliment:
                case CIFManager.SEs_Enum.GiveGift:
                case CIFManager.SEs_Enum.Admiration:
                    Intention = IntentionEnum.Positive;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTypeRef = 0;
                        CustomAgentInitiator.PlayAnimation("act_greeting_front_1");
                        CustomAgentReceiver.PlayAnimation("act_greeting_front_2");
                    }
                    break;
                case CIFManager.SEs_Enum.Jealous:
                    Intention = IntentionEnum.Negative;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTypeRef = 2;
                        CustomAgentInitiator.PlayAnimation("act_gossip");
                        CustomAgentReceiver.PlayAnimation("act_gossip_2");
                    }
                    break;
                case CIFManager.SEs_Enum.FriendSabotage:
                    Intention = IntentionEnum.Negative;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTypeRef = 1;
                        CustomAgentInitiator.PlayAnimation("act_gossip");
                        CustomAgentReceiver.PlayAnimation("act_gossip_2");
                    }
                    break;
                case CIFManager.SEs_Enum.Flirt:
                    Intention = IntentionEnum.Romantic;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTypeRef = 0;
                        CustomAgentInitiator.PlayAnimation("act_greeting_front_1");
                        CustomAgentReceiver.PlayAnimation("act_greeting_front_2");
                    }
                    break;
                case CIFManager.SEs_Enum.Bully:
                    Intention = IntentionEnum.Hostile;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTypeRef = 2;
                        CustomAgentInitiator.PlayAnimation("act_bully");
                        CustomAgentReceiver.PlayAnimation("act_bullied");
                    }
                    break;
                case CIFManager.SEs_Enum.RomanticSabotage:
                    Intention = IntentionEnum.Hostile;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTypeRef = 1;
                        CustomAgentInitiator.PlayAnimation("act_gossip");
                        CustomAgentReceiver.PlayAnimation("act_gossip_2");
                    }
                    break;
                case CIFManager.SEs_Enum.AskOut:
                case CIFManager.SEs_Enum.Break:
                case CIFManager.SEs_Enum.HaveAChild:
                    Intention = IntentionEnum.Special;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTypeRef = 1;
                        CustomAgentInitiator.PlayAnimation("act_argue_3");
                        CustomAgentReceiver.PlayAnimation("act_argue_4");
                    }
                    break;
                default:
                    Intention = IntentionEnum.Undefined;
                    if (setMarkerAndAnimation)
                    {
                        CustomAgentInitiator.MarkerTypeRef = 0;
                    }
                    break;
            }
        }

        public void OnGoingSocialExchange(Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dialogsDictionary, string _CurrentLocation)
        {
            //CustomAgentReceiver.selfAgent.SetLookAgent(AgentInitiator);
            CustomAgentInitiator.AgentReference.SetLookAgent(AgentReceiver);

            if (auxToCheckWhoIsSpeaking % 2 == 0)
            {
                index++;

                CustomAgentInitiator.AgentGetMessage(true, CustomAgentInitiator, CustomAgentReceiver, Rnd, index, _dialogsDictionary, _CurrentLocation);
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

                CustomAgentReceiver.MarkerTypeRef = CustomAgentInitiator.MarkerTypeRef;

                CustomAgentReceiver.AgentGetMessage(false, CustomAgentInitiator, CustomAgentReceiver, Rnd, index, _dialogsDictionary, _CurrentLocation);

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
                if (ReceptorIsPlayer) 
                { 
                    CustomAgentInitiator.TalkingWithPlayer = true;
                    AgentInitiator.OnUse(AgentReceiver); 
                }
                IsCompleted = true;  
            }
        }

        public void OnFinalize()
        {
            List<string> tempListNames = new List<string>() { CustomAgentInitiator.Name , CustomAgentReceiver.Name };
            List<int> tempListIds = new List<int>() { CustomAgentInitiator.Id , CustomAgentReceiver.Id };
            MemorySE newMemory = new MemorySE(tempListNames, tempListIds, SE_Enum.ToString());
            CustomAgentInitiator.AddToMemory(newMemory);
            CustomAgentReceiver.AddToMemory(newMemory);

            if (!ReceptorIsPlayer)
            { 
                UpdateBeliefsAndStatus();
            }

            ResetCustomAgentVariables(CustomAgentReceiver);
            ResetCustomAgentVariables(CustomAgentInitiator);
            
            ReceptorIsPlayer = false;
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
                if (SE_Enum == CIFManager.SEs_Enum.AskOut)
                {
                    AskOutMethod(false);
                }
                else if (SE_Enum == CIFManager.SEs_Enum.Break)
                {
                    CustomAgentInitiator.UpdateAllStatus(0, 0, -1, -1, 0, 0);

                    BreakUpMethod();
                }
                else if (SE_Enum == CIFManager.SEs_Enum.Admiration)
                {

                }
            }
        }

        private void ConsequencesFromHostileIntention()
        {
            if (CustomAgentReceiver.SE_Accepted)
            {
                if (SE_Enum == CIFManager.SEs_Enum.Bully || SE_Enum == CIFManager.SEs_Enum.RomanticSabotage)
                {
                    //Bully or RomanticSabotage
                    CustomAgentInitiator.UpdateAllStatus(0, 0.5, 0, -0.3, 1, 0);
                    CustomAgentReceiver.UpdateAllStatus(0, 0.5, -0.2, 0, 0, 0);
                }               
            }
            else
            {
                CustomAgentInitiator.UpdateAllStatus(0, 0.5, 0, 0.5, 0, 0);

                SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", -1);
            }
            UpdateNPCsNearSocialMove();
        }

        private void ConsequencesFromNegativeIntention()
        {
            if (CustomAgentReceiver.SE_Accepted)
            {
                if (SE_Enum == CIFManager.SEs_Enum.Jealous)
                {
                    CustomAgentInitiator.UpdateAllStatus(0, -1, 0, -0.3, 0, 0);
                    CustomAgentInitiator.AddToTriggerRulesList(new TriggerRule("Bully", CustomAgentInitiator.thirdAgent, CustomAgentInitiator.thirdAgentId));
                }
                else if (SE_Enum == CIFManager.SEs_Enum.FriendSabotage)
                {
                    CustomAgentInitiator.UpdateAllStatus(0, -1, 0, -0.3, 0, 0);
                    CustomAgentInitiator.AddToTriggerRulesList(new TriggerRule("Bully", CustomAgentInitiator.thirdAgent, CustomAgentInitiator.thirdAgentId));

                }

                SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", -1);
            }
            else
            {
                if (SE_Enum == CIFManager.SEs_Enum.Jealous)
                {
                    CustomAgentInitiator.UpdateAllStatus(0, -1, 0, -0.3, 1, 0);
                    CustomAgentReceiver.UpdateAllStatus(0, 0, -0.2, 0, 0, 0);
                }
                else if (SE_Enum == CIFManager.SEs_Enum.FriendSabotage)
                {
                    CustomAgentInitiator.UpdateAllStatus(0, -1, -0.2, 0, 0, 0);

                    //Decreases relation 
                    CIF_Character CAtoDecrease = CustomAgentReceiver.GetCustomAgentByName(CustomAgentInitiator.thirdAgent, CustomAgentInitiator.thirdAgentId);

                    if (CAtoDecrease != null)
                    {
                        SocialNetworkBelief belief = CustomAgentReceiver.SelfGetBeliefWithAgent(CAtoDecrease);
                        CustomAgentReceiver.UpdateBeliefWithNewValue(belief, -1);

                        if (CAtoDecrease.AgentReference.IsHero && CAtoDecrease.AgentReference == Agent.Main)
                        {
                            ChangeHeroRelationInGame(-1, CAtoDecrease);
                        }
                    }
                }
            }
            UpdateNPCsNearSocialMove();
        }

        private void ConsequencesFromRomanticIntention()
        {
            if (CustomAgentReceiver.SE_Accepted)
            {
                if (SE_Enum == CIFManager.SEs_Enum.Flirt)
                {
                    SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Dating", 1);
                }
            }
            else
            {
                CustomAgentInitiator.AddToTriggerRulesList(new TriggerRule("Bully", CustomAgentReceiver.Name, CustomAgentReceiver.Id));
                CustomAgentInitiator.UpdateAllStatus(0, 0, 0, 1, 0, 0);
            }
            UpdateNPCsNearSocialMove();
        }

        private void ConsequencesFromPositiveIntention()
        {
            if (CustomAgentReceiver.SE_Accepted)
            {
                if (SE_Enum == CIFManager.SEs_Enum.Compliment)
                {
                    SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", 1);

                    CustomAgentInitiator.UpdateAllStatus(-1, 0, 0, 0, 0, 0);
                }
                else if (SE_Enum == CIFManager.SEs_Enum.GiveGift)
                {
                    SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", 1);

                    CustomAgentInitiator.UpdateAllStatus(-1, 0, 0, 0, 0, 0);

                    Item tempItem = CustomAgentInitiator.GetItem();
                    CustomAgentInitiator.RemoveItem(tempItem.itemName, -1);
                    CustomAgentReceiver.AddItem(tempItem.itemName, 1);
                }
                else if (SE_Enum == CIFManager.SEs_Enum.Admiration)
                {

                }
            }
            else
            {
                CustomAgentInitiator.AddToTriggerRulesList(new TriggerRule("Bully", CustomAgentReceiver.Name, CustomAgentReceiver.Id));
                CustomAgentInitiator.UpdateAllStatus(-1, 0, 0, 0, 1, 0);
            }
            
            UpdateNPCsNearSocialMove();
        }


        private static void ChangeHeroRelationInGame(int value, CIF_Character customAgent)
        {
            Hero hero = Hero.FindFirst(h => h.CharacterObject == customAgent.AgentReference.Character);
            if (hero != null && hero != Hero.MainHero)
            {
                float relationWithPlayer = hero.GetRelationWithPlayer();
                int newValue = (int)(relationWithPlayer + value);
                if (value > 0)
                {
                    //InformationManager.AddQuickInformation(new TextObject("Your relation is increased by " + value + " to " + newValue + " with " + hero.Name + "."), 0, hero.CharacterObject);
                    Hero.MainHero.SetPersonalRelation(hero, newValue);
                }
                else
                {
                    //InformationManager.AddQuickInformation(new TextObject("Your relation is decreased by " + value + " to " + newValue + " with " + hero.Name + "."), 0, hero.CharacterObject);
                    Hero.MainHero.SetPersonalRelation(hero, newValue);
                }
            }
        }

        private bool InteractionSawByThisNPC(CIF_Character customAgentInitiator, CIF_Character customAgent)
        {
            if (Agent.Main != null)
            {
                if (customAgentInitiator != customAgent && customAgent.AgentReference != Agent.Main && customAgentInitiator.AgentReference.Position.Distance(customAgent.AgentReference.Position) <= 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private void UpdateNPCsNearSocialMove() // Independentemente se aceitou ou não
        {
            foreach (CIF_Character customAgent in CustomAgentList)
            {
                if (customAgent != CustomAgentInitiator && customAgent != CustomAgentReceiver && InteractionSawByThisNPC(CustomAgentInitiator, customAgent)) // ele viu?
                {
                    SocialNetworkBelief beliefWithInitiator = customAgent.SelfGetBeliefWithAgent(CustomAgentInitiator);
                    SocialNetworkBelief beliefWithReceiver = customAgent.SelfGetBeliefWithAgent(CustomAgentReceiver);

                    int value;
                    //tem relaçao dating com o initiator e vê o initiator a começar interação romantic? Vai perder 1 pt na relaçao
                    if (beliefWithInitiator != null && beliefWithInitiator.value < 0 && beliefWithInitiator.relationship == "Dating")
                    {
                        value = -1;
                        customAgent.UpdateBeliefWithNewValue(beliefWithInitiator, value);
                    }

                    if (beliefWithReceiver != null)
                    {
                        if (beliefWithReceiver.relationship == "Dating") //tem relaçao com o receiver e essa relação é dating? Vai garrear com o Initiator
                        {
                            TriggerRule triggerRule = new TriggerRule("RomanticSabotage", CustomAgentInitiator.AgentReference.Name, CustomAgentInitiator.Id);
                            customAgent.AddToTriggerRulesList(triggerRule);
                        }
                        else // tem relaçao Friends
                        {
                            if (beliefWithReceiver.value < 0) //***  NPC A nao se dá bem com NPC B, então relaçao com initiator cai 1 pt se tiver interações positivas com B ou viceversa
                            {
                                bool RelationIncreased = false;
                                value = RelationIncreased ? -1 : 1;

                                customAgent.UpdateBeliefWithNewValue(beliefWithInitiator, value);

                                if (CustomAgentInitiator.AgentReference == Agent.Main && customAgent.AgentReference.IsHero)
                                {
                                    ChangeHeroRelationInGame(value, customAgent);
                                }
                            }
                            else if (beliefWithReceiver.value > 0) // NPC A dá-se bem com NPC B, então relaçao com initiator sobe 1 pt se tiver interações positivas com B ou viceversa
                            {
                                bool RelationIncreased = false;
                                value = RelationIncreased ? 1 : -1;

                                customAgent.UpdateBeliefWithNewValue(beliefWithInitiator, value);

                                if (CustomAgentInitiator.AgentReference == Agent.Main && customAgent.AgentReference.IsHero)
                                {
                                    ChangeHeroRelationInGame(value, customAgent);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void BreakUpMethod()
        {
            SocialNetworkBelief _belief = UpdateParticipantNPCBeliefs("Dating", -1);

            UpdateNPCsNearSocialMove();

            foreach (CIF_Character customAgent in CustomAgentList)
            {
                customAgent.UpdateBeliefWithNewRelation("Friends", _belief);
            }
        }

        public void AskOutMethod(bool PlayerIsInitiator)
        {
            SocialNetworkBelief _belief = UpdateParticipantNPCBeliefs("Friends", 1);

            foreach (CIF_Character customAgent in CustomAgentList)
            {
                customAgent.UpdateBeliefWithNewRelation("Dating", _belief);
            }

            if (PlayerIsInitiator)
            {
                UpdateNPCsNearSocialMove();
            }
        }

        private SocialNetworkBelief UpdateParticipantNPCBeliefs(string _relationName = "", int _value = 0)
        {
            SocialNetworkBelief belief = CustomAgentInitiator.SelfGetBeliefWithAgent(CustomAgentReceiver);
            if (belief == null)
            {
                List<string> agents = new List<string>() { CustomAgentInitiator.AgentReference.Name, CustomAgentReceiver.AgentReference.Name };
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
        
        public int InitiadorVolition()
        {
            SetIntention(false, false);

            int initialValue = 0;

            InfluenceRule IR = new InfluenceRule(CustomAgentInitiator, CustomAgentReceiver, false, initialValue)
            {
                SE = SE_Enum,
                RelationIntention = Intention
            };
            int Volition = ComputeVolitionWithInfluenceRule(IR, CustomAgentInitiator, CustomAgentReceiver);
            Volition = CheckMemory(Volition, 3);

            CustomAgentInitiator.SEVolition = Volition;

            return CustomAgentInitiator.SEVolition;
        }
        
        private int CheckMemory(int finalVolition, int multiplyToDecrease)
        {
            int howManyTimes = CustomAgentInitiator.MemorySEs.Count(
                memorySlot => 
                memorySlot.SE_Name == SE_Enum.ToString() && 
                memorySlot.agents.Contains(CustomAgentInitiator.Name) && 
                memorySlot.agents.Contains(CustomAgentReceiver.Name) && 
                memorySlot.IDs.Contains(CustomAgentInitiator.Id) && 
                memorySlot.IDs.Contains(CustomAgentReceiver.Id)
                );

            if (howManyTimes > 0)
            {
                finalVolition -= howManyTimes * multiplyToDecrease;
            }

            int howManyTimesNPCDidThatSE = CustomAgentInitiator.MemorySEs.Count(
                memorySlot =>
                memorySlot.SE_Name == SE_Enum.ToString()
                );

            if (howManyTimesNPCDidThatSE > 0)
            {
                finalVolition -= howManyTimesNPCDidThatSE * multiplyToDecrease;
            }

            return finalVolition;
        }
        
        public int ReceiverVolition()
        {
            int initialValue = 0;

            InfluenceRule IR = new InfluenceRule(CustomAgentInitiator, CustomAgentReceiver, true, initialValue)
            {
                SE = SE_Enum,
                RelationIntention = Intention
            };
            int finalVolition = ComputeVolitionWithInfluenceRule(IR, CustomAgentReceiver, CustomAgentInitiator);

            CustomAgentReceiver.SEVolition = finalVolition;

            return CustomAgentReceiver.SEVolition;
        }
        
        private int ComputeVolitionWithInfluenceRule(InfluenceRule IR, CIF_Character agentWhoWillCheck, CIF_Character agentChecked)
        {
            IR.InitialValue += (agentWhoWillCheck == CustomAgentInitiator) ? IR.CheckInitiatorTriggerRules(agentWhoWillCheck, agentChecked, IR.SE.ToString()) : 0;

            IR.InitialValue += IR.GetValueParticipantsRelation(agentWhoWillCheck, agentChecked);
            IR.InitialValue += IR.SRunRules();

            return IR.InitialValue;
        }

        public void PlayerConversationWithNPC(string relation, int value, bool PlayerInteractingWithHero)
        {
            SocialNetworkBelief belief = UpdateParticipantNPCBeliefs(relation, value);

            UpdateNPCsNearSocialMove();

            if (!PlayerInteractingWithHero)
            {
                if (value > 0)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Relation " + belief.relationship + " between " + belief.agents[0] + " and " + belief.agents[1] + " increased to " + belief.value));
                }
                else if (value < 0)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Relation " + belief.relationship + " between " + belief.agents[0] + " and " + belief.agents[1] + " decreased to " + belief.value));
                }
            }
        }

        private void ResetCustomAgentVariables(CIF_Character customAgent)
        {
            customAgent.SocialMove_SE = CIFManager.SEs_Enum.Undefined;
            customAgent.IsInitiator = false;
            customAgent.FullMessage = null;
            customAgent.Message = "";
            customAgent.Busy = false;

            if (customAgent.AgentReference == Agent.Main)
            {
                customAgent.UpdateAllStatus(0, 0, 0, 0, 0, 10);
            }
            else { customAgent.EnoughRest = false; }
            
            customAgent.customAgentTarget = null;
            customAgent.thirdAgent = "";
            customAgent.thirdAgentId = 0;
            customAgent.MarkerTypeRef = 1;
            customAgent.StopAnimation();

            customAgent.EndFollowBehavior();

            /*if (customAgent.selfAgent != Agent.Main)
            {
                customAgent.EndFollowBehavior();
                if (!customAgent.CompanionFollowingPlayer)
                {
                   customAgent.StartFollowBehavior(customAgent.selfAgent, Agent.Main);
                }
            }*/
        }    

        public bool IsCompleted { get; set; }
        public bool ReceptorIsPlayer { get; set; }
        public bool ReduceDelay { get; set; }

        public CIFManager.SEs_Enum SE_Enum { get; }

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

        private Random Rnd { get; set; }
        private int auxToCheckWhoIsSpeaking { get; set; }
        private int index { get; set; }

        private List<CIF_Character> CustomAgentList { get; set; }
        public CIF_Character CustomAgentInitiator { get; set; }
        public CIF_Character CustomAgentReceiver { get; set; }
        private Agent AgentInitiator { get; set; }
        private Agent AgentReceiver { get; set; }

    }
}