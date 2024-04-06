namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

public class FileHttpContent : MyHttpContent
{
    public FileHttpContent(string name, string fileName, string contentType)
    {
        Name = name;
        FileName = fileName;
        ContentType = contentType;
    }

    public string FileName { get; set; }
}