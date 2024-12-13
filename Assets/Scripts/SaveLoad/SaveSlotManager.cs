using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SaveLoad
{
    public static class SaveSlotManager
    {
        private static readonly string SaveRootFolder = Application.persistentDataPath + "/Save";

        // Gets the folder path for a specific slot
        public static string GetSlotFolder(int slot)
        {
            return $"{SaveRootFolder}/Slot{slot}";
        }

        // Checks if a slot exists
        public static bool IsSlotAvailable(int slot)
        {
            var slotFolder = GetSlotFolder(slot);
            return Directory.Exists(slotFolder);
        }

        // Creates a folder for a specific slot if it doesn't already exist
        public static void EnsureSlotExists(int slot)
        {
            var slotFolder = GetSlotFolder(slot);
            if (!Directory.Exists(slotFolder))
            {
                Directory.CreateDirectory(slotFolder);
                Debug.Log($"Created folder for Slot {slot}: {slotFolder}");
            }
        }

        // Deletes a specific slot and all its contents
        public static void DeleteSlot(int slot)
        {
            var slotFolder = GetSlotFolder(slot);
            if (Directory.Exists(slotFolder))
            {
                Directory.Delete(slotFolder, true); // true to delete all contents recursively
                Debug.Log($"Deleted Slot {slot} at {slotFolder}");
            }
            else
            {
                Debug.LogWarning($"Slot {slot} does not exist.");
            }
        }

        // Lists all existing slots
        public static List<int> GetAvailableSlots()
        {
            if (!Directory.Exists(SaveRootFolder))
                return new List<int>();

            return Directory.GetDirectories(SaveRootFolder)
                .Select(folder =>
                {
                    var folderName = Path.GetFileName(folder);
                    return int.TryParse(folderName.Replace("Slot", ""), out var slot) ? slot : -1;
                })
                .Where(slot => slot >= 0)
                .ToList();
        }
        
    }
}
