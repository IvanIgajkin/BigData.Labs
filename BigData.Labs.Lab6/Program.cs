// See https://aka.ms/new-console-template for more information

using BigData.Labs.Models;
using BigData.Labs.Utils;
using MathNet.Numerics.LinearAlgebra;

var dataModel = UtilsMethods.GetDataModel($"{Environment.CurrentDirectory}/data.csv").ToList();

double GetSigma(FactorRegressModel factor) =>
	Math.Sqrt(factor.Values.Select(x => Math.Pow(x - factor.Average, 2.0)).Sum() / factor.Values.Count());

if (dataModel.Any())
{
	var normalData = dataModel
		.Select(factor => new FactorRegressModel(
			factor.Values.Select(x => (x - factor.Average) / GetSigma(factor)),
			factor.Factor))
		.ToArray();

	var covList = new List<List<double>>();
	for (var rowIdx = 0; rowIdx < normalData.Length; rowIdx++)
	{
		var rowFactor = normalData.ElementAt(rowIdx);
		var tempRow = new List<double>();
		for (var colIdx = 0; colIdx < normalData.Length; colIdx++)
		{
			var columnFactor = normalData.ElementAt(colIdx);
			tempRow.Add((rowFactor * columnFactor).Summary / rowFactor.Values.Count());
		}
		
		covList.Add(tempRow);
	}

	var covMatrix = Matrix<double>.Build.DenseOfColumns(covList);
	IEnumerable<double> ownValues = covMatrix.Evd().EigenValues.Select(e => e.Real).ToList();
	var ownVectorsAsMatrix = covMatrix.Evd().EigenVectors;
	var ownVectors = Enumerable.Empty<Vector<double>>();
	for (var idx = 0; idx < ownVectorsAsMatrix.RowCount; idx++)
	{
		ownVectors = ownVectors.Append(ownVectorsAsMatrix.Row(idx));
	}

	ownVectors = ownVectors.ToArray();
	var vectors = ownVectors;
	var tempDict = new Dictionary<double, Vector<double>>(ownValues
			.Select((val, idx) => new KeyValuePair<double, Vector<double>>(val, vectors.ElementAt(idx)))
			.OrderByDescending(pair => pair.Key));

	ownValues = tempDict.Keys;
	ownVectors = tempDict.Values;

	var upLimitIdx = 0;
	do
	{
		if (upLimitIdx == 2)
		{
			break;
		}

		var disperssion = 
			ownValues.Select((value, idx) => idx <= upLimitIdx ? value : 0.0).Sum() / ownValues.Sum();

		upLimitIdx++;

	} while (true);

	var mainVectors = new List<Vector<double>>();
	for (var idx = 0; idx < upLimitIdx; idx++)
	{
		mainVectors.Add(ownVectors.ElementAt(idx));
	}

	var mainVectorsAsMtrx = Matrix<double>.Build.DenseOfColumns(mainVectors);

	var dataList = dataModel.Select(f => f.Values);
	var dataAsMtrx = Matrix<double>.Build.DenseOfColumns(dataList);

	var reducedMtrx = dataAsMtrx * mainVectorsAsMtrx;
	UtilsMethods.SaveDataModel(reducedMtrx);
}

Console.ReadKey();
