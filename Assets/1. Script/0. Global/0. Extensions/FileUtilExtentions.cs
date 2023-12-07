using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class FileUtilExtentions 
{
#if UNITY_EDITOR
    public static void _Editor_DeleteFileOrDirectory(string fullpath)
    {
        UnityEditor.FileUtil.DeleteFileOrDirectory(Application.dataPath + "/" + fullpath);
    }
#endif
}
