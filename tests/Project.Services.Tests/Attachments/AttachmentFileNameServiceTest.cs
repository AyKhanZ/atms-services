using ATMS.Project.Services.Attachments;

namespace Project.Services.Tests.Attachments;

public class AttachmentFileNameServiceTest
{
    private readonly AttachmentFileNameService _service = new();

    [Theory]
    [InlineData("Report.PDF", "pdf", "Report.pdf")]
    [InlineData("ТЗ интеграция v2.docx", "docx", "ТЗ интеграция v2.docx")]
    [InlineData(@"C:\Users\me\Desktop\plan.xlsx", "xlsx", "plan.xlsx")]
    [InlineData("../../etc/passwd.txt", "txt", "passwd.txt")]
    [InlineData("bad:na*me?.csv", "csv", "badname.csv")]
    [InlineData("  spaced  .txt", "txt", "spaced.txt")]
    [InlineData(".pdf", "pdf", "file.pdf")]
    [InlineData("???.zip", "zip", "file.zip")]
    [InlineData("archive.tar.zip", "zip", "archive.tar.zip")]
    public void FromUpload_KeepsOnlyASafeNameWithTheCheckedExtension(string uploaded, string extension, string expected)
    {
        Assert.Equal(expected, _service.FromUpload(uploaded, extension));
    }

    [Fact]
    public void FromUpload_WhenNameIsTooLong_CutsTheBaseNameAndKeepsTheExtension()
    {
        var name = _service.FromUpload(new string('a', 300) + ".pdf", "pdf");

        Assert.Equal(_service.MaxBaseNameLength + ".pdf".Length, name.Length);
        Assert.EndsWith(".pdf", name);
    }

    [Theory]
    [InlineData("New name", "old.pdf", "New name.pdf")]
    [InlineData("  trimmed ", "old.docx", "trimmed.docx")]
    [InlineData("still.a.pdf", "old.exe.pdf", "still.a.pdf.pdf")]
    public void Rename_KeepsTheOriginalExtension(string newBaseName, string current, string expected)
    {
        Assert.Equal(expected, _service.Rename(newBaseName, current));
    }

    [Theory]
    [InlineData("plain name", false)]
    [InlineData("Отчёт 2026", false)]
    [InlineData("a/b", true)]
    [InlineData(@"a\b", true)]
    [InlineData("a:b", true)]
    [InlineData("a|b", true)]
    [InlineData("a\tb", true)]
    public void HasInvalidCharacters_FindsCharactersAFileNameCannotHave(string baseName, bool invalid)
    {
        Assert.Equal(invalid, _service.HasInvalidCharacters(baseName));
    }
}
