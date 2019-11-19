using System;

public enum GetCatalogType
{
    EMPTY,
    TEACHER_TASK,
	SYSTEM_TASK,
    PYTHON,
    SELF_PROJECT_V2,
    GAME_BOARD_V2,
    TEACHER_TASK_PY,
    SYSTEM_TASK_PY
}

public enum CloudSaveAsType
{
    Project,
    GameBoard,
    ProjectPy,
    GameBoardPy
}

public static class HttpCommon
{
    public const string c_tasktemplateV3 = "/download/tasktemplate_v3/graphy/";
    public const string c_tasktemplatePyV3 = "/download/tasktemplate_v3/python/";
    public const string c_taskSystemV3 = "/download/tasktemplate_v3/graphy/system/";
    public const string c_Savegameboard = "/uploadproject_v3.php";
    public const string c_taskSystemPyV3 = "/download/tasktemplate_v3/python/system/";

    public const string c_UpLoadAudio = "/uploadaudio.php";
    public const string c_VoicePath = "download/audio/";
    public const string c_uploadmedia = "/uploadmedia.php";
    public const string c_game = "/game.php";

    public const string c_downloadmedia = "/download/media/";
    public const string c_VideoExtension = ".mp4";

    public static string GetRootPath(GetCatalogType type, uint userId) {
        switch(type) {
            case GetCatalogType.TEACHER_TASK:
                return c_tasktemplateV3 + userId + "/";
            case GetCatalogType.SELF_PROJECT_V2:
                return "/download/saved_project_v3/graphy/" + userId + "/";
            case GetCatalogType.GAME_BOARD_V2:
                return "/download/saved_gb_v3/graphy/" + userId + "/";
            case GetCatalogType.SYSTEM_TASK:
                return c_taskSystemV3;
            case GetCatalogType.PYTHON:
                return "/download/saved_project_v3/python/" + userId + "/";
                case GetCatalogType.EMPTY:
                return "";
            case GetCatalogType.TEACHER_TASK_PY:
                return c_tasktemplatePyV3 + userId + "/";
            case GetCatalogType.SYSTEM_TASK_PY:
                return c_taskSystemPyV3;
            default:
                throw new ArgumentOutOfRangeException("type");
        }
    }
}
