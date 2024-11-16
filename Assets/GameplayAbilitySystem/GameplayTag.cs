using System.Collections.Generic;
using UnityEngine;

namespace GameplayAbilitySystem
{
    public class TrieNode
    {
        public bool IsCompleteTag { get; set; }
        public Dictionary<string, TrieNode> Children { get; private set; }

        public TrieNode()
        {
            IsCompleteTag = false;
            Children = new Dictionary<string, TrieNode>();
        }
    }

    [System.Serializable]
    public class GameplayTag
    {
        private TrieNode _root = new TrieNode();
    
        // Add a tag, e.g., "Ability.Melee"
        public void AddTag(string tag)
        {
            var parts = tag.Split('.');
            TrieNode currentNode = _root;

            foreach (var part in parts)
            {
                if (!currentNode.Children.ContainsKey(part))
                {
                    currentNode.Children[part] = new TrieNode();
                }
                currentNode = currentNode.Children[part];
            }

            currentNode.IsCompleteTag = true;
        }
    
        // Removes a specific tag, e.g., "Ability.Melee"
        public void RemoveTag(string tag)
        {
            RemoveTagRecursive(_root, tag.Split('.'), 0);
        }
    
        private bool RemoveTagRecursive(TrieNode node, string[] parts, int index)
        {
            if (index == parts.Length)
            {
                // End of tag reached
                if (!node.IsCompleteTag)
                    return false; // Tag doesn't exist

                node.IsCompleteTag = false;
                return node.Children.Count == 0; // True if no children, meaning this node can be removed
            }

            var part = parts[index];
            if (!node.Children.ContainsKey(part))
                return false; // Tag part doesn't exist

            bool shouldRemoveChildNode = RemoveTagRecursive(node.Children[part], parts, index + 1);

            if (shouldRemoveChildNode)
            {
                node.Children.Remove(part);
                return node.Children.Count == 0 && !node.IsCompleteTag;
            }

            return false;
        }
    
        // Check if a tag exists, e.g., "Ability.Melee"
        public bool HasTag(string tag)
        {
            var parts = tag.Split('.');
            TrieNode currentNode = _root;

            foreach (var part in parts)
            {
                if (!currentNode.Children.ContainsKey(part))
                {
                    return false;
                }
                currentNode = currentNode.Children[part];
            }

            return currentNode.IsCompleteTag;
        }
    
        // Checks if any of the given tags exist in the trie
        public bool HasAnyTag(List<string> tags)
        {
            foreach (var tag in tags)
            {
                if (HasTag(tag))
                {
                    return true;
                }
            }
            return false;
        }
    
        // Checks if all of the given tags exist in the trie
        public bool HasAllTags(List<string> tags)
        {
            foreach (var tag in tags)
            {
                if (!HasTag(tag))
                {
                    return false;
                }
            }
            return true;
        }
    
        // Find tags by prefix, e.g., "Ability" to find "Ability.Melee", "Ability.Ranged"
        public List<string> GetTagsByPrefix(string prefix)
        {
            var results = new List<string>();
            var parts = prefix.Split('.');
            TrieNode currentNode = _root;

            foreach (var part in parts)
            {
                if (!currentNode.Children.ContainsKey(part))
                {
                    return results; // No tags with this prefix
                }
                currentNode = currentNode.Children[part];
            }

            // Recursively gather all tags from this point
            GatherTags(currentNode, prefix, results);
            return results;
        }
    
        private void GatherTags(TrieNode node, string currentPath, List<string> results)
        {
            if (node.IsCompleteTag)
            {
                results.Add(currentPath);
            }

            foreach (var child in node.Children)
            {
                GatherTags(child.Value, $"{currentPath}.{child.Key}", results);
            }
        }
    
        public List<string> GetAllTags()
        {
            List<string> tags = new List<string>();
            GatherTagsRecursive(_root, "", tags); // Use the existing method to gather tags
            return tags;
        }

        public void PrintAllTags()
        {
            List<string> tags = new List<string>();
            GatherTagsRecursive(_root, "", tags);

            string output = string.Join(", ", tags);
            Debug.Log("All Tags: " + output);
        }

        private void GatherTagsRecursive(TrieNode node, string currentPath, List<string> tags)
        {
            if (node.IsCompleteTag)
            {
                tags.Add(currentPath);
            }

            foreach (var child in node.Children)
            {
                string newPath = string.IsNullOrEmpty(currentPath) ? child.Key : $"{currentPath}.{child.Key}";
                GatherTagsRecursive(child.Value, newPath, tags);
            }
        }
    
        public void ClearAllTags()
        {
            _root = new TrieNode(); // Reset the root node to a new instance, effectively clearing all tags
        }
    
    }
}