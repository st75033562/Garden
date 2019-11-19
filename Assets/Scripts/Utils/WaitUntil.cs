using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class WaitUntil : CustomYieldInstruction
{
    private Func<bool> m_predicate;

    public WaitUntil(Func<bool> pred)
    {
        if (pred == null)
        {
            throw new ArgumentNullException();
        }

        m_predicate = pred;
    }

    public override bool keepWaiting
    {
        get { return !m_predicate(); }
    }
}
