using System.Globalization;

namespace BlockyCatTree.Mesh;

/// <summary>
/// Multiple <see cref="BuildItem"/>s can use the same <see cref="Solid"/>,
/// this is used to identify solids.
/// </summary>
/// <param name="Value">3MF spec requires range 1 .. 2147483647 inclusive</param>
public readonly record struct ObjectId(int Value) : IComparable<ObjectId>
{
    public static readonly ObjectId FirstId = new (1);

    public ObjectId(string s) : this(int.Parse(s))
    {
    }
   
    public int CompareTo(ObjectId other) => Value.CompareTo(other.Value);
    public ObjectId Next => new ObjectId(Value + 1);
    public string AsString => Value.ToString(CultureInfo.InvariantCulture);
    public override string ToString() => AsString;
}
