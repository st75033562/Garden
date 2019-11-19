using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Google.Protobuf;

using g_WebRequestManager = Singleton<WebRequestManager>;

public class ClassInfoData{
    public AddClassController addClassController;
    public ClassInfo banji;
}
public class AddClassController : PopupController {
    [SerializeField]
    private ScrollLoopController scrollController;
    [SerializeField]
    private InputField inputFile;


    private const uint recommandCount = 8;
    // Use this for initialization
    protected override void Start ()
    {
        CMD_Recommand_Class_r_Parameters recommandClass = new CMD_Recommand_Class_r_Parameters ();
        recommandClass.ReqCount = recommandCount;
        if(Preference.scriptLanguage == ScriptLanguage.Python) {
            recommandClass.ReqProjectType = Project_Language_Type.ProjectLanguagePython;
        } else {
            recommandClass.ReqProjectType = Project_Language_Type.ProjectLanguageGraphy;
        }
        
        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdRecommandClassR, recommandClass.ToByteString(), (res, content) =>{
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                CMD_Recommand_Class_a_Parameters classInfo_a = CMD_Recommand_Class_a_Parameters.Parser.ParseFrom(content);
                parseClassInfo(classInfo_a.ClassInfoList);
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public void OnClickSearchClassInfo ()
    {
        if(string.IsNullOrEmpty (inputFile.text)) {
            return;
        }
        
        CMD_Get_Classinfo_r_Parameters classInfo = new CMD_Get_Classinfo_r_Parameters ();
        classInfo.ReqClassName = inputFile.text;
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            classInfo.ReqProjectType = (uint)Project_Language_Type.ProjectLanguageGraphy;
        } else {
            classInfo.ReqProjectType = (uint)Project_Language_Type.ProjectLanguagePython;
        }
        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdGetClassinfoR, classInfo.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                CMD_Get_Classinfo_a_Parameters classInfo_a = CMD_Get_Classinfo_a_Parameters.Parser.ParseFrom(content);
                parseClassInfo(classInfo_a.ClassInfoList);
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    void parseClassInfo (Google.Protobuf.Collections.RepeatedField<A8_Class_Info> classInfos) {
        List<ClassInfoData> listBanji = new List<ClassInfoData> ();
        foreach(A8_Class_Info classInfo in classInfos) {
            ClassInfo banjiinfo = new ClassInfo ();
            banjiinfo.UpdateInfo (classInfo);

            ClassInfoData classInfoData = new ClassInfoData ();
            classInfoData.banji = banjiinfo;
            classInfoData.addClassController = this;
            listBanji.Add (classInfoData);
        }
        scrollController.initWithData (listBanji);
    }
}
