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
             // Set the data context of the listbox control to the category data
            DataContext = App.ViewModel.categoryViewModel;
            this.Loaded += new RoutedEventHandler(FilterByCategoryPage_Loaded);
            InitializeComponent();
        }

        /// <summary>
        /// Load data for the ViewModel Items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterByCategoryPage_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void categoryListBox_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// A category checkbox has been checked, update local variable and send the data to the cache to update what categories to display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            App.ViewModel.categoryViewModel.FilterCategoryUpdate(((bool)((System.Windows.Controls.CheckBox)sender).IsChecked), int.Parse(((System.Windows.Controls.CheckBox)sender).Tag.ToString()));
            
            //I reset the jokePageType because the joke data will not update in the following scenario. If the user filters jokes, presses back and applys a new filter and views those jokes. 
            //It will not update the new filtered joke data since the PageType would be the same (filterCategoryJokes). Setting the PageType to ResetJokes 
            //allows the page to update with the newest Category data if the users chooses to view the filtered jokes.
            App.ViewModel.JokePageType = MainViewModel.PageType.ResetJokes;
        }

        /// <summary>
        /// Load filtered jokes button has been pressed. Load the jokes page to display certain categories.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilteredCategoryJokes_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/JokePage.xaml?id=" + MainViewModel.PageType.FilteredCategoryJokes, UriKind.Relative));
        }
    }
}