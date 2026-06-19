using System.Text;

namespace FSM.FileHandling;

internal static class FileTokenizer
{
    // Quoted tokens retain their surrounding double-quotes
    internal static List<string> Tokenize(string line)
    {
        List<String> tokens = new List<string>();
        int i = 0;
        while (i < line.Length)
        {
            if (char.IsWhiteSpace(line[i])) { i++; continue; }
            if (line[i] == '#') break;
            if (line[i] == ';') { i++; break; }

            if (line[i] == '"')
            {
                i++;
                var sb = new StringBuilder("\"");

                while (i < line.Length && line[i] != '"') sb.Append(line[i++]);
                if (i < line.Length) i++;

                sb.Append('"');
                tokens.Add(sb.ToString());
            }
            else
            {
                var sb = new StringBuilder();
                while (i < line.Length && !char.IsWhiteSpace(line[i]) && line[i] != ';' && line[i] != '"')
                    sb.Append(line[i++]);

                if (sb.Length > 0) tokens.Add(sb.ToString());
            }
        }
        return tokens;
    }

    internal static bool IsQuoted(string token)
    {
        return token.StartsWith('"');
    }

    internal static string Unquote(string token)
    {
        return token.StartsWith('"') ? token[1..^1] : token;
    }
        
}
