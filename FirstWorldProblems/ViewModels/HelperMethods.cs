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

        //propertyType options are: lastJokeUpdate or lastCategoryUpdate or userPermittedAppToConnectToInternet
        protected String getIsolatedStorageProperty(IsolatedStorageSettingsProperties propertyName)
        {
            String propertyNameString = propertyName.ToString();
            if (isolatedStorageSettings.Contains(propertyName.ToString()) == false)
            {
                if (propertyName == IsolatedStorageSettingsProperties.userPermittedAppToConnectToInternet)
                {
                    //TODO: change default to true;
                    isolatedStorageSettings.Add(propertyName.ToString(), false);
                    isolatedStorageSettings.Save();
                    return "false";
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

       
        //code Credit: http://www.windowsphonegeek.com/tips/all-about-wp7-isolated-storage-store-data-in-isolatedstoragesettings
        /// <summary>
        /// Method specific to updating the dateAdded for categories or joke
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

        //Isolated Storage code credit:http://www.windowsphonegeek.com/tips/All-about-WP7-Isolated-Storage---File-manipulations      
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

        //Isolated Storage code credit:http://www.windowsphonegeek.com/tips/All-about-WP7-Isolated-Storage---File-manipulations
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

        //Finds position of categories' or joke's attribute in isolatedStorage and overwrittes with new value. Will not work if newAttributeValue is not the exact size of the current attribute's value.
        //TODO: write code such that the size of newAttributeValue does not matter. (When I implement the isolated storage database I will not need to)
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


        //Isolated Storage code credit:http://www.windowsphonegeek.com/tips/All-about-WP7-Isolated-Storage---File-manipulations
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
