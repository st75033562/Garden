using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public struct ClosesetConnectionTestResult
{
    public Connection target;
    public float sqrDistance;

    public bool isValid
    {
        get { return target != null; }
    }

    public static ClosesetConnectionTestResult Create()
    {
        return new ClosesetConnectionTestResult {
            sqrDistance = float.MaxValue
        };
    }
}

public interface IClosestConnectionFilter
{
    bool Filter(Connection source, Connection target);
}

public class ConnectionRegistry
{
    private const float s_kSnapDistance = 60;

    private delegate bool ClosestLineTestHandler(Vector3 dir, GameObject target);
    private delegate ClosesetConnectionTestResult ClosestLineQueryHandler(
        Connection source, List<Connection> candidates, IClosestConnectionFilter filter);

    private readonly Dictionary<ConnectionTypes, List<Connection>> m_Connections = 
        new Dictionary<ConnectionTypes, List<Connection>>();

    private readonly Dictionary<ConnectionTypes, ClosestLineQueryHandler> m_ClosestConnTests =
        new Dictionary<ConnectionTypes, ClosestLineQueryHandler>();

    private readonly Transform m_panelTrans;

    public ConnectionRegistry(Transform panelTrans)
    {
        if (panelTrans == null)
        {
            throw new ArgumentNullException("panelTrans");
        }

        m_panelTrans = panelTrans;
        InitCloseLineTestsTable();
    }

    void InitCloseLineTestsTable()
    {
        m_ClosestConnTests.Add(ConnectionTypes.Top, FindClosestToTopConnection);
        m_ClosestConnTests.Add(ConnectionTypes.Bottom, FindClosestToBottomConnection);
        m_ClosestConnTests.Add(ConnectionTypes.RoundInsert, FindClosestToInsertNodeConnection);
        m_ClosestConnTests.Add(ConnectionTypes.CuspInsert, FindClosestToInsertNodeConnection);
    }

    ClosesetConnectionTestResult FindClosestToTopConnection(
        Connection source, List<Connection> candidates, IClosestConnectionFilter filter)
    {
        return InternalFindClosestConnection(source, candidates, (dir, target) => {
            return dir.sqrMagnitude <= s_kSnapDistance * s_kSnapDistance && dir.y <= 0;
        }, filter);
    }

    ClosesetConnectionTestResult FindClosestToBottomConnection(
        Connection source, List<Connection> candidates, IClosestConnectionFilter filter)
    {
        return InternalFindClosestConnection(source, candidates, (dir, target) => {
            return dir.sqrMagnitude <= s_kSnapDistance * s_kSnapDistance && dir.y >= 0;
        }, filter);
    }

    ClosesetConnectionTestResult FindClosestToInsertNodeConnection(
        Connection source, List<Connection> candidates, IClosestConnectionFilter filter)
    {
        return InternalFindClosestConnection(source, candidates, (dir, target) => {
            if (dir.x < 0)
            {
                return false;
            }

            var height = ((RectTransform)target.transform).rect.height;
            if (Mathf.Abs(dir.y) >= height * 0.5f)
            {
                return false;
            }
            var parentTrans = (RectTransform)target.transform.parent;
            var width = parentTrans.localToWorldMatrix.m00 * parentTrans.rect.width;
            return dir.x <= width;
        }, filter);
    }

    ClosesetConnectionTestResult InternalFindClosestConnection(
        Connection source, List<Connection> candidates, ClosestLineTestHandler matched, IClosestConnectionFilter filter)
    {
        var result = ClosesetConnectionTestResult.Create();

        Vector3 myPos = m_panelTrans.InverseTransformPoint(source.line.transform.position);
        float minDist = float.MaxValue;
        var firstNode = source.node.GetFirstNode();

        for (int i = 0; i < candidates.Count; ++i)
        {
            Connection target = candidates[i];

            if (!target.enabledAsTarget)
            {
                continue;
            }

            // ignore lines in the current moving block chain
            if (firstNode == target.node.GetFirstNode())
            {
                continue;
            }

            if (filter != null && filter.Filter(source, target))
            {
                continue;
            }

            Vector3 targetPos = m_panelTrans.InverseTransformPoint(target.line.transform.position);

            Vector3 dirToThisLine = myPos - targetPos;
            if (dirToThisLine.sqrMagnitude < minDist && matched(dirToThisLine, target.line))
            {
                minDist = dirToThisLine.sqrMagnitude;
                result.target = target;
                result.sqrDistance = minDist;
            }
        }

        return result;
    }


    public void Register(FunctionNode node)
    {
        if (node == null)
        {
            throw new ArgumentNullException("node");
        }

        for (int i = 0; i < node.Connections.Count; ++i)
        {
            Assert.IsTrue((int)node.Connections[i].type != 0);
            Register(node.Connections[i]);
        }
    }

    public void Unregister(FunctionNode node)
    {
        if (node == null)
        {
            throw new ArgumentNullException("node");
        }

        foreach (var lines in m_Connections.Values)
        {
            lines.RemoveAll(x => x.node == node);
        }
    }

    public void Register(Connection conn)
    {
        if (conn == null)
        {
            throw new ArgumentNullException("conn");
        }

        List<Connection> lines;
        if (!m_Connections.TryGetValue(conn.type, out lines))
        {
            lines = new List<Connection>();
            m_Connections.Add(conn.type, lines);
        }
        lines.Add(conn);
    }

    public void Unregister(Connection conn)
    {
        if (conn == null)
        {
            throw new ArgumentNullException("conn");
        }

        List<Connection> lines;
        if (m_Connections.TryGetValue(conn.type, out lines))
        {
            lines.Remove(conn);
        }
    }

    public void Register(IEnumerable<Connection> connections)
    {
        if (connections == null)
        {
            throw new ArgumentNullException("connections");
        }

        foreach (var conn in connections)
        {
            Register(conn);
        }
    }

    public void Unregister(IEnumerable<Connection> connections)
    {
        if (connections == null)
        {
            throw new ArgumentNullException("connections");
        }

        foreach (var conn in connections)
        {
            Unregister(conn);
        }
    }

    public ClosesetConnectionTestResult FindClosestMatchingConnection(
        Connection source, IClosestConnectionFilter filter = null)
    {
        var result = ClosesetConnectionTestResult.Create();

        foreach (ConnectionTypes targetConnType in new Flags32((int)source.matchingTypes))
        {
            List<Connection> candidates = null;
            ClosestLineQueryHandler testFunc = null;

            if (m_Connections.TryGetValue(targetConnType, out candidates) &&
                m_ClosestConnTests.TryGetValue(source.type, out testFunc))
            {
                var testResult = testFunc(source, candidates, filter);
                if (testResult.sqrDistance < result.sqrDistance)
                {
                    result = testResult;
                }
            }
        }

        return result;
    }
}
