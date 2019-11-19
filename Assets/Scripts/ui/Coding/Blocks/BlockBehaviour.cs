using System;
using System.Collections;
using UnityEngine;

public class BlockBehaviour : MonoBehaviour
{
    public FunctionNode Node { get; private set; }

	protected virtual void Start()
	{
		Node = GetComponent<FunctionNode>();
		Node.BlockBehaviour = this;
	}

    protected virtual void OnDestroy()
    { }

	public virtual IEnumerator ActionBlock(ThreadContext context)
    {
        yield break;
    }

	public virtual IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
        retValue.value = "";
        yield break;
	}

    protected CodeContext CodeContext
    {
        get
        {
            return Node != null ? Node.CodeContext : null;
        }
    }
}
