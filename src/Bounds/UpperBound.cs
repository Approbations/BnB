using System.Diagnostics.Metrics;
using System.Numerics;
using CncTsp.src.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;

namespace CncTsp.src.Bounds;

class UpperBound
{
    public void FindUpperBound(TspInstance inst){

    }

    public static List<(int u, int v, double weight)>? MST(int n, double[] matrix){
        // алгоритм Прима
        var result = new List<(int, int, double)>(n - 1);

        const double INF = double.MaxValue;
        bool[] inMST = new bool[n];
        double[] key = new double[n];
        int[] parent = new int[n];

        for (int i = 0; i < n; i++){
            key[i] = INF;
            parent[i] = -1;
        }

        key[0] = 0;

        for (int iter = 0; iter < n; iter++){
            double minimum = INF;
            int closestVertex = -1;
            for (int i = 0; i < n; i++){
                if (!inMST[i] && key[i] < minimum){
                    minimum = key[i];
                    closestVertex = i;
                }
            }

            // проверка на связность
            if (closestVertex == -1) return null;

            inMST[closestVertex] = true;

            if (parent[closestVertex] != -1) result.Add((parent[closestVertex], closestVertex, key[closestVertex]));

            int rowOffset = closestVertex * n;

            for (int v = 0; v < n; v++){
                double weight = matrix[rowOffset + v];

                if (v == closestVertex || inMST[v] || weight <= 0) continue;

                if (weight < key[v]){
                    key[v] = weight;
                    parent[v] = closestVertex;
                }
            }
        }
        return result;
    }

    public static double GetWeight(List<(int u, int v, double weight)> mst){
        double sum = 0;
        foreach (var (u, v, weight) in mst)
            sum += weight;
        return sum;
    }

    public void DFS(){
        
    }


}