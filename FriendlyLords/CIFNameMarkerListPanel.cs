using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Mission.NameMarker;
using TaleWorlds.TwoDimension;

namespace FriendlyLords
{
    class CIFNameMarkerListPanel : ListPanel
    {
        public float FarAlphaTarget { get; set; } = 0.2f;
        public float FarDistanceCutoff { get; set; } = 50f;
        public float CloseDistanceCutoff { get; set; } = 25f;
        public bool HasTypeMarker { get; private set; }
        public MarkerRect Rect { get; private set; }
        public bool IsInScreenBoundaries { get; private set; }
        public CIFNameMarkerListPanel(UIContext context) : base(context)
        {
            this._parentScreenWidget = base.EventManager.Root.GetChild(0).GetChild(0);
            this.Rect = new MarkerRect();
        }
        public void Update(float dt)
        {
            this._transitionDT = MathF.Clamp(dt * 12f, 0f, 1f);
            this._targetAlpha = (this.IsMarkerEnabled ? this.GetDistanceRelatedAlphaTarget(this.Distance) : 0f);
            this.ApplyActionForThisAndAllChildren(new Action<Widget>(this.UpdateAlpha));
            TextWidget nameTextWidget = this.NameTextWidget;
            if ((nameTextWidget != null && nameTextWidget.IsVisible) || this.TypeVisualWidget.IsVisible)
            {
                base.ScaledPositionYOffset = this.Position.y - base.Size.Y / 2f;
                base.ScaledPositionXOffset = this.Position.x - base.Size.X / 2f;
            }
            this.UpdateRectangle();
        }
        private void UpdateAlpha(Widget item)
        {
            if ((item == this.NameTextWidget || item == this.DistanceTextWidget || item == this.DistanceIconWidget) && this.HasTypeMarker && !this.IsFocused)
            {
                return;
            }
            float alphaFactor = this.LocalLerp(item.AlphaFactor, this._targetAlpha, this._transitionDT);
            item.SetAlpha(alphaFactor);
            item.IsVisible = ((double)item.AlphaFactor > 0.05);
        }
        public void UpdateRectangle()
        {
            this.Rect.Reset();
            this.Rect.UpdatePoints(base.ScaledPositionXOffset, base.ScaledPositionXOffset + base.Size.X, base.ScaledPositionYOffset, base.ScaledPositionYOffset + base.Size.Y);
            this.IsInScreenBoundaries = (this.Rect.Left > -50f && this.Rect.Right < base.EventManager.PageSize.X + 50f && this.Rect.Top > -50f && this.Rect.Bottom < base.EventManager.PageSize.Y + 50f);
        }
        private float GetDistanceRelatedAlphaTarget(int distance)
        {
            if (this.IsFocused)
            {
                return 1f;
            }
            if ((float)distance > this.FarDistanceCutoff)
            {
                return this.FarAlphaTarget;
            }
            if ((float)distance <= this.FarDistanceCutoff && (float)distance >= this.CloseDistanceCutoff)
            {
                float amount = (float)Math.Pow((double)(((float)distance - this.CloseDistanceCutoff) / (this.FarDistanceCutoff - this.CloseDistanceCutoff)), 0.3333333333333333);
                return MathF.Clamp(MathF.Lerp(1f, this.FarAlphaTarget, amount, 1E-05f), this.FarAlphaTarget, 1f);
            }
            return 1f;
        }
        private float LocalLerp(float start, float end, float delta)
        {
            if (Math.Abs(start - end) > 1E-45f)
            {
                return (end - start) * delta + start;
            }
            return end;
        }
        private void OnStateChanged()
        {
            TextWidget nameTextWidget = this.NameTextWidget;
            if (nameTextWidget != null)
            {
                nameTextWidget.SetState(this.NameType);
            }
            BrushWidget typeVisualWidget = this.TypeVisualWidget;
            if (typeVisualWidget != null)
            {
                typeVisualWidget.SetState(this.IconType);
            }
            this.HasTypeMarker = (this.IconType != string.Empty);
            if (this.HasTypeMarker && this.IsFocused)
            {
                TextWidget nameTextWidget2 = this.NameTextWidget;
                if (nameTextWidget2 != null)
                {
                    nameTextWidget2.SetAlpha(1f);
                }
                TextWidget distanceTextWidget = this.DistanceTextWidget;
                if (distanceTextWidget != null)
                {
                    distanceTextWidget.SetAlpha(1f);
                }
                BrushWidget distanceIconWidget = this.DistanceIconWidget;
                if (distanceIconWidget != null)
                {
                    distanceIconWidget.SetAlpha(1f);
                }
            }
            else if (this.HasTypeMarker && !this.IsFocused)
            {
                TextWidget nameTextWidget3 = this.NameTextWidget;
                if (nameTextWidget3 != null)
                {
                    nameTextWidget3.SetAlpha(0f);
                }
                TextWidget distanceTextWidget2 = this.DistanceTextWidget;
                if (distanceTextWidget2 != null)
                {
                    distanceTextWidget2.SetAlpha(0f);
                }
                BrushWidget distanceIconWidget2 = this.DistanceIconWidget;
                if (distanceIconWidget2 != null)
                {
                    distanceIconWidget2.SetAlpha(0f);
                }
            }
            if (this.IsEnemy)
            {
                this.TypeVisualWidget.Brush.GlobalColor = this.EnemyColor;
            }
            else if (this.IsFriendly)
            {
                this.TypeVisualWidget.Brush.GlobalColor = this.FriendlyColor;
            }
            else if (this.IsNeutral)
            {
                this.TypeVisualWidget.Brush.GlobalColor = this.NeutralColor;
            }
            else if (this.HasMainQuest)
            {
                this.TypeVisualWidget.Brush.GlobalColor = this.MainQuestNotificationColor;
            }
            else if (this.HasIssue)
            {
                this.TypeVisualWidget.Brush.GlobalColor = this.IssueNotificationColor;
            }
            BrushWidget typeVisualWidget2 = this.TypeVisualWidget;
            Sprite sprite;
            if (typeVisualWidget2 == null)
            {
                sprite = null;
            }
            else
            {
                Style style = typeVisualWidget2.Brush.GetStyle(this.IconType);
                if (style == null)
                {
                    sprite = null;
                }
                else
                {
                    StyleLayer layer = style.GetLayer(0);
                    sprite = ((layer != null) ? layer.Sprite : null);
                }
            }
            Sprite sprite2 = sprite;
            if (sprite2 != null)
            {
                base.SuggestedWidth = base.SuggestedHeight / (float)sprite2.Height * (float)sprite2.Width;
            }
        }

        [DataSourceProperty]
        public TextWidget NameTextWidget
        {
            get
            {
                return this._nameTextWidget;
            }
            set
            {
                if (this._nameTextWidget != value)
                {
                    this._nameTextWidget = value;
                    base.OnPropertyChanged(value, "NameTextWidget");
                    this.OnStateChanged();
                }
            }
        }
        [DataSourceProperty]
        public BrushWidget TypeVisualWidget
        {
            get
            {
                return this._typeVisualWidget;
            }
            set
            {
                if (this._typeVisualWidget != value)
                {
                    this._typeVisualWidget = value;
                    base.OnPropertyChanged(value, "TypeVisualWidget");
                    this.OnStateChanged();
                }
            }
        }
        [DataSourceProperty]
        public BrushWidget DistanceIconWidget
        {
            get
            {
                return this._distanceIconWidget;
            }
            set
            {
                if (this._distanceIconWidget != value)
                {
                    this._distanceIconWidget = value;
                    base.OnPropertyChanged(value, "DistanceIconWidget");
                    this.OnStateChanged();
                }
            }
        }
        [DataSourceProperty]
        public TextWidget DistanceTextWidget
        {
            get
            {
                return this._distanceTextWidget;
            }
            set
            {
                if (this._distanceTextWidget != value)
                {
                    this._distanceTextWidget = value;
                    base.OnPropertyChanged(value, "DistanceTextWidget");
                    this.OnStateChanged();
                }
            }
        }
        [DataSourceProperty]
        public Vec2 Position
        {
            get
            {
                return this._position;
            }
            set
            {
                if (this._position != value)
                {
                    this._position = value;
                    base.OnPropertyChanged(value, "Position");
                }
            }
        }
        [Editor(false)]
        public Color IssueNotificationColor
        {
            get
            {
                return this._issueNotificationColor;
            }
            set
            {
                if (value != this._issueNotificationColor)
                {
                    this._issueNotificationColor = value;
                    base.OnPropertyChanged(value, "IssueNotificationColor");
                    this.OnStateChanged();
                }
            }
        }
        [Editor(false)]
        public Color MainQuestNotificationColor
        {
            get
            {
                return this._mainQuestNotificationColor;
            }
            set
            {
                if (value != this._mainQuestNotificationColor)
                {
                    this._mainQuestNotificationColor = value;
                    base.OnPropertyChanged(value, "MainQuestNotificationColor");
                    this.OnStateChanged();
                }
            }
        }
        [Editor(false)]
        public Color EnemyColor
        {
            get
            {
                return this._enemyColor;
            }
            set
            {
                if (value != this._enemyColor)
                {
                    this._enemyColor = value;
                    base.OnPropertyChanged(value, "EnemyColor");
                    this.OnStateChanged();
                }
            }
        }
        [Editor(false)]
        public Color FriendlyColor
        {
            get
            {
                return this._friendlyColor;
            }
            set
            {
                if (value != this._friendlyColor)
                {
                    this._friendlyColor = value;
                    base.OnPropertyChanged(value, "FriendlyColor");
                    this.OnStateChanged();
                }
            }
        }
        [Editor(false)]
        public Color NeutralColor
        {
            get
            {
                return this._neutralColor;
            }
            set
            {
                if (value != this._neutralColor)
                {
                    this._neutralColor = value;
                    base.OnPropertyChanged(value, "NeutralColor");
                    this.OnStateChanged();
                }
            }
        }
        [Editor(false)]
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
                    base.OnPropertyChanged(value, "IconType");
                    this.OnStateChanged();
                }
            }
        }
        [Editor(false)]
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
                    base.OnPropertyChanged(value, "NameType");
                    this.OnStateChanged();
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
                if (this._distance != value)
                {
                    this._distance = value;
                    base.OnPropertyChanged(value, "Distance");
                }
            }
        }
        [DataSourceProperty]
        public bool IsMarkerEnabled
        {
            get
            {
                return this._isMarkerEnabled;
            }
            set
            {
                if (this._isMarkerEnabled != value)
                {
                    this._isMarkerEnabled = value;
                    base.OnPropertyChanged(value, "IsMarkerEnabled");
                }
            }
        }
        [DataSourceProperty]
        public bool HasIssue
        {
            get
            {
                return this._hasIssue;
            }
            set
            {
                if (this._hasIssue != value)
                {
                    this._hasIssue = value;
                    base.OnPropertyChanged(value, "HasIssue");
                    this.OnStateChanged();
                }
            }
        }
        [DataSourceProperty]
        public bool HasMainQuest
        {
            get
            {
                return this._hasMainQuest;
            }
            set
            {
                if (this._hasMainQuest != value)
                {
                    this._hasMainQuest = value;
                    base.OnPropertyChanged(value, "HasMainQuest");
                    this.OnStateChanged();
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
                if (this._isEnemy != value)
                {
                    this._isEnemy = value;
                    base.OnPropertyChanged(value, "IsEnemy");
                    this.OnStateChanged();
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
                if (this._isFriendly != value)
                {
                    this._isFriendly = value;
                    base.OnPropertyChanged(value, "IsFriendly");
                    this.OnStateChanged();
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
                if (this._isNeutral != value)
                {
                    this._isNeutral = value;
                    base.OnPropertyChanged(value, "IsNeutral");
                    this.OnStateChanged();
                }
            }
        }
        [Editor(false)]
        public new bool IsFocused
        {
            get
            {
                return this._isFocused;
            }
            set
            {
                if (value != this._isFocused)
                {
                    this._isFocused = value;
                    base.OnPropertyChanged(value, "IsFocused");
                    if (!value && this.IsMarkerEnabled)
                    {
                        TextWidget nameTextWidget = this.NameTextWidget;
                        if (nameTextWidget != null)
                        {
                            nameTextWidget.SetAlpha(0f);
                        }
                        TextWidget distanceTextWidget = this.DistanceTextWidget;
                        if (distanceTextWidget != null)
                        {
                            distanceTextWidget.SetAlpha(0f);
                        }
                        BrushWidget distanceIconWidget = this.DistanceIconWidget;
                        if (distanceIconWidget != null)
                        {
                            distanceIconWidget.SetAlpha(0f);
                        }
                    }
                    else if (value && this.IsMarkerEnabled)
                    {
                        TextWidget nameTextWidget2 = this.NameTextWidget;
                        if (nameTextWidget2 != null)
                        {
                            nameTextWidget2.SetAlpha(1f);
                        }
                        TextWidget distanceTextWidget2 = this.DistanceTextWidget;
                        if (distanceTextWidget2 != null)
                        {
                            distanceTextWidget2.SetAlpha(1f);
                        }
                        BrushWidget distanceIconWidget2 = this.DistanceIconWidget;
                        if (distanceIconWidget2 != null)
                        {
                            distanceIconWidget2.SetAlpha(1f);
                        }
                    }
                    base.RenderLate = value;
                }
            }
        }
        private Widget _parentScreenWidget;
        private const float BoundaryOffset = 50f;
        private float _transitionDT;
        private float _targetAlpha;
        private string _iconType = string.Empty;
        private string _nameType = string.Empty;
        private int _distance;
        private TextWidget _nameTextWidget;
        private BrushWidget _typeVisualWidget;
        private BrushWidget _distanceIconWidget;
        private TextWidget _distanceTextWidget;
        private Vec2 _position;
        private Color _issueNotificationColor;
        private Color _mainQuestNotificationColor;
        private Color _enemyColor;
        private Color _friendlyColor;
        private Color _neutralColor;
        private bool _isMarkerEnabled;
        private bool _hasIssue;
        private bool _hasMainQuest;
        private bool _isEnemy;
        private bool _isFriendly;
        private bool _isNeutral;
        private bool _isFocused;
    }
}