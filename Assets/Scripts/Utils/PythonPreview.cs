using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PythonPreview {
    const string taskPyTempPath = "/taskpython/";

    public static void Preview(FileList fileList) {
        string mainPath = null;
        string path = null;
        foreach(FileNode df in fileList.FileList_) {
            try {
                path = FileUtils.appTempPath + taskPyTempPath + df.PathName;
                if((FN_TYPE)df.FnType == FN_TYPE.FnDir) {
                    FileUtils.createParentDirectory(path);
                } else {
                    if(df.PathName.Contains(PythonConstants.Main)) {
                        mainPath = path;
                    }
                    FileUtils.createParentDirectory(path);
                    File.WriteAllBytes(path, df.FileContents.ToByteArray() ?? new byte[0]);
                }
            } catch(Exception e) {
                Debug.LogException(e);
                return;
            }
        }
        if(mainPath == null) {
            mainPath = path;
        }

        SelectPreview(mainPath);
    }

    public static void Preview(string localPath) {
        string path = PythonRepository.instance.getAbsPath(localPath);
        string mainPath = null;
        if(File.Exists(path)) {
            mainPath = path;
        } else {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] fil = dir.GetFiles();
            foreach(FileInfo f in fil) {
                if(f.Name.Contains(PythonConstants.Main)) {
                    mainPath = f.FullName;
                    break;
                }
            }
        }
        SelectPreview(mainPath);
    }

    static void SelectPreview(string mainPath) {
        PopupManager.TwoBtnDialog("ui_choose_py_operation".Localize(), "ui_py_run".Localize(), () => {
            using(ScriptUtils.Run(mainPath)) { }
        }, "ui_py_check_code".Localize(), () => {
            PythonEditorManager.instance.Open(mainPath);
        });
    }
}
