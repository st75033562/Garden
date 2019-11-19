public class Timer
{
    private bool m_started;

    public void Start()
    {
        m_started = true;
    }

    public void Pause()
    {
        m_started = false;
    }

    public void Reset()
    {
        elapsedTime = 0.0f;
    }

    public float elapsedTime
    {
        get;
        private set;
    }

    public void Update(float deltaTime)
    {
        if (m_started)
        {
            elapsedTime += deltaTime;
        }
    }
}
