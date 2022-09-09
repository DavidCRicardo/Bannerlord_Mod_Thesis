using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.TwoDimension;

namespace FriendlyLords
{
    public class CIFManagerTarget : ViewModel
    {
        public bool IsMovingTarget { get; }
        public Agent TargetAgent { get; }

        public Vec3 TargetWorkshopPosition { get; private set; }
        public Vec3 WorldPosition
        {
            get
            {
                return this._getPosition();
            }
        }
        public CIFManagerTarget(Agent agent, int id = -1)
        {
            this.Id = id;
            this.Message = "";
            this.IsMovingTarget = true;
            this.TargetAgent = agent;
            this.Name = agent.Name.ToString();

            CharacterObject characterObject = (CharacterObject)agent.Character;
            if (characterObject != null) { }

            this.NameType = "Normal";
            this.IconType = "character";

            this.IsFriendly = false;
            this.IsEnemy = false;
            this.IsNeutral = false;

            this.MessageColor = 1;

            // 2 red negative / 1 white neutral / 0 green positive
            if (this.MessageColor == 0)
            {
                BrushColor = "#4EE04CFF"; // = "Friendly"
            }
            else if (this.MessageColor == 1)
            {
                BrushColor = "#FFFFFFFF"; // = "Neutral"
            }
            else
            {
                BrushColor = "#ED1C24FF"; // = "Negative"
            }

            this._getPosition = (() => agent.Position);
            this._getMarkerObjectName = (() => agent.Name);

            this.RefreshValues();
        }

        private string _iconType = string.Empty;
        private string _nameType = string.Empty;

        private Func<Vec3> _getPosition = () => Vec3.Zero;
        private Func<string> _getMarkerObjectName = () => string.Empty;

        private Vec2 _screenPosition;
        private int _distance;
        private bool _isEnabled;

        public int Id;
        public string Name;
        private string _message;

		private bool _isTracked;
		private bool _isQuestMainStory;
        private bool _isFriendly;
        private bool _isEnemy;
        private bool _isNeutral;

        public int MessageColor
        {
            get
            {
                return this._messageColor;
            }
            set
            {
                if (value != this._messageColor)
                {
                    this._messageColor = value;
                    base.OnPropertyChangedWithValue(value, "MessageColor");
                }
            }
        }
        private int _messageColor;

        private string _brushColor;
        [DataSourceProperty]
        public string BrushColor
        {
            get
            {
                return this._brushColor;
            }
            set
            {
                if (value != this._brushColor)
                {
                    this._brushColor = value;
                    base.OnPropertyChangedWithValue(value, "BrushColor");
                }
            }
        }

        [DataSourceProperty]
        public bool IsNeutral
        {
            get
            {
                return this._isNeutral;
            }
            set
            {
                if (value != this._isNeutral)
                {
                    this._isNeutral = value;
                    base.OnPropertyChangedWithValue(value, "IsNeutral");
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
	}
}