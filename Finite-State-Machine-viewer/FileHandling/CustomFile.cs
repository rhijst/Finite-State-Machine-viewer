namespace FSM.FileHandling;

public class CustomFile
{
    public string Content { get; }
    public string Path { get; }
    public string Extension { get; } // # TODO extensie nodig?

    public CustomFile(string content, string path, string extension)
    {
        Content = content;
        Path = path;
        Extension = extension;
    }
}
