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
using static System.Net.WebRequestMethods;

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
        private List<string> list;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();

            _dataSource = new CustomMissionNameMarkerVM(mission, base.MissionScreen.CombatCamera);
            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriorty, "GauntletLayer");
            this._gauntletLayer.LoadMovie("NameMarkerMessage", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);

            CheckIfUserFileExists();

            ListFiles();

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

                if (_firstTick || CBB_ref.customAgents == null)
                {
                    _dataSource.IsEnabled = true;
                    CBB_ref.customAgents = _dataSource.customAgentsList;
                    _firstTick = false;

                    UploadFileToFTP();
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
                }
            }
        }

        public override void OnMissionScreenFinalize()
        {
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
                foreach (CustomAgent custom in _dataSource.customAgentsList)
                {
                    if (custom.selfAgent.Character == characterObject)
                    {
                        CBB_ref.customAgentConversation = custom;
                        break;
                    }
                }

                CBB_ref.FriendlyOptionExists = false;

                CheckIfThereIsAnyChange(CBB_ref.customAgentConversation);
                _dataSource.OnConversationEndWithPlayer(CBB_ref.customAgentConversation);
            }
        }
        
        private void CheckIfThereIsAnyChange(CustomAgent customAgentConversation)
        {
            if (CBB_ref.StartDating)
            {
                Start_Dating(customAgentConversation);
                CBB_ref.StartDating = false;
                InformationManager.DisplayMessage(new InformationMessage(Agent.Main.Name + " is now Dating with " + customAgentConversation.Name));
            }
            else if (CBB_ref.DoBreak)
            {
                DoBreak(customAgentConversation);
                CBB_ref.DoBreak = false;

                InformationManager.DisplayMessage(new InformationMessage(Agent.Main.Name + " is broke up with " + customAgentConversation.Name));

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
            else if (CBB_ref.giveCourage)
            {
                GiveCourageToCharacter(customAgentConversation);
                CBB_ref.giveCourage = false;
            }
        }

        private void CheckOptionToLock(CustomAgent customAgentConversation, string localRelation, int value)
        {
            string socialExchange = "";
            if (localRelation == "Friends")
            {
                if (value > 0)
                {
                    SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Friendly, true);
                    socialExchange = "Friendly";
                }
                else
                {
                    SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Unfriendly, true);
                    socialExchange = "UnFriendly";
                }
            }
            else
            {
                if (value > 0)
                {
                    SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Romantic, true);
                    socialExchange = "Romantic";
                }
                else
                {
                    SetOptionAsUnavailable(customAgentConversation, CustomAgent.Intentions.Hostile, true);
                    socialExchange = "Hostile";
                }
            }
            _dataSource.SaveSavedSEs(customAgentConversation, socialExchange);
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
                float relation = hero.GetRelationWithPlayer();
                int newValue = (int)(relation + value);
                if (value > 0)
                {
                    InformationManager.AddQuickInformation(new TextObject(Agent.Main.Name + " increased relation with " + hero.Name + " from " + relation.ToString() + " to " + (relation + 1).ToString()), 0, hero.CharacterObject);
                    Hero.MainHero.SetPersonalRelation(hero, newValue);
                }
                else 
                {
                    InformationManager.AddQuickInformation(new TextObject(Agent.Main.Name + " decreased relation with " + hero.Name + " from " + relation.ToString() + " to " + (relation - 1).ToString()), 0, hero.CharacterObject);
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

        private void GiveCourageToCharacter(CustomAgent customAgentConversation)
        {
            CustomAgent customAgent = _dataSource.customAgentsList.Find(c => c.selfAgent.Name == customAgentConversation.selfAgent.Name && c.Id == customAgentConversation.Id);
            customAgent.UpdateAllStatus(0, 0, 1, 0, 0, 0);
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
                Random rnd = new Random();
                int randomInt = rnd.Next(10000000);

                fileName = "user_" + randomInt;
                System.IO.File.Create(filePath + fileName);
            }
        }

        private void ListFiles()
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

                list = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                /**/
                foreach (string file in list)
                {
                    if (file.Contains("user"))
                    {
                        string number = file.Remove(0, 5);
                        int id = Int32.Parse(number) + 1;

                        string aaa = "user_" + id.ToString();

                        break;
                    }
                }
                /**/

            }
            catch (Exception)
            {

            }
        }
    }
}