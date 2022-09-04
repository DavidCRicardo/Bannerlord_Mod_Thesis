using System;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using SandBox.ViewModelCollection.Missions.NameMarker;
using SandBox.ViewModelCollection;
using SandBox.Missions.MissionLogics.Towns;
using SandBox.Objects.AreaMarkers;
using SandBox.Objects;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.Settlements;
using System.Runtime.ExceptionServices;

namespace FriendlyLords
{
    class MyMissionNameMarkerVM : ViewModel
    {
        public bool IsTargetsAdded { get; private set; }

        public MyMissionNameMarkerVM(Mission mission, Camera missionCamera, Dictionary<Agent, SandBoxUIHelper.IssueQuestFlags> additionalTargetAgents, Dictionary<string, ValueTuple<Vec3, string, string>> additionalGenericTargets)
        {
            this.Targets = new MBBindingList<MyMissionNameMarkerTargetVM>();
            this._distanceComparer = new MyMissionNameMarkerVM.MarkerDistanceComparer();
            this._missionCamera = missionCamera;
            this._additionalTargetAgents = additionalTargetAgents;
            this._additionalGenericTargets = additionalGenericTargets;
            this._genericTargets = new Dictionary<string, MyMissionNameMarkerTargetVM>();
            this._mission = mission;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            this.Targets.ApplyActionOnAllItems(delegate (MyMissionNameMarkerTargetVM x)
            {
                x.RefreshValues();
            });
        }

        public void Tick(float dt)
        {
            //try
           // {
                if (!this.IsTargetsAdded)
                {

                    if (this._mission.MainAgent != null)
                    {
                        if (this._additionalTargetAgents != null)
                        {
                            foreach (KeyValuePair<Agent, SandBoxUIHelper.IssueQuestFlags> keyValuePair in this._additionalTargetAgents)
                            {
                                this.AddAgentTarget(keyValuePair.Key, true);
                                this.UpdateAdditionalTargetAgentQuestStatus(keyValuePair.Key, keyValuePair.Value);
                            }
                        }
                        if (this._additionalGenericTargets != null)
                        {
                            foreach (KeyValuePair<string, ValueTuple<Vec3, string, string>> keyValuePair2 in this._additionalGenericTargets)
                            {
                                this.AddGenericMarker(keyValuePair2.Key, keyValuePair2.Value.Item1, keyValuePair2.Value.Item2, keyValuePair2.Value.Item3);
                            }
                        }
                        foreach (Agent agent in this._mission.Agents)
                        {
                            this.AddAgentTarget(agent, false);
                        }
                        if (Hero.MainHero.CurrentSettlement != null)
                        {
                            List<CommonAreaMarker> list = (from x in this._mission.ActiveMissionObjects.FindAllWithType<CommonAreaMarker>()
                                                           where x.GameEntity.HasTag("common_area_marker")
                                                           select x).ToList<CommonAreaMarker>();
                            if (Hero.MainHero.CurrentSettlement.CommonAreas.Count > 0)
                            {
                                foreach (CommonAreaMarker commonAreaMarker in list)
                                {
                                    CommonArea commonArea = Hero.MainHero.CurrentSettlement.CommonAreas[commonAreaMarker.AreaIndex - 1];
                                    CommonAreaPartyComponent commonAreaPartyComponent = commonArea.CommonAreaPartyComponent;
                                    if ((commonAreaPartyComponent != null && commonAreaPartyComponent.MobileParty.MemberRoster.TotalManCount > 0) || Campaign.Current.VisualTrackerManager.CheckTracked(commonArea))
                                    {
                                        this.Targets.Add(new MyMissionNameMarkerTargetVM(commonAreaMarker));
                                    }
                                }
                            }
                            foreach (PassageUsePoint passageUsePoint in from passage in this._mission.ActiveMissionObjects.FindAllWithType<PassageUsePoint>().ToList<PassageUsePoint>()
                                                                        where passage.ToLocation != null && !this.PassagePointFilter.Exists((string s) => passage.ToLocation.Name.Contains(s))
                                                                        select passage)
                            {
                                if (!passageUsePoint.ToLocation.CanBeReserved || passageUsePoint.ToLocation.IsReserved)
                                {
                                    this.Targets.Add(new MyMissionNameMarkerTargetVM(passageUsePoint));
                                }
                            }
                            if (this._mission.HasMissionBehavior<WorkshopMissionHandler>())
                            {
                                foreach (Tuple<Workshop, GameEntity> tuple in from s in this._mission.GetMissionBehavior<WorkshopMissionHandler>().WorkshopSignEntities.ToList<Tuple<Workshop, GameEntity>>()
                                                                              where s.Item1.WorkshopType != null
                                                                              select s)
                                {
                                    this.Targets.Add(new MyMissionNameMarkerTargetVM(tuple.Item1.WorkshopType, tuple.Item2.GlobalPosition - this._heightOffset));
                                }
                            }
                        }
                    }
                }
                this.IsTargetsAdded = true;

                if (this.IsEnabled)
                {
                    this.UpdateTargetScreenPositions();
                    this._fadeOutTimerStarted = false;
                    this._fadeOutTimer = 0f;
                    this._prevEnabledState = this.IsEnabled;
                }
                else
                {
                    if (this._prevEnabledState)
                    {
                        this._fadeOutTimerStarted = true;
                    }
                    if (this._fadeOutTimerStarted)
                    {
                        this._fadeOutTimer += dt;
                    }
                    if (this._fadeOutTimer < 2f)
                    {
                        this.UpdateTargetScreenPositions();
                    }
                    else
                    {
                        this._fadeOutTimerStarted = false;
                    }
                }
                this._prevEnabledState = this.IsEnabled;
           // }
           // catch
           // {
           // }  
        }

        private void UpdateTargetScreenPositions()
        {
            foreach (MyMissionNameMarkerTargetVM missionNameMarkerTargetVM in this.Targets)
            {
                float a = -100f;
                float b = -100f;
                float num = 0f;
                MBWindowManager.WorldToScreenInsideUsableArea(this._missionCamera, missionNameMarkerTargetVM.WorldPosition + this._heightOffset, ref a, ref b, ref num);
                if (num > 0f)
                {
                    missionNameMarkerTargetVM.ScreenPosition = new Vec2(a, b);
                    missionNameMarkerTargetVM.Distance = (int)(missionNameMarkerTargetVM.WorldPosition - this._missionCamera.Position).Length;
                }
                else
                {
                    missionNameMarkerTargetVM.Distance = -1;
                    missionNameMarkerTargetVM.ScreenPosition = new Vec2(-100f, -100f);
                }
            }
            this.Targets.Sort(this._distanceComparer);
        }

        public void OnConversationEnd()
        {
            foreach (Agent agent in this._mission.Agents)
            {
                this.AddAgentTarget(agent, false);
            }
            foreach (MyMissionNameMarkerTargetVM missionNameMarkerTargetVM in this.Targets)
            {
                if (!missionNameMarkerTargetVM.IsAdditionalTargetAgent)
                {
                    missionNameMarkerTargetVM.UpdateQuestStatus();
                }
            }
        }

        public void OnAgentBuild(Agent agent)
        {
            this.AddAgentTarget(agent, false);
        }

        public void OnAgentRemoved(Agent agent)
        {
            this.RemoveAgentTarget(agent);
        }

        public void OnAgentDeleted(Agent agent)
        {
            this.RemoveAgentTarget(agent);
        }

        public void UpdateAdditionalTargetAgentQuestStatus(Agent agent, SandBoxUIHelper.IssueQuestFlags issueQuestFlags)
        {
            MyMissionNameMarkerTargetVM missionNameMarkerTargetVM = this.Targets.FirstOrDefault((MyMissionNameMarkerTargetVM t) => t.TargetAgent == agent);
            if (missionNameMarkerTargetVM == null)
            {
                return;
            }
            missionNameMarkerTargetVM.UpdateQuestStatus(issueQuestFlags);
        }

        public void AddGenericMarker(string markerIdentifier, Vec3 markerPosition, string name, string iconType)
        {
            MyMissionNameMarkerTargetVM missionNameMarkerTargetVM;
            if (this._genericTargets.TryGetValue(markerIdentifier, out missionNameMarkerTargetVM))
            {
                Debug.FailedAssert("Marker with identifier: " + markerIdentifier + " already exists", "C:\\Develop\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Missions\\NameMarker\\MissionNameMarkerVM.cs", "AddGenericMarker", 217);
                return;
            }
            MyMissionNameMarkerTargetVM missionNameMarkerTargetVM2 = new MyMissionNameMarkerTargetVM(markerPosition, name, iconType);
            this._genericTargets.Add(markerIdentifier, missionNameMarkerTargetVM2);
            this.Targets.Add(missionNameMarkerTargetVM2);
        }

        public void RemoveGenericMarker(string markerIdentifier)
        {
            MyMissionNameMarkerTargetVM item;
            if (this._genericTargets.TryGetValue(markerIdentifier, out item))
            {
                this._genericTargets.Remove(markerIdentifier);
                this.Targets.Remove(item);
                return;
            }
            Debug.FailedAssert("Marker with identifier: " + markerIdentifier + " does not exist", "C:\\Develop\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Missions\\NameMarker\\MissionNameMarkerVM.cs", "RemoveGenericMarker", 236);
        }

        public void AddAgentTarget(Agent agent, bool isAdditional = false)
        {
            if (agent != Agent.Main && agent.Character != null && agent.IsActive() && !this.Targets.Any((MyMissionNameMarkerTargetVM t) => t.TargetAgent == agent))
            {
                bool flag4;
                if (!isAdditional && !agent.Character.IsHero)
                {
                    Settlement currentSettlement = Settlement.CurrentSettlement;
                    bool flag;
                    if (currentSettlement == null)
                    {
                        flag = false;
                    }
                    else
                    {
                        LocationCharacter locationCharacter = currentSettlement.LocationComplex.FindCharacter(agent);
                        bool? flag2 = (locationCharacter != null) ? new bool?(locationCharacter.IsVisualTracked) : null;
                        bool flag3 = true;
                        flag = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
                    }
                    CharacterObject characterObject;
                    if (!flag && ((characterObject = (agent.Character as CharacterObject)) == null || (characterObject.Occupation != Occupation.RansomBroker && characterObject.Occupation != Occupation.Tavernkeeper)))
                    {
                        BasicCharacterObject character = agent.Character;
                        Settlement currentSettlement2 = Settlement.CurrentSettlement;
                        object obj;
                        if (currentSettlement2 == null)
                        {
                            obj = null;
                        }
                        else
                        {
                            CultureObject culture = currentSettlement2.Culture;
                            obj = ((culture != null) ? culture.Blacksmith : null);
                        }
                        if (character != obj)
                        {
                            BasicCharacterObject character2 = agent.Character;
                            Settlement currentSettlement3 = Settlement.CurrentSettlement;
                            object obj2;
                            if (currentSettlement3 == null)
                            {
                                obj2 = null;
                            }
                            else
                            {
                                CultureObject culture2 = currentSettlement3.Culture;
                                obj2 = ((culture2 != null) ? culture2.Barber : null);
                            }
                            if (character2 != obj2)
                            {
                                BasicCharacterObject character3 = agent.Character;
                                Settlement currentSettlement4 = Settlement.CurrentSettlement;
                                object obj3;
                                if (currentSettlement4 == null)
                                {
                                    obj3 = null;
                                }
                                else
                                {
                                    CultureObject culture3 = currentSettlement4.Culture;
                                    obj3 = ((culture3 != null) ? culture3.TavernGamehost : null);
                                }
                                flag4 = (character3 == obj3);
                                goto IL_172;
                            }
                        }
                    }
                }
                flag4 = true;
            IL_172:
                if (flag4)
                {
                    MyMissionNameMarkerTargetVM item = new MyMissionNameMarkerTargetVM(agent, isAdditional);
                    this.Targets.Add(item);
                }
            }
        }

        public void RemoveAgentTarget(Agent agent)
        {
            if (this.Targets.SingleOrDefault((MyMissionNameMarkerTargetVM t) => t.TargetAgent == agent) != null)
            {
                this.Targets.Remove(this.Targets.Single((MyMissionNameMarkerTargetVM t) => t.TargetAgent == agent));
            }
        }

        private void UpdateTargetStates(bool state)
        {
            foreach (MyMissionNameMarkerTargetVM missionNameMarkerTargetVM in this.Targets)
            {
                missionNameMarkerTargetVM.IsEnabled = state;
            }
        }

        [DataSourceProperty]
        public MBBindingList<MyMissionNameMarkerTargetVM> Targets
        {
            get
            {
                return this._targets;
            }
            set
            {
                if (value != this._targets)
                {
                    this._targets = value;
                    base.OnPropertyChangedWithValue(value, "Targets");
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
                    this.UpdateTargetStates(value);
                    Game.Current.EventManager.TriggerEvent<MissionNameMarkerToggleEvent>(new MissionNameMarkerToggleEvent(value));
                }
            }
        }

        private readonly Camera _missionCamera;

        private readonly Mission _mission;

        private Vec3 _heightOffset = new Vec3(0f, 0f, 2f, -1f);

        private bool _prevEnabledState;

        private bool _fadeOutTimerStarted;

        private float _fadeOutTimer;

        private Dictionary<Agent, SandBoxUIHelper.IssueQuestFlags> _additionalTargetAgents;

        private Dictionary<string, ValueTuple<Vec3, string, string>> _additionalGenericTargets;

        private Dictionary<string, MyMissionNameMarkerTargetVM> _genericTargets;

        private readonly MyMissionNameMarkerVM.MarkerDistanceComparer _distanceComparer;

        private readonly List<string> PassagePointFilter = new List<string>
        {
            "Empty Shop"
        };

        private MBBindingList<MyMissionNameMarkerTargetVM> _targets;

        private bool _isEnabled;

        private class MarkerDistanceComparer : IComparer<MyMissionNameMarkerTargetVM>
        {
            public int Compare(MyMissionNameMarkerTargetVM x, MyMissionNameMarkerTargetVM y)
            {
                return y.Distance.CompareTo(x.Distance);
            }
        }

    }
}
