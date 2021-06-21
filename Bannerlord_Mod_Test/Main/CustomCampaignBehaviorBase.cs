using System;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using System.Linq;
using SandBox;
using Helpers;
using System.Collections.Generic;

namespace Bannerlord_Mod_Test
{
    class CustomCampaignBehaviorBase : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoaded));
            CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.Tick2)); // Working
        }

        private void Tick2(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
        }
        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
        }
        public override void SyncData(IDataStore dataStore)
        {
        }
        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            //StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject, null, null, false);
            this.TavernEmployeesCampaignBehavior(campaignGameStarter);
            this.AddVillageFarmerTradeAndLootDialogs(campaignGameStarter);
            this.AddTownspersonAndVillagerDialogs(campaignGameStarter);
            this.LordConversationsCampaignBehavior(campaignGameStarter);
        }
        //TaleWorlds.CampaignSystem.dll : TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.Towns
        private void TavernEmployeesCampaignBehavior(CampaignGameStarter campaignGameStarter)
        {

            //Start Dialog
            //campaignGameStarter.AddDialogLine("tavernmaid_introduction", "start", "tavernmaid_greeting", "Hello Adventure! ", new ConversationSentence.OnConditionDelegate(testing_intro_tavernmaid), null, 101, null);
            //campaignGameStarter.AddPlayerLine("tavernmaid_introduction", "tavernmaid_greeting", "close_window", "Thank you. ", null, null, 100, null, null);

            //Dialog
            campaignGameStarter.AddPlayerLine("tavernmaid_order_food", "tavernmaid_talk", "tavernmaid_order_testnews1", "{=E57VFXqU}Any News on this Town?", new ConversationSentence.OnConditionDelegate(this.Conversation1), null, 100, null, null);
            campaignGameStarter.AddPlayerLine("tavernmaid_order_food", "tavernmaid_talk", "tavernmaid_order_test1", "AI - Can you say 'Hello World'? {GOLD_ICON}", null, null, 100, null, null);
            campaignGameStarter.AddDialogLine("tavernmaid_test", "tavernmaid_order_test1", "tavernmaid_order_test2", "Social Engagement? That sounds interesting.", null, null, 100, null);

            // Closes dialog with an Answer
            campaignGameStarter.AddPlayerLine("tavernmaid_test", "tavernmaid_order_test2", "close_window", "Yes, it is!", null, null, 100, null, null);

            // Closes dialog with no Answer
            campaignGameStarter.AddDialogLine("tavernmaid_test", "tavernmaid_order_testnews1", "close_window", "No news sr.", null, null, 100, null);

            campaignGameStarter.AddPlayerLine("tavernmaid_order_food", "tavernmaid_talk", "tavernmaid_order_teleport", "Can you guide me to a merchant?", null, null, 100, null, null);
            campaignGameStarter.AddDialogLine("tavernmaid_test", "tavernmaid_order_teleport", "merchantTurn", "Sure.", null, new ConversationSentence.OnConsequenceDelegate(this.Conversation_tavernmaid_test_on_condition), 100, null);

        }
        private bool talking_with_Weak()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;

            CustomAgent agentConversation = customAgents.Find(c => c.selfAgent.Character == Hero.OneToOneConversationHero.CharacterObject);
            if (agentConversation != null)
            {
                CustomAgent customAgent = new CustomAgent(agentConversation.selfAgent, agentConversation.Id) { Name = agentConversation.Name };
                customAgent.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                Trait trait = customAgent.TraitList.Find(t => t.traitName == "Weak");
                if (trait != null)
                {
                    return true;
                }
                else { return false; }
            }

            //foreach (Agent agent in Mission.Current.Agents)
            //{
            //    if (agent.Character == Hero.OneToOneConversationHero.CharacterObject)
            //    {

            //        CustomAgent customAgent = new CustomAgent(agent) { Name = agent.Name };
            //        customAgent.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
            //        Trait trait = customAgent.TraitList.Find(t => t.traitName == "Weak");
            //        if (trait != null)
            //        {
            //            return true;
            //        }
            //        else { return false; }
            //    }
            //}
            return false;
        }
        private bool talking_with_Charming()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;

            CustomAgent agentConversation = customAgents.Find(c => c.selfAgent.Character == Hero.OneToOneConversationHero.CharacterObject);
            if (agentConversation != null)
            {
                CustomAgent customAgent = new CustomAgent(agentConversation.selfAgent, agentConversation.Id) { Name = agentConversation.Name };
                customAgent.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                Trait trait = customAgent.TraitList.Find(t => t.traitName == "Charming");
                if (trait != null)
                {
                    return true;
                }
                else { return false; }
            }

            //string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            //string _currentLocation = CampaignMission.Current.Location.StringId;
            //foreach (Agent agent in Mission.Current.Agents)
            //{
            //    if (agent.Character == Hero.OneToOneConversationHero.CharacterObject)
            //    {
            //        CustomAgent customAgent = new CustomAgent(agent) { Name = agent.Name };
            //        customAgent.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
            //        Trait trait = customAgent.TraitList.Find(t => t.traitName == "Charming");
            //        if (trait != null)
            //        {
            //            return true;
            //        }
            //        else { return false; }
            //    }
            //}
            return false;
        }

        //TaleWorlds.CampaignSystem.dll : CampaignSystem.SandBox.CampaignBehaviors.VillageBehaviors.VillagerCampaignBehavior
        private void AddVillageFarmerTradeAndLootDialogs(CampaignGameStarter campaignGameStarter)
        {
            //campaignGameStarter.AddDialogLine("village_farmer_talk_start", "start", "village_farmer_talk", "Greetings", null, null, 100, null);
            //campaignGameStarter.AddDialogLine("village_farmer_pretalk_start", "village_farmer_pretalk", "village_farmer_talk", "Hello, you are crazy?", null, null, 100, null);
            campaignGameStarter.AddPlayerLine("village_farmer_buy_products", "village_farmer_talk", "village_farmer_player_trade", "I'm going to market too. Bye bye bye", null, null, 100, null, null);
        }
        //TaleWorlds.CampaignSystem.dll : CampaignSystem.SandBox.Source.Towns.CommonVillagersCampaignBehavior
        private void AddTownspersonAndVillagerDialogs(CampaignGameStarter campaignGameStarter)
        {
            /*Child or Teenager*/
            //TavernKeeper
            campaignGameStarter.AddPlayerLine("1", "tavernkeeper_talk", "lord_friendly", "You are awesome! [Friendly]", null, new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "tavernkeeper_talk", "lord_unfriendly", "My pet is smarter than you! [Unfriendly]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            //TavernMaid
            campaignGameStarter.AddPlayerLine("1", "tavernmaid_talk", "lord_friendly", "You are awesome! [Friendly]", new ConversationSentence.OnConditionDelegate(CheckIfIsDatingWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "tavernmaid_talk", "lord_unfriendly", "My pet is smarter than you! [Unfriendly]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            //Child
            campaignGameStarter.AddPlayerLine("1", "town_or_village_children_player_no_rhyme", "lord_friendly", "You are awesome! [Friendly]", null, new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player_children_post_rhyme", "lord_friendly", "You are awesome! [Friendly]", null, new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "town_or_village_children_player_no_rhyme", "lord_unfriendly", "My pet is smarter than you! [Unfriendly]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player_children_post_rhyme", "lord_unfriendly", "My pet is smarter than you! [Unfriendly]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            //Town Or Village
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player", "lord_friendly", "You are awesome! [Friendly]", null, new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player", "lord_unfriendly", "My pet is smarter than you! [Unfriendly]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
        }
        private void LordConversationsCampaignBehavior(CampaignGameStarter campaignGameStarter)
        {
            /*campaignGameStarter.AddDialogLine("1175", "lord_give_oath_give_up1", "close_window", "{=kvrZ4HIT} {PLAYER} here on {CURRENT_SETTLEMENT_NAME} and me {CONVERSATION_CHARACTER} .... No, I don't want to test your code. Go away[rf:idle_angry][ib:nervous]!", null, delegate ()
            {
                PlayerEncounter.LeaveEncounter = true;
            }, 100, null);*/

            /* Increase Courage */
            campaignGameStarter.AddPlayerLine("1175", "hero_main_options", "hero_increase_courage", "You can fight against the bully. [Increase Courage]", new ConversationSentence.OnConditionDelegate(talking_with_Weak), null, 100, null, null);
            campaignGameStarter.AddDialogLine("1175", "hero_increase_courage", "close_window", "Ok, I will try.", null, new ConversationSentence.OnConsequenceDelegate(Increase_Courage), 100, null);

            /*Hero Dialog*/
            campaignGameStarter.AddPlayerLine("1", "start", "lord_date", "You must give a chance to date. [Increase Courage]", new ConversationSentence.OnConditionDelegate(talking_with_Charming), new ConversationSentence.OnConsequenceDelegate(Increase_Courage), 100, null, null);
            campaignGameStarter.AddDialogLine("1", "lord_date", "close_window", "I don't know... Well, why not.", null, null, 100, null);
         
            campaignGameStarter.AddDialogLine("1", "merchantTurn", "close_window", "I am a merchant.", null, null, 100, null);

            campaignGameStarter.AddPlayerLine("1", "t1", "lord_emergencyCall", "Let's call everyone!", new ConversationSentence.OnConditionDelegate(Condition_EmergencyCall), null, 100, null, null);
            campaignGameStarter.AddDialogLine("1", "lord_emergencyCall", "close_window", "What happened?[rf:idle_angry][ib:nervous]!", null, new ConversationSentence.OnConsequenceDelegate(Consequence_EmergencyCall), 100, null);

            campaignGameStarter.AddPlayerLine("1", "t2", "lord_emergencyCall2", "Ok, everything is fine!", new ConversationSentence.OnConditionDelegate(Condition_StopEmergencyCall), null, 100, null, null);
            campaignGameStarter.AddDialogLine("1", "lord_emergencyCall2", "close_window", "Hum...[rf:idle_angry][ib:nervous]!", null, new ConversationSentence.OnConsequenceDelegate(Consequence_StopEmergencyCall), 100, null);

            campaignGameStarter.AddDialogLine("1", "lord_emergencyCall3", "close_window", "So... What's going on?", new ConversationSentence.OnConditionDelegate(Condition_EmergencyCallGoingOn), new ConversationSentence.OnConsequenceDelegate(Consequence_EmergencyCallGoingOn), 101, null);
            
            /* Player Interactions with NPC */
            //Friendly - Working
            campaignGameStarter.AddPlayerLine("1", "hero_main_options", "lord_friendly", "You are awesome! [Friendly]", null, new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            campaignGameStarter.AddDialogLine("1", "lord_friendly", "close_window", "Oh...that's nice. Thank you![if:idle_pleased]", null, null, 100, null);
            //UnFriendly - Working
            campaignGameStarter.AddPlayerLine("1", "hero_main_options", "lord_unfriendly", "My pet is smarter than you! [Unfriendly]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            campaignGameStarter.AddDialogLine("1", "lord_unfriendly", "close_window", "Oh...that's not nice![if:idle_angry]", null, null, 100, null);
            //Hostile
            //campaignGameStarter.AddPlayerLine("1", "hero_main_options", "lord_hostile", "I need some gold, these coins are enough for now! [Hostile]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            //campaignGameStarter.AddDialogLine("1", "lord_hostile", "close_window", "You don't have a job, or what? [ib:nervous][if:idle_angry]", null, new ConversationSentence.OnConsequenceDelegate(StealFromNPC), 100, null);
            //Romantic
            campaignGameStarter.AddPlayerLine("1", "hero_main_options", "lord_romantic", "My day is better with you! [Romantic]", new ConversationSentence.OnConditionDelegate(CheckIfIsDatingWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Dating), 100, null, null);
            campaignGameStarter.AddDialogLine("1", "lord_romantic", "close_window", "Oh, you're so kind![if:idle_pleased][ib:confident]", new ConversationSentence.OnConditionDelegate(NPC_Accept_Dating_condition), null, 100, null); // Accept depending if have Faithful Trait and not dating or not having the trait and dating
            campaignGameStarter.AddDialogLine("1", "lord_romantic", "close_window", "Oh, sorry but I'm currently dating![if:idle_pleased][ib:confident]", new ConversationSentence.OnConditionDelegate(NPC_Accept_Dating_condition), null, 100, null); // Reject depending if have Faithful Trait & Dating with anyone
            /* NPC Friendly Interactions With Player */  //Working
            campaignGameStarter.AddDialogLine("1", "start", "Friendly_start", "Hi Friend... If you need something just tell me, maybe I can help you.[ib:closed][if:idle_pleased]", new ConversationSentence.OnConditionDelegate(Friendly), null, 200, null);
            campaignGameStarter.AddPlayerLine("1", "Friendly_start", "Friendly_step1", "Yes, sure. I appreciate it. [Accept]", null, new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "Friendly_start", "close_window", "Huh.. Someone is calling me!", null, null, 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "Friendly_start", "Friendly_step2", "Do you think that I am a kid or something? I don't need your help! [Reject]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            campaignGameStarter.AddDialogLine("1", "Friendly_step1", "close_window", "It's a pleasure to help you. [if:idle_pleased]", null, null, 100, null);
            campaignGameStarter.AddDialogLine("1", "Friendly_step2", "close_window", "Take it easy. There is no need to be rude. [rf:idle_angry]", null, null, 100, null);
            /**/
            campaignGameStarter.AddDialogLine("1", "start", "Romantic_start", "You are looking really charming today.[if:idle_pleased]", new ConversationSentence.OnConditionDelegate(Romantic), null, 200, null);
            campaignGameStarter.AddPlayerLine("1", "Romantic_start", "Romantic_step1", "Hehe You're kind as always. [Accept]", null, new ConversationSentence.OnConsequenceDelegate(Increase_Dating), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "Romantic_start", "Romantic_step2", "Are you blind? Go away! [Reject]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Dating), 100, null, null);
            campaignGameStarter.AddDialogLine("1", "Romantic_step1", "close_window", "Thank you Sr.[ib:confident]", null, null, 100, null);
            campaignGameStarter.AddDialogLine("1", "Romantic_step2", "close_window", "Not nice.", null, null, 100, null);

            campaignGameStarter.AddDialogLine("1", "start", "UnFriendly_start", "Why are you listening people's conversation?", new ConversationSentence.OnConditionDelegate(UnFriendly), null, 200, null);
            campaignGameStarter.AddPlayerLine("1", "UnFriendly_start", "UnFriendly_step1", "Sorry, it wouldn't happen again. [Accept]", null, null, 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "UnFriendly_start", "UnFriendly_step2", "Just curiosity. [Reject]", null, null, 100, null, null);
            campaignGameStarter.AddDialogLine("1", "UnFriendly_step1", "close_window", "I hope not, idiot.", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null);
            campaignGameStarter.AddDialogLine("1", "UnFriendly_step2", "close_window", "Curiosity, huh.", null, null, 100, null);

            campaignGameStarter.AddDialogLine("1", "start", "Hostile_start", "You have some gold for me, huh? [ib:aggressive]", new ConversationSentence.OnConditionDelegate(Hostile), null, 200, null);
            campaignGameStarter.AddPlayerLine("1", "Hostile_start", "Hostile_step1", "There you go. [Accept]", null, new ConversationSentence.OnConsequenceDelegate(NPCStealPlayer), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "Hostile_start", "Hostile_step2", "Are you kidding me? [Reject]", null, null, 100, null, null);
            campaignGameStarter.AddDialogLine("1", "Hostile_step1", "close_window", "Thank you, friend hehe.", null, null, 100, null);
            campaignGameStarter.AddDialogLine("1", "Hostile_step2", "close_window", "Hum... maybe next time.", null, null, 100, null);

            /*campaignGameStarter.AddDialogLine("1", "start", "Special_start", "Special", new ConversationSentence.OnConditionDelegate(Special), null, 200, null);
            campaignGameStarter.AddPlayerLine("1", "Special_start", "Special_step1", "[Accept]!", null, null, 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "Special_start", "Special_step2", "[Reject]!", null, null, 100, null, null);
            campaignGameStarter.AddDialogLine("1", "Special_step1", "close_window", "SpecialAccept", null, null, 100, null);
            campaignGameStarter.AddDialogLine("1", "Special_step2", "close_window", "SpecialReject", null, null, 100, null);*/
        }

        internal BasicCharacterObject characterRef { get; set; }
        private bool ThisAgentWillInteractWithPlayer()
        {
            if (CharacterObject.OneToOneConversationCharacter == characterRef)
            {
                characterRef = null;
                return true;
            }
            else { return false; }
        }
        public bool FriendlyBool { get; set; }
        public bool RomanticBool { get; set; }
        public bool UnFriendlyBool { get; set; }
        public bool HostileBool { get; set; }
        public bool SpecialBool { get; set; }
        private bool Friendly()
        {
            if (FriendlyBool && ThisAgentWillInteractWithPlayer())
            {
                FriendlyBool = false;
                return true;
            }
            else { return false; }
        }
        private bool Romantic()
        {
            bool auxBool = ThisAgentWillInteractWithPlayer();
            if (RomanticBool && auxBool)
            {
                RomanticBool = false;
                return true;
            }
            else { return false; }
        }
        private bool UnFriendly()
        {
            if (UnFriendlyBool && ThisAgentWillInteractWithPlayer())
            {
                UnFriendlyBool = false;
                return true;
            }
            else { return false; }
        }
        private bool Hostile()
        {
            if (HostileBool && ThisAgentWillInteractWithPlayer())
            {
                HostileBool = false;
                return true;
            }
            else { return false; }
        }
        private bool Special()
        {
            if (SpecialBool && ThisAgentWillInteractWithPlayer())
            {
                SpecialBool = false;
                return true;
            }
            else { return false; }
        }
        private bool NPC_Reject_Dating_condition()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;

            CustomAgent agentConversation = customAgents.Find(c => c.selfAgent.Character == Hero.OneToOneConversationHero.CharacterObject);
            if (agentConversation != null)
            {
                CustomAgent customAgent = new CustomAgent(agentConversation.selfAgent, agentConversation.Id) { Name = agentConversation.Name };
                customAgent.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                Trait trait = customAgent.TraitList.Find(t => t.traitName == "Faithful");
                
                int datingHowMany = customAgent.CheckHowManyTheAgentIsDating(customAgent);

                if (trait != null || datingHowMany > 0)
                {
                    return false;
                }
                else { return true; }
            }

            //foreach (Agent agent in Mission.Current.Agents)
            //{
            //    if (agent.Character == Hero.OneToOneConversationHero.CharacterObject)
            //    {
            //        CustomAgent customAgent = new CustomAgent(agent) { Name = agent.Name };
            //        Trait hasTrait = customAgent.TraitList.Find(t => t.traitName == "Faithful");
            //        int datingHowMany = customAgent.CheckHowManyTheAgentIsDating(customAgent);

            //        if (hasTrait != null || datingHowMany > 0)
            //        {
            //            return false;
            //        }
            //        else 
            //        {
            //            return true;
            //        }
            //    }
            //}
            return true;
        }
        private bool NPC_Accept_Dating_condition()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;

            CustomAgent agentConversation = customAgents.Find(c => c.selfAgent.Character == Hero.OneToOneConversationHero.CharacterObject);
            if (agentConversation != null)
            {
                CustomAgent customAgent = new CustomAgent(agentConversation.selfAgent, agentConversation.Id) { Name = agentConversation.Name };
                customAgent.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                Trait trait = customAgent.TraitList.Find(t => t.traitName == "Faithful");
                Trait trait2 = customAgent.TraitList.Find(t => t.traitName == "Charming");
                
                int datingHowMany = customAgent.CheckHowManyTheAgentIsDating(customAgent);
                
                if (trait == null || trait2 == null || datingHowMany > 0)
                {
                    return false;
                }
                else { return true; }
            }

            //foreach (Agent agent in Mission.Current.Agents)
            //{
            //    if (agent.Character == Hero.OneToOneConversationHero.CharacterObject)
            //    {
            //        CustomAgent customAgent = new CustomAgent(agent) { Name = agent.Name };
            //        Trait hasTrait = customAgent.TraitList.Find(t => t.traitName == "Faithful");
            //        int datingHowMany = customAgent.CheckHowManyTheAgentIsDating(customAgent);

            //        if (hasTrait != null || datingHowMany > 0)
            //        {
            //            return true;
            //        }
            //        else
            //        {
            //            return false;
            //        }
            //    }
            //}
            return false;
        }
        public bool giveCourage { get; set; }
        private void Increase_Courage()
        {
            giveCourage = true;
        }

        public bool DecreaseFriendshipWithPlayer { get; set; }
        public bool IncreaseFriendshipWithPlayer { get; set; }
        private void Increase_Friendship()
        {
            IncreaseFriendshipWithPlayer = true;
        }
        private void Decrease_Friendship()
        {
            DecreaseFriendshipWithPlayer = true;
        }
        public bool DecreaseDatingWithPlayer { get; set; }
        public bool IncreaseDatingWithPlayer { get; set; }
        private void Increase_Dating()
        {
            IncreaseDatingWithPlayer = true;
        }
        private void Decrease_Dating()
        {
            DecreaseDatingWithPlayer = true;
        }
        /// <summary>
        /// //////////////////////////////////
        /// </summary>
        /// <returns></returns>
        public List<CustomAgent> customAgents;
        private bool CheckIfIsDatingWithNPC_condition()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;
            //Agent agent1 = null;

            var agentConversation = customAgents.Find(c => c.selfAgent.Character == Hero.OneToOneConversationHero.CharacterObject);
            if (agentConversation != null)
            {
                InformationManager.DisplayMessage(new InformationMessage("-"));
            }

            //foreach (Agent agent in Mission.Current.Agents)
            //{
            //    if (agent.Character == Hero.OneToOneConversationHero.CharacterObject)
            //    {
            //        agent1 = agent;
            //        break;
            //    }
            //}

            //CustomAgent agent2 = customAgents.Find(c => c.selfAgent == agent1);
            ////CustomAgent customAgent = new CustomAgent(agent1) { Name = agent1.Name };

            //CustomAgent customMainAgent = new CustomAgent(Agent.Main) { Name = Agent.Main.Name + "0" };
            //customMainAgent.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
            //customMainAgent.SelfGetBeliefWithAgent(customAgent);

            return true;
        }
        /// <summary>
        /// /////////////////////////
        /// </summary>
        public bool AskWhatsGoinOn { get; set; }
        private bool Condition_EmergencyCallGoingOn()
        {
            return AskWhatsGoinOn;
        }
        private void Consequence_EmergencyCallGoingOn()
        {
            AskWhatsGoinOn = false;
        }

        #region Emergency Call 
        public bool EmergencyCallRunning = false;
        private bool Condition_EmergencyCall()
        {
            if (EmergencyCallRunning == false)
            {
                return true;
            }
            else { return false; }
        }
        private void Consequence_EmergencyCall()
        {
            foreach (Agent agent in Mission.Current.Agents)
            {
                if (agent != Agent.Main && agent != null && agent.IsHuman)
                {
                    DailyBehaviorGroup behaviorGroup = agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
                    behaviorGroup.AddBehavior<FollowAgentBehavior>().SetTargetAgent(Agent.Main);
                    behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
                }
            }
            EmergencyCallRunning = true;
            AskWhatsGoinOn = true;

        }
        private bool Condition_StopEmergencyCall()
        {
            return EmergencyCallRunning;
        }
        private void Consequence_StopEmergencyCall()
        {
            foreach (Agent agent in Mission.Current.Agents)
            {
                if (agent != Agent.Main && agent != null && agent.IsHuman)
                {
                    DailyBehaviorGroup behaviorGroup = agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
                    behaviorGroup.RemoveBehavior<FollowAgentBehavior>();
                }
            }

            EmergencyCallRunning = false;
        }
        #endregion

        private void Conversation_tavernmaid_test_on_condition()
        {
            //Teleport character near to NPC
            CharacterObject characterObject = CharacterObject.All.FirstOrDefault((CharacterObject k) => k.Occupation == Occupation.Merchant && Settlement.CurrentSettlement == Hero.MainHero.CurrentSettlement && k.Name.ToString() == "Caribos the Mercer");
            Location locationOfCharacter = LocationComplex.Current.GetLocationOfCharacter(characterObject.HeroObject);
            CampaignEventDispatcher.Instance.OnPlayerStartTalkFromMenu(characterObject.HeroObject);
            PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(locationOfCharacter, null, characterObject, null);
        }

        private bool Conversation_with_lord()
        {
            //Hero.MainHero.ChangeHeroGold(500000);                  // Increase Gold to the Player
            //SetPersonalRelation(Hero.OneToOneConversationHero, 1000); // Increase NPC Influence


            /*foreach (TraitObject traitObject in DefaultTraits.Personality)
            {
                // InformationManager.DisplayMessage(new InformationMessage("Hero.OneToOneConversationHero = " + Hero.OneToOneConversationHero.Name)); // Output: Caribos the Mercer
                int traitLevel = Hero.OneToOneConversationHero.GetTraitLevel(traitObject);
                
                if (traitLevel != 0)
                {
                    MBTextManager.SetTextVariable("PERSONALITY_DESCRIPTION", traitObject.Description, false);
                    if (traitLevel < 0)
                    {
                        MBTextManager.SetTextVariable("SIGN", "{=!}Neg", false);
                    }
                    if (traitLevel > 0)
                    {
                        MBTextManager.SetTextVariable("SIGN", "{=!}Pos", false);
                    }
                }
            }*/
            SetPersonalRelation(Hero.OneToOneConversationHero, 1000); // Increase NPC Personal Relation
            Hero.MainHero.GetRelation(Hero.OneToOneConversationHero);
            return true;
        }
        private void StealFromNPC()
        {
            GameTexts.SetVariable("GOLD_ICON", "{=!}<img src=\"Icons\\Coin@2x\" extend=\"8\">");
            Hero.MainHero.ChangeHeroGold(5); // Increase Gold to the Player
            InformationManager.DisplayMessage(new InformationMessage(Agent.Main.Name.ToString() + " stoled 5 {GOLD_ICON} + GOLD_ICON from " + Hero.OneToOneConversationHero.Name));
        }
        private void NPCStealPlayer()
        {
            Hero.MainHero.ChangeHeroGold(-5);
            InformationManager.DisplayMessage(new InformationMessage(Hero.OneToOneConversationHero.Name + " stoled 5 gold from " + Agent.Main.Name));
        }
        private void Conversation_with_lord3()
        {
            //string a = "";
            //foreach (Agent agent in Mission.Current.Agents)
            //{
            //    if (agent.Character == Hero.OneToOneConversationHero.CharacterObject)
            //    {
            //        a = agent.Name;
            //        CustomAgent customAgent = new CustomAgent(agent) { Name = agent.Name };
            //        //customAgent.SetUpdateEmotion("happiness", 0.1);
            //        //customAgent.AddGoal("insult", "Anbard the Brave");
            //        customAgent.UpdateTarget("Anbard the Brave");
            //        break;
            //    }
            //}

            //foreach (Agent agent in Mission.Current.Agents)
            //{
            //    if (agent.Name == "Anbard the Brave")
            //    {
            //        CustomAgent customAgent = new CustomAgent(agent) { Name = agent.Name };
            //        customAgent.UpdateTarget(a);
            //    }
            //    break;
            //}
        }

        private bool Conversation1()
        {
            //bool a = CharacterObject.OneToOneConversationCharacter.Occupation == Occupation.TavernWench;
            //return CharacterObject.OneToOneConversationCharacter.Occupation == Occupation.TavernWench && !this._orderedDrinkThisVisit && this._orderedDrinkThisDayInSettlement == Settlement.CurrentSettlement;
            InformationManager.DisplayMessage(new InformationMessage("Testing OnConditionDelegate conversation1"));

            return true;
        }

        public void SetPersonalRelation(Hero otherHero, int value)
        {
            value = MBMath.ClampInt(value, -100, 100);
            CharacterRelationManager.SetHeroRelation(Hero.MainHero, otherHero, value);
        }

    }

    //internal ConversationSentence(string idString, TextObject text, string inputToken, string outputToken, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnClickableConditionDelegate clickableConditionDelegate, ConversationSentence.OnConsequenceDelegate consequenceDelegate, uint flags = 0U, int priority = 100, int agentIndex = 0, int nextAgentIndex = 0, object relatedObject = null, bool withVariation = false, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null, ConversationSentence.OnPersuasionOptionDelegate persuasionOptionDelegate = null);
    //public ConversationSentence AddPlayerLine(string id, string inputToken, string outputToken, string text, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnConsequenceDelegate consequenceDelegate, int priority = 100, ConversationSentence.OnClickableConditionDelegate clickableConditionDelegate = null, ConversationSentence.OnPersuasionOptionDelegate persuasionOptionDelegate = null)
    //public ConversationSentence AddDialogLine(string id, string inputToken, string outputToken, string text, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnConsequenceDelegate consequenceDelegate, int priority = 100, ConversationSentence.OnClickableConditionDelegate clickableConditionDelegate = null)
    //public ConversationSentence AddDialogLineMultiAgent(string id, string inputToken, string outputToken, TextObject text, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnConsequenceDelegate consequenceDelegate, int agentIndex, int nextAgentIndex, int priority = 100, ConversationSentence.OnClickableConditionDelegate clickableConditionDelegate = null)
}


/* TaleWorlds.CampaignSystem.dll
 * CharacterTraits on TaleWorlds.CampaignSystem.CharacterTraits
 * 
 * DefaultTraits on TaleWorlds.CampaignSystem.DefaultTraits
 */

//Location location = CampaignMission.Current.Location;
//Location locationWithId = LocationComplex.Current.GetLocationWithId("center");

//var p = location.GetCharacterList();
//foreach (var item in p) { InformationManager.DisplayMessage(new InformationMessage(item.Character.ToString())); }

//Hero heroObject = ConversationHelper.AskedLord.HeroObject;
//Agent agent = (Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent;


/*
 * TaleWorlds.CampaignSystem.Campaign.dll
 * AddBehaviors
 * 
 *             //[rf:idle_angry][ib:nervous]
 * */