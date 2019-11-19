using Google.Protobuf;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameboard
{
    public class ObjectAssetInfo
    {
        public int assetId { get; private set; }

        public int nextObjectNum;

        internal ObjectAssetInfo() { }

        public ObjectAssetInfo(int assetId)
        {
            this.assetId = assetId;
        }

        public ObjectAssetInfo(ObjectAssetInfo rhs)
        {
            assetId = rhs.assetId;
            nextObjectNum = rhs.nextObjectNum;
        }

        public Save_GameboardObjectAssetInfo Serialize()
        {
            var data = new Save_GameboardObjectAssetInfo();
            data.AssetId = assetId;
            data.NextObjectNum = nextObjectNum;
            return data;
        }

        public void Deserialize(Save_GameboardObjectAssetInfo data)
        {
            assetId = data.AssetId;
            nextObjectNum = data.NextObjectNum;
        }
    }

    public interface IObjectInfo
    {
        Vector3 position { get; set; }

        /// <summary>
        /// depending on the constraint of the implementation, not all components are valid
        /// </summary>
        Vector3 rotation { get; set; }

        Vector3 scale { get; set; }

        IObjectInfo Clone();

        void CopyFrom(IObjectInfo o);
    }

    public interface IObjectInfo<T> : IObjectInfo
    {
        new T Clone();
    }

    public class ObjectInfo : IObjectInfo<ObjectInfo>
    {
        public string name { get; set; }

        public int assetId { get; private set; }

        public Vector3 position { get; set; }

        public Vector3 rotation { get; set; }

        public Vector3 scale { get; set; }

        internal ObjectInfo() { }

        public ObjectInfo(int assetId)
        {
            this.assetId = assetId;
            scale = Vector3.one;
        }

        public ObjectInfo(ObjectInfo rhs)
        {
            name = rhs.name;
            assetId = rhs.assetId;
            position = rhs.position;
            rotation = rhs.rotation;
            scale = rhs.scale;
        }

        public Save_GameboardObject Serialize()
        {
            var data = new Save_GameboardObject();
            data.Name = name;
            data.AssetId = assetId;
            data.Position = new Save_Vector3(position);
            data.Rotation = new Save_Vector3(rotation);
            data.Scale = new Save_Vector3(scale);
            return data;
        }

        public void Deserialize(Save_GameboardObject data)
        {
            name = data.Name;
            assetId = data.AssetId;
            position = data.Position.ToVector3();
            if (data.OldRot != 0)
            {
                rotation = new Vector3(0, data.OldRot, 0);
            }
            else if (data.Rotation != null)
            {
                rotation = data.Rotation.ToVector3();
            }
            scale = data.Scale.ToVector3();
        }

        IObjectInfo IObjectInfo.Clone()
        {
            return Clone();
        }

        public ObjectInfo Clone()
        {
            return (ObjectInfo)MemberwiseClone();
        }

        public void CopyFrom(IObjectInfo o)
        {
            var rhs = (ObjectInfo)o;

            name = rhs.name;
            assetId = rhs.assetId;
            position = rhs.position;
            scale = rhs.scale;
            rotation = rhs.rotation;
        }
    }

    public class RobotInfo : IObjectInfo<RobotInfo>
    {
        public Vector3 position { get; set; }

        Vector3 IObjectInfo.rotation
        {
            get { return new Vector3(0, rotation, 0); }
            set { rotation = value.y; }
        }

        public Vector3 scale { get; set; }

        public float rotation { get; set; }

        /// <summary>
        /// color id of the robot, default is 0
        /// </summary>
        public int colorId { get; set; }

        public RobotInfo()
        {
            scale = Vector3.one;
        }

        public RobotInfo(RobotInfo rhs)
        {
            position = rhs.position;
            rotation = rhs.rotation;
            scale = rhs.scale;
            colorId = rhs.colorId;
        }

        // NOTE: in version 1, Y is actually Z, so we swap them on serialization
        public Save_GameboardRobot Serialize()
        {
            var data = new Save_GameboardRobot();
            data.X = position.x;
            data.Y = position.z;
            data.Z = position.y;
            data.Rotation = rotation;
            data.Scale = new Save_Vector3(scale);
            data.ColorId = colorId;
            return data;
        }

        public void Deserialize(Save_GameboardRobot data)
        {
            position = new Vector3(data.X, data.Z, data.Y);
            rotation = data.Rotation;
            scale = data.Scale.ToVector3();
            colorId = data.ColorId;
        }

        IObjectInfo IObjectInfo.Clone()
        {
            return Clone();
        }

        public RobotInfo Clone()
        {
            return (RobotInfo)MemberwiseClone();
        }

        public void CopyFrom(IObjectInfo o)
        {
            var rhs = (RobotInfo)o;

            position = rhs.position;
            rotation = rhs.rotation;
            scale = rhs.scale;
            colorId = rhs.colorId;
        }
    }

    public static class ProjectUrl
    {
        // to distinguish with local path
        // a remote path cannot coincide with a local path as ':' is not valid character
        private const char RemotePrefix = ':';

        public static bool IsRemote(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            if (url == "")
            {
                return false;
            }

            return url[0] == RemotePrefix;
        }

        public static string ToRemote(string path)
        {
            if (IsRemote(path)) { return path; }
            return RemotePrefix + path;
        }

        public static string GetPath(string url)
        {
            if (!IsRemote(url)) { return url; }
            return url.Substring(1);
        }
    }

    public class RobotCodeGroups : IEnumerable<RobotCodeGroupInfo>
    {
        protected readonly List<RobotCodeGroupInfo> m_codeGroups = new List<RobotCodeGroupInfo>();

        public Action onChanged { get; set; }

        protected virtual void OnChanged()
        {
            if (onChanged != null)
            {
                onChanged();
            }
        }

        /// <summary>
        /// create a copy for given gameboard
        /// </summary>
        public RobotCodeGroups Clone()
        {
            var clone = new RobotCodeGroups();
            clone.m_codeGroups.AddRange(m_codeGroups.Select(x => new RobotCodeGroupInfo(x)));
            return clone;
        }

        public void OnRobotRemoved(int robotIndex)
        {
            foreach (var group in m_codeGroups)
            {
                group.OnRobotRemoved(robotIndex);
            }
        }

        public RobotCodeGroupInfo GetGroup(int robotIndex)
        {
            return m_codeGroups.Find(x => x.Contains(robotIndex));
        }

        public void Add(RobotCodeGroupInfo group)
        {
            if (GetGroup(group.projectPath) != null)
            {
                throw new InvalidOperationException("duplicate");
            }

            m_codeGroups.Add(group);
            OnChanged();
        }

        public void Add(IEnumerable<RobotCodeGroupInfo> groups)
        {
            foreach (var g in groups)
            {
                if (GetGroup(g.projectPath) != null)
                {
                    throw new InvalidOperationException("duplicate " + g.projectPath);
                }
                m_codeGroups.Add(g);
            }
            OnChanged();
        }

        public void ChangeGroupPath(string path0, string path1)
        {
            if (string.IsNullOrEmpty(path0))
            {
                throw new ArgumentException("path0");
            }

            if (string.IsNullOrEmpty(path1))
            {
                throw new ArgumentException("path1");
            }

            var group = GetGroup(path0);
            if (group == null)
            {
                throw new ArgumentException("invalid path name", "path0");
            }

            if (GetGroup(path1) != null)
            {
                throw new ArgumentException("duplicate path name", "path1");
            }

            group.projectPath = path1;
        }

        public void SetRobotCode(int robotIndex, string projectPath)
        {
            bool changed = false;

            // remove existing info
            for (int i = 0; i < m_codeGroups.Count; ++i)
            {
                var group = m_codeGroups[i];
                if (group.Contains(robotIndex))
                {
                    // do nothing if not changed
                    if (group.projectPath == projectPath)
                    {
                        return;
                    }
                    else
                    {
                        group.Remove(robotIndex);
                        if (group.empty)
                        {
                            m_codeGroups.RemoveAt(i);
                        }

                        changed = true;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(projectPath))
            {
                var group = m_codeGroups.Find(x => x.projectPath == projectPath);
                if (group == null)
                {
                    group = new RobotCodeGroupInfo(projectPath);
                    m_codeGroups.Add(group);
                }
                group.Add(robotIndex);
                changed = true;
            }

            if (changed)
            {
                OnChanged();
            }
        }

        public IEnumerable<RobotCodeGroupInfo> codeGroups
        {
            get { return m_codeGroups; }
        } 

        public void RemoveCodeGroup(RobotCodeGroupInfo group)
        {
            if (m_codeGroups.Remove(group))
            {
                OnChanged();
            }
        }

        public RobotCodeGroupInfo GetGroup(string path)
        {
            return m_codeGroups.Find(x => x.projectPath == path);
        }

        public bool RemoveCodeGroup(string path)
        {
            var group = GetGroup(path);
            if (group != null)
            {
                RemoveCodeGroup(group);
                return true;
            }
            return false;
        }

        public void ClearCodeGroups()
        {
            if (m_codeGroups.Count > 0)
            {
                m_codeGroups.Clear();
                OnChanged();
            }
        }

        public IEnumerable<Save_GameboardCodeGroup> Serialize()
        {
            return m_codeGroups.Select(x => x.Serialize());
        }

        public void Deserialize(IEnumerable<Save_GameboardCodeGroup> groups)
        {
            m_codeGroups.Clear();
            m_codeGroups.AddRange(groups.Select(x => {
                return new RobotCodeGroupInfo(x.ProjectPath, x.RobotIndices) {
                    projectName = x.ProjectName
                };
            }));
        }

        public IEnumerator<RobotCodeGroupInfo> GetEnumerator()
        {
            return codeGroups.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class RobotCodeGroupInfo
    {
        private readonly List<int> m_robotIndices = new List<int>();

        public RobotCodeGroupInfo(string path, IEnumerable<int> robotIndices = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("path");
            }

            this.projectPath = path;
            if (robotIndices != null)
            {
                m_robotIndices.AddRange(robotIndices);
            }
        }

        public RobotCodeGroupInfo(RobotCodeGroupInfo rhs)
        {
            projectPath = rhs.projectPath;
            m_robotIndices.AddRange(rhs.m_robotIndices);
        }

        public string projectPath { get; internal set; }

        // can be empty, used by mini course
        public string projectName { get; set; }

        public IEnumerable<int> robotIndices { get { return m_robotIndices; } }

        public void OnRobotRemoved(int robotIndex)
        {
            for (int i = 0; i < m_robotIndices.Count; ++i)
            {
                if (m_robotIndices[i] > robotIndex)
                {
                    m_robotIndices[i] = m_robotIndices[i] - 1;
                }
            }
        }

        public void Add(int robotIndex)
        {
            m_robotIndices.Add(robotIndex);
        }

        public void Remove(int robotIndex)
        {
            m_robotIndices.Remove(robotIndex);
        }

        public bool Contains(int robotIndex)
        {
            return m_robotIndices.Contains(robotIndex);
        }

        public bool empty
        {
            get { return m_robotIndices.Count == 0; }
        }

        public Save_GameboardCodeGroup Serialize()
        {
            var data = new Save_GameboardCodeGroup();
            data.ProjectPath = projectPath;
            data.ProjectName = projectName ?? string.Empty;
            data.RobotIndices.AddRange(robotIndices);
            return data;
        }
    }

    public class Gameboard
    {
        // the serialization version
        public const int version = 2;

        public event Action<int> onRobotAdded;
        public event Action<int> onRobotRemoved;
        public event Action<int> onRobotUpdated;
        public event Action<int, ObjectInfo> onObjectAdded;
        public event Action<ObjectInfo> onObjectRemoved;
        public event Action<ObjectInfo> onObjectUpdated;

        private readonly List<RobotInfo> m_robots = new List<RobotInfo>();
        private readonly RobotCodeGroups[] m_codeGroups = new RobotCodeGroups[(int)ScriptLanguage.Num];

        private readonly List<ObjectInfo> m_objects = new List<ObjectInfo>();
        private readonly List<ObjectAssetInfo> m_assetsInfo = new List<ObjectAssetInfo>();

        public Gameboard()
        {
            sourceCodeAvailable = true;
            for (int i = 0; i < m_codeGroups.Length; ++i)
            {
                m_codeGroups[i] = new RobotCodeGroups();
                m_codeGroups[i].onChanged = OnCodeGroupChanged;
            }
        }

        public Gameboard(Gameboard rhs)
        {
            m_robots.AddRange(rhs.m_robots.Select(x => new RobotInfo(x)));
            themeId = rhs.themeId;
            sourceCodeAvailable = rhs.sourceCodeAvailable;
            name = rhs.name;
            for (int i = 0; i < m_codeGroups.Length; ++i)
            {
                m_codeGroups[i] = rhs.m_codeGroups[i].Clone();
                m_codeGroups[i].onChanged = OnCodeGroupChanged;
            }

            m_objects.AddRange(rhs.m_objects.Select(x => new ObjectInfo(x)));
            m_assetsInfo.AddRange(rhs.m_assetsInfo.Select(x => new ObjectAssetInfo(x)));
        }

        private void OnCodeGroupChanged()
        {
            isDirty = true;
        }

        public int themeId { get; set; }

        // #refactor rename to isOpenSource
        public bool sourceCodeAvailable { get; set; }

        public string name { set; get; }

        // TODO: move this to controller
        public bool isDirty { get; set; }

        public IList<RobotInfo> robots
        {
            get { return m_robots.AsReadOnly(); }
        }

        public void InsertRobot(int index, RobotInfo info)
        {
            if (info  == null)
            {
                throw new ArgumentNullException("info");
            }

            m_robots.Insert(index, info);
            isDirty = true;

            if (onRobotAdded != null)
            {
                onRobotAdded(index);
            }
        }

        public void AddRobot(RobotInfo info)
        {
            InsertRobot(m_robots.Count, info);
        }

        public void RemoveRobot(int index)
        {
            m_robots.RemoveAt(index);
            // robot index changed, update indices in groups
            for (int i = 0; i < m_codeGroups.Length; ++i)
            {
                m_codeGroups[i].OnRobotRemoved(index);
            }

            isDirty = true;
            if (onRobotRemoved != null)
            {
                onRobotRemoved(index);
            }
        }

        public void AddRobots(IEnumerable<RobotInfo> robots)
        {
            m_robots.AddRange(robots);
            isDirty = true;
        }

        public void ClearRobots()
        {
            m_robots.Clear();
            ClearCodeGroups();
            isDirty = true;
        }

        public RobotCodeGroups GetCodeGroups(ScriptLanguage language)
        {
            return m_codeGroups[(int)language];
        }

        public void ClearCodeGroups()
        {
            for (int i = 0; i < m_codeGroups.Length; ++i)
            {
                m_codeGroups[i].ClearCodeGroups();
            }
        }

        public IList<ObjectInfo> objects
        {
            get { return m_objects.AsReadOnly(); }
        }

        public ObjectAssetInfo GetAssetInfo(int assetId)
        {
            var assetInfo = m_assetsInfo.Find(x => x.assetId == assetId);
            if (assetInfo == null)
            {
                assetInfo = new ObjectAssetInfo(assetId);
                m_assetsInfo.Add(assetInfo);
            }
            return assetInfo;
        }

        public void InsertObject(int index, ObjectInfo objectInfo)
        {
            if (objectInfo == null)
            {
                throw new ArgumentNullException("objectInfo");
            }

            if (m_objects.Any(x => x.name == objectInfo.name))
            {
                throw new ArgumentException("duplicate object name");
            }

            m_objects.Insert(index, objectInfo);

            if (onObjectAdded != null)
            {
                onObjectAdded(index, objectInfo);
            }
        }

        public void AddObject(ObjectInfo objectInfo)
        {
            InsertObject(m_objects.Count, objectInfo);
        }

        public void RemoveObject(string name)
        {
            var objInfo = GetObject(name);
            if (objInfo != null)
            {
                RemoveObject(objInfo);
            }
        }

        public void RemoveObject(ObjectInfo objectInfo)
        {
            if (objectInfo == null)
            {
                throw new ArgumentNullException("objectInfo");
            }

            if (m_objects.Remove(objectInfo))
            {
                if (onObjectRemoved != null)
                {
                    onObjectRemoved(objectInfo);
                }
            }
        }

        public ObjectInfo GetObject(string name)
        {
            return m_objects.Find(x => x.name == name);
        }

        public void NotifyObjectUpdated(ObjectInfo objectInfo)
        {
            if (objectInfo == null)
            {
                throw new ArgumentNullException("objectInfo");
            }

            if (onObjectUpdated != null)
            {
                onObjectUpdated(objectInfo);
            }
        }

        public void NotifyRobotUpdated(int index)
        {
            if (index < 0 || index >= m_robots.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (onRobotUpdated != null)
            {
                onRobotUpdated(index);
            }
        }

        public Save_Gameboard Serialize()
        {
            var data = new Save_Gameboard();
            data.Version = version;
            data.ThemeId = themeId;
            data.SourceCodeAvailable = sourceCodeAvailable;
            data.Robots.AddRange(m_robots.Select(x => x.Serialize()));
            data.VisualCodeGroups.AddRange(m_codeGroups[(int)ScriptLanguage.Visual].Serialize());
            data.PythonCodeGroups.AddRange(m_codeGroups[(int)ScriptLanguage.Python].Serialize());
            data.Objects.AddRange(m_objects.Select(x => x.Serialize()));
            data.AssetsInfo.AddRange(m_assetsInfo.Select(x => x.Serialize()));

            return data;
        }

        public void Deserialize(Save_Gameboard data)
        {
            themeId = data.ThemeId;
            sourceCodeAvailable = data.SourceCodeAvailable;
            isDirty = false;

            m_robots.Clear();
            m_robots.AddRange(data.Robots.Select(x => {
                var robot = new RobotInfo();
                robot.Deserialize(x);
                return robot;
            }));

            m_codeGroups[(int)ScriptLanguage.Visual].Deserialize(data.VisualCodeGroups);
            m_codeGroups[(int)ScriptLanguage.Python].Deserialize(data.PythonCodeGroups);

            m_objects.Clear();
            m_objects.AddRange(data.Objects.Select(x => {
                var obj = new ObjectInfo();
                obj.Deserialize(x);
                return obj;
            }));

            m_assetsInfo.Clear();
            m_assetsInfo.AddRange(data.AssetsInfo.Select(x => {
                var obj = new ObjectAssetInfo();
                obj.Deserialize(x);
                return obj;
            }));
        }

        public static Gameboard Parse(byte[] data)
        {
            var saveData = Save_Gameboard.Parser.ParseFrom(data);
            var gameboard = new Gameboard();
            gameboard.Deserialize(saveData);
            return gameboard;
        }
    }
}