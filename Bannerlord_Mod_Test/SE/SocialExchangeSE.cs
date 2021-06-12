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
                this.AgentReceiver = _customAgentinitiator.targetAgent;
                this.CustomAgentInitiator = _customAgentinitiator;
                this.CustomAgentList = customAgents;
                this.CustomAgentReceiver = CustomAgentList.Single(item => item.Name == AgentReceiver.Name);
                this.index = -1;
            }

            switch (SEName)
            {
                case "Flirt": Intention = IntentionEnum.Romantic; break;
                case "Compliment": Intention = IntentionEnum.Friendly; break;
                case "Bully": Intention = IntentionEnum.Hostile; break;
                case "Jealous": Intention = IntentionEnum.UnFriendly; break;
                case "Break": Intention = IntentionEnum.Special; break;
                default: Intention = IntentionEnum.Undefined; break;
            }
        }
        private Random rnd { get; set; }

        internal int auxToCheckWhoIsSpeaking;
        internal int index;

        internal void OnInitialize(Random Rnd)
        {
            rnd = Rnd;
            SocialExchangeDoneAndReacted = false;

            if (AgentReceiver.Name == Agent.Main.Name)
            { 
                ReceptorIsPlayer = true; count = -1; 
            }
            else 
            { 
                ReceptorIsPlayer = false; count = 0; 
            }

            CustomAgentInitiator.SEIntention = Intention;

            CustomAgentReceiver.busy = true;
            CustomAgentReceiver.SEIntention = Intention;
            CustomAgentReceiver.SocialMove = SEName;

            CustomAgentReceiver.selfAgent.SetLookAgent(AgentInitiator);
        }
        internal bool ReduceDelay { get; set; }
        internal void OnGoingSocialExchange(RootMessageJson rootMessageJson, Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> megaDictionary)
        {
            if (auxToCheckWhoIsSpeaking % 2 == 0)
            {
                index++;

                //CustomAgentReceiver.message = "";

                CustomAgentInitiator.InitiatorToSocialMove(CustomAgentInitiator, CustomAgentReceiver, rootMessageJson, rnd, index, megaDictionary);
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
                //CustomAgentInitiator.message = "";

                CustomAgentReceiver.SEVolition = ReceiverVolition();
                CustomAgentReceiver.ReceiverToSocialMove(CustomAgentInitiator, CustomAgentReceiver, rootMessageJson, rnd, index, megaDictionary);

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
                if (ReceptorIsPlayer) { /*Check Booleans on CampaignBehavior*/ AgentInitiator.OnUse(AgentReceiver); }
                SocialExchangeDoneAndReacted = true;
            }
        }
        internal void OnFinalize()
        {
            CustomAgentInitiator.SocialMove = "";
            CustomAgentInitiator.NearEnoughToStartConversation = false;

            ResetCustomAgentVariables(CustomAgentInitiator);
            
            AgentInitiator.OnUseStopped(AgentReceiver, true, 0);

            CustomAgentInitiator.AddToMemory(new MemorySE(CustomAgentReceiver.Name, SEName));
            CustomAgentReceiver.AddToMemory(new MemorySE(CustomAgentInitiator.Name, SEName));

            if (!ReceptorIsPlayer)
            {
                ResetCustomAgentVariables(CustomAgentReceiver);
                UpdateBeliefsAndStatus();
            }
        }

        private void ResetCustomAgentVariables(CustomAgent customAgent)
        {    
            customAgent.FullMessage = null;
            customAgent.busy = false;
            customAgent.message = "";
            customAgent.cooldown = true;
            customAgent.targetAgent = null;
            customAgent.StopAnimation();
            customAgent.EndFollowBehavior();
        }

        private void UpdateBeliefsAndStatus()
        {
            switch (Intention)
            {
                case IntentionEnum.Friendly:
                    if (SEName == "Compliment")
                    {
                        Belief belief = UpdateParticipantNPCBeliefs("Friendship", 1);

                        UpdateThirdNPCsBeliefs("Friendship", belief, 1);

                        RunTriggerRulesForEveryone();

                        CustomAgentInitiator.UpdateStatus("SocialTalk", -1);
                    }
                    break;
                case IntentionEnum.Romantic:
                    if (!CustomAgentReceiver.SE_Accepted)
                    {
                        CustomAgentInitiator.UpdateStatus("Anger", 1);
                        InformationManager.DisplayMessage(new InformationMessage(CustomAgentReceiver.Name + " rejected " + CustomAgentInitiator.Name + " " + SEName));
                    }
                    else
                    {
                        Belief belief = UpdateParticipantNPCBeliefs("Dating", 1);

                        UpdateThirdNPCsBeliefs("Dating", belief, 1);

                        RunTriggerRulesForEveryone();
                        InformationManager.DisplayMessage(new InformationMessage(CustomAgentReceiver.Name + " is now dating " + CustomAgentInitiator.Name));
                    }
                    break;
                case IntentionEnum.UnFriendly:
                    if (!CustomAgentReceiver.SE_Accepted)
                    {
                        CustomAgentReceiver.UpdateStatus("Anger", 1);
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

                        Belief belief = UpdateParticipantNPCBeliefs("Friendship", -1);

                        UpdateThirdNPCsBeliefs("Friendship", belief, -1);

                        RunTriggerRulesForEveryone();
                    }
                    break;
                case IntentionEnum.Special:
                    CustomAgentInitiator.UpdateStatus("Anger", -1);
                    CustomAgentInitiator.UpdateStatus("Courage", -1);

                    Belief _belief = UpdateParticipantNPCBeliefs("Dating", -1);
                    UpdateThirdNPCsBeliefs("Dating", _belief, -1);

                    RunTriggerRulesForEveryone();

                    InformationManager.DisplayMessage(new InformationMessage(CustomAgentInitiator.Name + " broke up with " + CustomAgentReceiver.Name));
                    break;
                default:
                    break;
            }
        }
        private Belief UpdateParticipantNPCBeliefs(string _relationName, int _value)
        {
            Belief belief = CustomAgentInitiator.GetBelief(_relationName, CustomAgentReceiver);
            if (belief == null)
            {
                List<string> a = new List<string>() { CustomAgentInitiator.Name, CustomAgentReceiver.Name };
                Belief newBelief = new Belief(_relationName, a, _value);

                CustomAgentInitiator.AddBelief(newBelief);
                CustomAgentReceiver.AddBelief(newBelief);

                belief = newBelief;
            }
            else
            {
                CustomAgentInitiator.UpdateBelief(belief, _value);
                CustomAgentReceiver.UpdateBelief(belief, _value);
            }

            return belief;
        }
        private void UpdateThirdNPCsBeliefs(string _relationName, Belief _belief, int _value)
        {
            foreach (CustomAgent customAgent in CustomAgentList)
            {
                if (customAgent != CustomAgentInitiator && customAgent != CustomAgentReceiver)
                {
                    Belief belief = customAgent.BeliefsList.Find(b => b.relationship == _relationName
                    && b.agents.Contains(CustomAgentInitiator.Name)
                    && b.agents.Contains(CustomAgentReceiver.Name));

                    if (belief == null)
                    {
                        customAgent.AddBelief(_belief);
                    }
                    else
                    {
                        customAgent.UpdateBelief(_belief, _value);
                    }
                }
            }
        }
        internal int InitiadorVolition()
        {
            int initialValue = 0;

            InfluenceRule IR = new InfluenceRule(CustomAgentInitiator, CustomAgentReceiver, false, initialValue);

            IR.RelationName = SEName;
            IR.RelationType = Intention;
            int finalVolition = ComputeVolitionWithInfluenceRule(IR);

            //
            if (CustomAgentInitiator.MemorySEs.Exists(m => m.NPC_Name == CustomAgentReceiver.Name && m.SE_Name == SEName))
            {
                //if (rnd.NextDouble() < 0.5)
                //{
                //    finalVolition -= 2; // rnd.Next(2);
                //}
            }
            //

            CustomAgentInitiator.SEVolition = finalVolition;
            return CustomAgentInitiator.SEVolition;
        }
        internal int ReceiverVolition()
        {
            int initialValue = 0;

            InfluenceRule IR = new InfluenceRule(CustomAgentInitiator, CustomAgentReceiver, true, initialValue);

            IR.RelationName = SEName;
            IR.RelationType = Intention;
            int finalVolition = ComputeVolitionWithInfluenceRule(IR);

            CustomAgentReceiver.SEVolition = finalVolition;
            ComputeOutcome(CustomAgentReceiver.SEVolition, -0.5f, 0.5f);

            return CustomAgentReceiver.SEVolition;
        }
        private static int ComputeVolitionWithInfluenceRule(InfluenceRule IR)
        {
            IR.InitialValue = IR.CheckGoals(IR.RelationTypeString);
            IR.InitialValue += IR.GetValueParticipantsRelation();
            IR.InitialValue += IR.SRunRules();
            return IR.InitialValue;
        }

        public void PlayerConversationWithNPC(string relation, int value)
        {
            Belief belief = UpdateParticipantNPCBeliefs(relation, value);
            UpdateThirdNPCsBeliefs(relation, belief, value);
        }

        //*
        private void ComputeOutcome(int _SEVolition, float minThreshold, float maxThreshold)
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
        private void RunTriggerRulesForEveryone()
        {
            foreach (CustomAgent _customAgent in CustomAgentList)
            {
                _customAgent.RunTriggerRules();
            }
        }
        //*

        public enum IntentionEnum 
        { 
            Undefined = -1, 
            Friendly, 
            Romantic, 
            UnFriendly, 
            Hostile, 
            Special, 
            AllTypes
        }
        public IntentionEnum Intention { get; private set; }
        public bool SocialExchangeDoneAndReacted { get; set; }
        public bool ReceptorIsPlayer { get; set; }
        public int count { get; set; }
        public string SEName { get; set; }

        private List<CustomAgent> CustomAgentList { get; set; }
        public CustomAgent CustomAgentInitiator { get; set; }
        public CustomAgent CustomAgentReceiver { get; set; }
        private Agent AgentInitiator { get; set; }
        private Agent AgentReceiver { get; set; }
    }
}


/*
 * Friendly - The volition for Positive SEs is increased. [The Social Exchange cooldown for this character will also be reduced]
 * Hostile - The volition for Negative SEs is increased
 * 
 * Charming - The volition for Romantic SEs is increased
 * UnCharming - The volition for Romantic SEs is decreaed
 * 
 * Shy - The volition for SEs in general is decreased. [The Social Exchange cooldown for this character will also be increased]
 * Brave - The  volition for SEs in general is increased
 *
 * Faithful - If they are in a relationship, the volition for Romantic SEs with other participators that aren't their partner is decreased
 * Unfaithful - The volition for Romantic SEs is increased
 * 
 * Calm - The volition for Positive SEs is increased
 * Aggressive - The volition for Negative SEs is increased
 * 
 * Coward - The  volition for SEs in general is decreased
 * Gossiper - The volition for SEs in general is increased.
 * Obnoxious - The character will be more compelled to repeat SEs that they have just performed
 * Humble - The volition for Negative SEs is decreased along with the Brag Social Exchange
 */

/* 
 * Friendly - Hostile
 * Shy - Brave
 * Faithful - Unfaithful
 * Calm - Aggressive
 */



/* > Limitação 
 * todos os NPCs podem movimentar-se e ir de encontro a um outro npc mas
 * apenas os heroes podem conversar entre si devido ao missionScreen que foi desenvolvido para divulgar os locais e os Heroes relevantes 
 * e foi adaptado para mostrar as conversas. Os Townsman , por exemplo, não são considerados pelo jogo como personagens relevantes e então 
 * não estão incluidos no missionScreen que por sua vez não possibilita a divulgaçao da sua suposta mensagem, permanecendo calado junto do seu target.
 * Mas eventualmente quando a "silenciosa" interaçao termina, eles voltam cada um para a sua vida. Mas irá ser feito uma adaptação que permite a esses NPCs considerados
 * "normais" puderem conversar entre si mas levando a outras limitações maiores.
 
 * Jogo em earlier access
 * ainda nao foi lançado oficialmente o que pode influenciar no numero de jogadores
 * 
 * > Implementação
 * When a NPC it's calculating the new dialog to say, the receiver is reseting his sentence, and vice-versa.
 * If a NPC has more sentences to say than the receiver, so it will end the conversation because in some time both sentences will be resetted , and when both are resetted the conversation ends.
 *
 * if it is desire that a NPC speaks more sentences than the other, so the message it will be displayed on screen for a bigger time because it has the delay of himself and the delay of the receiver who doesn't have anything to say.
 * 
 * When a NPC comes to interact with Player, if they didn't have any first impression yet, so it will skip the social exchange to start the default dialog about the first impression between the NPC and the Player.
 * 
 * It is possible to make all the NPCs talk but how we will sabe their data if their don't have any identifier inside of the game? That is a limitation!
 * Also, the "normal" NPCs will only interact with the Heroes because are them who have the traits. It's not supported to give traits to the normal NPCs
 * 
 * > Para o futuro aprodundar interaçoes com o conhecimento de cada NPC em relaçao aos outros e ao ambiente
 * Expandir os diálogos e o CIF para todos os NPCs 
 * 
 * 
 * testes controlados e depois lançamento do mod
 */