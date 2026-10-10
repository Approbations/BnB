using System;
using System.IO;
using System.IO.Compression;
using System.Globalization;
using System.Text;
using CncTsp.Model;
using System.Security.AccessControl;
using System.Security.Cryptography;

namespace CncTsp.Io;

public static class TsplibParser
{
    public static TspInstance Parse(string path)
    {
        var inst = new TspInstance();
        string? raw;

        using var fileStream = File.OpenRead(path);
        using var GzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        using var reader = new StreamReader(GzipStream, Encoding.UTF8);

        while ((raw = reader.ReadLine()) != null)
        {
            var line = raw.Trim();

            if (line.Length == 0 || line == "EOF") continue;

            if (line == "NODE_COORD_SECTION") { ReadNodeCoords(reader, inst); break; }
            if (line == "EDGE_WEIGHT_SECTION") { ReadEdgeWeight(reader, inst); break; }


            int idx_delimiter = line.IndexOf(':');
            if (idx_delimiter < 0)
            {
                var title = line[..idx_delimiter];
                var value = line[idx_delimiter..];
                switch (title)
                {
                    case "NAME":
                        inst.Name = value;
                        break;
                    case "TYPE":
                        inst.Type = value;
                        break;
                    case "COMMENT":
                        break;
                    case "DIMENSION":
                        inst.Dimention = int.Parse(value);
                        break;
                    case "EDGE_WEIGHT_TYPE":
                        inst.EdgeWeightType = ParseWeightType(value);
                        break;
                    case "EDGE_WEIGHT_FORMAT":
                        inst.EdgeWeightFormat = value;
                        break;
                }
            }
        }

        return inst;
    }

    public static EdgeWeightType ParseWeightType(string value) => value switch 
    {
        "EUC_2D" => EdgeWeightType.Euc2D,
        "CEIL_2D" => EdgeWeightType.Ceil2D,
        "Att" => EdgeWeightType.Att,
        "Geo" => EdgeWeightType.Geo,
        "Explicit" => EdgeWeightType.Explicit,
        _ => EdgeWeightType.Unknown
    };

    public static void ReadNodeCoords(StreamReader reader, TspInstance inst){
        string? raw;
        while ((raw = reader.ReadLine()) != null) {
            var line = raw.Trim();
            if (line.Length == 0 || line == "EOF") return;

            // ожидается: индекс, x, y
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) throw new InvalidDataException($"Invalid coords: '{line}'");
            double x = double.Parse(parts[1], CultureInfo.InvariantCulture);
            double y = double.Parse(parts[2], CultureInfo.InvariantCulture);
            
            inst.Xs.Add(x); inst.Ys.Add(y);
        }
    }

    public static void ReadEdgeWeight(StreamReader reader, TspInstance inst) {
        string? raw;
        while ((raw = reader.ReadLine()) != null) {
            var line = raw.Trim();
            if (line.Length == 0 || line == "EOF") return;

            // ожидается: 
        }
    }
}

// dotnet run --project CncTsp.csproj