using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ECommons.ImGuiMethods;
using FFTriadBuddy;
using ImGuiNET;
using PunishLib.ImGuiMethods;
using Saucy.CuffACur;
using Saucy.TripleTriad;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using TriadBuddyPlugin;

namespace Saucy
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    public class PluginUI : IDisposable
    {
        private Configuration configuration;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        private bool settingsVisible = false;
        private GameNpcInfo currentNPC;

        public bool SettingsVisible
        {
            get { return settingsVisible; }
            set { settingsVisible = value; }
        }

        public GameNpcInfo CurrentNPC
        {
            get => currentNPC;
            set
            {
                if (currentNPC != value)
                {
                    TriadAutomater.TempCardsWonList.Clear();
                    currentNPC = value;
                }
            }
        }

        public PluginUI(Configuration configuration)
        {
            this.configuration = Service.Configuration;
        }

        public void Dispose()
        {
        }

        public bool Enabled { get; set; } = false;

        public void Draw()
        {
            DrawMainWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(520, 420), ImGuiCond.FirstUseEver);
            //ImGui.SetNextWindowSizeConstraints(new Vector2(520, 420), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Saucy Config", ref visible))
            {
                if (ImGui.BeginTabBar("Games"))
                {
                    if (ImGui.BeginTabItem("重击伽美什"))
                    {
                        DrawCufTab();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("九宫幻卡"))
                    {
                        DrawTriadTab();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Stats"))
                    {
                        DrawStatsTab();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("About"))
                    {
                        AboutTab.Draw(Saucy.P);
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
        }

        private void DrawStatsTab()
        {
            if (ImGui.BeginTabBar("Stats"))
            {
                if (ImGui.BeginTabItem("Lifetime"))
                {
                    this.DrawStatsTab(Service.Configuration.Stats, out bool reset);

                    if (reset)
                    {
                        Service.Configuration.Stats = new();
                        Service.Configuration.Save();
                    }

                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Session"))
                {
                    this.DrawStatsTab(Service.Configuration.SessionStats, out bool reset);
                    if (reset)
                        Service.Configuration.SessionStats = new();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        private void DrawStatsTab(Stats stat, out bool reset)
        {
            ImGui.Columns(3, "统计数据", false);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText(ImGuiColors.ParsedGold, "SAUCY统计", true);
            ImGui.Columns(1);
            ImGui.BeginChild("TT Stats", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
            ImGui.Columns(3, null, false);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "九宫幻卡", true);
            ImGuiHelpers.ScaledDummy(10f);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("已游玩次数", true);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.GamesPlayedWithSaucy.ToString("N0")}");
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.Spacing();
            ImGuiEx.CenterColumnText("胜局", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("败局", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("平局", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.GamesWonWithSaucy.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.GamesLostWithSaucy.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.GamesDrawnWithSaucy.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("胜率", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("获得的卡", true);
            ImGui.NextColumn();
            if (stat.NPCsPlayed.Count > 0)
            {
                ImGuiEx.CenterColumnText("对战最多的NPC", true);
                ImGui.NextColumn();
            }
            else
            {
                ImGui.NextColumn();
            }

            if (stat.GamesPlayedWithSaucy > 0)
            {
                ImGuiEx.CenterColumnText($"{Math.Round(((double)stat.GamesWonWithSaucy / (double)stat.GamesPlayedWithSaucy) * 100, 2)}%");
            }
            else
            {
                ImGuiEx.CenterColumnText("");
            }
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CardsDroppedWithSaucy.ToString("N0")}");
            ImGui.NextColumn();

            if (stat.NPCsPlayed.Count > 0)
            {
                ImGuiEx.CenterColumnText($"{stat.NPCsPlayed.OrderByDescending(x => x.Value).First().Key}");
                ImGuiEx.CenterColumnText($"{stat.NPCsPlayed.OrderByDescending(x => x.Value).First().Value.ToString("N0")} times");
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGui.NextColumn();
            }

            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("赢得的金碟币", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("卡片掉落总价值", true);
            ImGui.NextColumn();
            if (stat.CardsWon.Count > 0)
            {
                ImGuiEx.CenterColumnText("赢得最多的卡", true);
            }
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.MGPWon.ToString("N0")} 金碟币");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{GetDroppedCardValues(stat).ToString("N0")} 金碟币");
            ImGui.NextColumn();
            if (stat.CardsWon.Count > 0)
            {
                ImGuiEx.CenterColumnText($"{TriadCardDB.Get().FindById((int)stat.CardsWon.OrderByDescending(x => x.Value).First().Key).Name.GetLocalized()}");
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGuiEx.CenterColumnText($"{stat.CardsWon.OrderByDescending(x => x.Value).First().Value.ToString("N0")} times");
            }

            ImGui.Columns(1);
            ImGui.EndChild();
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
            reset = ImGui.Button("重置状态（按住Ctrl键）", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y)) && ImGui.GetIO().KeyCtrl;
        }

        private int GetDroppedCardValues(Stats stat)
        {
            int output = 0;
            foreach (var card in stat.CardsWon)
                output += GameCardDB.Get().FindById((int)card.Key).SaleValue * stat.CardsWon[card.Key];

            return output;
        }

        public void DrawTriadTab()
        {
            bool enabled = TriadAutomater.ModuleEnabled;

            ImGui.TextWrapped(@"How to use: 挑战一个你想对战的NPC。 一旦你发起挑战, 单击""启用九宫幻卡模块""。");
            ImGui.Separator();

            if (ImGui.Checkbox("启用九宫幻卡模块", ref enabled))
            {
                TriadAutomater.ModuleEnabled = enabled;
            }

            bool autoOpen = configuration.OpenAutomatically;

            if (ImGui.Checkbox("打开 Saucy 当与NPC对战时", ref autoOpen))
            {
                configuration.OpenAutomatically= autoOpen;
                configuration.Save();
            }

            int selectedDeck = configuration.SelectedDeckIndex;

            if (Saucy.TTSolver.profileGS.GetPlayerDecks().Count() > 0)
            {
                if (!Service.Configuration.UseRecommendedDeck)
                {
                    ImGui.PushItemWidth(200);
                    string preview = selectedDeck >= 0 ? Saucy.TTSolver.profileGS.GetPlayerDecks()[selectedDeck].name : string.Empty;
                    if (ImGui.BeginCombo("选择幻卡", preview))
                    {
                        if (ImGui.Selectable(""))
                        {
                            configuration.SelectedDeckIndex = -1;
                        }

                        foreach (var deck in Saucy.TTSolver.profileGS.GetPlayerDecks())
                        {
                            var index = deck.id;
                            //var index = Saucy.TTSolver.preGameDecks.Where(x => x.Value == deck).First().Key;
                            if (ImGui.Selectable(deck.name, index == selectedDeck))
                            {
                                configuration.SelectedDeckIndex = index;
                                configuration.Save();
                            }
                        }

                        ImGui.EndCombo();
                    }

                    ImGui.SameLine();
                }
                bool useAutoDeck = Service.Configuration.UseRecommendedDeck;
                if (ImGui.Checkbox("自动选择具有最佳获胜机率的卡组", ref useAutoDeck))
                {
                    Service.Configuration.UseRecommendedDeck = useAutoDeck;
                    Service.Configuration.Save();
                }
            }
            else
            {
                ImGui.TextWrapped("请向NPC发起挑战，以充实你的卡组列表");
            }

            if (ImGui.Checkbox("进行x次幻卡对局", ref TriadAutomater.PlayXTimes) && (TriadAutomater.NumberOfTimes <= 0 || TriadAutomater.PlayUntilCardDrops || TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.NumberOfTimes = 1;
                TriadAutomater.PlayUntilCardDrops = false;
                TriadAutomater.PlayUntilAllCardsDropOnce = false;
            }

            if (ImGui.Checkbox("一直玩到有任意幻卡掉落为止", ref TriadAutomater.PlayUntilCardDrops) && (TriadAutomater.NumberOfTimes <= 0 || TriadAutomater.PlayXTimes || TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.NumberOfTimes = 1;
                TriadAutomater.PlayXTimes = false;
                TriadAutomater.PlayUntilAllCardsDropOnce = false;
            }


            if (GameNpcDB.Get().mapNpcs.TryGetValue(Saucy.TTSolver.preGameNpc?.Id ?? -1, out var npcInfo))
            {
                CurrentNPC = npcInfo;
            }
            else
            {
                CurrentNPC = null;
            }

            if (ImGui.Checkbox($"至少进行x次对局直到NPC掉落所有幻卡 {(CurrentNPC is null ? "" : $"({TriadNpcDB.Get().FindByID(CurrentNPC.npcId).Name.GetLocalized()})")}", ref TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.TempCardsWonList.Clear();
                TriadAutomater.PlayUntilCardDrops = false;
                TriadAutomater.PlayXTimes = false;
                TriadAutomater.NumberOfTimes = 1;
            }

            bool onlyUnobtained = Service.Configuration.OnlyUnobtainedCards;

            if (TriadAutomater.PlayUntilAllCardsDropOnce)
            {
                ImGui.SameLine();
                if (ImGui.Checkbox("仅未获得的幻卡", ref onlyUnobtained))
                {
                    TriadAutomater.TempCardsWonList.Clear();
                    Service.Configuration.OnlyUnobtainedCards = onlyUnobtained;
                    Service.Configuration.Save();
                }
            }

            if (TriadAutomater.PlayUntilAllCardsDropOnce && CurrentNPC != null)
            {
                ImGui.Indent();
                GameCardDB.Get().Refresh();
                foreach (var card in CurrentNPC.rewardCards)
                {
                    if ((Service.Configuration.OnlyUnobtainedCards && !GameCardDB.Get().FindById(card).IsOwned) || !Service.Configuration.OnlyUnobtainedCards)
                    {
                        TriadAutomater.TempCardsWonList.TryAdd((uint)card, 0);
                        ImGui.Text($"- {TriadCardDB.Get().FindById((int)GameCardDB.Get().FindById(card).CardId).Name.GetLocalized()} {TriadAutomater.TempCardsWonList[(uint)card]}/{TriadAutomater.NumberOfTimes}");
                    }

                }

                if (Service.Configuration.OnlyUnobtainedCards && TriadAutomater.TempCardsWonList.Count == 0)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                    ImGui.TextWrapped($@"你已经拥有这个NPC的所有卡片。在你取消勾选""仅未获得的卡""或选择其他NPC之前，这个功能将不会起作用。");
                    ImGui.PopStyleColor();
                }
                ImGui.Unindent();
            }


            if (TriadAutomater.PlayXTimes || TriadAutomater.PlayUntilCardDrops || TriadAutomater.PlayUntilAllCardsDropOnce)
            {
                ImGui.PushItemWidth(150f);
                ImGui.Text("多少次:");
                ImGui.SameLine();

                if (ImGui.InputInt("###NumberOfTimes", ref TriadAutomater.NumberOfTimes))
                {
                    if (TriadAutomater.NumberOfTimes <= 0)
                        TriadAutomater.NumberOfTimes = 1;
                }

                ImGui.Checkbox("完成后登出", ref TriadAutomater.LogOutAfterCompletion);

                bool playSound = Service.Configuration.PlaySound;

                ImGui.Columns(2, null, false);
                if (ImGui.Checkbox("完成后播放声效", ref playSound))
                {
                    Service.Configuration.PlaySound = playSound;
                    Service.Configuration.Save();
                }

                if (playSound)
                {
                    ImGui.NextColumn();
                    ImGui.Text("选择音效");
                    if (ImGui.BeginCombo("###SelectSound", Service.Configuration.SelectedSound))
                    {
                        string path = Path.Combine(Service.Interface.AssemblyLocation.Directory.FullName, "Sounds");
                        foreach (var file in new DirectoryInfo(path).GetFiles())
                        {
                            if (ImGui.Selectable($"{Path.GetFileNameWithoutExtension(file.FullName)}", Service.Configuration.SelectedSound == Path.GetFileNameWithoutExtension(file.FullName)))
                            {
                                Service.Configuration.SelectedSound = Path.GetFileNameWithoutExtension(file.FullName);
                                Service.Configuration.Save();
                            }
                        }

                        ImGui.EndCombo();
                    }

                    if (ImGui.Button("打开音效文件夹"))
                    {
                        Process.Start("explorer.exe", @$"{Path.Combine(Service.Interface.AssemblyLocation.Directory.FullName, "Sounds")}");
                    }
                    ImGuiComponents.HelpMarker("放入任意MP3文件至音效文件夹以添加自定义声效。");
                }
                ImGui.Columns(1);
            }
        }


        public void DrawCufTab()
        {
            bool enabled = CufModule.ModuleEnabled;

            ImGui.TextWrapped(@"使用方法:单击""启用重击伽美什""模块，然后走到重击伽美什机器前。");
            ImGui.Separator();

            if (ImGui.Checkbox("启用重击伽美什", ref enabled))
            {
                CufModule.ModuleEnabled = enabled;
            }
        }
    }
}
