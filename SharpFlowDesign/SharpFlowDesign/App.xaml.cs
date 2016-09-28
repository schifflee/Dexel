﻿using System.Runtime.InteropServices;
using System.Windows;
using SharpFlowDesign.Model;

namespace SharpFlowDesign
{

    /// <summary>
    ///     Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            AllocConsole();

            var splitter = Mockdata.RomanNumbers();
            SoftwareCell.PrintRecursive(splitter);

            var mainwindow = new MainWindow();
            mainwindow.Show();
        }
    }

}