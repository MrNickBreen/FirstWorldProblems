using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;

//For JSON deserialization to object
using Newtonsoft.Json.Linq;


namespace FirstWorldProblems
{
    public class CategoryViewModel : HelperMethods
    {
        private const string FileName = "CategoriesInJSONFormat.txt";
        private const string FolderName = "IsolatedStorage";
        private string FilePath = System.IO.Path.Combine(FolderName, FileName);

        /// <summary>
        /// A collection of all category objects we have (stored as a variable so we don't have to access isolated storage every time we want to filter)
        /// </summary>
        public ObservableCollection<Category> AllCategories { get; private set; }

        /// <summary>
        /// A collection for category IDs to be displayed. This is an integer List because it is easier to search through, List has a contains method.
        /// </summary>
        public List<int> CategoriesToDisplay { get; private set; }

        private string _messageToDisplay;

        /// <summary>
        /// This is only used to display a message for the user to read. Currently, it notifies the user if the app doesn't have access to the internet.
        /// </summary>
        public string MessageToDisplay
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

        public CategoryViewModel()
        {
            this.AllCategories = new ObservableCollection<Category>();
            this.CategoriesToDisplay = new List<int>();
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Loads category data from cache (isolated storage) and starts the request to access live data from the remote database.
        /// </summary>
        public void LoadData()
        {
            
            string lastCategoryUpdateTime = getIsolatedStorageProperty(IsolatedStorageSettingsProperties.lastCategoryUpdate);
            //When we put data in isolated storage we record the datetime of the most recent joke. If we don't have this setting this means we don't have any
            //jokes in isolated storage.
            if (lastCategoryUpdateTime != "")
            {
                LoadDataFromIsolatedStorage();
                //We just loaded data from the isolated storage, therefore any message to display (current version just warning messages) are not relevent anymore. 
                this.MessageToDisplay = "";
            }

            if (HaveUseableInternetConnection() != false)
            {
                this.IsDataLoaded = true;

                //Sending the request to the database to get new jokes.
                WebClient SMEWebClient = new WebClient();
                SMEWebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(SMEWebClient_DownloadStringCompleted);
                SMEWebClient.DownloadStringAsync(new Uri("http://www.smewebsites.com/playground/FWP/get_categories.php?lastUpdate=" + lastCategoryUpdateTime));
            }
            else
            {
                if (lastCategoryUpdateTime == "")
                {
                    //We don't have access to the internet and nothing is stored in cache. We will let the user know they need an internet connection
                    //DRS
                    if (App.ViewModel.UserPermittedAppToConnectToInternet)
                    {
                        MessageToDisplay = "Please connect to the internet and try again.";
                    }
                    else
                    {
                        MessageToDisplay = "Go to the about page, allow the app to access the Internet.";
                    }
                }
            }
        }

        /// <summary>
        /// Finds the index of the category with categoryID in the collection of AllCategories. Returns -1 if category doesn't exist. This shouldn't ever happen
        /// since elements are never removed or modified.
        /// </summary>
        /// <param name="categoryID"></param>
        /// <returns>index of category with categoryID</returns>
        private int findIndexOfCategory(int categoryID)
        {
            for (int i = 0; i <= AllCategories.Count; i++)
            {
                if (AllCategories[i].CategoryID == categoryID)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Update the data for a category in the cache and in local data. This information is used for filter the jokes based on category type
        /// </summary>
        /// <param name="filterStatus">1 represents show the jokes in this category, 0 represents don't show the jokes in this category</param>
        /// <param name="categoryID">the identifier for the category we want to edit</param>
        public void FilterCategoryUpdate(bool filterStatus, int categoryID)
        {
            if (filterStatus == false)
            {
                this.CategoriesToDisplay.Remove(categoryID);
            }
            else
            {
                this.CategoriesToDisplay.Add(categoryID);
            }

            AllCategories[findIndexOfCategory(categoryID)].ViewCategoryFilter = filterStatus;

            //Updating isolated stoarge cache
            EditObjectAttribute(FilePath, (filterStatus ? "1" : "0"), categoryID, "viewCategoryFilter", "categoryID");
        }

        /// <summary>
        /// This method reads in the (cached) JSON joke data from isolated storage and loads it into our pivot.
        /// </summary>
        private void LoadDataFromIsolatedStorage()
        {
            string categoryString = ReadFile(FilePath);
            LoadCategoriesIntoCollection(categoryString);
        }

        /// <summary>
        /// Parses the category string into a JArray and adds the categories to collection.
        /// </summary>
        /// <param name="jsonCategoryString">JSON representation of the category data</param>
        private void LoadCategoriesIntoCollection(string jsonCategoryString)
        {
            JArray categoryJArray = JArray.Parse(jsonCategoryString);

            foreach (JObject category in categoryJArray)
            {
                //The AllCategories collection is bound to the category list item
                this.AllCategories.Add(new Category() { CategoryID = (int.Parse(category["categoryID"].ToString())), ViewCategoryFilter = (category["viewCategoryFilter"].ToString() == "0" ? false : true), DateAdded = DateTime.Parse(category["dateAdded"].ToString()), CategoryText = category["categoryText"].ToString() });
                if (category["viewCategoryFilter"].ToString() == "1")
                {
                    this.CategoriesToDisplay.Add(int.Parse(category["categoryID"].ToString()));
                }
            }

            this.IsDataLoaded = true;
        }

        /// <summary>
        /// Processing the response from the database. It will return a JSON object. Using JSON.net to deserlize the object.
        /// </summary>
        /// <param name="sender">The sender is the webClient created in the constructor</param>
        /// <param name="e"></param>
        private void SMEWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            //If we have an error we will have attempted to load the data from the cache before this. If nothing is loaded we will display an internet error message
            if (e.Error != null)
            {
                return;
            }

            //If we have new jokes from the database we will update our Isolated Storage cache
            if (e.Result.ToString() != "[]")
            {
                string newestCategories = (e.Result).ToString();

                updateLastPropertyUpdatedTime(newestCategories, IsolatedStorageSettingsProperties.lastCategoryUpdate);

                //The e.result contains only new items we don't have in our cache, add these items into our isolated storage.
                AddNewObjectsToIsolatedStorage(newestCategories, FilePath);

                LoadCategoriesIntoCollection(newestCategories);

                this.IsDataLoaded = true;
            }
        }
    }
}
            