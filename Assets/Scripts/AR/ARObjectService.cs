using DataAccess;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class ARObjectService
{
    public IEnumerable<ARObjectCategory> GetCategories()
    {
        return ARObjectDataSource.categories;
    }

    public IEnumerable<ARObjectData> GetObjectsByCategory(int categoryId)
    {
        return ARObjectDataSource.allObjects.Where(x => x.categoryId == categoryId);
    }

    public abstract void Unlock(int objectId, Action<bool> callback);
}

public class TestARObjectService : ARObjectService
{
    public override void Unlock(int objectId, Action<bool> callback)
    {
        if (callback != null)
        {
            callback(true);
        }
    }
}

public class RemoteARObjectService : ARObjectService
{
    public override void Unlock(int objectId, Action<bool> callback)
    {
        var objData = ARObjectDataSource.GetObject(objectId);
        if (objData == null)
        {
            throw new ArgumentException("objectId");
        }

        var oldCoins = UserManager.Instance.Coin;
        var request = new CMD_Buy_Ar_Obj_r_Parameters();
        request.ReqBuyArObjId = (uint)objectId;
        SocketManager.instance.send(Command_ID.CmdBuyArObjR, request.ToByteString(), (res, data) => {
            bool success = res == Command_Result.CmdNoError;
            if (success)
            {
                UserManager.Instance.Coin = oldCoins - objData.price;
            }
            if (callback != null)
            {
                callback(success);
            }
        });
    }
}
