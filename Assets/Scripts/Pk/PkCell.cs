using DataAccess;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.UI;

public class PkCell : ScrollableCell
{
    [SerializeField]
    private Text downloadCountText;
    [SerializeField]
    private Text answerCountText;
    [SerializeField]
    private UILikeWidget likeWidget;
    [SerializeField]
    private Text pkNameText;
    [SerializeField]
    private AssetBundleSprite assetBundleSprite;
    [SerializeField]
    private Image[] selectionMasks;

    public PK pkData
    {
        get { return (PK)dataObject; }
    }

    public override void ConfigureCellData()
    {
        Cleanup();

        foreach (var mask in selectionMasks)
        {
            mask.enabled = ((PkController)Context).IsDeleting;
        }

        pkNameText.text = pkData.PkName;
        pkData.onUpdated += UpdateStatsUI;
        UpdateStatsUI();

        GameboardTemplateData gbThemeData = GameboardTemplateData.Get((int)pkData.PkSenceId);
        assetBundleSprite.SetAsset(gbThemeData.thumbnailBundleName, GameboardTemplateData.Thumbnail);
    }

    private void Cleanup()
    {
        if (pkData != null)
        {
            pkData.onUpdated -= UpdateStatsUI;
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void UpdateStatsUI()
    {
        downloadCountText.text = pkData.PkDownloadCount.ToString();
        answerCountText.text = pkData.PkAnswerList.Count.ToString();
        likeWidget.SetLiked(pkData.Liked(UserManager.Instance.UserId), false);
        likeWidget.likeCount = pkData.PkLikeList.Count;
    }

    public void OnClickLike()
    {
        if (pkData.PkLikeList.ContainsKey(UserManager.Instance.UserId))
        {
            pkData.PkLikeList.Remove(UserManager.Instance.UserId);
            likeWidget.SetLiked(false, false);
        }
        else
        {
            pkData.PkLikeList.Add(UserManager.Instance.UserId, new PKLike_Info());
            likeWidget.SetLiked(true, true);
        }
        likeWidget.likeCount = pkData.PkLikeList.Count;

        var pkLike = new CMD_Like_PK_r_Parameters();
        pkLike.PkId = pkData.PkId;
        SocketManager.instance.send(Command_ID.CmdLikePkR, pkLike.ToByteString());
    }
}
