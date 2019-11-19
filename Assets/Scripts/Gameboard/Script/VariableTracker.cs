using Gameboard.Script;
using Networking;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard
{
    class VariableTracker
    {
        private readonly VariableManager m_gbVarManager;
        private bool m_updating;
        private readonly List<BaseVariable> m_changedVars = new List<BaseVariable>();

        public VariableTracker(VariableManager gbVarManager)
        {
            if (gbVarManager == null)
            {
                throw new ArgumentNullException("gbVarManager");
            }

            m_gbVarManager = gbVarManager;
            m_gbVarManager.onVariableChanged.AddListener(OnVariableChanged);
        }

        public ClientConnection connection { get; set; }

        private void OnVariableChanged(BaseVariable obj)
        {
            if (obj.scope == NameScope.Global &&
                obj.globalVarOwner != GlobalVarOwner.Robot &&
                !m_changedVars.Contains(obj) &&
                !m_updating)
            {
                m_changedVars.Add(obj);
            }
        }

        public void OnChange(UpdateVariableRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (m_updating)
            {
                return;
            }

            if (!request.Name.StartsWith(RobotCodeManager.GlobalDataPrefix))
            {
                Debug.LogError("invalid global variable: " + request.Name);
                return;
            }

            var variableName = request.Name.Substring(RobotCodeManager.GlobalDataPrefix.Length);
            var variable = m_gbVarManager.get(variableName);
            if (variable != null && variable.globalVarOwner != GlobalVarOwner.Gameboard)
            {
                m_updating = true;
                switch (variable.type)
                {
                case BlockVarType.Variable:
                    Update((VariableData)variable, request);
                    break;

                case BlockVarType.List:
                    Update((ListData)variable, request);
                    break;

                case BlockVarType.Queue:
                    Update((QueueData)variable, request);
                    break;

                case BlockVarType.Stack:
                    Update((StackData)variable, request);
                    break;
                }
                m_updating = false;
            }
            else
            {
                Debug.LogError("invalid global variable: " + request.Name);
            }
        }

        private void Update(VariableData variable, UpdateVariableRequest request)
        {
            variable.setValue(request.Value);
        }

        private void Update(ListData listData, UpdateVariableRequest request)
        {
            switch ((ListOperation)request.Operation)
            {
            case ListOperation.LopInsert:
                listData.insert(request.Index + 1, request.Value);
                break;

            case ListOperation.LopRemove:
                listData.removeAt(request.Index + 1);
                break;

            case ListOperation.LopClear:
                listData.reset();
                break;

            case ListOperation.LopReplace:
                listData[request.Index + 1] = request.Value;
                break;

            default:
                throw new ArgumentException("invalid list operation: " + request.Operation);
            }
        }

        private void Update(QueueData queueData, UpdateVariableRequest request)
        {
            switch ((QueueOperation)request.Operation)
            {
            case QueueOperation.QopEnqueue:
                queueData.enqueue(request.Value);
                break;

            case QueueOperation.QopDequeue:
                queueData.dequeue();
                break;

            case QueueOperation.QopClear:
                queueData.reset();
                break;

            default:
                throw new ArgumentException("invalid queue operation: " + request.Operation);
            }
        }

        private void Update(StackData stackData, UpdateVariableRequest request)
        {
            switch ((StackOperation)request.Operation)
            {
            case StackOperation.SopPush:
                stackData.push(request.Value);
                break;

            case StackOperation.SopPop:
                stackData.pop();
                break;

            case StackOperation.SopClear:
                stackData.reset();
                break;

            default:
                throw new ArgumentException("invalid stack operation: " + request.Operation);
            }
        }

        public void SendUpdates()
        {
            if (connection != null && m_changedVars.Count > 0)
            {
                var notification = new VariablesUpdateNotification();
                foreach (var variable in m_changedVars)
                {
                    VariableUpdate update = new VariableUpdate();
                    update.Name = RobotCodeManager.GlobalDataPrefix + variable.name;
                    update.Value = VariableJsonUtils.ValueToJson(variable);

                    notification.Updates.Add(update);
                }
                connection.Send(CommandId.CmdVariablesUpdateNotification, notification);
                m_changedVars.Clear();
            }
        }
    }
}
