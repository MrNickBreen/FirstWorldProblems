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
//Review App
using Microsoft.Phone.Tasks;

namespace FirstWorldProblems
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            DataContext = App.ViewModel;
            InitializeComponent();
            
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            //Contact our main view model and update wether the user permitts us to access the internet for data
            App.ViewModel.UserPermittedAppToConnectToInternet = ((System.Windows.Controls.CheckBox)sender).IsChecked == false ? false : true;
            
            //This handles an edge case. I set the JokePageType to Reset jokes because if the user allows access to the internet there may be new jokes to display.
            //If I don't reset the JokePageType and the user selects the last viewed joke page, he will not see the new joke data.
            //Example: if user's setting doesn't allow to connect to internet, user checks all jokes, sees that no jokes are available. User
            //activates internet by changing setting, user navigates back to all jokes and doesn't see new data because JokePageType is still set to
            //AllJokes, therefore there is no need to update. Now there will be a reason to update since ResetJokes is set, instead of AllJokes.
            App.ViewModel.JokePageType = FirstWorldProblems.MainViewModel.PageType.ResetJokes;
        }

        private void RateThisAppButton_Click(object sender, RoutedEventArgs e)
        {
            //DRS & TODO: Uncomment on the first update to the app marketplace (It doesn't work because the app is not not live on marketplace)
            //MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            //marketplaceReviewTask.Show();
        }
    }
}