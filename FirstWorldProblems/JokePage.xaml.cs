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


            //Update local copies favorite data
            App.ViewModel.JokesToDisplay[this.jokesPivot.SelectedIndex].Favorite = currentJokeFavoriteStatus;
            
            //Call MainViewModel to update the cache
            App.ViewModel.FavoriteJokeUpdate(currentJokeFavoriteStatus, int.Parse((((Joke)(this.jokesPivot.SelectedItem)).JokeID).ToString()));
        }

        //On pivot item loading the favorite icon must accurately represent if the joke is favorited or not
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