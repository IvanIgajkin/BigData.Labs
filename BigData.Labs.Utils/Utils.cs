using System.Globalization;
using BigData.Labs.Models;
using CsvHelper;
using MathNet.Numerics.LinearAlgebra;

namespace BigData.Labs.Utils;

public static class UtilsMethods
{
	public static void ShowRegressModel(IEnumerable<FactorRegressModel> regressModel, IEnumerable<Data> data)
	{
		var model = regressModel.ToList();
		var valueList = data.ToList();
	
		model.ForEach(m => Console.Write($"{m.Factor} |"));
		Console.WriteLine();
	
		valueList.ForEach(d => Console.WriteLine($@"
{d.X1} | {d.X3} | {d.X4} | {d.X5} | {d.X6} | {d.X7} | {d.X8} | {d.X9} | {d.X10} | {d.Y} |"));
		model.ForEach(m => Console.Write($"{m.Summary} |"));
		Console.WriteLine();
	
		model.ForEach(m => Console.Write($"{m.Average} |"));
		Console.WriteLine();
	
		model.ForEach(m => Console.Write($"{m.AvgSqrt} |"));
		Console.WriteLine();
	
		model.ForEach(m => Console.Write($"{m.Dispersion} |"));
		Console.WriteLine();
	}

	public static IEnumerable<FactorRegressModel> GetDataModel(string filePath)
	{
		using var streamReader = File.OpenText(filePath);
		using var csvReader = new CsvReader(streamReader, CultureInfo.CurrentCulture);
		
		var data = csvReader.GetRecords<Data>().ToList();
		
		//get factor regress model based on the input data
		return Enumerable.Range(1, 10)
			.Select(facNo => new FactorRegressModel(data, facNo))
			.ToList();
	}

	public static void SaveDataModel(Matrix<double> matrix)
	{
		var dataAsString = string.Empty;
		for (var row = 0; row < matrix.RowCount; row++)
		{
			for (var col = 0; col < 2; col++)
			{
				dataAsString += $"{matrix[row, col]}" + (col == 0 ? $";" : "\n");
			}
		}
		
		File.WriteAllText($"{Environment.CurrentDirectory}/out.csv", dataAsString);
	}
}