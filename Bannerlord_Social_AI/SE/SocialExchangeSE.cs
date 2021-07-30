using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Bannerlord_Social_AI
{
    public class SocialExchangeSE
    {
        public SocialExchangeSE(CustomMissionNameMarkerVM.SEs_Enum seEnum, CustomAgent _customAgentinitiator, List<CustomAgent> customAgents)
        {
            //this.SEName = seEnum.ToString();
            SE_Enum = seEnum;

            if (_customAgentinitiator != null)
            {
                this.AgentInitiator = _customAgentinitiator.selfAgent;
                this.CustomAgentInitiator = _customAgentinitiator;

                this.CustomAgentReceiver = CustomAgentInitiator.customAgentTarget;
                this.AgentReceiver = CustomAgentReceiver.selfAgent;

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
        public string SEName { get; set; }

        public void OnInitialize(Random _rnd)
        {
            Rnd = _rnd;

            SocialExchangeDoneAndReacted = false;

            ReceptorIsPlayer = AgentReceiver.Name == Agent.Main.Name;

            CustomAgentReceiver.Busy = true;
            CustomAgentReceiver.SocialMove_SE = SE_Enum;
        }

        private void SetIntention(bool setMarkerAndAnimation, bool OnSocialExchange)
        {
            switch (SE_Enum)
            {
                case CustomMissionNameMarkerVM.SEs_Enum.Compliment:
                case CustomMissionNameMarkerVM.SEs_Enum.GiveGift:
                case CustomMissionNameMarkerVM.SEs_Enum.Gratitude:
                    Intention = IntentionEnum.Positive;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTyperRef = 0;
                        CustomAgentInitiator.PlayAnimation("act_greeting_front_1");
                        CustomAgentReceiver.PlayAnimation("act_greeting_front_2");
                    }
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.Jealous:
                    Intention = IntentionEnum.Negative;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTyperRef = 2;
                        CustomAgentInitiator.PlayAnimation("act_gossip");
                        CustomAgentReceiver.PlayAnimation("act_gossip_2");
                    }
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.FriendSabotage:
                    Intention = IntentionEnum.Negative;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTyperRef = 1;
                        CustomAgentInitiator.PlayAnimation("act_gossip");
                        CustomAgentReceiver.PlayAnimation("act_gossip_2");
                    }
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.Flirt:
                    Intention = IntentionEnum.Romantic;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTyperRef = 0;
                        CustomAgentInitiator.PlayAnimation("act_greeting_front_1");
                        CustomAgentReceiver.PlayAnimation("act_greeting_front_2");
                    }
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.Bully:
                    Intention = IntentionEnum.Hostile;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTyperRef = 2;
                        CustomAgentInitiator.PlayAnimation("act_bully");
                        CustomAgentReceiver.PlayAnimation("act_bullied");
                    }
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.RomanticSabotage:
                    Intention = IntentionEnum.Hostile;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTyperRef = 1;
                        CustomAgentInitiator.PlayAnimation("act_gossip");
                        CustomAgentReceiver.PlayAnimation("act_gossip_2");
                    }
                    break;
                case CustomMissionNameMarkerVM.SEs_Enum.AskOut:
                case CustomMissionNameMarkerVM.SEs_Enum.Break:
                case CustomMissionNameMarkerVM.SEs_Enum.HaveAChild:
                    Intention = IntentionEnum.Special;
                    if (setMarkerAndAnimation && OnSocialExchange)
                    {
                        CustomAgentInitiator.MarkerTyperRef = 1;
                        CustomAgentInitiator.PlayAnimation("act_argue_3");
                        CustomAgentReceiver.PlayAnimation("act_argue_4");
                    }
                    break;
                default:
                    Intention = IntentionEnum.Undefined;
                    if (setMarkerAndAnimation)
                    {
                        CustomAgentInitiator.MarkerTyperRef = 0;
                    }
                    break;
            }
        }

        public void OnGoingSocialExchange(Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dialogsDictionary, string _CurrentLocation)
        {
            //CustomAgentReceiver.selfAgent.SetLookAgent(AgentInitiator);
            CustomAgentInitiator.selfAgent.SetLookAgent(AgentReceiver);

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
                if (ReceptorIsPlayer) { CustomAgentInitiator.TalkingWithPlayer = true; AgentInitiator.OnUse(AgentReceiver); }
                SocialExchangeDoneAndReacted = true;  
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
                if (SE_Enum == CustomMissionNameMarkerVM.SEs_Enum.AskOut)
                {
                    AskOutMethod(false);
                }
                //if (SEName == "AskOut")
                //{
                //    AskOutMethod(false);
                //}
                else if (SE_Enum == CustomMissionNameMarkerVM.SEs_Enum.Break)
                {
                    CustomAgentInitiator.UpdateAllStatus(0, 0, -1, -1, 0, 0);

                    BreakUpMethod();
                }
                else if (SE_Enum == CustomMissionNameMarkerVM.SEs_Enum.Gratitude)
                {

                }
            }
        }

        private void ConsequencesFromHostileIntention()
        {
            if (CustomAgentReceiver.SE_Accepted)
            {
                if (SE_Enum == CustomMissionNameMarkerVM.SEs_Enum.Bully || SE_Enum == CustomMissionNameMarkerVM.SEs_Enum.RomanticSabotage)
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
                if (SE_Enum == CustomMissionNameMarkerVM.SEs_Enum.Jealous)
                {
                    CustomAgentInitiator.UpdateAllStatus(0, -1, 0, -0.3, 0, 0);
                    CustomAgentInitiator.AddToTriggerRulesList(new TriggerRule("Bully", CustomAgentInitiator.thirdAgent, CustomAgentInitiator.thirdAgentId));
                }
                else if (SE_Enum == CustomMissionNameMarkerVM.SEs_Enum.FriendSabotage)
                {
                    CustomAgentInitiator.UpdateAllStatus(0, -1, 0, -0.3, 0, 0);
                    CustomAgentInitiator.AddToTriggerRulesList(new TriggerRule("Bully", CustomAgentInitiator.thirdAgent, CustomAgentInitiator.thirdAgentId));

                }
                //Decreases relation with Initiator
                //if (SEName == "Jealous")
                //{
                //    CustomAgentInitiator.UpdateAllStatus(0, -1, 0, -0.3, 0, 0);
                //    CustomAgentInitiator.AddToTriggerRulesList(new TriggerRule("Bully", CustomAgentInitiator.thirdAgent, CustomAgentInitiator.thirdAgentId));
                //}
                //else if (SEName == "FriendSabotage")
                //{
                //    CustomAgentInitiator.UpdateAllStatus(0, -1, 0, -0.3, 0, 0);
                //    CustomAgentInitiator.AddToTriggerRulesList(new TriggerRule("Bully", CustomAgentInitiator.thirdAgent, CustomAgentInitiator.thirdAgentId));
                //}

                SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", -1);
            }
            else
            {
                if (SE_Enum == CustomMissionNameMarkerVM.SEs_Enum.Jealous)
                {
                    CustomAgentInitiator.UpdateAllStatus(0, -1, 0, -0.3, 1, 0);
                    CustomAgentReceiver.UpdateAllStatus(0, 0, -0.2, 0, 0, 0);
                }
                else if (SE_Enum == CustomMissionNameMarkerVM.SEs_Enum.FriendSabotage)
                {
                    CustomAgentInitiator.UpdateAllStatus(0, -1, -0.2, 0, 0, 0);

                    //Decreases relation 
                    CustomAgent CAtoDecrease = CustomAgentReceiver.GetCustomAgentByName(CustomAgentInitiator.thirdAgent, CustomAgentInitiator.thirdAgentId);

                    if (CAtoDecrease != null)
                    {
                        SocialNetworkBelief belief = CustomAgentReceiver.SelfGetBeliefWithAgent(CAtoDecrease);
                        CustomAgentReceiver.UpdateBeliefWithNewValue(belief, -1);

                        if (CAtoDecrease.selfAgent.IsHero && CAtoDecrease.selfAgent == Agent.Main)
                        {
                            ChangeHeroRelationInGame(-1, CAtoDecrease);
                        }
                    }
                }
                //if (SEName == "Jealous")
                //{
                //    CustomAgentInitiator.UpdateAllStatus(0, -1, 0, -0.3, 1, 0);
                //    CustomAgentReceiver.UpdateAllStatus(0, 0, -0.2, 0, 0, 0);
                //}

                //else if (SEName == "FriendSabotage")
                //{
                //    CustomAgentInitiator.UpdateAllStatus(0, -1, -0.2, 0, 0, 0);

                //    //Decreases relation 
                //    CustomAgent CAtoDecrease = CustomAgentReceiver.GetCustomAgentByName(CustomAgentInitiator.thirdAgent, CustomAgentInitiator.thirdAgentId);

                //    if (CAtoDecrease != null)
                //    {
                //        SocialNetworkBelief belief = CustomAgentReceiver.SelfGetBeliefWithAgent(CAtoDecrease);
                //        CustomAgentReceiver.UpdateBeliefWithNewValue(belief, -1);

                //        if (CAtoDecrease.selfAgent.IsHero && CAtoDecrease.selfAgent == Agent.Main)
                //        {
                //            ChangeHeroRelationInGame(-1, CAtoDecrease);
                //        }
                //    }  
                //}
            }
            UpdateNPCsNearSocialMove();
        }

        private void ConsequencesFromRomanticIntention()
        {
            if (CustomAgentReceiver.SE_Accepted)
            {
                //Increases Relationship for both
                //if (SEName == "AskOut")
                //{ 
                //    AskOutMethod(false);
                //}
                ///*else*/ if (SEName == "Flirt")
                //{
                //    SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Dating", 1);
                //}
                if (SE_Enum == CustomMissionNameMarkerVM.SEs_Enum.Flirt)
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
                if (SE_Enum == CustomMissionNameMarkerVM.SEs_Enum.Compliment)
                {
                    SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", 1);

                    CustomAgentInitiator.UpdateAllStatus(-1, 0, 0, 0, 0, 0);
                }
                else if (SE_Enum == CustomMissionNameMarkerVM.SEs_Enum.GiveGift)
                {
                    SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", 1);

                    CustomAgentInitiator.UpdateAllStatus(-1, 0, 0, 0, 0, 0);

                    Item tempItem = CustomAgentInitiator.GetItem();
                    CustomAgentInitiator.RemoveItem(tempItem.itemName, -1);
                    CustomAgentReceiver.AddItem(tempItem.itemName, 1);
                }

                //if (SEName == "Compliment")
                //{
                //    SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", 1);

                //    CustomAgentInitiator.UpdateAllStatus(-1, 0, 0, 0, 0, 0);
                //}
                //else if (SEName == "GiveGift")
                //{
                //    SocialNetworkBelief belief = UpdateParticipantNPCBeliefs("Friends", 1);

                //    CustomAgentInitiator.UpdateAllStatus(-1, 0, 0, 0, 0, 0);

                //    Item tempItem = CustomAgentInitiator.GetItem();
                //    CustomAgentInitiator.RemoveItem(tempItem.itemName, -1);
                //    CustomAgentReceiver.AddItem(tempItem.itemName, 1);
                //}
            }
            else
            {
                CustomAgentInitiator.AddToTriggerRulesList(new TriggerRule("Bully", CustomAgentReceiver.Name, CustomAgentReceiver.Id));
                CustomAgentInitiator.UpdateAllStatus(-1, 0, 0, 0, 1, 0);
            }
            
            UpdateNPCsNearSocialMove();
        }


        private static void ChangeHeroRelationInGame(int value, CustomAgent customAgent)
        {
            Hero hero = Hero.FindFirst(h => h.CharacterObject == customAgent.selfAgent.Character);
            if (hero != null && hero != Hero.MainHero)
            {
                float relationWithPlayer = hero.GetRelationWithPlayer();
                int newValue = (int)(relationWithPlayer + value);
                if (value > 0)
                {
                    InformationManager.AddQuickInformation(new TextObject("Your relation is increased by " + value + " to " + newValue + " with " + hero.Name + "."), 0, hero.CharacterObject);
                    Hero.MainHero.SetPersonalRelation(hero, newValue);
                }
                else
                {
                    InformationManager.AddQuickInformation(new TextObject("Your relation is decreased by " + value + " to " + newValue + " with " + hero.Name + "."), 0, hero.CharacterObject);
                    Hero.MainHero.SetPersonalRelation(hero, newValue);
                }
            }
        }

        private bool InteractionSawByThisNPC(CustomAgent customAgentInitiator, CustomAgent customAgent)
        {
            if (Agent.Main != null)
            {
                if (customAgentInitiator != customAgent && customAgent.selfAgent != Agent.Main && customAgentInitiator.selfAgent.Position.Distance(customAgent.selfAgent.Position) <= 5)
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
            foreach (CustomAgent customAgent in CustomAgentList)
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
                            TriggerRule triggerRule = new TriggerRule("RomanticSabotage", CustomAgentInitiator.selfAgent.Name, CustomAgentInitiator.Id);
                            customAgent.AddToTriggerRulesList(triggerRule);
                        }
                        else // tem relaçao Friends
                        {
                            if (beliefWithReceiver.value < 0) //***  NPC A nao se dá bem com NPC B, então relaçao com initiator cai 1 pt se tiver interações positivas com B ou viceversa
                            {
                                bool RelationIncreased = false;
                                value = RelationIncreased ? -1 : 1;

                                customAgent.UpdateBeliefWithNewValue(beliefWithInitiator, value);

                                if (CustomAgentInitiator.selfAgent == Agent.Main && customAgent.selfAgent.IsHero)
                                {
                                    ChangeHeroRelationInGame(value, customAgent);
                                }
                            }
                            else if (beliefWithReceiver.value > 0) // NPC A dá-se bem com NPC B, então relaçao com initiator sobe 1 pt se tiver interações positivas com B ou viceversa
                            {
                                bool RelationIncreased = false;
                                value = RelationIncreased ? 1 : -1;

                                customAgent.UpdateBeliefWithNewValue(beliefWithInitiator, value);

                                if (CustomAgentInitiator.selfAgent == Agent.Main && customAgent.selfAgent.IsHero)
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

            foreach (CustomAgent customAgent in CustomAgentList)
            {
                customAgent.UpdateBeliefWithNewRelation("Friends", _belief);
            }
        }

        public void AskOutMethod(bool PlayerIsInitiator)
        {
            SocialNetworkBelief _belief = UpdateParticipantNPCBeliefs("Friends", 1);

            foreach (CustomAgent customAgent in CustomAgentList)
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
        
        public int InitiadorVolition()
        {
            SetIntention(false, false);

            int initialValue = 0;

            InfluenceRule IR = new InfluenceRule(CustomAgentInitiator, CustomAgentReceiver, false, initialValue)
            {
                SE_Enum_Name = SE_Enum,
                RelationIntention = Intention
            };
            int finalVolition = ComputeVolitionWithInfluenceRule(IR, CustomAgentInitiator, CustomAgentReceiver);
            finalVolition = CheckMemory(finalVolition, 3);

            CustomAgentInitiator.SEVolition = finalVolition;

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
                SE_Enum_Name = SE_Enum,
                RelationIntention = Intention
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

        private void ResetCustomAgentVariables(CustomAgent customAgent)
        {
            customAgent.SocialMove_SE = CustomMissionNameMarkerVM.SEs_Enum.Undefined;
            customAgent.IsInitiator = false;
            customAgent.FullMessage = null;
            customAgent.Message = "";
            customAgent.Busy = false;
            customAgent.EnoughRest = false;
            customAgent.customAgentTarget = null;
            customAgent.thirdAgent = "";
            customAgent.thirdAgentId = 0;
            customAgent.MarkerTyperRef = 1;
            customAgent.StopAnimation();
            customAgent.EndFollowBehavior();
        }    

        public bool SocialExchangeDoneAndReacted { get; set; }
        public bool ReceptorIsPlayer { get; set; }
        public bool ReduceDelay { get; set; }

        public CustomMissionNameMarkerVM.SEs_Enum SE_Enum { get; }

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

        private List<CustomAgent> CustomAgentList { get; set; }
        public CustomAgent CustomAgentInitiator { get; set; }
        public CustomAgent CustomAgentReceiver { get; set; }
        private Agent AgentInitiator { get; set; }
        private Agent AgentReceiver { get; set; }

    }
}