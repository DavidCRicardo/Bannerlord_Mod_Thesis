using System;
using SandBox.Source.Objects.SettlementObjects;
using SandBox.ViewModelCollection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord_Mod_Test
{
    public class CustomMissionNameMarkerTargetVM : ViewModel
    {
        public bool IsMovingTarget { get; }
        public Agent TargetAgent { get; }
        public CommonAreaMarker TargetCommonAreaMarker { get; }
        public CommonArea TargetCommonArea { get; }
        public PassageUsePoint TargetPassageUsePoint { get; }

        public Vec3 TargetWorkshopPosition { get; private set; }
        public Vec3 WorldPosition
        {
            get
            {
                switch (this.MarkerType)
                {
                    case 0:
                    case 1:
                    case 2:
                        return this.TargetAgent.Position;
                    case 3:
                    case 4:
                        return this.TargetCommonAreaMarker.GetPosition();
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                        return this.TargetPassageUsePoint.GameEntity.GlobalPosition;
                    case 22:
                        return this.TargetWorkshopPosition;
                    default:
                        return Vec3.One;
                }
            }
            set
            {
                _worldPosition = value;
            }
        }
        public CustomMissionNameMarkerTargetVM(CommonAreaMarker commonAreaMarker)
        {
            this.TargetCommonAreaMarker = commonAreaMarker;
            this.IsAgentInPrison = false;
            this.IsMovingTarget = false; // 0
            this.MarkerType = 4;
            this.QuestMarkerType = 0;
            this.IssueMarkerType = 0;
            this.TargetCommonArea = Hero.MainHero.CurrentSettlement.CommonAreas[commonAreaMarker.AreaIndex - 1];
            this.Name = this.TargetCommonArea.Name.ToString();
            CommonAreaPartyComponent commonAreaPartyComponent = this.TargetCommonArea.CommonAreaPartyComponent;
            if (commonAreaPartyComponent != null && commonAreaPartyComponent.MobileParty.MemberRoster.TotalManCount > 0 && Hero.MainHero.GetRelation(this.TargetCommonArea.Owner) <= 0)
            {
                this.MarkerType = 3;
            }
            this.QuestMarkerType = (Campaign.Current.VisualTrackerManager.CheckTracked(this.TargetCommonArea) ? 2 : 0);
        }
        public CustomMissionNameMarkerTargetVM(WorkshopType workshopType, Vec3 signPosition)
        {
            this.TargetWorkshopPosition = signPosition;
            this.IsAgentInPrison = false;
            this.IsMovingTarget = false; //0;
            this.Name = workshopType.Name.ToString();
            this.QuestMarkerType = 0;
            this.IssueMarkerType = 0;
            this.MarkerType = 22;
            this.QuestMarkerType = 0;
        }
        public CustomMissionNameMarkerTargetVM(PassageUsePoint passageUsePoint)
        {
            this.TargetPassageUsePoint = passageUsePoint;
            this.IsAgentInPrison = false;
            this.IsMovingTarget = false; // 0;
            this.Name = passageUsePoint.ToLocation.Name.ToString();
            this.QuestMarkerType = 0;
            this.IssueMarkerType = 0;
            if (passageUsePoint.ToLocation.Name.Contains("Lords Hall"))
            {
                this.MarkerType = 8;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Arena"))
            {
                this.MarkerType = 6;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Dungeon"))
            {
                this.MarkerType = 7;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Tavern"))
            {
                this.MarkerType = 5;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Smithy"))
            {
                this.MarkerType = 15;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Stable"))
            {
                this.MarkerType = 16;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Empty Shop"))
            {
                this.MarkerType = 11;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Velvet Weavery"))
            {
                this.MarkerType = 20;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Linen Weavery"))
            {
                this.MarkerType = 19;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Brewery"))
            {
                this.MarkerType = 9;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Mill"))
            {
                this.MarkerType = 12;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Pottery"))
            {
                this.MarkerType = 14;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Olive"))
            {
                this.MarkerType = 13;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Weavery"))
            {
                this.MarkerType = 18;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Tannery"))
            {
                this.MarkerType = 17;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Butcher"))
            {
                this.MarkerType = 10;
                return;
            }
            if (passageUsePoint.ToLocation.Name.Contains("Wood Workshop"))
            {
                this.MarkerType = 21;
                return;
            }
            this.MarkerType = 5;
        }

        public CustomMissionNameMarkerTargetVM(Agent agent, int id = -1)
        {
            if (id != -1)
            {
                //juntar as 2 strings
                this.Name = agent.Name.ToString() + id.ToString();
            }
            else
            {
                this.Name = agent.Name.ToString();
            }
            this.Text = "";
            this.IsMovingTarget = true; //1;
            this.TargetAgent = agent;
            //this.Name = agent.Name.ToString();
            this.MarkerType = 1;
            this.QuestMarkerType = 0;
            this.IssueMarkerType = 0;
            this.IsAgentInPrison = false;
            CharacterObject characterObject = (CharacterObject)agent.Character;
            if (characterObject != null)
            {
                Hero heroObject = characterObject.HeroObject;
                if (heroObject != null && heroObject.IsNoble)
                {
                    if (FactionManager.IsAtWarAgainstFaction(characterObject.HeroObject.MapFaction, Hero.MainHero.MapFaction))
                    {
                        this.MarkerType = 2;
                    }
                    else if (FactionManager.IsAlliedWithFaction(characterObject.HeroObject.MapFaction, Hero.MainHero.MapFaction))
                    {
                        this.MarkerType = 0;
                    }
                    else
                    {
                        this.MarkerType = 1;
                    }
                }
                if (characterObject.HeroObject != null)
                {
                    this.IsAgentInPrison = characterObject.HeroObject.IsPrisoner;
                }
                if (agent.IsHuman && characterObject.IsHero && agent != Agent.Main)
                {
                    this.UpdateQuestStatus();
                }
            }
        }

        public void UpdateQuestStatus()
        {
            this.QuestMarkerType = 0;
            this.IssueMarkerType = 0;
            CustomMissionNameMarkerTargetVM.QuestType questType = CustomMissionNameMarkerTargetVM.QuestType.None;
            if (this.TargetAgent != null && (CharacterObject)this.TargetAgent.Character != null && ((CharacterObject)this.TargetAgent.Character).HeroObject != null)
            {
                Tuple<SandBoxUIHelper.QuestType, SandBoxUIHelper.QuestState> questStateOfHero = SandBoxUIHelper.GetQuestStateOfHero(((CharacterObject)this.TargetAgent.Character).HeroObject);
                if (questStateOfHero.Item2 == SandBoxUIHelper.QuestState.Active)
                {
                    questType = CustomMissionNameMarkerTargetVM.QuestType.Active;
                }
                else if (questStateOfHero.Item2 == SandBoxUIHelper.QuestState.Available)
                {
                    questType = CustomMissionNameMarkerTargetVM.QuestType.Available;
                }
                if (questStateOfHero.Item1 == SandBoxUIHelper.QuestType.Issue)
                {
                    this.IssueMarkerType = (int)questType;
                    return;
                }
                if (questStateOfHero.Item1 == SandBoxUIHelper.QuestType.Main)
                {
                    this.QuestMarkerType = (int)questType;
                }
                return;
            }
            else
            {
                if (this.TargetCommonAreaMarker != null && this.TargetCommonArea != null)
                {
                    questType = (Campaign.Current.VisualTrackerManager.CheckTracked(this.TargetCommonArea) ? CustomMissionNameMarkerTargetVM.QuestType.Active : CustomMissionNameMarkerTargetVM.QuestType.None);
                    this.IssueMarkerType = (int)questType;
                    return;
                }
                Agent targetAgent = this.TargetAgent;
                if (targetAgent != null && !targetAgent.IsHero)
                {
                    questType = (Settlement.CurrentSettlement.LocationComplex.FindCharacter(this.TargetAgent).IsVisualTracked ? CustomMissionNameMarkerTargetVM.QuestType.Active : CustomMissionNameMarkerTargetVM.QuestType.None);
                    this.IssueMarkerType = (int)questType;
                    return;
                }
                return;
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
        public string Text
        {
            get
            {
                return this._text;
            }
            set
            {
                if (value != this._text)
                {
                    this._text = value;
                    base.OnPropertyChangedWithValue(value, "Text");
                }
            }
        }
        [DataSourceProperty]
        public int MarkerType
        {
            get
            {
                return this._markerType;
            }
            set
            {
                if (value != this._markerType)
                {
                    this._markerType = value;
                    base.OnPropertyChangedWithValue(value, "MarkerType");
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
        public int QuestMarkerType
        {
            get
            {
                return this._questMarkerType;
            }
            set
            {
                if (value != this._questMarkerType)
                {
                    this._questMarkerType = value;
                    base.OnPropertyChangedWithValue(value, "QuestMarkerType");
                }
            }
        }

        [DataSourceProperty]
        public int IssueMarkerType
        {
            get
            {
                return this._issueMarkerType;
            }
            set
            {
                if (value != this._issueMarkerType)
                {
                    this._issueMarkerType = value;
                    base.OnPropertyChangedWithValue(value, "IssueMarkerType");
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
        public bool IsAgentInPrison
        {
            get
            {
                return this._isAgentInPrison;
            }
            set
            {
                if (value != this._isAgentInPrison)
                {
                    this._isAgentInPrison = value;
                    base.OnPropertyChangedWithValue(value, "IsAgentInPrison");
                }
            }
        }

        private Vec3 _worldPosition;
        private Vec2 _screenPosition;
        private int _distance;
        private string _name;
        private int _markerType;
        private int _questMarkerType;
        private int _issueMarkerType;
        private bool _isEnabled;
        private bool _isTracked;
        private bool _isAgentInPrison;
        private bool _isQuestMainStory;
        private string _text;

        public enum EntitiyType
        {
            FriendlyNobleAgent,
            NeutralNobleAgent,
            EnemyNobleAgent,
            EnemyCommonArea,
            NeutralCommonArea,
            TavernPassage,
            ArenaPassage,
            DungeonPassage,
            LordsHallPassage,
            Brewery,
            Butcher,
            EmptyShop,
            Mill,
            OlivePress,
            Pottery,
            Smithy,
            Stable,
            Tannery,
            Weavery,
            WeaveryLinen,
            WeaveryVelvet,
            WoodWorkshop,
            GenericWorkshop
        }

        public enum QuestType
        {
            None,
            Available,
            Active
        }
    }
}