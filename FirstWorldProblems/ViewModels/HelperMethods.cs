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

namespace FirstWorldProblems
{
    public class HelperMethods
    {
        protected IsolatedStorageSettings isolatedStorageSettings = IsolatedStorageSettings.ApplicationSettings;

        //propertyType options are: lastJokeUpdate or lastCategoryUpdate
        protected String getLastPropertyUpdateDatetime(string propertyName)
        {
            if (isolatedStorageSettings.Contains(propertyName) == false)
            {
                isolatedStorageSettings.Add(propertyName, "");
                isolatedStorageSettings.Save();
                return "";
            }
            else
            {
                return isolatedStorageSettings[propertyName].ToString();
            }
        }

        //code Credit: http://www.windowsphonegeek.com/tips/all-about-wp7-isolated-storage-store-data-in-isolatedstoragesettings
        //propertyType options are: lastJokeUpdate or lastCategoryUpdate
        protected void updateLastPropertyUpdatedTime(string dataFromDatabase, string propertyType)
        {
            //To find find the newest entry we will use a very crude method of searching for the first index of the columnName 'dateAdded'
            //in the data we recieved from the database, which is ordered newest to oldest.
            int positionOfBegginingOfDate = dataFromDatabase.IndexOf(",\"dateAdded\":") + 14;
            int positionOfEndOfDate = dataFromDatabase.IndexOf("\"", positionOfBegginingOfDate);
            string newestDateFromProperty = dataFromDatabase.Substring(positionOfBegginingOfDate, positionOfEndOfDate - positionOfBegginingOfDate);

            //Add newestDateFromJokes to isolated storage settings
            if (isolatedStorageSettings.Contains(propertyType) == false)
            {
                //this is a sanity check, when we call the query we should have already created lastJokeUpdate in the settings.
                isolatedStorageSettings.Add(propertyType, newestDateFromProperty);
            }
            else
            {
                isolatedStorageSettings[propertyType] = newestDateFromProperty;
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
