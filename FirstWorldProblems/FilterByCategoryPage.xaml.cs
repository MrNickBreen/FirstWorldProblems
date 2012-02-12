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
    public partial class FilterByCategoryPage : PhoneApplicationPage
    {
        public FilterByCategoryPage()
        {
             // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel.categoryViewModel;
            this.Loaded += new RoutedEventHandler(FilterByCategoryPage_Loaded);
            InitializeComponent();
        }

        // Load data for the ViewModel Items
        private void FilterByCategoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        private void categoryListBox_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// The checkbox has been checked, update local variable and send the data to the cache to update what categories to display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            App.ViewModel.categoryViewModel.FilterCategoryUpdate((((System.Windows.Controls.CheckBox)sender).IsChecked == false ? false : true), int.Parse(((System.Windows.Controls.CheckBox)sender).Tag.ToString()));
            
            App.ViewModel.JokePageType = MainViewModel.PageType.AllJokes;
        }

        private void FilteredCategoryJokes_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/JokePage.xaml?id=" + MainViewModel.PageType.FilteredCategoryJokes, UriKind.Relative));
        }
    }
}