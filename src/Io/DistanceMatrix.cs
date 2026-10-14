using System.Data.Common;
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
            Euc2D(matrix, n, inst.Xs, inst.Ys);
            break;

            case EdgeWeightType.Ceil2D:
            Ceil2D(matrix, n, inst.Xs, inst.Ys);
            break;

            case EdgeWeightType.Att:
            Att(matrix, n, inst.Xs, inst.Ys);
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

    public static void Euc2D(double[] m, int n, List<double> X, List<double> Y){
        for (int i = 0; i < n; i++){
            m[i * n + i] = .0;
            for (int j = i + 1; j < n; j++){
                int d = FindEuc(X[i], Y[i], X[j], Y[j]);
                m[i * n + j] = m[j * n + i] = d;           
            }
        }
    }

    public static int FindEuc(double x1, double y1, double x2, double y2){
        double dx = x1 - x2;
        double dy = y1 - y2;
        return (int)(Math.Sqrt(dx * dx + dy * dy) + .5);
    }
    
    public static void Ceil2D(double[] m, int n, List<double> X, List<double> Y){
        for (int i = 0; i < n; i++){
            m[i * n + i] = .0;
            for (int j = i + 1; j < n; j++){
                int d = FindCeil2D(X[i], Y[i], X[j], Y[j]);
                m[i * n + j] = m[j * n + i] = d;           
            }
        }        
    }

    public static int FindCeil2D(double x1, double y1, double x2, double y2){
        double dx = x1 - x2;
        double dy = y1 - y2;
        return (int)Math.Ceiling(Math.Sqrt(dx * dx + dy * dy));
    }

    public static void Att(double[] m, int n, List<double> X, List<double> Y){
        for (int i = 0; i < n; i++){
            m[i * n + i] = .0;
            for (int j = i + 1; j < n; j++){
                int d = FindAtt(X[i], Y[i], X[j], Y[j]);
                m[i * n + j] = m[j * n + i] = d;           
            }
        }        
    }

    public static int FindAtt(double x1, double y1, double x2, double y2){
        double dx = x1 - x2;
        double dy = y1 - y2;
        // ceil с шагом 10
        double d = Math.Sqrt((dx * dx + dy * dy) / 10.0);
        int i = (int)(d + .5);
        return i < d ? i + 1 : i;
    }


    private static double ToRadians(double x){
        int deg = (int)(x + 0.5);           // nint
        double min = x - deg;
        return Math.PI * (deg + 5.0 * min / 3.0) / 180.0;
    }

}