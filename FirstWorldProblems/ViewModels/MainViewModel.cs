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
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string FileName = "JokesInJSONFormat.txt";
        private const string FolderName = "IsolatedStorage";
        private string FilePath = System.IO.Path.Combine(FolderName, FileName);
        private IsolatedStorageSettings isolatedStorageSettings = IsolatedStorageSettings.ApplicationSettings;

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
            this.AllJokes = new ObservableCollection<Joke>();
            this.JokesToDisplay = new ObservableCollection<Joke>();
        }

        /// <summary>
        /// A collection of all joke objects we have (stored as a variable so we don't have to access isolated storage every time we want to filter)
        /// </summary>
        public ObservableCollection<Joke> AllJokes { get; private set; }

        /// <summary>
        /// A collection for joke objects.(this could be a subset of AllJokes)
        /// </summary>
        public ObservableCollection<Joke> JokesToDisplay { get; private set; }

        private string _lastUpdated = "";
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding
        /// </summary>
        /// <returns></returns>
        public string LastUpdated
        {
            get
            {
                return _lastUpdated;
            }
            set
            {
                if (value != _lastUpdated)
                {
                    _lastUpdated = value;
                    NotifyPropertyChanged("LastUpdated");
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
            //When we put data in isolated storage we record the datetime of the most recent joke. If we don't have this setting this means we don't have any
            //jokes in isolated storage.
            if (getLastJokeUpdateDatetime() != "")
            {
                LoadDataFromIsolatedStorage();
            }
           
            this.IsDataLoaded = true;

            //Sending the request to the database to get new jokes.
            WebClient SMEWebClient = new WebClient();
            SMEWebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(SMEWebClient_DownloadStringCompleted);
            SMEWebClient.DownloadStringAsync(new Uri("http://www.smewebsites.com/playground/FWP/get_jokes.php?lastUpdate=" + getLastJokeUpdateDatetime()));
        }

        //This method reads in the (cached) JSON joke data from isolated storage and loads it into our pivot.
        private void LoadDataFromIsolatedStorage()
        {
            string jokesString = ReadFile(FilePath);
            LoadJokesIntoPivot(jokesString);         
        }

        //Parses the joke string into a JArray and adds the jokes to the JokesToDisplay collection.
        private void LoadJokesIntoPivot(string jsonJokeString)
        {
            JArray jokesJArray = JArray.Parse(jsonJokeString);

            foreach (JObject joke in jokesJArray)
            {
                //This collection is bound to the pivot item
                this.JokesToDisplay.Add(new Joke() { Author = joke["author"].ToString(), Charity = joke["charity"].ToString(), CharityURL = joke["charityURL"].ToString(), DateAdded = DateTime.Parse(joke["dateAdded"].ToString()), JokeID = int.Parse(joke["id"].ToString()), JokeText = joke["joke"].ToString(), Statistic = joke["statistic"].ToString(), StatisticURL = joke["statisticURL"].ToString() });
            }

            //TODO: add code for the filtering options.
            this.AllJokes = this.JokesToDisplay;

            this.IsDataLoaded = true;
        }

        //Processing the response from the database. It will return a JSON object. Using JSON.net to deserlize the object.
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

                updateLastJokeUpdatedTime(newestJokes);

                //The e.result contains only new items we don't have in our cache,  add these items into our isolated storage.
                AddNewJokesToIsolatedStorage(newestJokes);

                LoadJokesIntoPivot(newestJokes);

                this.IsDataLoaded = true;
            }
        }


        private String getLastJokeUpdateDatetime()
        {
            if (isolatedStorageSettings.Contains("lastJokeUpdate") == false)
            {
                isolatedStorageSettings.Add("lastJokeUpdate", "");
                isolatedStorageSettings.Save();
                return "";
            }
            else
            {
                return isolatedStorageSettings["lastJokeUpdate"].ToString();
            }
        }

        //code Credit: http://www.windowsphonegeek.com/tips/all-about-wp7-isolated-storage-store-data-in-isolatedstoragesettings
        private void updateLastJokeUpdatedTime(string dataFromDatabase)
        {
            //To find find the newest entry we will use a very crude method of searching for the first index of the columnName 'dateAdded'
            //in the data we recieved from the database, which is ordered newest to oldest.
            int positionOfBegginingOfDate = dataFromDatabase.IndexOf(",\"dateAdded\":") + 14;
            int positionOfEndOfDate = dataFromDatabase.IndexOf("\"", positionOfBegginingOfDate);
            string newestDateFromJokes = dataFromDatabase.Substring(positionOfBegginingOfDate, positionOfEndOfDate - positionOfBegginingOfDate);

            //Add newestDateFromJokes to isolated storage settings
            if (isolatedStorageSettings.Contains("lastJokeUpdate") == false)
            {
                //this is a sanity check, when we call the query we should have already created lastJokeUpdate in the settings.
                isolatedStorageSettings.Add("lastJokeUpdate", newestDateFromJokes);
            }
            else
            {
                isolatedStorageSettings["lastJokeUpdate"] = newestDateFromJokes;
            }

            isolatedStorageSettings.Save();
        }

        private void AddNewJokesToIsolatedStorage(string jokeData)
        {
            if (!(IsolatedStorageFile.GetUserStoreForApplication().FileExists(FilePath)))
            {
                //Create a new file in isolated storage if it doesn't exist already
                CreateNewFile(FilePath, jokeData);
            }
            else
            {
                EditExistingFile(FilePath, jokeData);
            }

            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        //Isolated Storage code credit:http://www.windowsphonegeek.com/tips/All-about-WP7-Isolated-Storage---File-manipulations      
        private void CreateNewFile(string filePath, string jokeData)
        {
            StreamResourceInfo streamResourceInfo = Application.GetResourceStream(new Uri(filePath, UriKind.Relative));

            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string directoryName = System.IO.Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directoryName) && !myIsolatedStorage.DirectoryExists(directoryName))
                {
                    myIsolatedStorage.CreateDirectory(directoryName);
                }

                using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine(jokeData);
                        writer.Close();
                    }
                }
            }
        }

        //Isolated Storage code credit:http://www.windowsphonegeek.com/tips/All-about-WP7-Isolated-Storage---File-manipulations
        private void EditExistingFile(string filePath, string insetedText)
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                //Open existing file
                if (myIsolatedStorage.FileExists(filePath))
                {
                    using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(filePath, FileMode.Open, FileAccess.Write))
                    {
                        fileStream.Seek(-3, SeekOrigin.End);
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            string someTextData = insetedText;
                            // get rid of the end of ] at the end of file and replace with , so the list will continue to work.       
                            writer.WriteLine("," + someTextData.Substring(1));
                            writer.Close();
                        }
                    }
                }
            }
        }

        //Isolated Storage code credit:http://www.windowsphonegeek.com/tips/All-about-WP7-Isolated-Storage---File-manipulations
        private String ReadFile(string filePath)
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (myIsolatedStorage.FileExists(filePath))
                {
                    using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            String text = reader.ReadToEnd();
                            return text;
                        }
                    }
                }
                else
                {
                    return "";
                }
            }
        }
    }
}