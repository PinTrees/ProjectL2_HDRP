using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class FileUtilExtentions 
{
    public static void _Editor_DeleteFileOrDirectory(string fullpath)
    {
        UnityEditor.FileUtil.DeleteFileOrDirectory(Application.dataPath + "/" + fullpath);
    }
}
