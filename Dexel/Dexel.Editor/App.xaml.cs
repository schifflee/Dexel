﻿using System;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Dexel.Editor.ViewModels;
using Dexel.Editor.Views;
using Dexel.Model;
using Dexel.Model.Mockdata;
using UnkownErrorDialog = Dexel.Editor.Views.AdditionalWindows.UnkownErrorDialog;

namespace Dexel.Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void SetConfig(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
            config.AppSettings.Settings.Remove(key);
            config.AppSettings.Settings.Add(key,value);
            config.Save(ConfigurationSaveMode.Minimal);
        }


        public static void TryGetConfig(string key, Action<string> onValue)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var value =  config.AppSettings.Settings[key];
            if (value != null)
            {
                onValue(value.Value);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AllocConsole();

            var mockMainModel = Mockdata.StartMainModel();
            mockMainModel = CodeAnalyser.CSharpToFlowDesign.FromFile(""); // For fabian so it runs his method and shows the result on startup
            var mainviewModel = MainViewModel.Instance();
            mainviewModel.LoadFromModel(mockMainModel);
            var mainwindow = new MainWindow(mainviewModel);

            LoadLastUsedTheme();

            mainwindow.Show();

            App.Current.DispatcherUnhandledException += AppOnDispatcherUnhandledException;
        }


        private static void LoadLastUsedTheme()
        {
            App.TryGetConfig("Theme", configname =>
            {
                if (configname == "Print")
                {
                    MainViewModel.Instance().ChangeTheme("Views/Themes/Print.xaml", @"Views/Themes/FlowDesignColorPrint.xshd");
                }
            });
        }


        private void AppOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            args.Handled = true;
            var inputDialog = new UnkownErrorDialog(args.Exception.ToString());
            if (inputDialog.ShowDialog() == true)
                App.Current.Shutdown();
        }


      
    }
}
