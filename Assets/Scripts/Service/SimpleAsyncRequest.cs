using System;

public class SimpleAsyncRequest<T> : AsyncRequest<T>
{
    private T m_result;

    public void SetResult(T result)
    {
        if (isCompleted)
        {
            return;
        }

        m_result = result;
        SetCompleted();
    }

    protected override T GetResult()
    {
        return m_result;
    }
}
