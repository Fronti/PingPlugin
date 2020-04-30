﻿using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using Dalamud.Plugin;
using ImGuiNET;

namespace PingPlugin
{
    public class PingUI
    {
        private readonly PingConfiguration config;
        private readonly PingTracker pingTracker;

        private bool configVisible;
        private ImFontPtr courierNew;

        public bool ConfigVisible
        {
            get => this.configVisible;
            set => this.configVisible = value;
        }

        public PingUI(PingTracker pingTracker, PingConfiguration config)
        {
            this.config = config;
            this.pingTracker = pingTracker;
        }

        public void BuildUi()
        {
            if (this.ConfigVisible) DrawConfigUi();
            if (this.config.GraphIsVisible) DrawGraph();
            if (this.config.MonitorIsVisible) DrawMonitor();
        }

        private void DrawConfigUi()
        {
            ImGui.SetNextWindowSize(new Vector2(250, 120), ImGuiCond.Always);

            ImGui.Begin("PingPlugin Configuration", ref this.configVisible, ImGuiWindowFlags.NoResize);
            var lockWindows = this.config.LockWindows;
            if (ImGui.Checkbox("Lock plugin windows", ref lockWindows))
            {
                this.config.LockWindows = lockWindows;
                this.config.Save();
            }
            var clickThrough = this.config.ClickThrough;
            if (ImGui.Checkbox("Click through plugin windows", ref clickThrough))
            {
                this.config.ClickThrough = clickThrough;
                this.config.Save();
            }
            ImGui.End();
        }

        private void DrawMonitor()
        {
            var windowFlags = BuildWindowFlags(ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
            ImGui.SetNextWindowPos(this.config.MonitorPosition, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new Vector2(170, 50), ImGuiCond.Always); // Auto-resize doesn't seem to work here

            ImGui.Begin("PingMonitor", windowFlags);
            ImGui.PushFont(this.courierNew);
            ImGui.TextColored(new Vector4(1, 1, 0, 1), $"Ping: {this.pingTracker.LastRTT}ms\nAverage ping: {Math.Round(this.pingTracker.AverageRTT), 2}ms"); // Yellow, it's ABGR instead of RGBA for some reason.
            ImGui.PopFont();
            ImGui.End();

            ImGui.PopStyleVar(1);
        }

        private void DrawGraph()
        {
            var windowFlags = BuildWindowFlags(ImGuiWindowFlags.NoResize);

            ImGui.SetNextWindowPos(this.config.GraphPosition, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new Vector2(316, 190), ImGuiCond.Always);

            ImGui.Begin("Ping Graph", windowFlags);
            var pingArray = this.pingTracker.RTTTimes.ToArray();
            if (pingArray.Length > 0)
                ImGui.PlotLines(string.Empty, ref pingArray[0], pingArray.Length, 0, null, float.MaxValue, float.MaxValue, new Vector2(300, 150));
            else
                ImGui.Text("No data to display at this time.");
            ImGui.End();
        }

        private ImGuiWindowFlags BuildWindowFlags(ImGuiWindowFlags flags)
        {
            if (this.config.ClickThrough)
            {
                flags |= ImGuiWindowFlags.NoInputs;
            }
            if (this.config.LockWindows)
            {
                flags |= ImGuiWindowFlags.NoMove;
            }
            return flags;
        }
    }
}
