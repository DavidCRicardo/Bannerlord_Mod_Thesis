using System;
using System.IO;
using System.Net;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.Library;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bannerlord_Social_AI
{
    class CustomMissionGauntletNameMarker : MissionView
    {
        public int ViewOrderPriority { get; }
        public CustomMissionGauntletNameMarker(CiF_CampaignBehavior_Dialogs CBB, Mission _mission) { this.ViewOrderPriorty = 1; CBB_ref = CBB; mission = _mission; }
        private CustomMissionNameMarkerVM _dataSource;
        private CiF_CampaignBehavior_Dialogs CBB_ref;
        private GauntletLayer _gauntletLayer;
        private Mission mission;

        private bool _firstTick = true;
        private string fileName = "";
        private string filePath = "";

        private int TotalSEs;
        private int NPCsInteractedWithPlayer;
        private int PlayerInteractedWithNPCs;
        private int NPCsInteractedWithNPCs;
        private int DaysPassed;
        private List<string> list;

        private enum DictionaryEnumWithSEs { Undefined, FriendlySEs, UnfriendlySEs, RomanticSEs, HostileSEs, SpecialSEs }

        private Dictionary<bool, Dictionary<Enum, int>> PlayerOrNPC_Dictionary =
            new Dictionary<bool, Dictionary<Enum, int>>
            {
                { false , new Dictionary<Enum, int> { 
                    { DictionaryEnumWithSEs.Undefined    , 0 },
                    { DictionaryEnumWithSEs.FriendlySEs  , 0 },
                    { DictionaryEnumWithSEs.UnfriendlySEs, 0 },
                    { DictionaryEnumWithSEs.RomanticSEs  , 0 },
                    { DictionaryEnumWithSEs.HostileSEs   , 0 },
                    { DictionaryEnumWithSEs.SpecialSEs   , 0 }, 
                } },
                { true , new Dictionary<Enum, int> {
                    { DictionaryEnumWithSEs.Undefined    , 0 },
                    { DictionaryEnumWithSEs.FriendlySEs  , 0 },
                    { DictionaryEnumWithSEs.UnfriendlySEs, 0 },
                    { DictionaryEnumWithSEs.RomanticSEs  , 0 },
                    { DictionaryEnumWithSEs.HostileSEs   , 0 },
                    { DictionaryEnumWithSEs.SpecialSEs   , 0 },
                } },
            };

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();

            _dataSource = new CustomMissionNameMarkerVM(mission, base.MissionScreen.CombatCamera);
            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriorty, "GauntletLayer");
            this._gauntletLayer.LoadMovie("NameMarkerMessage", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);

            CheckIfUserFileExists();

            LoadUserInfoFromFile();

            try
            {
                CampaignEvents.ConversationEnded.AddNonSerializedListener(this, new Action<CharacterObject>(this.OnConversationEnd));
            }
            catch (Exception e) { }
        }
        
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);

            if (!MBCommon.IsPaused && CampaignMission.Current != null)
            {
                _dataSource.Tick(dt);

                _dataSource.EnableDataSource();

                if (_dataSource.letsUpdate)
                {
                    _dataSource.letsUpdate = false;

                    var result = ConvertCustomAgentIntentionToDictionaryEnum(_dataSource.SE_identifier);
                    UpdateUserInfo(result, _dataSource.BooleanNumber);
                }

                if (_firstTick || CBB_ref.customAgents == null)
                {
                    _dataSource.IsEnabled = true;
                    CBB_ref.customAgents = _dataSource.customAgentsList;
                    _firstTick = false;

                }

                CheckIntentionFromNPCToPlayer();

                if (_dataSource.GetCanResetCBB_refVariables())
                {
                    ResetCBB_refVariables();
                    _dataSource.SetCanResetCBB_refVariables(false);
                }

                if (CBB_ref.ResetSocialExchanges)
                {
                    _dataSource.ResetSocialExchangesAllNPCsOptions();
                    CBB_ref.ResetSocialExchanges = false;
                    DaysPassed++;

                    SaveUserInfoToFile();
                    UploadFileToFTP();
                }
            }
        }

        public override void OnMissionScreenFinalize()
        {
            UploadFileToFTP();

            base.OnMissionScreenFinalize();
            base.MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
            _dataSource.OnFinalize();
            _dataSource = null;
        }

        private void CheckIntentionFromNPCToPlayer()
        {
            if (_dataSource.intentionRefToCBB != SocialExchangeSE.IntentionEnum.Undefined && _dataSource.customCharacterReftoCampaignBehaviorBase != null)
            {
                // check social move from character (offergift e.g) 
                CBB_ref.characterRef = _dataSource.customCharacterReftoCampaignBehaviorBase;
                CBB_ref.characterIdRef = _dataSource.customCharacterIdRefCampaignBehaviorBase;
                switch (_dataSource.intentionRefToCBB)
                {
                    case SocialExchangeSE.IntentionEnum.Positive:
                        if (CBB_ref.characterRef.SocialMove == "GiveGift")
                        {
                            CBB_ref.OfferGift = true;
                        }
                        else
                        {
                            CBB_ref.FriendlyBool = true;
                        }
                        break;
                    case SocialExchangeSE.IntentionEnum.Romantic:
                        CBB_ref.RomanticBool = true;
                        break;
                    case SocialExchangeSE.IntentionEnum.Negative:
                        CBB_ref.UnFriendlyBool = true;
                        break;
                    case SocialExchangeSE.IntentionEnum.Hostile:
                        CBB_ref.HostileBool = true;
                        break;
                    case SocialExchangeSE.IntentionEnum.Special:
                        CBB_ref.SpecialBool = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private void OnConversationEnd(CharacterObject characterObject)
        {
            if (_dataSource != null && _dataSource.customAgentsList != null)
            {
                if (CBB_ref.customAgentConversation == null) // prevent some kind of bug
                {
                    foreach (CustomAgent custom in _dataSource.customAgentsList)
                    {
                        if (custom.selfAgent.Character == characterObject && custom == _dataSource.customAgentInteractingWithPlayer)
                        {
                            CBB_ref.customAgentConversation = custom;
                            break;
                        }
                    }
                }         
     
                CBB_ref.FriendlyOptionExists = false;
                CBB_ref.UnFriendlyOptionExists = false;
                CBB_ref.RomanticOptionExists = false;
                CBB_ref.HostileOptionExists = false;

                CheckIfThereIsAnyChange(CBB_ref.customAgentConversation);
                _dataSource.OnConversationEndWithPlayer(CBB_ref.customAgentConversation);
            }
        }

        private void CheckIfThereIsAnyChange(CustomAgent customAgentConversation)
        {
            if (CBB_ref.AskOutPerformed)
            {
                CBB_ref.AskOutPerformed = false;
                CheckOptionToLock(customAgentConversation, "AskOut");
            }

            if (CBB_ref.StartDating)
            {
                Start_Dating(customAgentConversation);

                CBB_ref.StartDating = false;
                CheckOptionToLock(customAgentConversation, "AskOut");

                InformationManager.DisplayMessage(new InformationMessage(Agent.Main.Name + " is now Dating with " + customAgentConversation.Name));
            }
            else if (CBB_ref.DoBreak)
            {
                DoBreak(customAgentConversation);
                CBB_ref.DoBreak = false;
                CheckOptionToLock(customAgentConversation, "Break");

                InformationManager.DisplayMessage(new InformationMessage(Agent.Main.Name + " is broke up with " + customAgentConversation.Name));

                DictionaryEnumWithSEs b = ConvertCustomAgentIntentionToDictionaryEnum(CustomAgent.Intentions.Special);
                UpdateUserInfo(b, 1);
            }
            else if (CBB_ref.IncreaseRelationshipWithPlayer && CBB_ref.customAgentConversation != null)
            {
                string localRelation = GetRelationshipBetweenPlayerAndNPC();
                int value = 1;

                RelationInGameChanges(customAgentConversation, value);
                UpdateRelationWithPlayerChoice(customAgentConversation, localRelation, value);

                CheckOptionToLock(customAgentConversation, localRelation, value);

                CBB_ref.IncreaseRelationshipWithPlayer = false;
            }
            else if (CBB_ref.DecreaseRelationshipWithPlayer && CBB_ref.customAgentConversation != null)
            {
                string localRelation = GetRelationshipBetweenPlayerAndNPC();
                int value = -1;
                RelationInGameChanges(customAgentConversation, value);
                UpdateRelationWithPlayerChoice(customAgentConversation, localRelation, value);

                CheckOptionToLock(customAgentConversation, localRelation, value);

                CBB_ref.DecreaseRelationshipWithPlayer = false;
            }
            //else if (CBB_ref.giveCourage)
            //{
            //    GiveCourageToCharacter(customAgentConversation);
            //    CBB_ref.giveCourage = false;
            //}
        }

        //private void GiveCourageToCharacter(CustomAgent customAgentConversation)
        //{
        //    CustomAgent customAgent = _dataSource.customAgentsList.Find(c => c.selfAgent.Name == customAgentConversation.selfAgent.Name && c.Id == customAgentConversation.Id);
        //    customAgent.UpdateAllStatus(0, 0, 1, 0, 0, 0);
        //}

        private void CheckOptionToLock(CustomAgent customAgentConversation, string localRelation, int value = 0)
        {
            string socialExchange = "";
            CustomAgent.Intentions customAgentIntention = CustomAgent.Intentions.Undefined;

            if (localRelation == "AskOut" || localRelation == "Break")
            {
                SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Special, true);
                socialExchange = "AskOut";
                customAgentIntention = CustomAgent.Intentions.Special;
            }
            else
            {
                if (localRelation == "Friends")
                {
                    if (value > 0)
                    {
                        SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Friendly, true);
                        socialExchange = "Friendly";
                        customAgentIntention = CustomAgent.Intentions.Friendly;
                    }
                    else
                    {
                        SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Unfriendly, true);
                        socialExchange = "UnFriendly";
                        customAgentIntention = CustomAgent.Intentions.Unfriendly;
                    }
                }
                else
                {
                    if (value > 0)
                    {
                        SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Romantic, true);
                        socialExchange = "Romantic";
                        customAgentIntention = CustomAgent.Intentions.Romantic;
                    }
                    else
                    {
                        SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Hostile, true);
                        socialExchange = "Hostile";
                    }
                }
            }

            //Player fez uma SE com um NPC e vai guardar a info 
            DictionaryEnumWithSEs SE_Enum = ConvertCustomAgentIntentionToDictionaryEnum(customAgentIntention);
            // Save information from dictionary and variables to File
            UpdateUserInfo(SE_Enum, 1);

            _dataSource.SaveSavedSEs(customAgentConversation, socialExchange);
        }

        private static DictionaryEnumWithSEs ConvertCustomAgentIntentionToDictionaryEnum(CustomAgent.Intentions a)
        {
            var b = DictionaryEnumWithSEs.Undefined;
            switch (a)
            {
                case CustomAgent.Intentions.Friendly:
                    b = DictionaryEnumWithSEs.FriendlySEs;
                    break;
                case CustomAgent.Intentions.Unfriendly:
                    b = DictionaryEnumWithSEs.UnfriendlySEs;
                    break;
                case CustomAgent.Intentions.Romantic:
                    b = DictionaryEnumWithSEs.RomanticSEs;
                    break;
                case CustomAgent.Intentions.Hostile:
                    b = DictionaryEnumWithSEs.HostileSEs;
                    break;
                case CustomAgent.Intentions.Special:
                    b = DictionaryEnumWithSEs.SpecialSEs;
                    break;
                default:
                    break;
            }

            return b;
        }

        private void LoadUserInfoFromFile()
        {
            string json = File.ReadAllText(filePath + fileName);

            UserInfoJson deserializedUserInfoClass = JsonConvert.DeserializeObject<UserInfoJson>(json);
            if (deserializedUserInfoClass != null)
            {
                TotalSEs = deserializedUserInfoClass.TotalSocialExchanges;
                NPCsInteractedWithPlayer = deserializedUserInfoClass.NPCInteractedWithPlayer;
                PlayerInteractedWithNPCs = deserializedUserInfoClass.PlayerInteractedWithNPC;
                NPCsInteractedWithNPCs = deserializedUserInfoClass.NPCsInteractedWithNPC;
                DaysPassed = deserializedUserInfoClass.DaysPassedInGame;

                PlayerOrNPC_Dictionary.TryGetValue(false, out Dictionary<Enum, int> result);
                result[DictionaryEnumWithSEs.FriendlySEs] = deserializedUserInfoClass.NFriendly;
                result[DictionaryEnumWithSEs.FriendlySEs] = deserializedUserInfoClass.NFriendly;
                result[DictionaryEnumWithSEs.UnfriendlySEs] = deserializedUserInfoClass.NUnFriendly;
                result[DictionaryEnumWithSEs.RomanticSEs] = deserializedUserInfoClass.NRomantic;
                result[DictionaryEnumWithSEs.HostileSEs] = deserializedUserInfoClass.NHostile;
                result[DictionaryEnumWithSEs.SpecialSEs] = deserializedUserInfoClass.NSpecial;

                PlayerOrNPC_Dictionary[false] = result;

                PlayerOrNPC_Dictionary.TryGetValue(true, out result);
                result[DictionaryEnumWithSEs.FriendlySEs] = deserializedUserInfoClass.PFriendly;
                result[DictionaryEnumWithSEs.FriendlySEs] = deserializedUserInfoClass.PFriendly;
                result[DictionaryEnumWithSEs.UnfriendlySEs] = deserializedUserInfoClass.PUnFriendly;
                result[DictionaryEnumWithSEs.RomanticSEs] = deserializedUserInfoClass.PRomantic;
                result[DictionaryEnumWithSEs.HostileSEs] = deserializedUserInfoClass.PHostile;
                result[DictionaryEnumWithSEs.SpecialSEs] = deserializedUserInfoClass.PSpecial;

                PlayerOrNPC_Dictionary[true] = result;
            }
        }

        private void UpdateUserInfo(DictionaryEnumWithSEs dictionaryKey, int WhoWasTheInitiator)
        {
            Dictionary<Enum, int> result;
            int value; 

            switch (WhoWasTheInitiator)
            {
                case -1:
                    NPCsInteractedWithPlayer++;

                    PlayerOrNPC_Dictionary.TryGetValue(false, out result);
                    result.TryGetValue(dictionaryKey, out value);
                    result[dictionaryKey] = value + 1;
                    break;
                case 0:
                    NPCsInteractedWithNPCs++;

                    PlayerOrNPC_Dictionary.TryGetValue(false, out result);
                    result.TryGetValue(dictionaryKey, out value);
                    result[dictionaryKey] = value + 1;
                    break;
                case 1:
                    PlayerInteractedWithNPCs++;

                    PlayerOrNPC_Dictionary.TryGetValue(true, out result);
                    result.TryGetValue(dictionaryKey, out value);
                    result[dictionaryKey] = value + 1;
                    break;
                default:
                    break;
            }

            TotalSEs = NPCsInteractedWithNPCs + NPCsInteractedWithPlayer + PlayerInteractedWithNPCs;

            SaveUserInfoToFile();
        }

        private void SaveUserInfoToFile()
        {
            string json = File.ReadAllText(filePath + fileName);

            UserInfoJson deserializedUserInfoClass = JsonConvert.DeserializeObject<UserInfoJson>(json);
            if (deserializedUserInfoClass != null)
            {
                deserializedUserInfoClass.NPCInteractedWithPlayer = NPCsInteractedWithPlayer;
                deserializedUserInfoClass.PlayerInteractedWithNPC = PlayerInteractedWithNPCs;
                deserializedUserInfoClass.NPCsInteractedWithNPC = NPCsInteractedWithNPCs;
                deserializedUserInfoClass.TotalSocialExchanges = TotalSEs;
                deserializedUserInfoClass.DaysPassedInGame = DaysPassed;

                PlayerOrNPC_Dictionary.TryGetValue(false, out Dictionary<Enum, int> result);

                result.TryGetValue(DictionaryEnumWithSEs.FriendlySEs, out int value);
                deserializedUserInfoClass.NFriendly = value;
                result.TryGetValue(DictionaryEnumWithSEs.UnfriendlySEs, out value);
                deserializedUserInfoClass.NUnFriendly = value;
                result.TryGetValue(DictionaryEnumWithSEs.RomanticSEs, out value);
                deserializedUserInfoClass.NRomantic = value;
                result.TryGetValue(DictionaryEnumWithSEs.HostileSEs, out value);
                deserializedUserInfoClass.NHostile = value;
                result.TryGetValue(DictionaryEnumWithSEs.SpecialSEs, out value);
                deserializedUserInfoClass.NSpecial = value;

                PlayerOrNPC_Dictionary.TryGetValue(true, out result);

                result.TryGetValue(DictionaryEnumWithSEs.FriendlySEs, out value);
                deserializedUserInfoClass.PFriendly = value;
                result.TryGetValue(DictionaryEnumWithSEs.UnfriendlySEs, out value);
                deserializedUserInfoClass.PUnFriendly = value;
                result.TryGetValue(DictionaryEnumWithSEs.RomanticSEs, out value);
                deserializedUserInfoClass.PRomantic = value;
                result.TryGetValue(DictionaryEnumWithSEs.HostileSEs, out value);
                deserializedUserInfoClass.PHostile = value;
                result.TryGetValue(DictionaryEnumWithSEs.SpecialSEs, out value);
                deserializedUserInfoClass.PSpecial = value;
            }

            File.WriteAllText(filePath + fileName, JsonConvert.SerializeObject(deserializedUserInfoClass));
        }

        private void SetOptionAsUnavailable(CustomAgent customAgent, CustomAgent.Intentions intention, bool value)
        {
            customAgent.keyValuePairsSEs[intention] = value;
        }

        private static void RelationInGameChanges(CustomAgent customAgentConversation, int value)
        {
            Hero hero = Hero.FindFirst(h => h.CharacterObject == customAgentConversation.selfAgent.Character);
            if (hero != null && hero != Hero.MainHero)
            {
                float relationWithPlayer = hero.GetRelationWithPlayer();
                int newValue = (int)(relationWithPlayer + value);
                if (value > 0)
                {
                    if (newValue <= 100)
                    {
                        InformationManager.AddQuickInformation(new TextObject("Your relation is increased by " + value + " to " + newValue + " with " + hero.Name + "."), 0, hero.CharacterObject);
                        Hero.MainHero.SetPersonalRelation(hero, newValue);
                    }              
                }
                else
                {
                    InformationManager.AddQuickInformation(new TextObject("Your relation is decreased by " + value + " to " + newValue + " with " + hero.Name + "."), 0, hero.CharacterObject);
                    Hero.MainHero.SetPersonalRelation(hero, newValue);
                }
            }
        }

        private string GetRelationshipBetweenPlayerAndNPC()
        {
            CustomAgent AgentPlayer = _dataSource.customAgentsList.Find(c => c.selfAgent == Agent.Main);
            SocialNetworkBelief belief = AgentPlayer.SelfGetBeliefWithAgent(CBB_ref.customAgentConversation);

            string localRelation = "";
            if (belief == null)
            {
                localRelation = "Friends";
            }
            else
            {
                localRelation = belief.relationship;
            }

            return localRelation;
        }

        private void DoBreak(CustomAgent customAgentConversation)
        {
            SocialExchangeSE se = InitializeSocialExchange(customAgentConversation);
            se.BreakUpMethod();

            _dataSource.SaveToJson();
        }

        private void Start_Dating(CustomAgent customAgentConversation)
        {
            SocialExchangeSE se = InitializeSocialExchange(customAgentConversation);
            se.AskOutMethod(true);

            _dataSource.SaveToJson();
        }

        private void UpdateRelationWithPlayerChoice(CustomAgent customAgentConversation, string relation, int value)
        {
            SocialExchangeSE se = InitializeSocialExchange(customAgentConversation);
            se.PlayerConversationWithNPC(relation, value, true);

            _dataSource.SaveToJson();
        }

        private SocialExchangeSE InitializeSocialExchange(CustomAgent customAgentConversation)
        {
            CustomAgent customAgent = _dataSource.customAgentsList.Find(c => c.selfAgent.Name == customAgentConversation.selfAgent.Name && c.Id == customAgentConversation.Id);
            CustomAgent MainCustomAgent = _dataSource.customAgentsList.Find(c => c.selfAgent == Agent.Main);
            MainCustomAgent.customAgentTarget = customAgent;

            SocialExchangeSE se = new SocialExchangeSE("", MainCustomAgent, _dataSource.customAgentsList)
            {
                CustomAgentReceiver = customAgent
            };
            return se;
        }

        private void ResetCBB_refVariables()
        {
            CBB_ref.FriendlyBool = false;
            CBB_ref.OfferGift = false;
            CBB_ref.RomanticBool = false;
            CBB_ref.UnFriendlyBool = false;
            CBB_ref.HostileBool = false;
            CBB_ref.SpecialBool = false;
            CBB_ref.StartDating = false;
            CBB_ref.DoBreak = false;
            CBB_ref.IncreaseRelationshipWithPlayer = false;
            CBB_ref.DecreaseRelationshipWithPlayer = false;

            _dataSource.intentionRefToCBB = SocialExchangeSE.IntentionEnum.Undefined;
            _dataSource.customCharacterReftoCampaignBehaviorBase = null;
        }

        private void UploadFileToFTP()
        {
            string ftpServerIP = "ftp.davidricardo.x10host.com/";
            string ftpUserName = "user@davidricardo.x10host.com";
            string ftpPassword = "P2NVL60v";

            FileInfo objFile = new FileInfo(filePath + fileName);
            FtpWebRequest objFTPRequest;

            // Create FtpWebRequest object 
            objFTPRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + objFile.Name));

            // Set Credintials
            objFTPRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

            // By default KeepAlive is true, where the control connection is 
            // not closed after a command is executed.
            objFTPRequest.KeepAlive = false;

            // Set the data transfer type.
            objFTPRequest.UseBinary = true;

            // Set content length
            objFTPRequest.ContentLength = objFile.Length;

            // Set request method
            objFTPRequest.Method = WebRequestMethods.Ftp.UploadFile;

            // Set buffer size
            int intBufferLength = 16 * 1024;
            byte[] objBuffer = new byte[intBufferLength];

            // Opens a file to read
            FileStream objFileStream = objFile.OpenRead();

            try
            {
                // Get Stream of the file
                Stream objStream = objFTPRequest.GetRequestStream();

                int len = 0;

                while ((len = objFileStream.Read(objBuffer, 0, intBufferLength)) != 0)
                {
                    // Write file Content 
                    objStream.Write(objBuffer, 0, len);

                }

                objStream.Close();
                objFileStream.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CheckIfUserFileExists()
        {
            filePath = BasePath.Name + "/Modules/Bannerlord_Social_AI/Data/";

            string[] files = Directory.GetFiles(filePath);
            if (files.Length != 0)
            {
                foreach (string file in files)
                {
                    if (file.Contains("user"))
                    {
                        fileName = file.Remove(0, filePath.Length);
                        break;
                    }
                }
            }

            if (fileName == "")
            {
                //Create a new file
                fileName = GetListFiles();

                if (fileName != "")
                {
                    System.IO.File.Create(filePath + fileName).Close();

                    string text = "{ }";
                    UserInfoJson myDeserializedClass = JsonConvert.DeserializeObject<UserInfoJson>(text);
                    File.WriteAllText(filePath + fileName, JsonConvert.SerializeObject(myDeserializedClass));

                }
            }
        }

        private string GetListFiles()
        {
            list = new List<string>();

            try
            {
                string ftpServerIP = "ftp.davidricardo.x10host.com/";
                string ftpUserName = "user@davidricardo.x10host.com";
                string ftpPassword = "P2NVL60v";

                FileInfo objFile = new FileInfo(filePath);
                FtpWebRequest request;

                // Create FtpWebRequest object 
                request = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + objFile.Name));
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string names = reader.ReadToEnd();

                reader.Close();
                response.Close();
                responseStream.Close();

                list = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach (string file in list)
                {
                    if (file.Contains("user"))
                    {
                        string number = file.Remove(0, 5);
                        number = number.Replace(".json", "");

                        int id = int.Parse(number) + 1;

                        string newName = "user_" + id.ToString() + ".json";
                        return newName;
                    }
                }

                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}