using System;
using System.Collections.Generic;

public static class CompetitionUtils
{
    public static IEnumerable<LocalResData> GetAttachmentResources(CompetitionProblem problem)
    {
        foreach (var attachment in problem.attachments)
        {
            var res = new LocalResData();
            res.name = attachment.url;
            res.resType = ToResourceType(attachment.type);
            yield return res;
        }
    }

    public static IEnumerable<AttachData> GetAttachmentAttachData(CompetitionProblem problem)
    {
        foreach (var attachment in problem.attachments)
        {

            var res = new LocalResData();
            res.name = attachment.url;
            res.resType = ToResourceType(attachment.type);
            res.nickName = attachment.nickName;

            AttachData dataRes = new AttachData();
            dataRes.itemId = attachment.id;
            dataRes.type = AttachData.Type.Res;
            dataRes.resData = res;
            dataRes.programNickName = attachment.nickName;
            
           yield return dataRes;
        }
    }

    public static ResType ToResourceType(CompetitionItem.Type type)
    {
        switch(type)
        {
        case CompetitionItem.Type.Doc:
            return ResType.Course;
        case CompetitionItem.Type.Image:
            return ResType.Image;
        case CompetitionItem.Type.Video:
            return ResType.Video;
        default:
            throw new ArgumentException();
        }
    }

    public static CompetitionItem.Type ToItemType(ResType type)
    {
        switch (type)
        {
        case ResType.Course:
            return CompetitionItem.Type.Doc;
        case ResType.Image:
            return CompetitionItem.Type.Image;
        case ResType.Video:
            return CompetitionItem.Type.Video;
        default:
            throw new ArgumentException();
        }
    }
}
