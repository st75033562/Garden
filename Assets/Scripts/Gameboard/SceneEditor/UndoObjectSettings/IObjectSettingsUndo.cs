using System;

namespace Gameboard
{
    public interface IObjectSettingsUndo<T> where T : IObjectInfo
    {
        void Record(T oldInfo, T newInfo, int entityId);
    }
}
