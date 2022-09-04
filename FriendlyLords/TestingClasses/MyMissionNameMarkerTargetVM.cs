using System;
using System.Collections.Generic;
using SandBox.Objects;
using SandBox.Objects.AreaMarkers;
using SandBox.ViewModelCollection;
using SandBox.ViewModelCollection.Missions.NameMarker;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace FriendlyLords
{
    class MyMissionNameMarkerTargetVM : ViewModel
    {
        public bool IsAdditionalTargetAgent { get; private set; }
        public bool IsMovingTarget { get; }
        public Agent TargetAgent { get; }
        public CommonAreaMarker TargetCommonAreaMarker { get; }
        public CommonArea TargetCommonArea { get; }
        public PassageUsePoint TargetPassageUsePoint { get; private set; }

        public Vec3 WorldPosition
        {
            get
            {
                return this._getPosition();
            }
        }

        public MyMissionNameMarkerTargetVM(CommonAreaMarker commonAreaMarker)
        {
            this.TargetCommonAreaMarker = commonAreaMarker;
            this.IsMovingTarget = false; // 0;
            this.NameType = "Passage";
            this.IconType = "common_area";
            this.Quests = new MBBindingList<QuestMarkerVM>();
            this.TargetCommonArea = Hero.MainHero.CurrentSettlement.CommonAreas[commonAreaMarker.AreaIndex - 1];
            CommonAreaPartyComponent commonAreaPartyComponent = this.TargetCommonArea.CommonAreaPartyComponent;
            if (commonAreaPartyComponent != null)
            {
                if (commonAreaPartyComponent.MobileParty.LeaderHero == Hero.MainHero)
                {
                    this.NameType = "Friendly";
                    this.IsFriendly = true;
                }
                else if (commonAreaPartyComponent.MobileParty.MemberRoster.TotalManCount > 0)
                {
                    this.NameType = "Passage";
                    this.IsEnemy = true;
                }
            }
            if (Campaign.Current.VisualTrackerManager.CheckTracked(this.TargetCommonArea))
            {
                this.Quests.Add(new QuestMarkerVM(SandBoxUIHelper.IssueQuestFlags.TrackedIssue));
            }
            this._getPosition = (() => commonAreaMarker.GetPosition());
            this._getMarkerObjectName = (() => commonAreaMarker.GetName().ToString());
            this.RefreshValues();
        }

        public MyMissionNameMarkerTargetVM(WorkshopType workshopType, Vec3 signPosition)
        {
            this.IsMovingTarget = false; // 0;
            this.NameType = "Passage";
            this.IconType = workshopType.StringId;
            this.Quests = new MBBindingList<QuestMarkerVM>();
            this._getPosition = (() => signPosition);
            this._getMarkerObjectName = (() => workshopType.Name.ToString());
            this.RefreshValues();
        }

        public MyMissionNameMarkerTargetVM(PassageUsePoint passageUsePoint)
        {
            this.TargetPassageUsePoint = passageUsePoint;
            this.IsMovingTarget = false; // 0;
            this.NameType = "Passage";
            this.IconType = passageUsePoint.ToLocation.StringId;
            this.Quests = new MBBindingList<QuestMarkerVM>();
            this._getPosition = (() => passageUsePoint.GameEntity.GlobalPosition);
            this._getMarkerObjectName = (() => passageUsePoint.ToLocation.Name.ToString());
            this.RefreshValues();
        }

        public MyMissionNameMarkerTargetVM(Agent agent, bool isAdditionalTargetAgent)
        {
            this.IsMovingTarget = true;// 1;
            this.TargetAgent = agent;
            this.NameType = "Normal";
            this.IconType = "character";
            this.IsAdditionalTargetAgent = isAdditionalTargetAgent;
            this.Quests = new MBBindingList<QuestMarkerVM>();
            CharacterObject characterObject = (CharacterObject)agent.Character;
            if (characterObject != null)
            {
                Hero heroObject = characterObject.HeroObject;
                if (heroObject != null && heroObject.IsLord)
                {
                    this.IconType = "noble";
                    this.NameType = "Noble";
                    if (FactionManager.IsAtWarAgainstFaction(characterObject.HeroObject.MapFaction, Hero.MainHero.MapFaction))
                    {
                        this.NameType = "Enemy";
                        this.IsEnemy = true;
                    }
                    else if (FactionManager.IsAlliedWithFaction(characterObject.HeroObject.MapFaction, Hero.MainHero.MapFaction))
                    {
                        this.NameType = "Friendly";
                        this.IsFriendly = true;
                    }
                }
                if (characterObject.HeroObject != null && characterObject.HeroObject.IsPrisoner)
                {
                    this.IconType = "prisoner";
                }
                if (agent.IsHuman && characterObject.IsHero && agent != Agent.Main && !this.IsAdditionalTargetAgent)
                {
                    this.UpdateQuestStatus();
                }
                CharacterObject characterObject2 = characterObject;
                Settlement currentSettlement = Settlement.CurrentSettlement;
                object obj;
                if (currentSettlement == null)
                {
                    obj = null;
                }
                else
                {
                    CultureObject culture = currentSettlement.Culture;
                    obj = ((culture != null) ? culture.Barber : null);
                }
                if (characterObject2 == obj)
                {
                    this.IconType = "barber";
                }
                else
                {
                    CharacterObject characterObject3 = characterObject;
                    Settlement currentSettlement2 = Settlement.CurrentSettlement;
                    object obj2;
                    if (currentSettlement2 == null)
                    {
                        obj2 = null;
                    }
                    else
                    {
                        CultureObject culture2 = currentSettlement2.Culture;
                        obj2 = ((culture2 != null) ? culture2.Blacksmith : null);
                    }
                    if (characterObject3 == obj2)
                    {
                        this.IconType = "blacksmith";
                    }
                    else
                    {
                        CharacterObject characterObject4 = characterObject;
                        Settlement currentSettlement3 = Settlement.CurrentSettlement;
                        object obj3;
                        if (currentSettlement3 == null)
                        {
                            obj3 = null;
                        }
                        else
                        {
                            CultureObject culture3 = currentSettlement3.Culture;
                            obj3 = ((culture3 != null) ? culture3.TavernGamehost : null);
                        }
                        if (characterObject4 == obj3)
                        {
                            this.IconType = "game_host";
                        }
                    }
                }
            }
            this._getPosition = (() => agent.Position);
            this._getMarkerObjectName = (() => agent.Name);
            this.RefreshValues();
        }

        public MyMissionNameMarkerTargetVM(Vec3 position, string name, string iconType)
        {
            this.NameType = "Passage";
            this.IconType = iconType;
            this.Quests = new MBBindingList<QuestMarkerVM>();
            this._getPosition = (() => position);
            this._getMarkerObjectName = (() => name);
            this.RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            this.Name = this._getMarkerObjectName();
        }

        public void UpdateQuestStatus(SandBoxUIHelper.IssueQuestFlags issueQuestFlags)
        {
            /*using (IEnumerator enumerator = Enum.GetValues(typeof(SandBoxUIHelper.IssueQuestFlags)).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    SandBoxUIHelper.IssueQuestFlags questFlag = (SandBoxUIHelper.IssueQuestFlags)enumerator.Current;
                    if (questFlag != SandBoxUIHelper.IssueQuestFlags.None && (issueQuestFlags & questFlag) != SandBoxUIHelper.IssueQuestFlags.None && !this.Quests.Any((QuestMarkerVM q) => q.IssueQuestFlag == questFlag))
                    {
                        this.Quests.Add(new QuestMarkerVM(questFlag));
                        if ((questFlag & SandBoxUIHelper.IssueQuestFlags.ActiveIssue) != SandBoxUIHelper.IssueQuestFlags.None && (questFlag & SandBoxUIHelper.IssueQuestFlags.AvailableIssue) != SandBoxUIHelper.IssueQuestFlags.None && (questFlag & SandBoxUIHelper.IssueQuestFlags.TrackedIssue) != SandBoxUIHelper.IssueQuestFlags.None)
                        {
                            this.IsTracked = true;
                        }
                        else if ((questFlag & SandBoxUIHelper.IssueQuestFlags.ActiveIssue) != SandBoxUIHelper.IssueQuestFlags.None && (questFlag & SandBoxUIHelper.IssueQuestFlags.ActiveStoryQuest) != SandBoxUIHelper.IssueQuestFlags.None && (questFlag & SandBoxUIHelper.IssueQuestFlags.TrackedStoryQuest) != SandBoxUIHelper.IssueQuestFlags.None)
                        {
                            this.IsQuestMainStory = true;
                        }
                    }
                }
            }
            this.Quests.Sort(new MyMissionNameMarkerTargetVM.QuestMarkerComparer());*/
        }

        public void UpdateQuestStatus()
        {
            this.Quests.Clear();
            SandBoxUIHelper.IssueQuestFlags issueQuestFlags = SandBoxUIHelper.IssueQuestFlags.None;
            if (this.TargetAgent != null && (CharacterObject)this.TargetAgent.Character != null && ((CharacterObject)this.TargetAgent.Character).HeroObject != null)
            {
                List<ValueTuple<SandBoxUIHelper.IssueQuestFlags, TextObject, TextObject>> questStateOfHero = SandBoxUIHelper.GetQuestStateOfHero(((CharacterObject)this.TargetAgent.Character).HeroObject);
                for (int i = 0; i < questStateOfHero.Count; i++)
                {
                    issueQuestFlags |= questStateOfHero[i].Item1;
                }
            }
            if (this.TargetCommonAreaMarker != null && this.TargetCommonArea != null)
            {
                Campaign campaign = Campaign.Current;
                bool flag;
                if (campaign == null)
                {
                    flag = false;
                }
                else
                {
                    VisualTrackerManager visualTrackerManager = campaign.VisualTrackerManager;
                    bool? flag2 = (visualTrackerManager != null) ? new bool?(visualTrackerManager.CheckTracked(this.TargetCommonArea)) : null;
                    bool flag3 = true;
                    flag = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
                }
                if (flag)
                {
                    issueQuestFlags |= SandBoxUIHelper.IssueQuestFlags.TrackedIssue;
                }
            }
            Agent targetAgent = this.TargetAgent;
            if (targetAgent != null && !targetAgent.IsHero)
            {
                Settlement currentSettlement = Settlement.CurrentSettlement;
                bool flag4;
                if (currentSettlement == null)
                {
                    flag4 = false;
                }
                else
                {
                    LocationComplex locationComplex = currentSettlement.LocationComplex;
                    bool? flag5;
                    if (locationComplex == null)
                    {
                        flag5 = null;
                    }
                    else
                    {
                        LocationCharacter locationCharacter = locationComplex.FindCharacter(this.TargetAgent);
                        flag5 = ((locationCharacter != null) ? new bool?(locationCharacter.IsVisualTracked) : null);
                    }
                    bool? flag2 = flag5;
                    bool flag3 = true;
                    flag4 = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
                }
                if (flag4)
                {
                    issueQuestFlags |= SandBoxUIHelper.IssueQuestFlags.TrackedIssue;
                }
            }
            foreach (object obj in Enum.GetValues(typeof(SandBoxUIHelper.IssueQuestFlags)))
            {
                SandBoxUIHelper.IssueQuestFlags issueQuestFlags2 = (SandBoxUIHelper.IssueQuestFlags)obj;
                if (issueQuestFlags2 != SandBoxUIHelper.IssueQuestFlags.None && (issueQuestFlags & issueQuestFlags2) != SandBoxUIHelper.IssueQuestFlags.None)
                {
                    this.Quests.Add(new QuestMarkerVM(issueQuestFlags2));
                    if ((issueQuestFlags2 & SandBoxUIHelper.IssueQuestFlags.ActiveIssue) != SandBoxUIHelper.IssueQuestFlags.None && (issueQuestFlags2 & SandBoxUIHelper.IssueQuestFlags.AvailableIssue) != SandBoxUIHelper.IssueQuestFlags.None && (issueQuestFlags2 & SandBoxUIHelper.IssueQuestFlags.TrackedIssue) != SandBoxUIHelper.IssueQuestFlags.None)
                    {
                        this.IsTracked = true;
                    }
                    else if ((issueQuestFlags2 & SandBoxUIHelper.IssueQuestFlags.ActiveIssue) != SandBoxUIHelper.IssueQuestFlags.None && (issueQuestFlags2 & SandBoxUIHelper.IssueQuestFlags.ActiveStoryQuest) != SandBoxUIHelper.IssueQuestFlags.None && (issueQuestFlags2 & SandBoxUIHelper.IssueQuestFlags.TrackedStoryQuest) != SandBoxUIHelper.IssueQuestFlags.None)
                    {
                        this.IsQuestMainStory = true;
                    }
                }
            }
            this.Quests.Sort(new MyMissionNameMarkerTargetVM.QuestMarkerComparer());
        }

        [DataSourceProperty]
        public MBBindingList<QuestMarkerVM> Quests
        {
            get
            {
                return this._quests;
            }
            set
            {
                if (value != this._quests)
                {
                    this._quests = value;
                    base.OnPropertyChangedWithValue(value, "Quests");
                }
            }
        }

        [DataSourceProperty]
        public Vec2 ScreenPosition
        {
            get
            {
                return this._screenPosition;
            }
            set
            {
                if (value.x != this._screenPosition.x || value.y != this._screenPosition.y)
                {
                    this._screenPosition = value;
                    base.OnPropertyChangedWithValue(value, "ScreenPosition");
                }
            }
        }

        [DataSourceProperty]
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    base.OnPropertyChangedWithValue(value, "Name");
                }
            }
        }

        [DataSourceProperty]
        public string Message
        {
            get
            {
                return this._message;
            }
            set
            {
                if (value != this._message)
                {
                    this._message = value;
                    base.OnPropertyChangedWithValue(value, "Message");
                }
            }
        }

        [DataSourceProperty]
        public string IconType
        {
            get
            {
                return this._iconType;
            }
            set
            {
                if (value != this._iconType)
                {
                    this._iconType = value;
                    base.OnPropertyChangedWithValue(value, "IconType");
                }
            }
        }

        [DataSourceProperty]
        public string NameType
        {
            get
            {
                return this._nameType;
            }
            set
            {
                if (value != this._nameType)
                {
                    this._nameType = value;
                    base.OnPropertyChangedWithValue(value, "NameType");
                }
            }
        }

        [DataSourceProperty]
        public int Distance
        {
            get
            {
                return this._distance;
            }
            set
            {
                if (value != this._distance)
                {
                    this._distance = value;
                    base.OnPropertyChangedWithValue(value, "Distance");
                }
            }
        }

        [DataSourceProperty]
        public bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }
            set
            {
                if (value != this._isEnabled)
                {
                    this._isEnabled = value;
                    base.OnPropertyChangedWithValue(value, "IsEnabled");
                }
            }
        }

        [DataSourceProperty]
        public bool IsTracked
        {
            get
            {
                return this._isTracked;
            }
            set
            {
                if (value != this._isTracked)
                {
                    this._isTracked = value;
                    base.OnPropertyChangedWithValue(value, "IsTracked");
                }
            }
        }

        [DataSourceProperty]
        public bool IsQuestMainStory
        {
            get
            {
                return this._isQuestMainStory;
            }
            set
            {
                if (value != this._isQuestMainStory)
                {
                    this._isQuestMainStory = value;
                    base.OnPropertyChangedWithValue(value, "IsQuestMainStory");
                }
            }
        }

        [DataSourceProperty]
        public bool IsEnemy
        {
            get
            {
                return this._isEnemy;
            }
            set
            {
                if (value != this._isEnemy)
                {
                    this._isEnemy = value;
                    base.OnPropertyChangedWithValue(value, "IsEnemy");
                }
            }
        }

        [DataSourceProperty]
        public bool IsFriendly
        {
            get
            {
                return this._isFriendly;
            }
            set
            {
                if (value != this._isFriendly)
                {
                    this._isFriendly = value;
                    base.OnPropertyChangedWithValue(value, "IsFriendly");
                }
            }
        }

        public const string NameTypeNeutral = "Normal";

        public const string NameTypeFriendly = "Friendly";

        public const string NameTypeEnemy = "Enemy";

        public const string NameTypeNoble = "Noble";

        public const string NameTypePassage = "Passage";

        public const string NameTypeEnemyPassage = "Passage";

        public const string IconTypeCommonArea = "common_area";

        public const string IconTypeCharacter = "character";

        public const string IconTypePrisoner = "prisoner";

        public const string IconTypeNoble = "noble";

        public const string IconTypeBarber = "barber";

        public const string IconTypeBlacksmith = "blacksmith";

        public const string IconTypeGameHost = "game_host";

        private Func<Vec3> _getPosition = () => Vec3.Zero;

        private Func<string> _getMarkerObjectName = () => string.Empty;

        private MBBindingList<QuestMarkerVM> _quests;

        private Vec2 _screenPosition;

        private int _distance;

        private string _name;
        private string _message;

        private string _iconType = string.Empty;

        private string _nameType = string.Empty;

        private bool _isEnabled;

        private bool _isTracked;

        private bool _isQuestMainStory;

        private bool _isEnemy;

        private bool _isFriendly;

        private class QuestMarkerComparer : IComparer<QuestMarkerVM>
        {
            public int Compare(QuestMarkerVM x, QuestMarkerVM y)
            {
                return x.QuestMarkerType.CompareTo(y.QuestMarkerType);
            }
        }
    }
}