﻿#pragma checksum "C:\Users\David\Documents\Visual Studio 2010\Projects\FirstWorldProblems\FirstWorldProblems\JokePage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "AA4D7E7FC1EC8164B30A7B7F382AEB17"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace FirstWorldProblems {
    
    
    public partial class JokePage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal Microsoft.Phone.Controls.Pivot jokesPivot;
        
        internal System.Windows.Controls.Grid grid1;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton backButton;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton favoriteButton;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton forwardButtton;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/FirstWorldProblems;component/JokePage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.jokesPivot = ((Microsoft.Phone.Controls.Pivot)(this.FindName("jokesPivot")));
            this.grid1 = ((System.Windows.Controls.Grid)(this.FindName("grid1")));
            this.backButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("backButton")));
            this.favoriteButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("favoriteButton")));
            this.forwardButtton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("forwardButtton")));
        }
    }
}

