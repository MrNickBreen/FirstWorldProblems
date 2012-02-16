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
using System.IO.IsolatedStorage;
using System.ComponentModel;
using System.Windows.Resources;
using System.IO;

//Network information
using System.Net.NetworkInformation;

namespace FirstWorldProblems
{
    public class HelperMethods
    {

        //Note: Do not create any global variables that can be changed in the helperMethod, even if both MainViewModel and CategoryViewModel need to both use said
        //variable. Since mainViewModel and CategoryViewModel are both inherited from HelperMethods class, each model will have their own instance of said variable.
        //Which means there may be two seperate values (this is not good).

        protected enum IsolatedStorageSettingsProperties
        {
            lastJokeUpdate = 1,
            lastCategoryUpdate = 2,
            userPermittedAppToConnectToInternet = 3
        }

        protected IsolatedStorageSettings isolatedStorageSettings = IsolatedStorageSettings.ApplicationSettings;
       
        /// <summary>
        /// Returns true if the app can access the internet, false otherwise.
        /// </summary>
        /// <returns></returns>
        protected bool HaveUseableInternetConnection()
        {
            if (NetworkInterface.GetIsNetworkAvailable() == false || App.ViewModel.UserPermittedAppToConnectToInternet == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// returns the value of a specific IsolatedStorage property. PropertyType options are: lastJokeUpdate or lastCategoryUpdate or userPermittedAppToConnectToInternet
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected String getIsolatedStorageProperty(IsolatedStorageSettingsProperties propertyName)
        {
            String propertyNameString = propertyName.ToString();
            if (isolatedStorageSettings.Contains(propertyName.ToString()) == false)
            {
                if (propertyName == IsolatedStorageSettingsProperties.userPermittedAppToConnectToInternet)
                {
                    isolatedStorageSettings.Add(propertyName.ToString(), true);
                    isolatedStorageSettings.Save();
                    return "true";
                }
                else
                {
                    isolatedStorageSettings.Add(propertyName.ToString(), "");
                    isolatedStorageSettings.Save();
                    return "";
                }
            }
            else
            {
                return isolatedStorageSettings[propertyName.ToString()].ToString();
            }
        }

       
       
        /// <summary>
        /// Specific method for updating the dateAdded for categories or jokes.
        /// code Credit: http://www.windowsphonegeek.com/tips/all-about-wp7-isolated-storage-store-data-in-isolatedstoragesettings
        /// </summary>
        /// <param name="dataFromDatabase">New update value</param>
        /// <param name="propertyType">the property we are updating.</param>
        protected void updateLastPropertyUpdatedTime(string dataFromDatabase, IsolatedStorageSettingsProperties propertyType)
        {
            //To find find the newest entry we will use a very crude method of searching for the first index of the columnName 'dateAdded'
            //in the data we recieved from the database, which is ordered newest to oldest.
            int positionOfBegginingOfDate = dataFromDatabase.IndexOf(",\"dateAdded\":") + 14;
            int positionOfEndOfDate = dataFromDatabase.IndexOf("\"", positionOfBegginingOfDate);
            string newestDateFromProperty = dataFromDatabase.Substring(positionOfBegginingOfDate, positionOfEndOfDate - positionOfBegginingOfDate);

            updateIsolatedStorageProperty(newestDateFromProperty, propertyType);
        }

        /// <summary>
        /// General method to update settings in isolated storage.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propertyType"></param>
        protected void updateIsolatedStorageProperty(string value, IsolatedStorageSettingsProperties propertyType)
        {
            //Add newestDateFromJokes to isolated storage settings
            if (isolatedStorageSettings.Contains(propertyType.ToString()) == false)
            {
                //this is a sanity check, when we call the query we should have already created lastJokeUpdate in the settings.
                isolatedStorageSettings.Add(propertyType.ToString(), value);
            }
            else
            {
                isolatedStorageSettings[propertyType.ToString()] = value;
            }

            isolatedStorageSettings.Save();
        }

        /// <summary>
        /// Adds new joke or category objects to the isolated storage file.
        /// </summary>
        /// <param name="objectData"></param>
        /// <param name="filePath"></param>
        protected void AddNewObjectsToIsolatedStorage(string objectData, string filePath)
        {
            if (!(IsolatedStorageFile.GetUserStoreForApplication().FileExists(filePath)))
            {
                //Create a new file in isolated storage if it doesn't exist already
                CreateNewFile(filePath, objectData);
            }
            else
            {
                EditExistingFile(filePath, objectData);
            }

            isolatedStorageSettings.Save();
        }

        /// <summary>
        /// Create an isolate storage file with a JSON formatted list of joke objects or category objects. 
        /// //Isolated Storage code credit:http://www.windowsphonegeek.com/tips/All-about-WP7-Isolated-Storage---File-manipulations     
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="objectData"></param>
 
        protected void CreateNewFile(string filePath, string objectData)
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
                        writer.WriteLine(objectData);
                        writer.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Edit an existing isolate storage file by appending data at the end of the list of joke objects or category objects. The information is stored in JSON format.
        /// Isolated Storage code credit:http://www.windowsphonegeek.com/tips/All-about-WP7-Isolated-Storage---File-manipulations
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="insertedText"></param>
        protected void EditExistingFile(string filePath, string insertedText)
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
                            string someTextData = insertedText;
                            // get rid of the end of ] at the end of file and replace with , so the list will continue to work.       
                            writer.WriteLine("," + someTextData.Substring(1));
                            writer.Close();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds position of categories' or joke's attribute in the isolatedStorage's file and overwrittes with new value. Will not work if newAttributeValue is not the exact size of the current attribute's value.
        /// </summary>
        /// <param name="filePath">Where the isolated storage's file is stored</param>
        /// <param name="newAttributeValue"></param>
        /// <param name="objectID">CategoryID or jokeID</param>
        /// <param name="attribute">The name of the attribute we are updating</param>
        /// <param name="objectIDPropertyName">Category or joke</param>
        protected void EditObjectAttribute(string filePath, string newAttributeValue, int objectID, string attribute, string objectIDPropertyName)
        {
            String isolatedStorageContents = "";

            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (myIsolatedStorage.FileExists(filePath))
                {
                    using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            isolatedStorageContents = reader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    //TODO: Throw exception, the file doesn't exist
                }
            }

            //Find position of joke Attribute
            int indexOfJoke = isolatedStorageContents.IndexOf("\"" + objectIDPropertyName + "\":\"" + objectID + "\",");
            int indexOfJokeAttributeValue = isolatedStorageContents.IndexOf("\"" + attribute + "\":\"", indexOfJoke) + 4 + attribute.Length;

            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                //Open existing file
                if (myIsolatedStorage.FileExists(filePath))
                {
                    using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(filePath, FileMode.Open, FileAccess.Write))
                    {
                        fileStream.Seek(indexOfJokeAttributeValue, SeekOrigin.Begin);
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            writer.Write(newAttributeValue);
                            writer.Close();
                        }
                    }
                }
                else
                {
                    //TODO: Throw exception, the file doesn't exist
                }
            }
        }


        /// <summary>
        /// Reads all of the text in the isolated storage file. The file contains data about all jokes or the categories
        /// Isolated Storage code credit:http://www.windowsphonegeek.com/tips/All-about-WP7-Isolated-Storage---File-manipulations
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected String ReadFile(string filePath)
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
