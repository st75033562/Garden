using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCompetitionMonitor : MonoBehaviour
{
    private CompetitionListModel m_openModel;
    private CompetitionListModel m_closedModel;

    public void Initialize(CompetitionListModel openModel, CompetitionListModel closedModel)
    {
        if (openModel == null)
        {
            throw new ArgumentNullException("openModel");
        }
        if (closedModel == null)
        {
            throw new ArgumentNullException("closedModel");
        }
        m_openModel = openModel;
        m_closedModel = closedModel;
    }

    IEnumerator Start()
    {
        for (; ;)
        {
            var changedIndices = new List<int>();
            for (int i = 0; i < m_openModel.count; ++i )
            {
                var competition = (Competition)m_openModel.getItem(i);
                if (competition.state == Competition.OpenState.Closed)
                {
                    changedIndices.Add(i);
                }
            }

            foreach (var index in changedIndices)
            {
                var competition = (Competition)m_openModel.getItem(index);
                competition.category = CompetitionCategory.Closed;
                m_openModel.removeItem(index);
                if (!m_closedModel.hasCompetition(competition.id))
                {
                    m_closedModel.addItem(competition);
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
}
