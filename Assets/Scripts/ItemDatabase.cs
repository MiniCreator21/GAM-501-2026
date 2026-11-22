using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCDatabase", menuName = "NPC/Database")]
public class NPCDatabase : ScriptableObject
{
    public List<ItemDefinition> allItems;

#if UNITY_EDITOR
    [ContextMenu("Auto-Populate From Folder")]
    void AutoPopulate()
    {
        allItems.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemDefinition");
        foreach (var guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var def = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            allItems.Add(def);
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}