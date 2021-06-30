using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord_Social_AI
{
    public class CustomMissionNameMarkerTargetVM : ViewModel
    {
        public bool IsMovingTarget { get; }
        public Agent TargetAgent { get; }

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
                    default:
                        return Vec3.One;
                }
            }
            set
            {
                _worldPosition = value;
            }
        }

        public CustomMissionNameMarkerTargetVM(Agent agent, int id = -1)
        {
            this.Id = id;
            this.Message = "";
            this.IsMovingTarget = true;
            this.TargetAgent = agent;
            this.Name = agent.Name.ToString();
            this.MarkerType = 1; // 2 = red . 1 = yellow . 0 = green
            CharacterObject characterObject = (CharacterObject)agent.Character;
            if (characterObject != null) { }
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

        private Vec3 _worldPosition;
        private Vec2 _screenPosition;
        private int _distance;
        private int _markerType;
        private bool _isEnabled;

        public int Id;
        public string Name;
        private string _message;

		private int _questMarkerType;
		private int _issueMarkerType;
		private bool _isTracked;
		private bool _isAgentInPrison;
		private bool _isQuestMainStory;

        //
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
        //
	}
}