using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Metadata;
using System.Net;

namespace CncTsp.src.Model;

public enum EdgeWeightType
{
    Unknown,
    Euc2D,
    Ceil2D,
    Att,
    Geo,
    Explicit
}

public enum EdgeWeightFormat
{
    Function,
    FullMatrix,
    UpperRow,
    LowerRow,
    UpperDiagRow,
    LowerDiagRow,
    /*UpperCol,
    LowerCol,
    UpperDiagCol,
    LowerDiagCol*/
}

public sealed class TspInstance
{
    public string Name {get; set; } = "";
    public string Type {get; set; } = "";
    public int Dimention {get; set;}
    public EdgeWeightType EdgeWeightType {get; set;} = EdgeWeightType.Unknown;
    public EdgeWeightFormat EdgeWeightFormat {get; set;} = EdgeWeightFormat.FullMatrix;

    // координаты точек
    public List<double> Xs {get;} = [];
    public List<double> Ys {get;} = [];

    // плоская матрица n*n
    public double[]? ExplicitMatrix { get; set;}

    // эталонный оптимум
    // public int? OptimalLength { get; set; }
}