public class VariableData : BaseVariable
{
    private float mValue;
    private string mText;
    private bool mIsNumber;

    private const string DEFAULT_VALUE = "0";

    // for serialization
    public VariableData()
    {
    }

    public VariableData(string name, NameScope scope)
        : base(name, scope)
    {
        reset();
    }

    public VariableData(VariableData rhs)
        : base(rhs)
    {
        mValue = rhs.mValue;
        mText = rhs.mText;
        mIsNumber = rhs.mIsNumber;
    }

    public override BlockVarType type
    {
        get { return BlockVarType.Variable; }
    }

    public override void reset()
    {
        setValue(DEFAULT_VALUE);
    }

    public bool isNumber()
    {
        return mIsNumber;
    }

    public float getValue()
    {
        return mValue;
    }

    public void setValue(float value)
    {
        if (!isNumber() || mValue != value)
        {
            mIsNumber = true;
            mValue = value;
            mText = mValue.ToString();

            fireOnChanged();
        }
    }

    public void setValue(string value)
    {
        if (value == null)
            value = DEFAULT_VALUE;

        if (mText != value)
        {
            mText = value;
            mIsNumber = float.TryParse(value, out mValue);

            fireOnChanged();
        }
    }

    public void addValue(float value)
    {
        if (mIsNumber)
        {
            mValue += value;
            mText = mValue.ToString();

            fireOnChanged();
        }
    }

    public string getString()
    {
        return mText;
    }

    public override string ToString()
    {
        return string.Format("variable: {0} - {1}", name, getString());
    }

    public override BaseVariable clone(string name)
    {
        var copy = new VariableData(this);
        copy.name = name;
        return copy;
    }

    public override void readFrom(BaseVariable o)
    {
        var rhs = (VariableData)o;
        mValue = rhs.mValue;
        mText = rhs.mText;
        mIsNumber = rhs.mIsNumber;
        fireOnChanged();
    }

    #region serialization

    public override Save_Variable serialize()
    {
        var data = base.serialize();
        data.VariableType = Save_VariableType.VariabelType;
        data.VariableVar = getString();
        return data;
    }

    public override void deserialize(Save_Variable data)
    {
        base.deserialize(data);
        setValue(data.VariableVar);
    }

    #endregion
}