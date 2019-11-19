using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class L_FileNode{
    public string pathName ;
    public string base64PathName ;
    public uint fnType ;
    public string fileMd5 ;
    public long createTime;
    public long updateTime;
    public byte[] fileContents;

    public static L_FileNode Build(FileNode info) {
        L_FileNode lFileNode = new L_FileNode();
        lFileNode.pathName = info.PathName;
        lFileNode.base64PathName = info.Base64PathName;
        lFileNode.fnType = info.FnType;
        lFileNode.fileMd5 = info.FileMd5;
        lFileNode.createTime = (long)info.CreateTime;
        lFileNode.updateTime = (long)info.UpdateTime;
        lFileNode.fileContents = info.FileContents.ToByteArray();
        return lFileNode;
    }
}

public class L_FileList {
    public List<L_FileNode> fileList ;
    public string rootPath ;

    public static L_FileList Build(FileList info) {
        L_FileList l_fileList = new L_FileList();
        if(info.FileList_ != null) {
            foreach(var fileNode in info.FileList_) {
                l_fileList.fileList.Add(L_FileNode.Build(fileNode));
            }
        }
        l_fileList.rootPath = info.RootPath;
        return l_fileList;
    }
}
public enum L_K8AttachType {
    KAT_Image,
    KAT_Video,
    KAT_Course,
    KAT_Projects, 
    KAT_Gameboard 
}

public enum L_ProjectLanguageType {
    projectLanguageGraphy ,
    projectLanguagePython 
}

public class L_K8AttachUnit {
    public L_K8AttachType attachType ;
    public string attachUrl ;
    public string attachName ;
    public L_FileList attachFiles;

    public static L_K8AttachUnit Build(K8_Attach_Unit info) {
        L_K8AttachUnit l_k8AttachUnit = new L_K8AttachUnit();
        l_k8AttachUnit.attachType = (L_K8AttachType)info.AttachType;
        l_k8AttachUnit.attachUrl = info.AttachUrl;
        l_k8AttachUnit.attachName = info.AttachName;
        l_k8AttachUnit.attachFiles = L_FileList.Build(info.AttachFiles);
        return l_k8AttachUnit;
    }
}

public class L_K8AttachInfo {
    public Dictionary<uint, L_K8AttachUnit> attachList ; 
    public uint attachMaxId ;
    public string attachUniqueId ;
    public L_ProjectLanguageType attachType ;

    public static L_K8AttachInfo Build(K8_Attach_Info info) {
        L_K8AttachInfo l_k8AttachInfo = new L_K8AttachInfo();
        if(info.AttachList != null) {
            l_k8AttachInfo.attachList = new Dictionary<uint, L_K8AttachUnit>();
            foreach(var key in info.AttachList.Keys) {
                l_k8AttachInfo.attachList.Add(key, L_K8AttachUnit.Build(info.AttachList[key]));
            }
        }
        l_k8AttachInfo.attachMaxId = info.AttachMaxId;
        l_k8AttachInfo.attachUniqueId = info.AttachUniqueId;
        l_k8AttachInfo.attachType = (L_ProjectLanguageType)info.AttachType;
        return l_k8AttachInfo;
    }
}
