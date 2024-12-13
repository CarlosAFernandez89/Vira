using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace SaveLoad
{
    public static class SaveManager
    {
        private static readonly string SaveFolder = Application.persistentDataPath + "/Save";
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("g5@D#s3kP1Q!mN7X"); // 16 characters
        private static readonly byte[] EncryptionIV = Encoding.UTF8.GetBytes("A1@2c#D$e!3FgH7!");  // 16 characters
        
        private static int _activeSaveSlot = 0;

        public static void SetActiveSaveSlot(int slot)
        {
            _activeSaveSlot = slot;
        }

        public static int GetActiveSaveSlot()
        {
            return _activeSaveSlot;
        }

        private static string GetFilePath(string profileName, int slot)
        {
            return $"{SaveFolder}/{profileName}_Slot{slot}.json";
        }

        public static void Delete(string profileName, int slot = 0)
        {
            var filePath = GetFilePath(profileName, slot);
            
            if(!File.Exists(SaveFolder + "/" + profileName + ".json"))
                throw new Exception($"Profile {profileName} does not exist");
            
            File.Delete(SaveFolder + "/" + profileName + ".json");
            Debug.Log($"Successfully deleted profile {profileName}");
        }
        
        public static SaveProfile<T> Load<T>(string profileName, int slot = 0) where T : SaveProfileData, new()
        {
            var slotFolder = SaveSlotManager.GetSlotFolder(slot);
            SaveSlotManager.EnsureSlotExists(slot);
            
            var filePath = $"{slotFolder}/{profileName}.json";
            
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
            
            var fileContents = File.ReadAllText(filePath);

            //Decrypt if not in editor
            if (!Application.isEditor)
            {
                fileContents = Decrypt(fileContents);
            }
            
            Debug.Log($"Successfully loaded profile {profileName}");
            
            return JsonConvert.DeserializeObject<SaveProfile<T>>(fileContents);
        }

        public static void Save<T>(SaveProfile<T> save, int slot = 0) where T : SaveProfileData
        {
            var slotFolder = SaveSlotManager.GetSlotFolder(slot);
            SaveSlotManager.EnsureSlotExists(slot);
            
            var filePath = $"{slotFolder}/{save.profileName}.json";
            
            var json = JsonConvert.SerializeObject(save, formatting: Formatting.Indented,
                new JsonSerializerSettings{ ReferenceLoopHandling = ReferenceLoopHandling.Ignore}
                );
            
            //Encrypt if not in editor.
            if (!Application.isEditor)
            {
                json = Encrypt(json);
            }

            if (!Directory.Exists(SaveFolder))
            {
                Directory.CreateDirectory(SaveFolder);
            }
            
            File.WriteAllText(filePath, json);
        }
        
        private static string Encrypt(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV = EncryptionIV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    var plainBytes = Encoding.UTF8.GetBytes(plainText);
                    var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }
        
        private static string Decrypt(string cipherText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV = EncryptionIV;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    var cipherBytes = Convert.FromBase64String(cipherText);
                    var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
    }
}
