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
        /// A collection for category objects to be displayed.(this could be a subset of AllJokes)
        /// </summary>
        public ObservableCollection<Category> CategoriesToDisplay { get; private set; }

        public CategoryViewModel()
        {
            this.AllCategories = new ObservableCollection<Category>();
            this.CategoriesToDisplay = new ObservableCollection<Category>();
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
            //Sample data to test the page to see if it displays correctly
            this.CategoriesToDisplay.Add(new Category() { CategoryID = 21, ViewCategoryFilter = false, DateAdded = new DateTime(2010,12,12), CategoryText = "testing1" });
            this.CategoriesToDisplay.Add(new Category() { CategoryID = 41, ViewCategoryFilter = true, DateAdded = new DateTime(2012, 12, 12), CategoryText = "testing2" });
            this.CategoriesToDisplay.Add(new Category() { CategoryID = 51, ViewCategoryFilter = false, DateAdded = new DateTime(2014, 12, 12), CategoryText = "testing3" });


            string lastCategoryUpdateTime = getLastPropertyUpdateDatetime("lastCategoryUpdate");
            //When we put data in isolated storage we record the datetime of the most recent joke. If we don't have this setting this means we don't have any
            //jokes in isolated storage.
            if (lastCategoryUpdateTime != "")
            {
                LoadDataFromIsolatedStorage();
            }

            this.IsDataLoaded = true;

            //Sending the request to the database to get new jokes.
            WebClient SMEWebClient = new WebClient();
            SMEWebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(SMEWebClient_DownloadStringCompleted);
            SMEWebClient.DownloadStringAsync(new Uri("http://www.smewebsites.com/playground/FWP/get_categories.php?lastUpdate=" + lastCategoryUpdateTime));
        }

        /// <summary>
        /// Update the data for a category in the cache. This information is used for filter the jokes based on category type
        /// </summary>
        /// <param name="filterStatus">1 represents show the jokes in this category, 0 represents don't show the jokes in this category</param>
        /// <param name="categoryID">the identifier for the category we want to edit</param>
        public void FilterCategoryUpdate(bool filterStatus, int categoryID)
        {
            EditObjectAttribute(FilePath, (filterStatus ? "1" : "0"), categoryID, "viewCategoryFilter", "categoryID");
        }

        //This method reads in the (cached) JSON joke data from isolated storage and loads it into our pivot.
        private void LoadDataFromIsolatedStorage()
        {
            string categoryString = ReadFile(FilePath);
            LoadCategoriesIntoCollection(categoryString);
        }

        //Parses the category string into a JArray and adds the categories to collection.
        private void LoadCategoriesIntoCollection(string jsonCategoryString)
        {
            JArray categoryJArray = JArray.Parse(jsonCategoryString);

            foreach (JObject category in categoryJArray)
            {
                //This collection is bound to the pivot item                
                this.CategoriesToDisplay.Add(new Category() { CategoryID = (int.Parse(category["categoryID"].ToString())), ViewCategoryFilter = (category["viewCategoryFilter"].ToString() == "0" ? false : true), DateAdded = DateTime.Parse(category["dateAdded"].ToString()), CategoryText = category["categoryText"].ToString() });
            }

            //TODO: add code for the filtering options.
            this.AllCategories = this.CategoriesToDisplay;

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
                string newestCategories = (e.Result).ToString();

                updateLastPropertyUpdatedTime(newestCategories, "lastCategoryUpdate");

                //The e.result contains only new items we don't have in our cache,  add these items into our isolated storage.
                AddNewObjectsToIsolatedStorage(newestCategories, FilePath);

                LoadCategoriesIntoCollection(newestCategories);

                this.IsDataLoaded = true;
            }
        }
    }
}
            