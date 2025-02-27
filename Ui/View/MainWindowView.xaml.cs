﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using PRM.Model;
using PRM.Service;
using PRM.Utils;
using PRM.View.Host.ProtocolHosts;
using Shawn.Utils;
using Shawn.Utils.Interface;
using Shawn.Utils.Wpf;
using Shawn.Utils.WpfResources.Theme.Styles;
using Stylet;
using Ui;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace PRM.View
{
    public partial class MainWindowView : WindowChromeBase
    {
        public MainWindowViewModel Vm { get; }
        private readonly ConfigurationService _configurationService;


        public MainWindowView(MainWindowViewModel vm, LocalityService localityService, ConfigurationService configurationService)
        {
            InitializeComponent();
            Vm = vm;
            _configurationService = configurationService;
            this.DataContext = Vm;
            Title = AppPathHelper.APP_DISPLAY_NAME;
            // restore the window size from 
            this.Width = localityService.MainWindowWidth;
            this.Height = localityService.MainWindowHeight;
            // check the current screen size
            var screenEx = ScreenInfoEx.GetCurrentScreenBySystemPosition(ScreenInfoEx.GetMouseSystemPosition());
            if (this.Width > screenEx.VirtualWorkingArea.Width)
                this.Width = Math.Min(screenEx.VirtualWorkingArea.Width * 0.8, this.Width * 0.8);
            if (this.Height > screenEx.VirtualWorkingArea.Height)
                this.Height = Math.Min(screenEx.VirtualWorkingArea.Height * 0.8, this.Height * 0.8);

            this.SizeChanged += (sender, args) =>
            {
                if (this.WindowState == WindowState.Normal)
                {
                    localityService.MainWindowHeight = this.Height;
                    localityService.MainWindowWidth = this.Width;
                    SimpleLogHelper.Info($"Main window resize to: w = {this.Width}, h = {this.Height}");
                }
            };

            WinTitleBar.PreviewMouseDown += WinTitleBar_OnPreviewMouseDown;
            WinTitleBar.MouseUp += WinTitleBar_OnMouseUp;
            WinTitleBar.PreviewMouseMove += WinTitleBar_OnPreviewMouseMove;

            // Startup Location
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Top = screenEx.VirtualWorkingAreaCenter.Y - this.Height / 2;
            this.Left = screenEx.VirtualWorkingAreaCenter.X - this.Width / 2;


            if (IoC.Get<ConfigurationService>().General.AppStartMinimized == true)
            {
                this.Visibility = Visibility.Collapsed;
                this.WindowState = WindowState.Minimized;
            }

            BtnClose.Click += (sender, args) =>
            {
                if ((_configurationService.Engagement.DoNotShowAgain == false || AppVersion.VersionData > _configurationService.Engagement.DoNotShowAgainVersion)
                    && _configurationService.Engagement.InstallTime < DateTime.Now.AddDays(-15)
                    && _configurationService.Engagement.LastRequestRatingsTime < DateTime.Now.AddDays(-60)
                    && _configurationService.Engagement.ConnectCount > 30
                   )
                {
                    // 显示“请求应用的评分和评价”页面 https://docs.microsoft.com/zh-cn/windows/uwp/monetize/request-ratings-and-reviews
                    Vm.TopLevelViewModel = IoC.Get<RequestRatingViewModel>();
                    return;
                }
                vm.HideMe();
#if DEBUG
                App.Close();
#endif
            };
            this.Closing += (sender, args) =>
            {
                if (this.ShowInTaskbar)
                {
                    vm.HideMe();
                    args.Cancel = true;
                }
            };

            BtnMaximize.Click += (sender, args) => this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
            BtnMinimize.Click += (sender, args) => { this.WindowState = WindowState.Minimized; };
        }




        private void CommandFocusFilter_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SimpleLogHelper.Debug($"CommandFocusFilter_OnExecuted");
            if (Vm.IsShownList())
                Vm.MainFilterIsFocused = true;
        }



        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
            {
                //SimpleLogHelper.Debug($"Current FocusedElement is " + textBox.Name);
            }
            else if (e.Key == Key.Escape && this.DataContext is MainWindowViewModel vm && vm.IsShownList() == false)
            {
                vm.ShowList();
            }
            else if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl)
            {
                Vm.MainFilterIsFocused = true;
                //TbFilter.Focus();
                //TbFilter.CaretIndex = TbFilter.Text.Length;
            }
        }

        private void ProcessingRing_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
                return;
            base.WinTitleBar_OnPreviewMouseDown(sender, e);
        }


        private void MainFilter_OnKeyUp(object sender, KeyEventArgs e)
        {
            // When press Esc, clear all of the search keywords, but keep selected tags;
            if (e.Key != Key.Escape || sender is TextBox == false) return;
            var s = TagAndKeywordEncodeHelper.DecodeKeyword(Vm.MainFilterString);
            Vm.SetMainFilterString(s.Item1, null);
        }
    }
}