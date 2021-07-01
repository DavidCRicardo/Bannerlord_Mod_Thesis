using System;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using System.Linq;
using SandBox;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace Bannerlord_Social_AI
{
    class CustomCampaignBehaviorBase : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoaded));
        }

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
        }
        
        public override void SyncData(IDataStore dataStore)
        {
        }

        private String inputToken;
        private String outputToken;
        private String text;

        private Dictionary<int, ConversationSentence.OnConditionDelegate> dictionaryConditions;
        private Dictionary<int, ConversationSentence.OnConsequenceDelegate> dictionaryConsequences;

        public void ReadJsonFile(CampaignGameStarter campaignGameStarter)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/player_conversations.json");
            CBB_Root myDeserializedCBB = JsonConvert.DeserializeObject<CBB_Root>(json);

            if (myDeserializedCBB != null)
            {
                foreach (PlayerNPCDialog item in myDeserializedCBB.PlayerNPCDialogs)
                {
                    inputToken = item.InputToken;
                    outputToken = item.OutputToken;
                    text = item.Text;

                    dictionaryConditions.TryGetValue(item.Condition, out ConversationSentence.OnConditionDelegate condition);
                    dictionaryConsequences.TryGetValue(item.Consequence, out ConversationSentence.OnConsequenceDelegate consequence);

                    if (item.PlayerDialog)
                    {
                        campaignGameStarter.AddPlayerLine("1", inputToken, outputToken, text, condition, consequence, 100, null, null);
                    }
                    else
                    {
                        campaignGameStarter.AddDialogLine("1", inputToken, outputToken, text, condition, consequence, 100, null);
                    }
                }
            }
        }

        public void InitializeDictionaries()
        {
            dictionaryConditions = new Dictionary<int, ConversationSentence.OnConditionDelegate>() {
                { 0 , null },
                { 1 , new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationWithNPC_condition) },
                { 2 , new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanFlirtWithNPC_condition) },
                { 3 , new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBullyWithNPC_condition) }
            };

            dictionaryConsequences = new Dictionary<int, ConversationSentence.OnConsequenceDelegate>()
            {
                { 0 , null}, 
                { 1 , new ConversationSentence.OnConsequenceDelegate(Increase_Friendship) },
                { 2 , new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship) },
                { 3 , new ConversationSentence.OnConsequenceDelegate(Increase_Dating) },
                { 4 , new ConversationSentence.OnConsequenceDelegate(Decrease_Dating) }
            };
        }

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            this.AddSocialAgentsDialogs(campaignGameStarter);

            InitializeDictionaries();

            ReadJsonFile(campaignGameStarter);
        }
        
        private bool talking_with_NotNegativeTraits()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;
            //
            CustomAgent agentConversation = customAgents.Find(c => c.selfAgent.Character == Hero.OneToOneConversationHero.CharacterObject);
            if (agentConversation != null)
            {
                CustomAgent customAgent = new CustomAgent(agentConversation.selfAgent, agentConversation.Id);
                customAgent.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                bool trait = customAgent.TraitList.Exists(t => t.traitName == "Calm" || t.traitName == "Shy" || t.traitName == "Friendly");
                if (trait)
                {
                    return true;
                }
            }

            return false;
        }
        private bool talking_with_Charming()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;

            CustomAgent agentConversation = customAgents.Find(c => c.selfAgent.Character == Hero.OneToOneConversationHero.CharacterObject);
            if (agentConversation != null)
            {
                CustomAgent customAgent = new CustomAgent(agentConversation.selfAgent, agentConversation.Id);
                customAgent.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                Trait trait = customAgent.TraitList.Find(t => t.traitName == "Charming");
                if (trait != null)
                {
                    return true;
                }
                else { return false; }
            }

            return false;
        }
  
        private void AddSocialAgentsDialogs(CampaignGameStarter campaignGameStarter)
        {
            //TavernKeeper
            // Friendly
            campaignGameStarter.AddPlayerLine("1", "tavernkeeper_talk", "lord_friendly", "You are awesome! [Friendly]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            // UnFriendly
            campaignGameStarter.AddPlayerLine("1", "tavernkeeper_talk", "lord_unfriendly", "My pet is smarter than you! [Unfriendly]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            // AskOut
            campaignGameStarter.AddPlayerLine("1", "tavernkeeper_talk", "lord_romanticAskOut", "Uh.. you want to start dating? [Romantic]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanAskOutWithNPC_condition), null, 100, null, null);
            // Flirt
            campaignGameStarter.AddPlayerLine("1", "tavernkeeper_talk", "lord_romanticFlirt", "You still beautiful as always! [Romantic]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanFlirtWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Dating), 100, null, null);
            // Hostile 
            campaignGameStarter.AddPlayerLine("1", "tavernkeeper_talk", "lord_hostile", "What are you doing here? Go home and wash some dishes! [Hostile]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBullyWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Decrease_Dating), 100, null, null);
            // Break
            campaignGameStarter.AddPlayerLine("1", "tavernkeeper_talk", "lord_break", "Time to break. This is not working anymore. [Special]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBreakWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Do_BreakUp), 100, null, null);
            // Sleep With NPC
            campaignGameStarter.AddPlayerLine("1", "tavernkeeper_talk", "lord_sleep", "What do you think about having a family? [Special]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerSleepWithNPC_condition), null, 100, null, null);

            //TavernMaid
            // Friendly
            campaignGameStarter.AddPlayerLine("1", "tavernmaid_talk", "lord_friendly", "You are awesome! [Friendly]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            // UnFriendly
            campaignGameStarter.AddPlayerLine("1", "tavernmaid_talk", "lord_unfriendly", "My pet is smarter than you! [Unfriendly]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            // AskOut
            campaignGameStarter.AddPlayerLine("1", "tavernmaid_talk", "lord_romanticAskOut", "Uh.. you want to start dating? [Romantic]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanAskOutWithNPC_condition), null, 100, null, null);
            // Flirt
            campaignGameStarter.AddPlayerLine("1", "tavernmaid_talk", "lord_romanticFlirt", "You still beautiful as always! [Romantic]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanFlirtWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Dating), 100, null, null);
            // Hostile 
            campaignGameStarter.AddPlayerLine("1", "tavernmaid_talk", "lord_hostile", "What are you doing here? Go home and wash some dishes! [Hostile]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBullyWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Decrease_Dating), 100, null, null);
            // Break
            campaignGameStarter.AddPlayerLine("1", "tavernmaid_talk", "lord_break", "Time to break. This is not working anymore. [Special]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBreakWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Do_BreakUp), 100, null, null);
            // Sleep With NPC
            campaignGameStarter.AddPlayerLine("1", "tavernmaid_talk", "lord_sleep", "What do you think about having a family? [Special]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerSleepWithNPC_condition), null, 100, null, null);

            //Child
            // Friendly
            campaignGameStarter.AddPlayerLine("1", "town_or_village_children_player_no_rhyme", "lord_friendly", "You are awesome! [Friendly]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player_children_post_rhyme", "lord_friendly", "You are awesome! [Friendly]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            // UnFriendly
            campaignGameStarter.AddPlayerLine("1", "town_or_village_children_player_no_rhyme", "lord_unfriendly", "My pet is smarter than you! [Unfriendly]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player_children_post_rhyme", "lord_unfriendly", "My pet is smarter than you! [Unfriendly]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            // AskOut
            campaignGameStarter.AddPlayerLine("1", "town_or_village_children_player_no_rhyme", "lord_romanticAskOut", "Uh.. you want to start dating? [Romantic]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanAskOutWithNPC_condition), null, 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player_children_post_rhyme", "lord_romanticAskOut", "Uh.. you want to start dating? [Romantic]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanAskOutWithNPC_condition), null, 100, null, null);
            // Flirt
            campaignGameStarter.AddPlayerLine("1", "town_or_village_children_player_no_rhyme", "lord_romanticFlirt", "You still beautiful as always! [Romantic]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanFlirtWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Dating), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player_children_post_rhyme", "lord_romanticFlirt", "You still beautiful as always! [Romantic]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanFlirtWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Dating), 100, null, null);
            // Hostile 
            campaignGameStarter.AddPlayerLine("1", "town_or_village_children_player_no_rhyme", "lord_hostile", "What are you doing here? Go home and wash some dishes! [Hostile]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBullyWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Decrease_Dating), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player_children_post_rhyme", "lord_hostile", "What are you doing here? Go home and wash some dishes! [Hostile]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBullyWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Decrease_Dating), 100, null, null);
            // Break
            campaignGameStarter.AddPlayerLine("1", "town_or_village_children_player_no_rhyme", "lord_break", "Time to break. This is not working anymore. [Special]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBreakWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Do_BreakUp), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player_children_post_rhyme", "lord_break", "Time to break. This is not working anymore. [Special]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBreakWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Do_BreakUp), 100, null, null);
            // Sleep With NPC
            campaignGameStarter.AddPlayerLine("1", "town_or_village_children_player_no_rhyme", "lord_sleep", "What do you think about having a family? [Special]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerSleepWithNPC_condition), null, 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player_children_post_rhyme", "lord_sleep", "What do you think about having a family? [Special]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerSleepWithNPC_condition), null, 100, null, null);

            //Town Or Village
            // Friendly
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player", "lord_friendly", "You are awesome! [Friendly]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            // UnFriendly
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player", "lord_unfriendly", "My pet is smarter than you! [Unfriendly]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            // AskOut
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player", "lord_romanticAskOut", "Uh.. you want to start dating? [Romantic]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanAskOutWithNPC_condition), null, 100, null, null);
            // Flirt
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player", "lord_romanticFlirt", "You still beautiful as always! [Romantic]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanFlirtWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Dating), 100, null, null);
            // Hostile 
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player", "lord_hostile", "What are you doing here? Go home and wash some dishes! [Hostile]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBullyWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Decrease_Dating), 100, null, null);
            // Break
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player", "lord_break", "Time to break. This is not working anymore. [Special]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBreakWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Do_BreakUp), 100, null, null);
            // Sleep With NPC
            campaignGameStarter.AddPlayerLine("1", "town_or_village_player", "lord_sleep", "What do you think about having a family? [Special]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerSleepWithNPC_condition), null, 100, null, null);


            /* Increase Courage */
            campaignGameStarter.AddPlayerLine("1175", "hero_main_options", "hero_increase_courage", "You can fight against the bully. [Increase Courage]", new ConversationSentence.OnConditionDelegate(talking_with_NotNegativeTraits), null, 100, null, null);
            campaignGameStarter.AddDialogLine("1175", "hero_increase_courage", "close_window", "Ok, I will try.", null, new ConversationSentence.OnConsequenceDelegate(Increase_Courage), 100, null);

            campaignGameStarter.AddPlayerLine("1", "hero_main_options", "lord_date", "You must give a chance to date. [Increase Courage]", new ConversationSentence.OnConditionDelegate(talking_with_Charming), new ConversationSentence.OnConsequenceDelegate(Increase_Courage), 100, null, null);
            campaignGameStarter.AddDialogLine("1", "lord_date", "close_window", "I don't know... Well, why not.", null, null, 100, null);

            /* Lords */
            #region /* Player Interactions with NPC */

            // Friendly
            campaignGameStarter.AddPlayerLine("1", "hero_main_options", "lord_friendly", "You are awesome! [Friendly]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            // UnFriendly
            campaignGameStarter.AddPlayerLine("1", "hero_main_options", "lord_unfriendly", "My pet is smarter than you! [Unfriendly]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            // AskOut
            campaignGameStarter.AddPlayerLine("1", "hero_main_options", "lord_romanticAskOut", "Uh.. you want to start dating? [Romantic]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanAskOutWithNPC_condition), null, 100, null, null);
            // Flirt
            campaignGameStarter.AddPlayerLine("1", "hero_main_options", "lord_romanticFlirt", "You still beautiful as always! [Romantic]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanFlirtWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Increase_Dating), 100, null, null);
            // Hostile 
            campaignGameStarter.AddPlayerLine("1", "hero_main_options", "lord_hostile", "What are you doing here? Go home and wash some dishes! [Hostile]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBullyWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Decrease_Dating), 100, null, null);
            // Break
            campaignGameStarter.AddPlayerLine("1", "hero_main_options", "lord_break", "Time to break. This is not working anymore. [Special]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBreakWithNPC_condition), new ConversationSentence.OnConsequenceDelegate(Do_BreakUp), 100, null, null);
            // Sleep With NPC
            campaignGameStarter.AddPlayerLine("1", "hero_main_options", "lord_sleep", "What do you think about having a family? [Special]", new ConversationSentence.OnConditionDelegate(CheckIfPlayerSleepWithNPC_condition), null, 100, null, null);


            // React to Friendly
            campaignGameStarter.AddDialogLine("1", "lord_friendly", "close_window", "Oh...that's nice. Thank you![if:idle_pleased]", null, null, 100, null);
            // React to UnFriendly
            campaignGameStarter.AddDialogLine("1", "lord_unfriendly", "close_window", "Oh...that's not nice![if:idle_angry]", null, null, 100, null);
            // Accept AskOut
            campaignGameStarter.AddDialogLine("1", "lord_romanticAskOut", "close_window", "Oh, you're so kind![if:idle_pleased][ib:confident]", new ConversationSentence.OnConditionDelegate(NPC_AcceptReject_Dating_condition), new ConversationSentence.OnConsequenceDelegate(Start_Dating), 100, null); // Accept depending if have Faithful Trait and not dating or not having the trait and dating
            // Reject AskOut
            //campaignGameStarter.AddDialogLine("1", "lord_romanticAskOut", "close_window", "Oh, sorry but I'm currently dating![if:idle_pleased][ib:confident]", new ConversationSentence.OnConditionDelegate(NPC_AcceptReject_Dating_condition), null, 100, null); // Reject depending if have Faithful Trait & Dating with anyone
            //campaignGameStarter.AddDialogLine("1", "lord_romanticAskOut", "close_window", message + "Oh, sorry... I'm not interested![if:idle_pleased][ib:confident]", new ConversationSentence.OnConditionDelegate(NPC_Gender_condition), null, 100, null); // Reject if they are the same gender
            campaignGameStarter.AddDialogLine("1", "lord_romanticAskOut", "close_window", "Oh, sorry... I'm not interested![if:idle_pleased][ib:confident]", null, null, 100, null); // Reject if they are the same gender
            // Reacting to Flirt
            campaignGameStarter.AddDialogLine("1", "lord_romanticFlirt", "close_window", "Oh, you're so kind![if:idle_pleased][ib:confident]", null, new ConversationSentence.OnConsequenceDelegate(Increase_Dating), 100, null); // Accept depending if have Faithful Trait and not dating or not having the trait and dating
            // Reacting to Hostile
            campaignGameStarter.AddDialogLine("1", "lord_hostile", "close_window", "Oh, don't need to be so rude![if:idle_pleased][ib:confident]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Dating), 100, null);
            // Reacting to Break
            campaignGameStarter.AddDialogLine("1", "lord_break", "close_window", "Oh, fine! Have a good day sir![if:idle_angry]", null, null, 100, null);
            // Reacting to Sleep
            campaignGameStarter.AddDialogLine("1", "lord_sleep", "close_window", "Oh, I don't feel ready to take that step. Maybe next time![if:idle_pleased][ib:confident]", null, null, 100, null);

            #endregion

            #region /* NPC Interactions with Player */

            /* NPC Friendly Interactions With Player */
            campaignGameStarter.AddDialogLine("1", "start", "Friendly_start", "Hi Friend... If you need something just tell me, maybe I can help you.[ib:closed][if:idle_pleased]", new ConversationSentence.OnConditionDelegate(Friendly), null, 200, null);
            campaignGameStarter.AddPlayerLine("1", "Friendly_start", "Friendly_step1", "Yes, sure. I appreciate it. [Accept]", null, new ConversationSentence.OnConsequenceDelegate(Increase_Friendship), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "Friendly_start", "close_window", "Huh.. Someone is calling me!", null, null, 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "Friendly_start", "Friendly_step2", "Do you think that I am a kid or something? I don't need your help! [Reject]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null, null);
            campaignGameStarter.AddDialogLine("1", "Friendly_step1", "close_window", "It's a pleasure to help you. [if:idle_pleased]", null, null, 100, null);
            campaignGameStarter.AddDialogLine("1", "Friendly_step2", "close_window", "Take it easy. There is no need to be rude. [rf:idle_angry]", null, null, 100, null);

            /* NPC UnFriendly Interactions With Player */
            campaignGameStarter.AddDialogLine("1", "start", "UnFriendly_start", "Why are you listening people's conversation?", new ConversationSentence.OnConditionDelegate(UnFriendly), null, 200, null);
            campaignGameStarter.AddPlayerLine("1", "UnFriendly_start", "UnFriendly_step1", "Sorry, it wouldn't happen again. [Accept]", null, null, 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "UnFriendly_start", "UnFriendly_step2", "Just curiosity. [Reject]", null, null, 100, null, null);
            campaignGameStarter.AddDialogLine("1", "UnFriendly_step1", "close_window", "I hope not.", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Friendship), 100, null);
            campaignGameStarter.AddDialogLine("1", "UnFriendly_step2", "close_window", "Curiosity, huh.", null, null, 100, null);

            /* NPC Romantic Interactions With Player */
            campaignGameStarter.AddDialogLine("1", "start", "Romantic_start", "You are looking really charming today.[if:idle_pleased]", new ConversationSentence.OnConditionDelegate(Romantic), null, 200, null);
            campaignGameStarter.AddPlayerLine("1", "Romantic_start", "Romantic_step1", "Hehe You're kind as always. [Accept]", null, new ConversationSentence.OnConsequenceDelegate(Increase_Dating), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "Romantic_start", "Romantic_step2", "Are you blind? Go away! [Reject]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Dating), 100, null, null);
            campaignGameStarter.AddDialogLine("1", "Romantic_step1", "close_window", "Thank you Sr.[ib:confident]", null, null, 100, null);
            campaignGameStarter.AddDialogLine("1", "Romantic_step2", "close_window", "Not nice.", null, null, 100, null);

            /* NPC Hostile Interactions With Player */
            campaignGameStarter.AddDialogLine("1", "start", "Hostile_start", "You want to fight, huh? [ib:aggressive]", new ConversationSentence.OnConditionDelegate(Hostile), null, 200, null);
            campaignGameStarter.AddPlayerLine("1", "Hostile_start", "Hostile_step1", "You want to hurt yourself? [Accept]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Dating), 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "Hostile_start", "Hostile_step2", "Are you kidding me? [Reject]", null, new ConversationSentence.OnConsequenceDelegate(Decrease_Dating), 100, null, null);
            campaignGameStarter.AddDialogLine("1", "Hostile_step1", "close_window", "You don't deserve my time.", null, null, 100, null);
            campaignGameStarter.AddDialogLine("1", "Hostile_step2", "close_window", "Hum... maybe next time...", null, null, 100, null);

            /* NPC Special Interactions With Player */
            campaignGameStarter.AddDialogLine("1", "start", "Special_start", "Special", new ConversationSentence.OnConditionDelegate(Special), null, 200, null);
            campaignGameStarter.AddPlayerLine("1", "Special_start", "Special_step1", "Yes, I think it's better for both. [Accept]", null, null, 100, null, null);
            campaignGameStarter.AddPlayerLine("1", "Special_start", "Special_step2", "What? Are you kidding me? [Reject]", null, null, 100, null, null);
            campaignGameStarter.AddDialogLine("1", "Special_step1", "close_window", "Yes, maybe we can be friends.", null, new ConversationSentence.OnConsequenceDelegate(Do_BreakUp), 100, null);
            campaignGameStarter.AddDialogLine("1", "Special_step2", "close_window", "No, I am not. Have a good day!", null, new ConversationSentence.OnConsequenceDelegate(Do_BreakUp), 100, null);

            #endregion

            /**/
            campaignGameStarter.AddPlayerLine("1", "tavernmaid_talk", "tavernmaid_order_teleport", "Can you guide me to a merchant?", null, null, 100, null, null);
            campaignGameStarter.AddDialogLine("1", "tavernmaid_order_teleport", "merchantTurn", "Sure.", null, new ConversationSentence.OnConsequenceDelegate(this.Conversation_tavernmaid_test_on_condition), 100, null);
            campaignGameStarter.AddDialogLine("1", "merchantTurn", "close_window", "I am a merchant.", null, null, 100, null);
            campaignGameStarter.AddPlayerLine("1", "t1", "lord_emergencyCall", "Let's call everyone!", new ConversationSentence.OnConditionDelegate(Condition_EmergencyCall), null, 100, null, null);
            campaignGameStarter.AddDialogLine("1", "lord_emergencyCall", "close_window", "What happened?[rf:idle_angry][ib:nervous]!", null, new ConversationSentence.OnConsequenceDelegate(Consequence_EmergencyCall), 100, null);
            campaignGameStarter.AddPlayerLine("1", "t2", "lord_emergencyCall2", "Ok, everything is fine!", new ConversationSentence.OnConditionDelegate(Condition_StopEmergencyCall), null, 100, null, null);
            campaignGameStarter.AddDialogLine("1", "lord_emergencyCall2", "close_window", "Hum...[rf:idle_angry][ib:nervous]!", null, new ConversationSentence.OnConsequenceDelegate(Consequence_StopEmergencyCall), 100, null);
            campaignGameStarter.AddDialogLine("1", "lord_emergencyCall3", "close_window", "So... What's going on?", new ConversationSentence.OnConditionDelegate(Condition_EmergencyCallGoingOn), new ConversationSentence.OnConsequenceDelegate(Consequence_EmergencyCallGoingOn), 101, null);

        }

        public CustomAgent customAgentConversation { get; set; }
        public CustomAgent characterRef { get; set; }

        private bool ThisAgentWillInteractWithPlayer()
        {
            customAgentConversation = customAgents.Find( c => c == characterRef);

            if (customAgentConversation != null)
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
            if (RomanticBool && ThisAgentWillInteractWithPlayer())
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


        public bool giveCourage { get; set; }
        public bool IncreaseFriendshipWithPlayer { get; set; }
        public bool DecreaseFriendshipWithPlayer { get; set; }
        public bool IncreaseDatingWithPlayer { get; set; }
        public bool DecreaseDatingWithPlayer { get; set; }

        private void Increase_Courage()
        {
            giveCourage = true;
        }

        private void Increase_Friendship()
        {
            IncreaseFriendshipWithPlayer = true;
        }

        private void Decrease_Friendship()
        {
            DecreaseFriendshipWithPlayer = true;
        }

        private void Increase_Dating()
        {
            IncreaseDatingWithPlayer = true;
        }

        private void Decrease_Dating()
        {
            DecreaseDatingWithPlayer = true;
        }

        /* Start Dating aka AskOut */
        public bool StartDating { get; set; }
        private void Start_Dating()
        {
            StartDating = true;
        }

        /* Break */
        public bool DoBreak { get; set; }
        private void Do_BreakUp()
        {
            DoBreak = true;
        }

        public List<CustomAgent> customAgents;

        private bool CheckIfPlayerHasFriendOrNullRelationWithNPC_condition()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;

            customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);

            if (customAgentConversation != null)
            {
                customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                CustomAgent customMainAgent = customAgents.Find(c => c.selfAgent == Agent.Main);
                SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                if (belief == null || belief.relationship == "Friends")
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckIfPlayerCanAskOutWithNPC_condition()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;

            customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);

            if (customAgentConversation != null && customAgentConversation.selfAgent.Age > 18)
            {
                customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                CustomAgent customMainAgent = customAgents.Find(c => c.selfAgent == Agent.Main);
                SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);

                if (belief != null && belief.relationship == "Dating")
                {
                    return false;
                }
                if (belief != null && belief.relationship == "Friends" && belief.value >= 3)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckIfPlayerCanFlirtWithNPC_condition()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;

            customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);

            if (customAgentConversation != null)
            {
                customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                CustomAgent customMainAgent = customAgents.Find(c => c.selfAgent == Agent.Main);
                SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                if (belief != null && belief.relationship == "Dating")
                {
                    return true;
                }
                
            }
            return false;
        }

        private bool CheckIfPlayerCanBullyWithNPC_condition()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;

            customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);

            if (customAgentConversation != null)
            {
                customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                    CustomAgent customMainAgent = customAgents.Find(c => c.selfAgent == Agent.Main);
                    SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                    if (belief != null && belief.relationship == "Dating")
                    {
                        return true;
                    }
                
            }
            return false;
        }

        private bool CheckIfPlayerCanBreakWithNPC_condition()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;

            customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);

            if (customAgentConversation != null)
            {
                customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                    CustomAgent customMainAgent = customAgents.Find(c => c.selfAgent == Agent.Main);
                    SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                    if (belief != null && belief.relationship == "Dating" && belief.value <= 0)
                    {
                        return true;
                    }
            }
            
            return false;
        }
    
        private bool CheckIfPlayerSleepWithNPC_condition()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;

            customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);

            if (customAgentConversation != null)
            {
                    customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                    CustomAgent customMainAgent = customAgents.Find(c => c.selfAgent == Agent.Main);
                    SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                    if (belief != null && belief.relationship == "Friends")
                    {
                        return false;
                    }
                    if (belief != null && belief.relationship == "Dating" && belief.value >= 5)
                    {
                        return true;
                    }
                
            }
            return false;
        }

        private bool NPC_AcceptReject_Dating_condition()
        {
            string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
            string _currentLocation = CampaignMission.Current.Location.StringId;

            customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);

            if (customAgentConversation != null)
            {
                if (NPC_Gender_condition(customAgentConversation))
                {
                    customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                    bool isFaithful = customAgentConversation.TraitList.Exists(t => t.traitName == "Faithful");
                    bool isCharming = customAgentConversation.TraitList.Exists(t => t.traitName == "Charming");

                    int datingHowMany = customAgentConversation.CheckHowManyTheAgentIsDating(customAgentConversation);

                    if (isFaithful && isCharming && datingHowMany <= 0)
                    {
                        return true;
                    }
                    else if (!isFaithful || isCharming)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private bool NPC_Gender_condition(CustomAgent customAgentConversation)
        {
            if (Agent.Main.IsFemale || customAgentConversation.selfAgent.IsFemale)
            {
                if (Agent.Main.IsFemale && customAgentConversation.selfAgent.IsFemale)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        #region Emergency Call 

        public bool AskWhatsGoinOn { get; set; }
        private bool Condition_EmergencyCallGoingOn()
        {
            return AskWhatsGoinOn;
        }
        private void Consequence_EmergencyCallGoingOn()
        {
            AskWhatsGoinOn = false;
        }

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
            Hero.MainHero.ChangeHeroGold(500000);                     // Increase Gold to the Player
            SetPersonalRelation(Hero.OneToOneConversationHero, 1000); // Increase NPC Personal Relation
            Hero.MainHero.GetRelation(Hero.OneToOneConversationHero);
            return true;
        }
        
        public void SetPersonalRelation(Hero otherHero, int value)
        {
            value = MBMath.ClampInt(value, -100, 100);
            CharacterRelationManager.SetHeroRelation(Hero.MainHero, otherHero, value);
        }
    }
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


//internal ConversationSentence(string idString, TextObject text, string inputToken, string outputToken, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnClickableConditionDelegate clickableConditionDelegate, ConversationSentence.OnConsequenceDelegate consequenceDelegate, uint flags = 0U, int priority = 100, int agentIndex = 0, int nextAgentIndex = 0, object relatedObject = null, bool withVariation = false, ConversationSentence.OnMultipleConversationConsequenceDelegate speakerDelegate = null, ConversationSentence.OnMultipleConversationConsequenceDelegate listenerDelegate = null, ConversationSentence.OnPersuasionOptionDelegate persuasionOptionDelegate = null);
//public ConversationSentence AddPlayerLine(string id, string inputToken, string outputToken, string text, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnConsequenceDelegate consequenceDelegate, int priority = 100, ConversationSentence.OnClickableConditionDelegate clickableConditionDelegate = null, ConversationSentence.OnPersuasionOptionDelegate persuasionOptionDelegate = null)
//public ConversationSentence AddDialogLine(string id, string inputToken, string outputToken, string text, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnConsequenceDelegate consequenceDelegate, int priority = 100, ConversationSentence.OnClickableConditionDelegate clickableConditionDelegate = null)
//public ConversationSentence AddDialogLineMultiAgent(string id, string inputToken, string outputToken, TextObject text, ConversationSentence.OnConditionDelegate conditionDelegate, ConversationSentence.OnConsequenceDelegate consequenceDelegate, int agentIndex, int nextAgentIndex, int priority = 100, ConversationSentence.OnClickableConditionDelegate clickableConditionDelegate = null)

// TaleWorlds.CampaignSystem.dll : TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.Towns
// private void TavernEmployeesCampaignBehavior(CampaignGameStarter campaignGameStarter)

// TaleWorlds.CampaignSystem.dll : CampaignSystem.SandBox.Source.Towns.CommonVillagersCampaignBehavior
// private void AddTownspersonAndVillagerDialogs(CampaignGameStarter campaignGameStarter)