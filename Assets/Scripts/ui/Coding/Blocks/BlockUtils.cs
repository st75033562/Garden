public static class BlockUtils
{
    public static bool ParseBool(string text)
    {
        // bool_true will be returned if no embedded slots
        return text.EqualsIgnoreCase("true") || text == "bool_true";
    }

    public static float ParseBeat(string text)
    {
        float mBeat = 0;
        if (text == "1/4")
        {
            mBeat = 0.25f;
        }
        else if (text == "1/2")
        {
            mBeat = 0.5f;
        }
        else if (text == "3/4")
        {
            mBeat = 0.75f;
        }
        else if (text == "1")
        {
            mBeat = 1.0f;
        }
        else if (text == "1 1/4")
        {
            mBeat = 1.25f;
        }
        else if (text == "1 1/2")
        {
            mBeat = 1.5f;
        }
        else if (text == "1 3/4")
        {
            mBeat = 1.75f;
        }
        else if (text == "2")
        {
            mBeat = 2.0f;
        }
        else if (text == "3")
        {
            mBeat = 3.0f;
        }
        else if (text == "4")
        {
            mBeat = 4.0f;
        }
        else
        {
            float.TryParse(text, out mBeat);
        }
        return mBeat;
    }
}
