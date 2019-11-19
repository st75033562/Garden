public class SaveProjectAsRequest : ProjectDownloadRequestV3 {
    public CloudSaveAsType saveAsType { get; set; }

    public string saveAs { get; set; }
    protected override void InitSaveAs(DownloadDirt_V3 tDownLoad) {
        base.InitSaveAs(tDownLoad);
        if(saveAsType == CloudSaveAsType.Project) {
            tDownLoad.SaveAsGraphyProjectName = saveAs;
        } else if(saveAsType == CloudSaveAsType.ProjectPy) {
            tDownLoad.SaveAsPythonProjectName = saveAs;
        } else if(saveAsType == CloudSaveAsType.GameBoard) {
            tDownLoad.SaveAsGraphyGbName = saveAs;
        } else if(saveAsType == CloudSaveAsType.GameBoardPy) {
            tDownLoad.SaveAsPythonGbName = saveAs;
        }
    }
}
