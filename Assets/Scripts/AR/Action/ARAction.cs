using Gameboard;

namespace AR
{
    [ObjectActionName("ARAction")]
    public abstract class ARAction : ObjectAction
    {
        public ARSceneManager SceneManager
        {
            get { return ParentObj.SceneManager; }
        }

        protected virtual void Awake()
        {
            ParentObj = GetComponent<ArObjActionBase>();
        }

        public ArObjActionBase ParentObj
        {
            get;
            private set;
        }
    }
}