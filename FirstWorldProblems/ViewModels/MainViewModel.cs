using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

//For JSON deserialization to object
using Newtonsoft.Json.Linq;

//For XML serialization to object
using System.Xml.Linq;

//Isolated Storage
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Resources;

namespace FirstWorldProblems
{
    public class MainViewModel : HelperMethods
    {
        private const string FileName = "JokesInJSONFormat.txt";
        private const string FolderName = "IsolatedStorage";
        private string FilePath = System.IO.Path.Combine(FolderName, FileName);

        private bool _userPermittedAppToConnectToInternet;

        public bool UserPermittedAppToConnectToInternet
        {
            get
            {
                return _userPermittedAppToConnectToInternet;
            }
            set
            {
                //If the old value was set to false and we now set it to true
                if (!_userPermittedAppToConnectToInternet && value)
                {
                    //Clear the error messages from all the pages
                    this.JokesToDisplay.Clear();
                    this.AllJokes.Clear();
                    this.categoryViewModel.AllCategories.Clear();
                    this.categoryViewModel.CategoriesToDisplay.Clear();
                    this.MessageToDisplay = "";
                    this.categoryViewModel.MessageToDisplay = "";

                    //If data hasn't loaded load it now (send request to database for more information)
                    if (!this.categoryViewModel.IsDataLoaded)
                    {
                        this.categoryViewModel.LoadData();
                    }

                    if (!this.IsDataLoaded)
                    {
                        this.LoadData();
                    }
                }
                _userPermittedAppToConnectToInternet = value;
                updateIsolatedStorageProperty(value.ToString(), IsolatedStorageSettingsProperties.userPermittedAppToConnectToInternet);
            }
        } 

        public enum PageType
        {
            AllJokes = 1,
            Favorites = 2,
            FilteredCategoryJokes = 3,
            ResetJokes = 4
        }

        public CategoryViewModel categoryViewModel;

        /// <summary>
        /// Fairly certain we don't use this property
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;        
       
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainViewModel()
        {
            //This must be set to true or else we have an infinite loop on app start because of the handling of an edge case if we start with no internet and regain a connection.
            _userPermittedAppToConnectToInternet = true;

            this.AllJokes = new ObservableCollection<Joke>();
            this.JokesToDisplay = new ObservableCollection<Joke>();
            this.categoryViewModel = new CategoryViewModel();

            //Checks wether the user has permitted us to access the internet to get more jokes
            UserPermittedAppToConnectToInternet = bool.Parse(getIsolatedStorageProperty(IsolatedStorageSettingsProperties.userPermittedAppToConnectToInternet));
        }

        /// <summary>
        /// A collection of all joke objects we have (stored as a variable so we don't have to access isolated storage every time we want to filter)
        /// </summary>
        public ObservableCollection<Joke> AllJokes { get; private set; }

        /// <summary>
        /// A collection for joke objects. This could be a subset of AllJokes
        /// </summary>
        public ObservableCollection<Joke> JokesToDisplay { get; private set; }

        
        private string _messageToDisplay;

        /// <summary>
        /// This is only used to display a message for the user to read. 
        /// </summary>
        protected string MessageToDisplay
        {
            get
            {
                return _messageToDisplay;
            }
            set
            {
                _messageToDisplay = value;
            }
        }

        private PageType _jokePageType;

        /// <summary>
        /// This property is used to determine what joke page to display.
        /// </summary>
        /// <returns></returns>
        public PageType JokePageType
        {
            get
            {
                return _jokePageType;
            }
            set
            {
                if (value != _jokePageType)
                {
                    _jokePageType = value;

                    //Reload the data to be shown in the pivot since the user wants to see less/more information from the last time the joke page was loaded
                    loadJokesToDisplay();
                }
            }
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            string lastJokeUpdateTime = getIsolatedStorageProperty(IsolatedStorageSettingsProperties.lastJokeUpdate);
            //When we put data in isolated storage we record the datetime of the most recent joke. If we don't have this setting this means we don't have any
            //jokes in isolated storage.
            if (lastJokeUpdateTime != "")
            {
                LoadDataFromIsolatedStorage();
                //We just loaded data from the isolated storage, therefore any message to display (current version just warning messages) is not relevent anymore. 
                this.MessageToDisplay = "";
            }

            if (HaveUseableInternetConnection() != false)
            {
                this.IsDataLoaded = true;
                //We just loaded data from the isolated storage, therefore any message to display (current version just warning messages) are not relevent anymore.
                this.MessageToDisplay = "";

                //Sending the request to the database to get new jokes.
                WebClient SMEWebClient = new WebClient();
                SMEWebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(SMEWebClient_DownloadStringCompleted);
                SMEWebClient.DownloadStringAsync(new Uri("http://www.smewebsites.com/playground/FWP/get_jokes.php?lastUpdate=" + lastJokeUpdateTime));
            }
            else
            {
                //We don't have any internet connection.
                if (lastJokeUpdateTime == "")
                {
                    //We don't have any cached data. Therefore we need to display a warning message.
                    //However, if there is already a joke in the JokesToDisplay we know its a warning message for the user.
                    if (this.JokesToDisplay.Count != 1)
                    {
                        //We don't have access to the internet and nothing is stored in cache. We will let the user know they need an internet connection
                        //DRS
                        if (App.ViewModel.UserPermittedAppToConnectToInternet)
                        {
                            this.MessageToDisplay = "Please connect to the internet.";
                        }
                        else
                        {
                            this.MessageToDisplay = "Go to the about page, allow the app to access the Internet.";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A joke was just favorited or un-favorited. Updates cache accordingly.
        /// </summary>
        /// <param name="favoriteStatus"></param>
        /// <param name="jokeID"></param>
        public void FavoriteJokeUpdate(bool favoriteStatus, int jokeID)
        {
            EditObjectAttribute(FilePath, (favoriteStatus ? "1" : "0"), jokeID, "favorite", "id");
        }

        /// <summary>
        /// This method reads in the (cached) JSON joke data from isolated storage and loads it into our pivot.
        /// </summary>
        private void LoadDataFromIsolatedStorage()
        {
            string jokesString = ReadFile(FilePath);
            LoadJokesIntoPivot(jokesString);         
        }

        /// <summary>
        /// Parses the joke string into a JArray and adds the jokes to the AllJokes collection.
        /// </summary>
        /// <param name="jsonJokeString"></param>
        private void LoadJokesIntoPivot(string jsonJokeString)
        {
            JArray jokesJArray = JArray.Parse(jsonJokeString);

            foreach (JObject joke in jokesJArray)
            {
                Joke currentJokeToAdd = new Joke() { CategoryID = (int.Parse(joke["categoryID"].ToString())), Favorite = (joke["favorite"].ToString()=="0" ? false : true), Author = joke["author"].ToString(), Charity = joke["charity"].ToString(), CharityURL = joke["charityURL"].ToString(), DateAdded = DateTime.Parse(joke["dateAdded"].ToString()), JokeID = int.Parse(joke["id"].ToString()), JokeText = joke["joke"].ToString(), Statistic = joke["statistic"].ToString(), StatisticURL = joke["statisticURL"].ToString() };
                             
                this.AllJokes.Add(currentJokeToAdd);
            }

            this.IsDataLoaded = true;
        }

        /// <summary>
        /// Loads the proper subset of jokes into the jokesToDisplay collection
        /// </summary>
        private void loadJokesToDisplay()
        {
            this.JokesToDisplay.Clear();
       
            if (JokePageType == PageType.Favorites)
            {
                foreach (Joke joke in this.AllJokes)
                {
                    if (joke.Favorite)
                    {
                        this.JokesToDisplay.Add(joke);
                    }
                }
                if (this.JokesToDisplay.Count == 0)
                {
                    //DRS
                    this.JokesToDisplay.Add(new Joke() { JokeText = "There are no favorited jokes. Press back, try something else.", Favorite=false});
                }
            }
            else if (JokePageType == PageType.FilteredCategoryJokes)
            {
                foreach (Joke joke in this.AllJokes)
                {
                    if (categoryViewModel.CategoriesToDisplay.Contains(joke.CategoryID))
                    {
                        this.JokesToDisplay.Add(joke);
                    }
                }
                if (this.JokesToDisplay.Count == 0)
                {
                    this.JokesToDisplay.Add(new Joke() { JokeText = "There are no filtered jokes. Press back, try something else.", Favorite=false });
                }
            }
            else
            {
                foreach (Joke joke in this.AllJokes)
                {
                    this.JokesToDisplay.Add(joke);
                }
            }

            //If we have a message to display and there are no jokes to display we should display the message in the joke pivot as the only item. 
            //We display the message in the joke pivot because there is not enough room to have space for a warning dialog box that is used so infrequently.
            if (!(this.MessageToDisplay.Equals("")) && this.JokesToDisplay.Count == 0)
            {
                this.JokesToDisplay.Add(new Joke() { JokeText = this.MessageToDisplay, Favorite = false });
            }
        }

        /// <summary>
        /// Processing the response from the database. It will return a JSON object. Using JSON.net to deserlize the object.
        /// </summary>
        /// <param name="sender">Webclient in the constructor</param>
        /// <param name="e"></param>
        private void SMEWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            //If we have an error 
            if (e.Error != null)
            {
                return;
            }
            
            //If we have new jokes from the database we will update our Isolated Storage cache
            if (e.Result.ToString() != "[]")
            {
                string newestJokes = (e.Result).ToString();

                updateLastPropertyUpdatedTime(newestJokes, IsolatedStorageSettingsProperties.lastJokeUpdate);

                //The e.result contains only new items we don't have in our cache,  add these items into our isolated storage.
                AddNewObjectsToIsolatedStorage(newestJokes, FilePath);

                LoadJokesIntoPivot(newestJokes);

                this.IsDataLoaded = true;
            }
        }
    }
}