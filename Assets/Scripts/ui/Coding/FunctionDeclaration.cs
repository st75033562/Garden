using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using Google.Protobuf;

public enum FunctionPartType
{
    Label,
    Bool,
    Number,
    Text,
}

public class FunctionPart : IEquatable<FunctionPart>
{
    public FunctionPart(string text, FunctionPartType type)
    {
        if (string.IsNullOrEmpty(text))
        {
            throw new ArgumentException("text");
        }
        this.text = text;
        this.type = type;
    }

    // parameter name for parameter
    // label text for label
    public string text
    {
        get;
        private set;
    }

    public FunctionPartType type
    {
        get;
        private set;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as FunctionPart);
    }

    public bool Equals(FunctionPart other)
    {
        if (other == null)
        {
            return false;
        }

        return type == other.type && text == other.text;
    }

    public override int GetHashCode()
    {
        return type.GetHashCode() * 31 + text.GetHashCode();
    }
}

public class FunctionDeclaration : IEquatable<FunctionDeclaration>
{
    private readonly List<FunctionPart> m_parts = new List<FunctionPart>();
    private readonly IList<FunctionPart> m_readonlyParts;

    public FunctionDeclaration()
        : this(Guid.NewGuid())
    {
    }

    public FunctionDeclaration(Guid id)
    {
        functionId = id;
        m_readonlyParts = m_parts.AsReadOnly();
    }

    public FunctionDeclaration(FunctionDeclaration rhs)
    {
        if (rhs == null)
        {
            throw new ArgumentNullException("rhs");
        }

        functionId = rhs.functionId;
        m_readonlyParts = m_parts.AsReadOnly();
        m_parts.AddRange(rhs.m_parts);
    }

    public Guid functionId
    {
        get;
        set;
    }

    public void AddPart(FunctionPart part)
    {
        if (part == null)
        {
            throw new ArgumentNullException("part");
        }

        if (GetParameter(part.text) != null)
        {
            throw new ArgumentException("duplicate name", "part");
        }

        m_parts.Add(part);
    }

    public void RemovePart(FunctionPart part)
    {
        m_parts.Remove(part);
    }

    public IList<FunctionPart> parts
    {
        get { return m_readonlyParts; }
    }

    public FunctionPart GetPart(string text)
    {
        return m_parts.Find(x => x.text == text);
    }

    public FunctionPart GetParameter(string text)
    {
        return m_parts.Find(x => x.type != FunctionPartType.Label && x.text == text);
    }

    public IEnumerable<FunctionPart> parameters
    {
        get { return m_parts.Where(x => x.type != FunctionPartType.Label); }
    }

    public int GetPartCount(FunctionPartType type)
    {
        return m_parts.Count(x => x.type == type);
    }

    public Save_FunctionDecl Serialize()
    {
        var saveData = new Save_FunctionDecl();
        saveData.FunctionId = ProtobufUtils.ToByteString(functionId);
        saveData.Parts.AddRange(m_parts.Select(x => new Save_FunctionPart {
            Type = (int)x.type,
            Name = x.text
        }));
        return saveData;
    }

    public static FunctionDeclaration Deserialize(Save_FunctionDecl saveData)
    {
        var decl = new FunctionDeclaration(ProtobufUtils.ToGuid(saveData.FunctionId));
        decl.m_parts.AddRange(saveData.Parts.Select(x => new FunctionPart(x.Name, (FunctionPartType)x.Type)));
        return decl;
    }

    public override bool Equals(object obj)
    {
        var rhs = obj as FunctionDeclaration;
        if (rhs == null)
        {
            return false;
        }

        return Equals(rhs);
    }

    public bool Equals(FunctionDeclaration other)
    {
        if (other == null)
        {
            return false;
        }

        return functionId == other.functionId && m_parts.SequenceEqual(other.m_parts);
    }

    public override int GetHashCode()
    {
        int hash = functionId.GetHashCode();
        return m_parts.Aggregate(hash, (x, y) => x * 31 + y.GetHashCode());
    }
}
