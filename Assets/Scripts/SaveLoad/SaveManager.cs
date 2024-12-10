using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SaveLoad
{
    public static class SaveManager
    {
        private static readonly string SaveFolder = Application.persistentDataPath + "/Save";

        public static void Delete(string profileName)
        {
            if(!File.Exists(SaveFolder + "/" + profileName + ".json"))
                throw new Exception($"Profile {profileName} does not exist");
            
            File.Delete(SaveFolder + "/" + profileName + ".json");
            Debug.Log($"Successfully deleted profile {profileName}");
        }
        
        public static SaveProfile<T> Load<T>(string profileName) where T : SaveProfileData, new()
        {
            var filePath = SaveFolder + "/" + profileName + ".json";
            
            if (!File.Exists(filePath))
            {
                // Save file does not exist, create one with default values
                Debug.Log($"Save file for profile {profileName} does not exist. Creating a new one with default values.");

                // Create a default save profile
                var defaultProfile = new SaveProfile<T>
                {
                    profileName = profileName,
                    saveData = new T() // Assuming T has a parameterless constructor for default values
                };

                Save(defaultProfile); // Save the default profile to create the file
                return defaultProfile; // Return the default profile
            }
            
            var fileContents = File.ReadAllText(SaveFolder + "/" + profileName + ".json");

            //Decrypt
            
            Debug.Log($"Successfully loaded profile {profileName}");
            
            return JsonConvert.DeserializeObject<SaveProfile<T>>(fileContents);
        }

        public static void Save<T>(SaveProfile<T> save) where T : SaveProfileData
        {
            var filePath = SaveFolder + "/" + save.profileName + ".json";
            
            var json = JsonConvert.SerializeObject(save, formatting: Formatting.Indented,
                new JsonSerializerSettings{ ReferenceLoopHandling = ReferenceLoopHandling.Ignore}
                );
            
            //Encrypt

            if (!Directory.Exists(SaveFolder))
            {
                Directory.CreateDirectory(SaveFolder);
            }
            
            File.WriteAllText(filePath, json);
        }
    }
}
