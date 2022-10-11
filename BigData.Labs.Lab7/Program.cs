// See https://aka.ms/new-console-template for more information

using System.Numerics;
using BigData.Labs.Models;
using BigData.Labs.Utils;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

var dataModel = UtilsMethods.GetDataModel($"{Environment.CurrentDirectory}/data.csv").ToList();

if (dataModel.Any())
{
	var normalData = dataModel
		.Select(factor => new FactorRegressModel(
			factor.Values.Select(x => x / factor.Average),
			factor.Factor))
		.ToArray();

	var diffList = new List<List<double>>();
	for (var mainRowIdx = 0; mainRowIdx < normalData.First().Values.Count(); mainRowIdx++)
	{
		var mainRowFactor = normalData.Select(dl => dl.Values.ElementAt(mainRowIdx));
		var tempRow = new List<double>();
		var mainRow = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(mainRowFactor.ToArray());
		for (var rowIdx = 0; rowIdx < normalData.First().Values.Count(); rowIdx++)
		{
			var rowFactor = normalData.Select(dl => dl.Values.ElementAt(rowIdx));
			var row = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(rowFactor.ToArray());
			tempRow.Add((mainRow - row).L2Norm());
		}
		
		diffList.Add(tempRow);
	}

	var diffMatrix = Matrix<double>.Build.DenseOfColumns(diffList).Transpose();
	var allSum = diffList.Select(dl => dl.Sum(d => d * d)).Sum();
	diffList.Clear();
	for (int i = 0; i < diffMatrix.RowCount; i++)
	{
		var temp = new List<double>();
		for (int j = 0; j < diffMatrix.ColumnCount; j++)
		{
			var iSum = diffMatrix.RowSums()[i];
			var jSum = diffMatrix.ColumnSums()[j];
			temp.Add(-0.5 * (Math.Pow(diffMatrix[i, j], 2.0)
			                 - 1.0 / diffMatrix.RowCount * jSum
			                 - 1.0 / diffMatrix.ColumnCount * iSum
			                 + 1.0 / (diffMatrix.RowCount * diffMatrix.ColumnCount) * allSum));
		}
		
		diffList.Add(temp);
	}
	
	diffMatrix = Matrix<double>.Build.DenseOfColumns(diffList);
	
	var ownValues = diffMatrix.Evd().EigenValues;
	var ownVectorsAsMatrix = diffMatrix.Evd().EigenVectors;
	var ownVectors = Enumerable.Empty<MathNet.Numerics.LinearAlgebra.Vector<double>>();
	for (var idx = 0; idx < ownVectorsAsMatrix.RowCount; idx++)
	{
		ownVectors = ownVectors.Append(ownVectorsAsMatrix.Row(idx));
	}

	var x = ownVectors.Select((v, idx) => v.Divide(v.L2Norm()).Select(vk => 
		ownValues.ElementAt(idx).SquareRoot() * new Complex(vk, 0.0)));

	var xn = new List<List<double>>();
	xn.Add(x.ElementAt(1).Select(xk => Math.Abs(xk.Real)).ToList());
	xn.Add(x.ElementAt(2).Select(xk => Math.Abs(xk.Real)).ToList());

	var xAsMatrix = Matrix<double>.Build.DenseOfColumns(xn);
	UtilsMethods.SaveDataModel(xAsMatrix);
}

Console.ReadKey();
