﻿#pragma checksum "C:\Users\David\Documents\Visual Studio 2010\Projects\FirstWorldProblems\FirstWorldProblems\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "92B8524C5F7C3DE7929C4596E2F28EB2"
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
    
    
    public partial class MainPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Image imgHeader;
        
        internal System.Windows.Controls.Button AllJokes;
        
        internal System.Windows.Controls.Button FavoriteJokes;
        
        internal System.Windows.Controls.Button FilterByCategory;
        
        internal System.Windows.Controls.Button AboutPage;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/FirstWorldProblems;component/MainPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.imgHeader = ((System.Windows.Controls.Image)(this.FindName("imgHeader")));
            this.AllJokes = ((System.Windows.Controls.Button)(this.FindName("AllJokes")));
            this.FavoriteJokes = ((System.Windows.Controls.Button)(this.FindName("FavoriteJokes")));
            this.FilterByCategory = ((System.Windows.Controls.Button)(this.FindName("FilterByCategory")));
            this.AboutPage = ((System.Windows.Controls.Button)(this.FindName("AboutPage")));
        }
    }
}

