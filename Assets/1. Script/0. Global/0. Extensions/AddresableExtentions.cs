using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;


public class AddresableExtentions
{
    public static void SetActiveAddresableFile(string path, string filename, bool active=true, string groupName="Default Local Group", string labelName="Default Label")
    {
        //Use this object to manipulate addressables
        var settings = AddressableAssetSettingsDefaultObject.Settings;

        string group_name = groupName;
        string label_name = labelName;


        string path_to_object = path;
        string custom_address = path + filename;

        // Create Group
        //settings.CreateGroup(sim_name, false, false, false, settings.DefaultGroup.Schemas);

        // Create Label
        //settings.AddLabel(label_name, false);


        // Remove Group
        //AddressableAssetGroup g = settings.FindGroup(group_name);
        //settings.RemoveGroup(g);

        // Remove a label
        //settings.RemoveLabel(label_name, false);


        //Make a gameobject an addressable
        AddressableAssetGroup g = settings.FindGroup(group_name);

        // FullPath /Directory/name.file
        var guid = AssetDatabase.AssetPathToGUID(path + filename);
        
        //This is the function that actually makes the object addressable
        var entry = settings.CreateOrMoveEntry(guid, g);

        // Add Lavel Data
        //entry.labels.Add(label_name);
        entry.address = custom_address;

        //You'll need these to run to save the changes!
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);

        AssetDatabase.SaveAssets();
        Debug.Log("[Addresseable] SetActive Path");
    }
}
