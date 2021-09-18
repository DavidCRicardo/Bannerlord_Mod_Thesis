using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TaleWorlds.Core;

namespace FriendlyLords
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

        public override void SyncData(IDataStore dataStore) { }

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
        public bool AskOutPerformed;
        public bool HaveAChildInitialMovePerformed;

        private Dictionary<string, ConversationSentence.OnConditionDelegate> dictionaryConditions;
        private Dictionary<string, ConversationSentence.OnConsequenceDelegate> dictionaryConsequences;

        public void ReadJsonFile(CampaignGameStarter campaignGameStarter)
        {
            string json;
            switch (BannerlordConfig.Language)
            {
                case "English":
                default:
                    json = File.ReadAllText(BasePath.Name + "/Modules/FriendlyLords/ModuleData/Localization/en/player_conversations.json");
                    break;
            }

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
                { "FriendlySE_condition" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationForFriendlySEWithNPC_condition) }, // daily countdown
                { "UnFriendlySE_condition" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerHasFriendOrNullRelationForUnFriendlySEWithNPC_condition) }, // daily countdown
                { "AskOut_condition" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanAskOutWithNPC_condition) },
                { "Romantic_condition" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerIsDatingForRomanticSEWithNPC_condition) },
                { "Hostile_condition" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerIsDatingForHostileSEWithNPC_condition) },
                { "Break_condition" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanBreakWithNPC_condition) },
                { "RomanticAdvanced_condition" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanHaveAChildWithNPC_condition) },
                { "NPCReactsToAskOut_condition" , new ConversationSentence.OnConditionDelegate(NPC_AcceptReject_AskOut_condition) },
                { "PlayerOfferGift_condition" , new ConversationSentence.OnConditionDelegate(CheckIfPlayerCanOfferGiftToCompanion_condition) },
                { "CompanionOfferGift_condition" , new ConversationSentence.OnConditionDelegate(CheckIfCompanionCanOfferGiftToPlayer_condition) },

                { "Friendly_NPC" , new ConversationSentence.OnConditionDelegate(FriendlyNPC) },
                { "OfferGift_NPC" , new ConversationSentence.OnConditionDelegate(OfferGiftNPC) },
                { "UnFriendly_NPC" , new ConversationSentence.OnConditionDelegate(UnFriendlyNPC) },
                { "Romantic_NPC" , new ConversationSentence.OnConditionDelegate(RomanticNPC) },
                { "Hostile_NPC" , new ConversationSentence.OnConditionDelegate(HostileNPC) },
                { "Break_NPC" , new ConversationSentence.OnConditionDelegate(BreakNPC) },
                { "Gratitude_NPC" , new ConversationSentence.OnConditionDelegate(GratitudeNPC) }


            };

            dictionaryConsequences = new Dictionary<string, ConversationSentence.OnConsequenceDelegate>()
            {
                { "None" , null},
                { "Start_Dating_consequence" , new ConversationSentence.OnConsequenceDelegate(Start_Dating) },
                { "DoBreak_consequence" , new ConversationSentence.OnConsequenceDelegate(Do_BreakUpSE) },
                { "PlayerGiveItem_consequence" , new ConversationSentence.OnConsequenceDelegate(PlayerGivesItem) },
                { "RomanticAdvanced_consequence" , new ConversationSentence.OnConsequenceDelegate(PlayerPerformHaveAChildSE) },
                { "Increase_Relation" , new ConversationSentence.OnConsequenceDelegate(Increase_Relation) },
                { "Decrease_Relation" , new ConversationSentence.OnConsequenceDelegate(Decrease_Relation) }
            };
        }

        private void AddSocialAgentsDialogs(CampaignGameStarter campaignGameStarter)
        {
        }

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            this.AddSocialAgentsDialogs(campaignGameStarter);

            InitializeDictionaries();

            ReadJsonFile(campaignGameStarter);

            rnd = new Random();
            value = NewRandomValue();

            FriendlyOptionExists = false;
            UnFriendlyOptionExists = false;
            RomanticOptionExists = false;
            HostileOptionExists = false;
            AskOutPerformed = false;
        }

        private int NewRandomValue()
        {
            return rnd.Next(1, 5);
        }

        public CIF_Character customAgentConversation { get; set; }
        public CIF_Character characterRefWithDesireToPlayer { get; set; }
        public int characterIdRefWithDesireToPlayer { get; set; }

        private bool ThisAgentWillInteractWithPlayer()
        {
            try
            {
                if (customAgents != null)
                {
                    customAgentConversation = customAgents.Find(c => c == characterRefWithDesireToPlayer && c.Id == characterIdRefWithDesireToPlayer);
                    if (customAgentConversation != null && CharacterObject.OneToOneConversationCharacter == characterRefWithDesireToPlayer.AgentReference.Character)
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {

            }

            return false;
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

        public bool GratitudeBool { get; set; }
        private bool GratitudeNPC()
        {
            if (GratitudeBool && ThisAgentWillInteractWithPlayer())
            {
                GratitudeBool = false;
                return true;
            }

            return false;
        }

        public bool FriendlyBool { get; set; }
        private bool FriendlyNPC()
        {
            if (FriendlyBool && ThisAgentWillInteractWithPlayer())
            {
                if (value <= 0)
                {
                    FriendlyBool = false;
                    value = NewRandomValue();
                    return true;
                }
                else { value--; }
            }

            return false;
        }

        public bool RomanticBool { get; set; }
        private bool RomanticNPC()
        {
            if (RomanticBool && ThisAgentWillInteractWithPlayer())
            {
                RomanticBool = false;
                return true;
            }
            else { return false; }
        }

        public bool UnFriendlyBool { get; set; }
        private bool UnFriendlyNPC()
        {
            if (UnFriendlyBool && ThisAgentWillInteractWithPlayer())
            {
                if (value <= 0)
                {
                    UnFriendlyBool = false;
                    value = NewRandomValue();
                    return true;
                }
                else { value--; }
            }

            return false;
        }

        public bool HostileBool { get; set; }
        private bool HostileNPC()
        {
            if (HostileBool && ThisAgentWillInteractWithPlayer())
            {
                HostileBool = false;
                return true;
            }
            else { return false; }
        }

        public bool BreakBool { get; set; }
        private bool BreakNPC()
        {
            if (BreakBool && ThisAgentWillInteractWithPlayer())
            {
                BreakBool = false;
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

        public bool DoBreak { get; set; }
        private void Do_BreakUpSE()
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
            CIF_Character customMainAgent = customAgents.Find(c => c.AgentReference == Agent.Main);
            Item item = customMainAgent.GetItem();
            customAgentConversation.AddItem(item.itemName, item.quantity);
            customMainAgent.RemoveItem(item.itemName, item.quantity);

            InformationManager.DisplayMessage(new InformationMessage(Agent.Main.Name + " receives " + item.itemName));
        }

        private void PlayerPerformHaveAChildSE()
        {
            HaveAChildInitialMovePerformed = true;
        }

        public List<CIF_Character> customAgents;

        private bool CheckIfPlayerHasFriendOrNullRelationForFriendlySEWithNPC_condition()
        {
            if (FriendlyOptionExists || customAgents == null)
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
                        customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.AgentReference.Character == CharacterObject.OneToOneConversationCharacter);

                        if (customAgentConversation != null)
                        {
                            bool available = CheckIfIsAvailable(CIF_Character.Intentions.Friendly);
                            if (!available)
                            {
                                customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                                CIF_Character customMainAgent = customAgents.Find(c => c.AgentReference == Agent.Main);
                                SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                                if (belief == null || belief.relationship == "Friends")
                                {
                                    value = NewRandomValue();
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
            if (UnFriendlyOptionExists || customAgents == null)
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
                        customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.AgentReference.Character == CharacterObject.OneToOneConversationCharacter);

                        if (customAgentConversation != null)
                        {
                            bool available = CheckIfIsAvailable(CIF_Character.Intentions.Unfriendly);
                            if (!available)
                            {
                                customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                                CIF_Character customMainAgent = customAgents.Find(c => c.AgentReference == Agent.Main);
                                SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                                if (belief == null || belief.relationship == "Friends")
                                {
                                    value = NewRandomValue();
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

        private bool CheckIfIsAvailable(CIF_Character.Intentions intention)
        {
            customAgentConversation.keyValuePairsSEs.TryGetValue(intention, out bool value);
            return value;
        }

        public bool auxBool { get; set; }
        private bool CheckIfPlayerCanAskOutWithNPC_condition()
        {
            if (AskOutPerformed || auxBool)
            {
                return false;
            }

            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null && customAgents != null)
            {
                string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                string _currentLocation = CampaignMission.Current.Location.StringId;

                if (_currentLocation != "arena")
                {
                    customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.AgentReference.Character == CharacterObject.OneToOneConversationCharacter);

                    if (customAgentConversation != null && customAgentConversation.AgentReference.Age > 18)
                    {
                        bool available = CheckIfIsAvailable(CIF_Character.Intentions.Special);
                        if (!available)
                        {
                            customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                            CIF_Character customMainAgent = customAgents.Find(c => c.AgentReference == Agent.Main);
                            SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);

                            if (belief != null && belief.relationship == "Dating")
                            {
                                return false;
                            }
                            if (belief != null && belief.relationship == "Friends" && belief.value >= 3)
                            {
                                auxBool = true;
                                return true; // NPC_Gender_condition(customAgentConversation);
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool CheckIfPlayerIsDatingForRomanticSEWithNPC_condition()
        {
            if (RomanticOptionExists || customAgents == null)
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
                        customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.AgentReference.Character == CharacterObject.OneToOneConversationCharacter);

                        if (customAgentConversation != null)
                        {
                            bool available = CheckIfIsAvailable(CIF_Character.Intentions.Romantic);
                            if (!available)
                            {
                                customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                                CIF_Character customMainAgent = customAgents.Find(c => c.AgentReference == Agent.Main);
                                SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                                if (belief != null && belief.relationship == "Dating")
                                {
                                    value = NewRandomValue();
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
            if (HostileOptionExists || customAgents == null)
            {
                return false;
            }

            if (value <= 0)
            {
                if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null && customAgents != null)
                {
                    string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                    string _currentLocation = CampaignMission.Current.Location.StringId;

                    if (_currentLocation != "arena")
                    {
                        customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.AgentReference.Character == CharacterObject.OneToOneConversationCharacter);

                        if (customAgentConversation != null)
                        {
                            bool available = CheckIfIsAvailable(CIF_Character.Intentions.Hostile);
                            if (!available)
                            {
                                customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                                CIF_Character customMainAgent = customAgents.Find(c => c.AgentReference == Agent.Main);
                                SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                                if (belief != null && belief.relationship == "Dating")
                                {
                                    value = NewRandomValue();
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
            if (DoBreak || customAgents == null)
            {
                return false;
            }

            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null && customAgents != null)
            {
                string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                string _currentLocation = CampaignMission.Current.Location.StringId;

                customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.AgentReference.Character == CharacterObject.OneToOneConversationCharacter);

                if (customAgentConversation != null)
                {
                    bool available = CheckIfIsAvailable(CIF_Character.Intentions.Special);
                    if (!available)
                    {
                        customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                        CIF_Character customMainAgent = customAgents.Find(c => c.AgentReference == Agent.Main);
                        SocialNetworkBelief belief = customMainAgent.SelfGetBeliefWithAgent(customAgentConversation);
                        if (belief != null && belief.relationship == "Dating" && belief.value <= 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool CheckIfPlayerCanHaveAChildWithNPC_condition()
        {
            if (HaveAChildInitialMovePerformed || customAgents == null)
            {
                return false;
            }

            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null && customAgents != null)
            {
                string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                string _currentLocation = CampaignMission.Current.Location.StringId;

                customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.AgentReference.Character == CharacterObject.OneToOneConversationCharacter);

                if (customAgentConversation != null)
                {
                    bool available = CheckIfIsAvailable(CIF_Character.Intentions.Special);
                    if (!available)
                    {
                        customAgentConversation.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
                        CIF_Character customMainAgent = customAgents.Find(c => c.AgentReference == Agent.Main);
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
            }
            return false;
        }

        private bool NPC_AcceptReject_AskOut_condition()
        {
            if (AskOutPerformed || customAgents == null)
            {
                return false;
            }

            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
            {
                string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                string _currentLocation = CampaignMission.Current.Location.StringId;

                customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.AgentReference.Character == CharacterObject.OneToOneConversationCharacter);

                if (customAgentConversation != null)
                {
                    AskOutPerformed = true;

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

        private bool NPC_Gender_condition(CIF_Character customAgentConversation)
        {
            if (Agent.Main.IsFemale || customAgentConversation.AgentReference.IsFemale)
            {
                if (Agent.Main.IsFemale && customAgentConversation.AgentReference.IsFemale)
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
            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null && customAgents != null)
            {
                string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                string _currentLocation = CampaignMission.Current.Location.StringId;

                if (_currentLocation != "arena")
                {
                    customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.AgentReference.Character == CharacterObject.OneToOneConversationCharacter);

                    if (customAgentConversation != null)
                    {
                        Hero hero = Hero.OneToOneConversationHero;
                        CIF_Character customMainAgent = customAgents.Find(c => c.AgentReference == Agent.Main);

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
            if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null && customAgents != null)
            {
                string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
                string _currentLocation = CampaignMission.Current.Location.StringId;

                if (_currentLocation != "arena" && CharacterObject.OneToOneConversationCharacter != null)
                {
                    customAgentConversation = customAgents.Find(c => c.NearPlayer == true && c.AgentReference.Character == CharacterObject.OneToOneConversationCharacter);

                    if (customAgentConversation != null && customAgentConversation.customAgentTarget != null && customAgentConversation.customAgentTarget.AgentReference == Agent.Main)
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

        public void SetPersonalRelation(Hero otherHero, int value)
        {
            value = MBMath.ClampInt(value, -100, 100);
            CharacterRelationManager.SetHeroRelation(Hero.MainHero, otherHero, value);
        }
    }
}

//#region old
//public bool giveCourage { get; set; }
//private void Increase_Courage()
//{
//    giveCourage = true;
//}

//private bool talking_with_NotNegativeTraits()
//{
//    if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
//    {
//        string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
//        string _currentLocation = CampaignMission.Current.Location.StringId;
////
//        CustomAgent agentConversation = customAgents.Find(c => c.selfAgent.Character == Hero.OneToOneConversationHero.CharacterObject);
//        if (agentConversation != null)
//        {
//            CustomAgent customAgent = new CustomAgent(agentConversation.selfAgent, agentConversation.Id);
//            customAgent.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
//            bool trait = customAgent.TraitList.Exists(t => t.traitName == "Calm" || t.traitName == "Shy" || t.traitName == "Friendly");
//            if (trait)
//            {
//                return true;
//            }
//        }
//    }

//    return false;
//}
//private bool talking_with_Charming()
//{
//    if (Hero.MainHero.CurrentSettlement != null && CampaignMission.Current.Location != null)
//    {
//        string _currentSettlement = Hero.MainHero.CurrentSettlement.Name.ToString();
//        string _currentLocation = CampaignMission.Current.Location.StringId;

//        CustomAgent agentConversation = customAgents.Find(c => c.selfAgent.Character == Hero.OneToOneConversationHero.CharacterObject);
//        if (agentConversation != null)
//        {
//            CustomAgent customAgent = new CustomAgent(agentConversation.selfAgent, agentConversation.Id);
//            customAgent.LoadDataFromJsonToAgent(_currentSettlement, _currentLocation);
//            Trait trait = customAgent.TraitList.Find(t => t.traitName == "Charming");
//            if (trait != null)
//            {
//                return true;
//            }
//            else { return false; }
//        }
//    }

//    return false;
//}
//#endregion
//private void Conversation_tavernmaid_test_on_condition()
//{
//    //Teleport character near to NPC
//    CharacterObject characterObject = CharacterObject.All.FirstOrDefault((CharacterObject k) => k.Occupation == Occupation.Merchant && Settlement.CurrentSettlement == Hero.MainHero.CurrentSettlement && k.Name.ToString() == "Caribos the Mercer");
//    Location locationOfCharacter = LocationComplex.Current.GetLocationOfCharacter(characterObject.HeroObject);
//    CampaignEventDispatcher.Instance.OnPlayerStartTalkFromMenu(characterObject.HeroObject);
//    PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(locationOfCharacter, null, characterObject, null);
//}
//Hero.MainHero.ChangeHeroGold(-5);
//TextObject text = new TextObject(Hero.MainHero.Name + " offers 5 {GOLD_ICON} to " + customAgentConversation.Name);
//GameTexts.SetVariable("GOLD_ICON", "{=!}<img src=\"Icons\\Coin@2x\" extend=\"8\">");
//InformationManager.DisplayMessage(new InformationMessage(text.ToString()));