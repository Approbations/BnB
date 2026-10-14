// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using CncTsp.src.Bounds;
using CncTsp.src.Io;
using CncTsp.src.Model;
string path = Path.Combine(Directory.GetCurrentDirectory(), "tests", "burma14.tsp.gz");
TspInstance inst = TsplibParser.Parse(path);
// TspInstance inst = TsplibParser.Parse(path);
if (inst.ExplicitMatrix == null) return;
TsplibParser.ShowMatrix(inst);
var mst = UpperBound.MST(inst.Dimention, inst.ExplicitMatrix);
if (mst == null){
    Console.WriteLine("Граф несвязный — MST не существует");
    return;
}
Console.WriteLine("Рёбра MST:");
foreach (var (u, v, w) in mst)
    Console.WriteLine($"  {u} -- {v}  (вес = {w})");

Console.WriteLine($"Суммарный вес: {UpperBound.GetWeight(mst)}");
    