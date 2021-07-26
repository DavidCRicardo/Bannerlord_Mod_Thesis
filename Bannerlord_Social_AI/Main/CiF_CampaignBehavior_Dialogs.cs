using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Bannerlord_Social_AI
{
    class CiF_CampaignBehavior_Dialogs : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoaded));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(this.DailyTick));
        }

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
        }
        
        public override void SyncData(IDataStore dataStore)
        {
        }

        private void DailyTick()
        {
            //InformationManager.DisplayMessage(new InformationMessage("Daily Tick"));
            ResetSocialExchanges = true;
        }

        public bool ResetSocialExchanges = false;

        private String inputToken;
        private String outputToken;
        private String text;
        private Random rnd;
        private int value;
        public bool FriendlyOptionExists;
        public bool UnFriendlyOptionExists;
        public bool RomanticOptionExists;
        public bool HostileOptionExists;

        private Dictionary<string, ConversationSentence.OnConditionDelegate> dictionaryConditions;
        private Dictionary<string, ConversationSentence.OnConsequenceDelegate> dictionaryConsequences;

        public void ReadJsonFile(CampaignGameStarter campaignGameStarter)
        {
            string json = File.ReadAllText(BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/player_conversations.json");
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
                        campaignGameStarter.AddPlayerLine("1", inputToken, outputToken, text, condition, consequence, 101, null, null);
                    }
                    else
                    {
                        campaignGameStarter.AddDialogLine("1", inputToken, outputToken, text, condition, consequence, 101, null);
                    }
                }
            }
        }

        public void InitializeDictionaries()
        {
            dictionaryConditions = new Dictionary<string, ConversationSentence.OnConditionDelegate>() {
                { "None" , null },
                { "NotRelationOrFriendForFriendly" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationForFriendlySEWithNPC_condition) }, // daily countdown
                { "NotRelationOrFriendForUnFriendly" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationForUnFriendlySEWithNPC_condition) }, // daily countdown
                { "CanAskOut" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanAskOutWithNPC_condition) },
                { "DatingForRomantic" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerIsDatingForRomanticSEWithNPC_condition) },
                { "DatingForHostile" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerIsDatingForHostileSEWithNPC_condition) },
                { "CanBreak" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBreakWithNPC_condition) },
                { "RomanticAdvanced" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerSleepWithNPC_condition) },
                { "NPCReactsToAskOut" , new ConversationSentence.OnConditionDelegate(NPC_AcceptReject_AskOut_condition) },
                { "PlayerOfferGift" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanOfferGiftToCompanion_condition) },
                { "CompanionOfferGift" , new ConversationSentence.OnConditionDelegate(CheckIfCompanionCanOfferGiftToPlayer_condition) },

                { "Friendly_NPC" , new ConversationSentence.OnConditionDelegate(FriendlyNPC) },
                { "OfferGift_NPC" , new ConversationSentence.OnConditionDelegate(OfferGiftNPC) },
                { "UnFriendly_NPC" , new ConversationSentence.OnConditionDelegate(UnFriendlyNPC) },
                { "Romantic_NPC" , new ConversationSentence.OnConditionDelegate(RomanticNPC) },
                { "Hostile_NPC" , new ConversationSentence.OnConditionDelegate(HostileNPC) },
                { "Special_NPC" , new ConversationSentence.OnConditionDelegate(SpecialNPC) }

            };

            dictionaryConsequences = new Dictionary<string, ConversationSentence.OnConsequenceDelegate>()
            {
                { "None" , null}, 
                { "Start_Dating" , new ConversationSentence.OnConsequenceDelegate(Start_Dating) },
                { "DoBreak" , new ConversationSentence.OnConsequenceDelegate(Do_BreakUp) },
                { "PlayerGiveItem" , new ConversationSentence.OnConsequenceDelegate(PlayerGivesItem) },
                { "Increase_Relation" , new ConversationSentence.OnConsequenceDelegate(Increase_Relation) },
                { "Decrease_Relation" , new ConversationSentence.OnConsequenceDelegate(Decrease_Relation) }
            };
        }


        private void AddSocialAgentsDialogs(CampaignGameStarter campaignGameStarter)
        {
            /**/
            //campaignGameStarter.AddPlayerLine("1", "tavernmaid_talk", "tavernmaid_order_teleport", "Can you guide me to a merchant? {GOLD_ICON} ", null, null, 100, null, null);
            //campaignGameStarter.AddDialogLine("1", "tavernmaid_order_teleport", "merchantTurn", "Sure.", null, new ConversationSentence.OnConsequenceDelegate(this.Conversation_tavernmaid_test_on_condition), 100, null);
            //campaignGameStarter.AddDialogLine("1", "merchantTurn", "close_window", "I am a merchant.", null, null, 100, null);
            
            //campaignGameStarter.AddPlayerLine("1", "t1", "lord_emergencyCall", "Let's call everyone!", new ConversationSentence.OnConditionDelegate(Condition_EmergencyCall), null, 100, null, null);
            //campaignGameStarter.AddDialogLine("1", "lord_emergencyCall", "close_window", "What happened?[rf:idle_angry][ib:nervous]!", null, new ConversationSentence.OnConsequenceDelegate(Consequence_EmergencyCall), 100, null);
            //campaignGameStarter.AddPlayerLine("1", "t2", "lord_emergencyCall2", "Ok, everything is fine!", new ConversationSentence.OnConditionDelegate(Condition_StopEmergencyCall), null, 100, null, null);
            //campaignGameStarter.AddDialogLine("1", "lord_emergencyCall2", "close_window", "Hum...[rf:idle_angry][ib:nervous]!", null, new ConversationSentence.OnConsequenceDelegate(Consequence_StopEmergencyCall), 100, null);
            //campaignGameStarter.AddDialogLine("1", "lord_emergencyCall3", "close_window", "So... What's going on?", new ConversationSentence.OnConditionDelegate(Condition_EmergencyCallGoingOn), new ConversationSentence.OnConsequenceDelegate(Consequence_EmergencyCallGoingOn), 101, null);
        }

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            this.AddSocialAgentsDialogs(campaignGameStarter);

            InitializeDictionaries();

            ReadJsonFile(campaignGameStarter);

            rnd = new Random();
            NewRandom();
            FriendlyOptionExists = false;
            UnFriendlyOptionExists = false;
            RomanticOptionExists = false;
            HostileOptionExists = false;
        }

        private void NewRandom()
        {
            value = rnd.Next(1, 3);
        }

        #region old
        public bool giveCourage { get; set; }
        private void Increase_Courage()
        {
            giveCourage = true;
        }

        private bool talking_with_NotNegativeTraits()
        {
            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
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
            }

            return false;
        }
        private bool talking_with_Charming()
        {
            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
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
            }

            return false;
        }
        #endregion 

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

        public bool OfferGift { get; set; }
        private bool OfferGiftNPC()
        {
            if (OfferGift && ThisAgentWillInteractWithPlayer())
            {
                OfferGift = false;
                return true;
            }
            else { return false; }
        }

        public bool FriendlyBool { get; set; }

        public bool RomanticBool { get; set; }

        public bool UnFriendlyBool { get; set; }

        public bool HostileBool { get; set; }

        public bool SpecialBool { get; set; }

        private bool FriendlyNPC() 
        {
            if (FriendlyBool && ThisAgentWillInteractWithPlayer())
            {
                FriendlyBool = false;
                return true;
            }
            else { return false; }
        }

        private bool RomanticNPC()
        {
            if (RomanticBool && ThisAgentWillInteractWithPlayer())
            {
                RomanticBool = false;
                return true;
            }
            else { return false; }
        }

        private bool UnFriendlyNPC()
        {
            if (UnFriendlyBool && ThisAgentWillInteractWithPlayer())
            {
                UnFriendlyBool = false;
                return true;
            }
            else { return false; }
        }

        private bool HostileNPC()
        {
            if (HostileBool && ThisAgentWillInteractWithPlayer())
            {
                HostileBool = false;
                return true;
            }
            else { return false; }
        }

        private bool SpecialNPC()
        {
            if (SpecialBool && ThisAgentWillInteractWithPlayer())
            {
                SpecialBool = false;
                return true;
            }
            else { return false; }
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

        public bool IncreaseRelationshipWithPlayer { get; set; }
        private void Increase_Relation()
        {
            IncreaseRelationshipWithPlayer = true;
        }

        public bool DecreaseRelationshipWithPlayer { get; set; }
        private void Decrease_Relation()
        {
            DecreaseRelationshipWithPlayer = true;
        }

        private void PlayerGivesItem()
        {
            CustomAgent customMainAgent = customAgents.Find(c => c.selfAgent == Agent.Main);
            Item item = customMainAgent.GetItem();
            customAgentConversation.AddItem(item.itemName, item.quantity);
            customMainAgent.RemoveItem(item.itemName, item.quantity);

            /*Hero.MainHero.ChangeHeroGold(-5);
            TextObject text = new TextObject(Hero.MainHero.Name + " offers 5 {GOLD_ICON} to " + customAgentConversation.Name);
            GameTexts.SetVariable("GOLD_ICON", "{=!}<img src=\"Icons\\Coin@2x\" extend=\"8\">");
            InformationManager.DisplayMessage(new InformationMessage(text.ToString()));*/

            InformationManager.DisplayMessage(new InformationMessage(Agent.Main.Name + " receives " + item.itemName));
        }

        public List<CustomAgent> customAgents;

        private bool CheckIfPlayerHasFriendOrNullRelationForFriendlySEWithNPC_condition()
        {
            if (FriendlyOptionExists)
            {
                return false;
            }

            if (value <= 0)
            {
                if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
                {
                    string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                    string _currentLocation = CampaignMission.Current.Location.StringId;

                    if (_currentLocation != "arena")
                    {
                        customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);

                        if (customAgentConversation != null)
                        {
                            bool value = CheckIfIsAvailable(CustomAgent.Intentions.Friendly);
                            if (!value)
                            {
                                customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                                CustomAgent customMainAgent = customAgents.Find(c => c.selfAgent == Agent.Main);
                                SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                                if (belief == null || belief.relationship == "Friends")
                                {
                                    NewRandom();
                                    FriendlyOptionExists = true;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            else 
            { 
                value--; 
            }

            return false;
        }

        private bool CheckIfPlayerHasFriendOrNullRelationForUnFriendlySEWithNPC_condition()
        {
            if (UnFriendlyOptionExists)
            {
                return false;
            }

            if (value <= 0)
            {
                if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
                {
                    string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                    string _currentLocation = CampaignMission.Current.Location.StringId;

                    if (_currentLocation != "arena")
                    {
                        customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);

                        if (customAgentConversation != null)
                        {
                            bool value = CheckIfIsAvailable(CustomAgent.Intentions.Unfriendly);
                            if (!value)
                            {
                                customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                                CustomAgent customMainAgent = customAgents.Find(c => c.selfAgent == Agent.Main);
                                SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                                if (belief == null || belief.relationship == "Friends")
                                {
                                    NewRandom();
                                    UnFriendlyOptionExists = true;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                value--;
            }

            return false;
        }

        private bool CheckIfIsAvailable(CustomAgent.Intentions intention)
        {
            customAgentConversation.keyValuePairsSEs.TryGetValue(intention, out bool value);
            return value;
        }

        private bool CheckIfPlayerCanAskOutWithNPC_condition()
        {
            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
            {
                string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                string _currentLocation = CampaignMission.Current.Location.StringId;

                if (_currentLocation != "arena")
                {
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
                }
            }
            return false;
        }

        private bool CheckIfPlayerIsDatingForRomanticSEWithNPC_condition()
        {
            if (RomanticOptionExists)
            {
                return false;
            }

            if (value <= 0)
            {
                if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
                {
                    string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                    string _currentLocation = CampaignMission.Current.Location.StringId;

                    if (_currentLocation != "arena")
                    {
                        customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);

                        if (customAgentConversation != null)
                        {
                            bool value = CheckIfIsAvailable(CustomAgent.Intentions.Romantic);
                            if (!value)
                            {
                                customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                                CustomAgent customMainAgent = customAgents.Find(c => c.selfAgent == Agent.Main);
                                SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                                if (belief != null && belief.relationship == "Dating")
                                {
                                    NewRandom();
                                    RomanticOptionExists = true;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                value--;
            }

            return false;
        }

        private bool CheckIfPlayerIsDatingForHostileSEWithNPC_condition()
        {
            if (HostileOptionExists)
            {
                return false;
            }

            if (value <= 0)
            {
                if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
                {
                    string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                    string _currentLocation = CampaignMission.Current.Location.StringId;

                    if (_currentLocation != "arena")
                    {
                        customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);

                        if (customAgentConversation != null)
                        {
                            bool value = CheckIfIsAvailable(CustomAgent.Intentions.Hostile);
                            if (!value)
                            {
                                customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                                CustomAgent customMainAgent = customAgents.Find(c => c.selfAgent == Agent.Main);
                                SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                                if (belief != null && belief.relationship == "Dating")
                                {
                                    NewRandom();
                                    HostileOptionExists = true;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                value--;
            }

            return false;
        }

        private bool CheckIfPlayerCanBreakWithNPC_condition()
        {
            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
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
            }
            
            return false;
        }
    
        private bool CheckIfPlayerSleepWithNPC_condition()
        {
            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
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
            }
            return false;
        }

        private bool NPC_AcceptReject_AskOut_condition()
        {
            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
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

        private bool CheckIfPlayerCanOfferGiftToCompanion_condition() 
        {
            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
            {
                string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                string _currentLocation = CampaignMission.Current.Location.StringId;

                if (_currentLocation != "arena")
                {
                    customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);

                    if (customAgentConversation != null)
                    {
                        Hero hero = Hero.OneToOneConversationHero;
                        CustomAgent customMainAgent = customAgents.Find(c => c.selfAgent == Agent.Main);

                        if (hero != null && hero.IsPlayerCompanion && !customMainAgent.ItemList.IsEmpty())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }         
            }
            return false;
        }

        private bool CheckIfCompanionCanOfferGiftToPlayer_condition()
        {
            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
            {
                string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                string _currentLocation = CampaignMission.Current.Location.StringId;

                if (_currentLocation != "arena")
                {
                    customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.selfAgent.Character == CharacterObject.OneToOneConversationCharacter);

                    if (customAgentConversation != null && customAgentConversation.customAgentTarget != null && customAgentConversation.customAgentTarget.selfAgent == Agent.Main)
                    {
                        Hero hero = Hero.OneToOneConversationHero;

                        if (hero != null && hero.IsPlayerCompanion)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }  
            }
            return false;
        }
        
        private void Conversation_tavernmaid_test_on_condition()
        {
            //Teleport character near to NPC
            CharacterObject characterObject = CharacterObject.All.FirstOrDefault((CharacterObject k) => k.Occupation == Occupation.Merchant && Settlement.CurrentSettlement == Hero.MainHero.CurrentSettlement && k.Name.ToString() == "Caribos the Mercer");
            Location locationOfCharacter = LocationComplex.Current.GetLocationOfCharacter(characterObject.HeroObject);
            CampaignEventDispatcher.Instance.OnPlayerStartTalkFromMenu(characterObject.HeroObject);
            PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(locationOfCharacter, null, characterObject, null);
        }
        
        public void SetPersonalRelation(Hero otherHero, int value)
        {
            value = MBMath.ClampInt(value, -100, 100);
            CharacterRelationManager.SetHeroRelation(Hero.MainHero, otherHero, value);
        }
    }
}