using System;
using System.IO;
using System.IO.Compression;
using System.Globalization;
using System.Text;
using CncTsp.src.Model;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Xml;

namespace CncTsp.src.Io;

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
            //System.Console.WriteLine(line);

            if (line.Length == 0 || line == "EOF") continue;

            if (line == "NODE_COORD_SECTION") { ReadNodeCoords(reader, inst); break; }
            if (line == "EDGE_WEIGHT_SECTION") { ReadEdgeWeight(reader, inst); break; }


            int idx_delimiter = line.IndexOf(':');
            if (idx_delimiter >= 0)
            {
                var title = line[..idx_delimiter];
                var value = line[(idx_delimiter + 2)..];
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
                        inst.EdgeWeightFormat = ParseWeightFormat(value);
                        break;
                }
            }
        }
        // ShowMatrix(inst);
        return inst;
    }

    public static EdgeWeightType ParseWeightType(string value) => value switch 
    {
        "EUC_2D" => EdgeWeightType.Euc2D,
        "CEIL_2D" => EdgeWeightType.Ceil2D,
        "ATT" => EdgeWeightType.Att,
        "GEO" => EdgeWeightType.Geo,
        "EXPLICIT" => EdgeWeightType.Explicit,
        _ => EdgeWeightType.Unknown
    };

    public static EdgeWeightFormat ParseWeightFormat(string value) => value switch
    {
        "FUNCTION" => EdgeWeightFormat.Function,
        "FULL_MATRIX" => EdgeWeightFormat.FullMatrix,
        "UPPER_ROW" => EdgeWeightFormat.UpperRow,
        "LOWER_ROW" => EdgeWeightFormat.LowerRow,
        "UPPER_DIAG_ROW" => EdgeWeightFormat.UpperDiagRow,
        "LOWER_DIAG_ROW" => EdgeWeightFormat.LowerDiagRow,
        _ => throw new NotImplementedException(),
        /*"UPPER_COL" => EdgeWeightFormat.UpperCol,
        "LOWER_COL" => EdgeWeightFormat.LowerCol,
        "UPPER_DIAG_COL" => EdgeWeightFormat.UpperDiagCol,
        "LOWER_DIAG_COL" => EdgeWeightFormat.LowerDiagCol*/

    };

    public static void ReadNodeCoords(StreamReader reader, TspInstance inst){
        string? raw;
        while ((raw = reader.ReadLine()) != null) {
            var line = raw.Trim();
            if (line.Length == 0 || line == "EOF") break;

            // ожидается: индекс, x, y
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) throw new InvalidDataException($"Invalid coords: '{line}'");
            double x = double.Parse(parts[1], CultureInfo.InvariantCulture);
            double y = double.Parse(parts[2], CultureInfo.InvariantCulture);
            
            inst.Xs.Add(x); inst.Ys.Add(y);
        }
        inst.ExplicitMatrix = DistanceMatrix.Build(inst);
    }

    public static void ReadEdgeWeight(StreamReader reader, TspInstance inst) {
        string? raw;
        double[] matrix = new double[ExpectWeightCount(inst.Dimention, inst.EdgeWeightFormat)];
        int idx = 0;

        while ((raw = reader.ReadLine()) != null) {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            if (line == "EOF") break;

            var coords = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < coords.Length; i++){
                matrix[idx++] = double.Parse(coords[i], CultureInfo.InvariantCulture);
            }
        }
        inst.ExplicitMatrix = ExpandWeights(matrix, inst.Dimention, inst.EdgeWeightFormat);
    }

    public static double[] ExpandWeights(double[] m, int n, EdgeWeightFormat format){
        double[] matrix = new double[n * n];
        int idx = 0;
        switch (format)
        {
            /*case EdgeWeightFormat.Function:
            break;
            case EdgeWeightFormat.UpperCol:
            break;
            case EdgeWeightFormat.LowerCol:
            break;
            case EdgeWeightFormat.UpperDiagCol:
            break;
            case EdgeWeightFormat.LowerDiagCol:
            break;*/

            case EdgeWeightFormat.FullMatrix:
            return m;

            case EdgeWeightFormat.UpperRow:
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++){
                    matrix[i * n + j] = matrix[j * n + i] = m[idx++];
                }
            break;

            case EdgeWeightFormat.LowerRow:
            for (int i = 1; i < n; i++)
                for (int j = 0; j < i; j++){
                    matrix[i * n + j] = matrix[j * n + i] = m[idx++];
                }
            break;

            case EdgeWeightFormat.UpperDiagRow:
            for (int i = 0; i < n; i++)
                for (int j = i; j < n; j++){
                    matrix[i * n + j] = matrix[j * n + i] = m[idx++];
                }
            break;

            case EdgeWeightFormat.LowerDiagRow:
            for (int i = 0; i < n; i++)
                for (int j = 0; j <= i; j++){
                    matrix[i * n + j] = matrix[j * n + i] = m[idx++];
                    // System.Console.WriteLine(matrix[i * n + j]);
                }
            break;

            default:
            throw new NotSupportedException($"Не поддреживает формат: {format}");
        }
        return matrix;
    }

    
    public static void ShowMatrix(TspInstance tsp){
        if (tsp.ExplicitMatrix == null) return;
        int n = tsp.Dimention;
        for (int i = 0; i < n ; i++){
            for (int j = 0; j < n; j++){
                Console.Write($" {tsp.ExplicitMatrix[i * n + j]}");
            }
            Console.WriteLine();
        }
        Console.WriteLine($"Длина матрицы {tsp.ExplicitMatrix.Length}");
    }

    public static int ExpectWeightCount(int n, EdgeWeightFormat format){
        return format switch
        {
            EdgeWeightFormat.FullMatrix => n * n,
            EdgeWeightFormat.UpperRow => (n - 1) * n / 2,
            EdgeWeightFormat.LowerRow => (n - 1) * n / 2,
            EdgeWeightFormat.UpperDiagRow => (n + 1) * n / 2,
            EdgeWeightFormat.LowerDiagRow => (n + 1) * n / 2,
            _ => n * n,
        };
    }
}

// dotnet run --project CncTsp.csproj