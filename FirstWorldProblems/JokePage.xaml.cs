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
//For JSON deserialization to object
using Newtonsoft.Json.Linq;

//For XML serialization to object
using System.Xml.Linq;

//Isolated Storage
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Resources;

//Access Button Class
using Microsoft.Phone.Shell;

namespace FirstWorldProblems
{
    public partial class JokePage : PhoneApplicationPage
    {

        private bool currentJokeFavoriteStatus = false;

        // Constructor
        public JokePage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(JokePage_Loaded);

            
        }

        // Load data for the ViewModel Items
        public void JokePage_Loaded(object sender, RoutedEventArgs e)
        {
            //Load the app button icons 
            DisableAppBarButtonsAndLabels();

        }

        /// <summary>
        /// disables app buttons and labels if there is only 1 joke displaying, therefore there are no jokes to rotate through. 
        /// code credit: http://www.diaryofaninja.com/blog/2011/07/05/solved-why-donrsquot-applicationbar-bindings-work-ndash-windows-phone-7-sdk#.TzbevRakSK0.twitter
        /// </summary>
        private void DisableAppBarButtonsAndLabels()
        {
            if (App.ViewModel.JokesToDisplay.Count < 2)
            {
                ApplicationBarIconButton forwardButtonLocal = ApplicationBar.Buttons[2] as ApplicationBarIconButton;
                if (forwardButtonLocal != null)
                {
                    forwardButtonLocal.IsEnabled = false;
                }

                ApplicationBarIconButton backButtonLocal = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                if (backButtonLocal != null)
                {
                    backButtonLocal.IsEnabled = false;
                }         
                
                //If we are displaying a message to the user explaning there are no data entries for the particular settings they have chose we need to disable the favorite button.
                if (App.ViewModel.JokesToDisplay.Count == 1 && App.ViewModel.JokesToDisplay[0].JokeID == 0 && App.ViewModel.JokesToDisplay[0].Statistic == null)
                {

                    ApplicationBarIconButton favoriteButtonLocal = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
                    if (favoriteButtonLocal != null)
                    {
                        favoriteButtonLocal.IsEnabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Notifies the ViewModel the user has opened up the jokePage. The user wants to see his favorited jokes, a filtered list of jokes based on the categories
        /// he has selcted, or all of the jokes. 
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (NavigationContext.QueryString["id"].ToString().Equals(MainViewModel.PageType.Favorites.ToString()))
            {
                App.ViewModel.JokePageType = MainViewModel.PageType.Favorites;
            }
            else if (NavigationContext.QueryString["id"].ToString().Equals(MainViewModel.PageType.FilteredCategoryJokes.ToString()))
            {
                //If the user presses back he probably wants to go to the main screen and not the category filter screen.
                this.NavigationService.RemoveBackEntry();
                App.ViewModel.JokePageType = MainViewModel.PageType.FilteredCategoryJokes;
            }
            else
            {
                App.ViewModel.JokePageType = MainViewModel.PageType.AllJokes;
            }
        }

        private void LastJokeButton_Click(object sender, EventArgs e)
        {
            if (this.jokesPivot.SelectedIndex != 0)
            {
                this.jokesPivot.SelectedIndex = this.jokesPivot.SelectedIndex - 1;
            }
            else
            {
                this.jokesPivot.SelectedIndex = this.jokesPivot.Items.Count - 1;
            }            
        }

        //Switch to the next joke.
        private void NextJokeButton_Click(object sender, EventArgs e)
        {
            if (this.jokesPivot.SelectedIndex != this.jokesPivot.Items.Count - 1)
            {
                this.jokesPivot.SelectedIndex = this.jokesPivot.SelectedIndex + 1;
            }
            else
            {
                this.jokesPivot.SelectedIndex = 0;
            }
        }

        private void FavoriteJokeButton_Click(object sender, EventArgs e)
        {
            currentJokeFavoriteStatus = !currentJokeFavoriteStatus;

            //Update local copies favorite data
            App.ViewModel.JokesToDisplay[this.jokesPivot.SelectedIndex].Favorite = currentJokeFavoriteStatus;

            //Call MainViewModel to update the cache
            App.ViewModel.FavoriteJokeUpdate(currentJokeFavoriteStatus, int.Parse((((Joke)(this.jokesPivot.SelectedItem)).JokeID).ToString()));

            //If we are clicking the favorite button when we are in the favorited joke section it must mean the user wants to remove a joke from their favorite list
            if (App.ViewModel.JokePageType == MainViewModel.PageType.Favorites)
            {
                //If we only have one joke we are removing the last joke from our favorite list
                if (App.ViewModel.JokesToDisplay.Count == 1)
                {
                    App.ViewModel.JokesToDisplay.Add(new Joke() { JokeText = "There are no favorited jokes. Press back, try something else.", Favorite = false });
                }
                //Remove the entry from the current pivoted items if we unfavorite it
                App.ViewModel.JokesToDisplay.RemoveAt(App.ViewModel.JokesToDisplay.IndexOf((Joke)this.jokesPivot.SelectedItem));

                if (App.ViewModel.JokesToDisplay.Count == 1)
                {
                    DisableAppBarButtonsAndLabels();
                }

                //I reset this property because the user's currentPivotItem is another favorited pivot item
                currentJokeFavoriteStatus = !currentJokeFavoriteStatus; 
            }
            else
            {
                //Change application bar favorite button image
                if (currentJokeFavoriteStatus)
                {
                    //Load favorite image
                    ((ApplicationBarIconButton)sender).IconUri = new Uri("/Images/appbar.favs.rest.png", UriKind.Relative);
                }
                else
                {
                    //Load not favorite image
                    ((ApplicationBarIconButton)sender).IconUri = new Uri("/Images/appbar.favs.addto.rest.png", UriKind.Relative);
                }
            }
        }

        /// <summary>
        /// When a new pivot item is loading this method confirms that the favorite icon accurately represents if the joke is favorited or not
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jokesPivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            bool oldFavoriteStatus = currentJokeFavoriteStatus;
            currentJokeFavoriteStatus = ((Joke)(this.jokesPivot.SelectedItem)).Favorite;
            if (oldFavoriteStatus!=currentJokeFavoriteStatus && currentJokeFavoriteStatus==true)
            {
                //If we add or remove any buttons from the application bar we will have to update this code
                ((ApplicationBarIconButton)this.ApplicationBar.Buttons[1]).IconUri = new Uri("/Images/appbar.favs.rest.png", UriKind.Relative);
            }
            else if(oldFavoriteStatus!=currentJokeFavoriteStatus && currentJokeFavoriteStatus==false)
            {
                ((ApplicationBarIconButton)this.ApplicationBar.Buttons[1]).IconUri = new Uri("/Images/appbar.favs.addto.rest.png", UriKind.Relative);
            }
        }
    }
}