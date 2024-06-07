using DiscountManager;
public class DiscountCodeManagerTests
{
    private readonly DiscountCodeManager _manager;

    public DiscountCodeManagerTests()
    {
        _manager = new DiscountCodeManager("testCodes.dat");
    }

    [Fact]
    public void GenerateCodes_ValidRequest_ReturnsGeneratedCodes()
    {
        ushort count = 5;
        byte length = 6;
        var generatedCodes = _manager.GenerateCodes(count, length);

        Assert.Equal(count, generatedCodes.Count);
        foreach (var code in generatedCodes)
        {
            Assert.NotNull(code.Code);
            Assert.False(code.Used);
            Assert.Equal(length, code.Code.Length);
        }
    }

    [Fact]
    public void GenerateCodes_ExceedMaxCount_ThrowsArgumentException()
    {
        ushort count = 2001; // Exceeds maximum allowed
        byte length = 6;

        Assert.Throws<ArgumentException>(() => _manager.GenerateCodes(count, length));
    }

    [Fact]
    public void UseCode_ValidCode_MarksCodeUsed()
    {
        // Generate a code beforehand
        var code = _manager.GenerateCodes(1, 6).FirstOrDefault();
        Assert.NotNull(code);

        var used = _manager.UseCode(code.Code);
        Assert.True(used);

        // Ensure the code is marked as used
        var usedCode = _manager._codes.FirstOrDefault(c => c.Code == code.Code);
        Assert.NotNull(usedCode);
        Assert.True(usedCode.Used);
    }

    [Fact]
    public void UseCode_InvalidCode_ReturnsFalse()
    {
        var used = _manager.UseCode("INVALID_CODE");
        Assert.False(used);
    }

    [Fact]
    public void UseCode_AlreadyUsedCode_ReturnsFalse()
    {
        // Generate and use a code
        var code = _manager.GenerateCodes(1, 6).FirstOrDefault();
        Assert.NotNull(code);
        _manager.UseCode(code.Code);

        var usedAgain = _manager.UseCode(code.Code);
        Assert.False(usedAgain);
    }
}