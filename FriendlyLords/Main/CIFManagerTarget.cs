using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace FriendlyLords
{
    public class CIFManagerTarget : ViewModel
    {
        public bool IsMovingTarget { get; }
        public Agent TargetAgent { get; }
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

        private Func<Vec3> _getPosition = () => Vec3.Zero;
        private Func<string> _getMarkerObjectName = () => string.Empty;

        private Vec2 _screenPosition;
        private int _distance;
        private bool _isEnabled;

        public int Id;
        public string Name;
        private string _message;

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
	}
}