using DataAccess;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.UI;

public class SinglePkCell : ScrollableCell
{
    [SerializeField]
    private Text downloadCount;
    [SerializeField]
    private Text answerCount;
    [SerializeField]
    private GameObject lockGo;
    [SerializeField]
    private AssetBundleSprite assetBundleSprite;
    [SerializeField]
    private Text nameText;

    public GameObject[] masks;
    public PopupSinglePk parentController;
    public UILikeWidget likeWidget;
    public UIImageMedia imageMedia;

    private GameBoard gameBoard;

    public override void ConfigureCellData()
    {
        RemoveListener();

        gameBoard = (GameBoard)dataObject;
        gameBoard.onUpdated = RefreshStats;
        RefreshStats();

        UpdateLikeWidget(false);

        nameText.text = gameBoard.GbName;
        GameboardTemplateData gbThemeData = GameboardTemplateData.Get((int)gameBoard.GbSenceId);
        

        foreach (var mask in masks)
        {
            mask.SetActive(parentController.isDeleting);
        }

        lockGo.SetActive(!gameBoard.HasSourceCode);

        if(gameBoard.GbAttachInfo.AttachUrlImage.Count > 0) {
            imageMedia.gameObject.SetActive(true);
            assetBundleSprite.gameObject.SetActive(false);
            imageMedia.SetImage(gameBoard.GbAttachInfo.AttachUrlImage[0]);
        } else {
            imageMedia.gameObject.SetActive(false);
            assetBundleSprite.gameObject.SetActive(true);
            assetBundleSprite.SetAsset(gbThemeData.thumbnailBundleName, GameboardTemplateData.Thumbnail);
        }
    }

    public GameBoard data
    {
        get { return (GameBoard)DataObject; }
    }

    private void RemoveListener()
    {
        if (gameBoard != null)
        {
            gameBoard.onUpdated = null;
        }
    }

    private void OnDisable()
    {
        RemoveListener();
        gameBoard = null;
    }

    private void RefreshStats()
    {
        downloadCount.text = gameBoard.GbDownloadCount.ToString();
        answerCount.text = gameBoard.GbAnswerList.Count.ToString();
    }

    public void OnClickLike()
    {
        var curGameboard = gameBoard;
        bool liked = curGameboard.Liked(UserManager.Instance.UserId);
        curGameboard.SetLiked(UserManager.Instance.UserId, !liked);
        UpdateLikeWidget(true);

        var like_r = new CMD_Like_Gameboard_r_Parameters();
        like_r.GbId = curGameboard.GbId;

        SocketManager.instance.send(Command_ID.CmdLikeGameboardR, like_r.ToByteString(), (res, content) => {
            if (res != Command_Result.CmdNoError)
            {
                curGameboard.SetLiked(UserManager.Instance.UserId, liked);
                if (curGameboard.GbId == gameBoard.GbId)
                {
                    UpdateLikeWidget(false);
                }
            }
        });
    }

    private void UpdateLikeWidget(bool showAnim)
    {
        likeWidget.likeCount = gameBoard.GbLikeList.Count;
        likeWidget.SetLiked(gameBoard.Liked(UserManager.Instance.UserId), showAnim);
    }
}
