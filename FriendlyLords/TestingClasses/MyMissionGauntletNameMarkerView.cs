using System;
using System.Collections.Generic;
using SandBox.View.Missions;
using SandBox.ViewModelCollection;
using SandBox.ViewModelCollection.Missions.NameMarker;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace FriendlyLords
{
    [OverrideView(typeof(MissionNameMarkerUIHandler))]
    class MyMissionGauntletNameMarkerView : MissionView
    { 
        public MyMissionGauntletNameMarkerView()
        {
            this._additionalTargetAgents = new Dictionary<Agent, SandBoxUIHelper.IssueQuestFlags>();
            this._additionalGenericTargets = new Dictionary<string, ValueTuple<Vec3, string, string>>();
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._dataSource = new MyMissionNameMarkerVM(base.Mission, base.MissionScreen.CombatCamera, this._additionalTargetAgents, this._additionalGenericTargets);
            this._gauntletLayer = new GauntletLayer(1, "GauntletLayer", false);
            this._gauntletLayer.LoadMovie("NameMarker", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            CampaignEvents.ConversationEnded.AddNonSerializedListener(this, new Action<CharacterObject>(this.OnConversationEnd));
        }

        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._gauntletLayer = null;
            this._dataSource.OnFinalize();
            this._dataSource = null;
            this._additionalTargetAgents.Clear();
            CampaignEvents.ConversationEnded.ClearListeners(this);
            InformationManager.ClearAllMessages();
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (base.Input.IsGameKeyDown(5))
            {
                this._dataSource.IsEnabled = true;
            }
            else
            {
                this._dataSource.IsEnabled = false;
            }
            this._dataSource.Tick(dt);
        }

        public override void OnAgentBuild(Agent affectedAgent, Banner banner)
        {
            base.OnAgentBuild(affectedAgent, banner);
            MyMissionNameMarkerVM dataSource = this._dataSource;
            if (dataSource == null)
            {
                return;
            }
            dataSource.OnAgentBuild(affectedAgent);
        }

        public override void OnAgentDeleted(Agent affectedAgent)
        {
            this._dataSource.OnAgentDeleted(affectedAgent);
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
        {
            this._dataSource.OnAgentRemoved(affectedAgent);
        }

        private void OnConversationEnd(CharacterObject character)
        {
            this._dataSource.OnConversationEnd();
        }

        public override void OnPhotoModeActivated()
        {
            base.OnPhotoModeActivated();
            this._gauntletLayer._gauntletUIContext.ContextAlpha = 0f;
        }

        public override void OnPhotoModeDeactivated()
        {
            base.OnPhotoModeDeactivated();
            this._gauntletLayer._gauntletUIContext.ContextAlpha = 1f;
        }

        public void UpdateAgentTargetQuestStatus(Agent agent, SandBoxUIHelper.IssueQuestFlags issueQuestFlags)
        {
            if (agent != null)
            {
                MyMissionNameMarkerVM dataSource = this._dataSource;
                if (dataSource != null && dataSource.IsTargetsAdded)
                {
                    this._dataSource.UpdateAdditionalTargetAgentQuestStatus(agent, issueQuestFlags);
                    return;
                }
                SandBoxUIHelper.IssueQuestFlags issueQuestFlags2;
                if (this._additionalTargetAgents.TryGetValue(agent, out issueQuestFlags2))
                {
                    this._additionalTargetAgents[agent] = issueQuestFlags;
                }
            }
        }

        public void AddGenericMarker(string markerIdentifier, Vec3 globalPosition, string name, string iconType)
        {
            MyMissionNameMarkerVM dataSource = this._dataSource;
            if (dataSource != null && dataSource.IsTargetsAdded)
            {
                this._dataSource.AddGenericMarker(markerIdentifier, globalPosition, name, iconType);
                return;
            }
            this._additionalGenericTargets.Add(markerIdentifier, new ValueTuple<Vec3, string, string>(globalPosition, name, iconType));
        }

        public void RemoveGenericMarker(string markerIdentifier)
        {
            MyMissionNameMarkerVM dataSource = this._dataSource;
            if (dataSource != null && dataSource.IsTargetsAdded)
            {
                this._dataSource.RemoveGenericMarker(markerIdentifier);
                return;
            }
            this._additionalGenericTargets.Remove(markerIdentifier);
        }

        public void AddAgentTarget(Agent agent, SandBoxUIHelper.IssueQuestFlags issueQuestFlags)
        {
            if (agent != null)
            {
                MyMissionNameMarkerVM dataSource = this._dataSource;
                if (dataSource != null && dataSource.IsTargetsAdded)
                {
                    this._dataSource.AddAgentTarget(agent, true);
                    this._dataSource.UpdateAdditionalTargetAgentQuestStatus(agent, issueQuestFlags);
                    return;
                }
                SandBoxUIHelper.IssueQuestFlags issueQuestFlags2;
                if (!this._additionalTargetAgents.TryGetValue(agent, out issueQuestFlags2))
                {
                    this._additionalTargetAgents.Add(agent, issueQuestFlags);
                }
            }
        }

        public void RemoveAgentTarget(Agent agent)
        {
            if (agent != null)
            {
                MyMissionNameMarkerVM dataSource = this._dataSource;
                if (dataSource != null && dataSource.IsTargetsAdded)
                {
                    this._dataSource.RemoveAgentTarget(agent);
                    return;
                }
                SandBoxUIHelper.IssueQuestFlags issueQuestFlags;
                if (this._additionalTargetAgents.TryGetValue(agent, out issueQuestFlags))
                {
                    this._additionalTargetAgents.Remove(agent);
                }
            }
        }

        private GauntletLayer _gauntletLayer;
        private MyMissionNameMarkerVM _dataSource;
        private readonly Dictionary<Agent, SandBoxUIHelper.IssueQuestFlags> _additionalTargetAgents;
        private Dictionary<string, ValueTuple<Vec3, string, string>> _additionalGenericTargets;
    }
}