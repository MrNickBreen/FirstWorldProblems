using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace FirstWorldProblems
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();

            if (!App.ViewModel.categoryViewModel.IsDataLoaded)
            {
                App.ViewModel.categoryViewModel.LoadData();
            }            

            if (!App.ViewModel.IsDataLoaded)
            {   
                App.ViewModel.LoadData();
            }
            
        }

        private void AllJokes_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/JokePage.xaml?id=" + MainViewModel.PageType.AllJokes, UriKind.Relative));
        }

        private void FavoriteJokes_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/JokePage.xaml?id=" + MainViewModel.PageType.Favorites, UriKind.Relative));
        }

        private void FilterByCategory_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/FilterByCategoryPage.xaml", UriKind.Relative));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

      
    }
}