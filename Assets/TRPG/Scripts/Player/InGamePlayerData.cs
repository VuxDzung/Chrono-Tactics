using FishNet.Object.Synchronizing.Internal;
using FishNet.Object.Synchronizing;

namespace TRPG
{
    [System.Serializable]
    public class InGamePlayerData
    {
        public string id;
        public string playerName;
        public bool status;
        public string[] selectedUnitIDArr;
    }

    //public class SyncInGamePlayerData : SyncBase, ICustomSync
    //{
    //    /* If you intend to serialize your type
    //    * as a whole at any point in your custom
    //    * SyncType and would like the automatic
    //    * serializers to include it then use
    //    * GetSerializedType() to return the type.
    //    * In this case, the type is MyContainer.
    //    * If you do not need a serializer generated
    //    * you may return null. */
    //    public object GetSerializedType() => typeof(InGamePlayerData);
    //}
}