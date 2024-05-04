using BlockyCatTree;

namespace BlockCatTree.Test;

public class SliceToTextTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestRoundTrip()
    {
        var input = new []
        {
            "#  #",
            "# o ",
            "#   ",
        };
        var slice = ArrayToSlice.Make(input, ConvertToSlice);
        var output = SliceToText.Make(slice, ConvertFromSlice).ToArray();
        CollectionAssert.AreEqual(input, output);
        return;
        int? ConvertToSlice(char c) => c switch
            {
                '#' => 1,
                'o' => 0,
                ' ' => null,
                _ => throw new Exception($"unexpected char '{c}'")
            };
        char ConvertFromSlice(int? i) => i switch
        {
            null => ' ',
            1 => '#',
            0 => 'o',
            _ => throw new Exception($"unexpected payload '{i}'")
        };
    }
}