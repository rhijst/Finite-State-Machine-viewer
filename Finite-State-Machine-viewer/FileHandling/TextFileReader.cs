namespace FSM.FileHandling;

public class TextFileReader : IFileReader
{
    public CustomFile Read(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found: {path}");

        var content = File.ReadAllText(path);
        var extension = System.IO.Path.GetExtension(path).TrimStart('.');
        return new CustomFile(content, path, extension);
    }
}
