using Google.Protobuf;
using System;

public static class Uploads
{
    public static CreateFolderRequest CreateFolder(string path)
    {
        var request = new CreateFolderRequest();
        request.folderPath = path;
        return request;
    }

    public static SimpleHttpRequest UploadGamboard(GameboardProject project)
    {
        var request = new SimpleHttpRequest();
        request.SetPath(HttpCommon.c_Savegameboard);

        var fileList = new FileList();
        fileList.FileList_.AddRange(project.ToFileNodeList(project.name + "/"));

        var vploadV3 = new UploadFileList_V3();
        vploadV3.ListRoot = FileList_Root_Type.SelfGbGraphy;
        vploadV3.Files = fileList;

        request.postData = vploadV3.ToByteArray();
        return request;
    }

    public static UploadFileRequest UploadGameboardV3(GameboardProject project, string path)
    {
        var request = new UploadFileRequest();
        path = FileUtils.ensureSlashIfNonEmpty(path);
        request.files.FileList_.AddRange(project.ToFileNodeList(path));
        request.type = GetCatalogType.GAME_BOARD_V2;
        return request;
    }

    public static UploadFileRequest UploadProjectV3(Project project, string path)
    {
        var request = new UploadFileRequest();
        path = FileUtils.ensureSlashIfNonEmpty(path);
        request.files.FileList_.AddRange(project.ToFileNodeList(path));
        request.type = GetCatalogType.SELF_PROJECT_V2;
        return request;
    }

    public static SimpleHttpRequest UploadMedia(byte[] data, string name, bool isVideo)
    {
        if (data == null)
        {
            throw new ArgumentNullException("data");
        }

        var request = new SimpleHttpRequest();
        request.SetPath(HttpCommon.c_uploadmedia);
        request.SetHeader("file_name", GetMediaFilename(name, isVideo));
        request.postData = data;
        return request;
    }

    private static string GetMediaFilename(string name, bool isVideo)
    {
        return isVideo ? name + HttpCommon.c_VideoExtension : name;
    }

    public static SimpleHttpRequest UploadAudio(byte[] data, string filename)
    {
        if (data == null)
        {
            throw new ArgumentNullException("data");
        }

        var request = new SimpleHttpRequest();
        request.SetHeader("file_name", filename);
        request.SetPath(HttpCommon.c_UpLoadAudio);
        request.postData = data;
        return request;
    }
}
