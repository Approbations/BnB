using System.Security.Cryptography;
using CncTsp.src.Model;

namespace CncTsp.src.Io;

public readonly struct DistanceMatrix
{
    public static double[] Build(TspInstance inst){
        int n = inst.Dimention;
        var matrix = new double[n * n];

        switch (inst.EdgeWeightType){
            case EdgeWeightType.Explicit:
            if (inst.ExplicitMatrix is null)
            throw new InvalidDataException("Матрица уже должна быть дана");
            return inst.ExplicitMatrix;

            case EdgeWeightType.Euc2D:
            break;

            case EdgeWeightType.Ceil2D:
            break;

            case EdgeWeightType.Att:
            break;

            case EdgeWeightType.Geo:
            Geo(matrix, n, inst.Xs, inst.Ys);
            break;

            default:
            throw new NotSupportedException($"Нет подержки типа: {inst.EdgeWeightType}");
        }
        return matrix;
    }

    public static void Geo(double[] m, int n, List<double> X, List<double> Y){
        for (int i = 0; i < n; i++){
            m[i * n + i] = .0;
            for (int j = i + 1; j < n; j++){
                int d = FindGeo(X[i], Y[i], X[j], Y[j]);
                m[i * n + j] = m[j * n + i] = d;
            }
        }
    }

    public static int FindGeo(double x1, double y1, double x2, double y2){
        // здесь не просто числа с плавающей точкой, а координаты в градусах и минутах
        const double R = 6378.388; // радиус Земли
        double rlat1 = ToRadians(x1);
        double rlon1 = ToRadians(y1);
        double rlat2 = ToRadians(x2);
        double rlon2 = ToRadians(y2);

        double q1 = Math.Cos(rlon1 - rlon2);
        double q2 = Math.Cos(rlat1 - rlat2);
        double q3 = Math.Cos(rlat1 + rlat2);

        return (int)(R * Math.Acos(0.5 * ((1 + q1) * q2 - (1 - q1) * q3)) + 1.0);
    }

    private static double ToRadians(double x){
        int deg = (int)(x + 0.5);           // nint
        double min = x - deg;
        return Math.PI * (deg + 5.0 * min / 3.0) / 180.0;
    }

}