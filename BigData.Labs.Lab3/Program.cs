// See https://aka.ms/new-console-template for more information

using System.Globalization;
using CsvHelper;
using MathNet.Numerics.LinearAlgebra;

using var streamReader = File.OpenText("C:/Users/igayk/Repos/BigData.Labs/BigData.Labs.Lab3/data.csv");
using var csvReader = new CsvReader(streamReader, CultureInfo.CurrentCulture);

var data = csvReader.GetRecords<Data>().ToList();
if (data.Any())
{
	//get factor regress model based on the input data
	var regressModel = Enumerable.Range(1, 10)
		.Select(facNo => new FactorRegressModel(data, facNo))
		.ToList();

	var endogenousFactor = regressModel.FirstOrDefault(m => m.Factor == FactorRegressModel.FactorSymbol.Y);
	if (endogenousFactor == null)
		throw new ApplicationException($"Regress model doesn't contain resulting data");
	
	double rDeterminationPast = default;
	var regressors = new List<FactorRegressModel>();
	int iterationNo = default;
	while (iterationNo < regressModel.Count - 1)
	{
		FactorRegressModel? nextRegressor = null;
		var maxCoefficient = double.MinValue;
		if (endogenousFactor.Factor == FactorRegressModel.FactorSymbol.Y)
		{
			foreach (var regressor in regressModel)
			{
				if (regressor.Factor == FactorRegressModel.FactorSymbol.Y)
				{
					continue;
				}

				var correlation = endogenousFactor.Correlliation(regressor);
				if (correlation > maxCoefficient)
				{
					maxCoefficient = correlation;
					nextRegressor = regressor;
				}
			}
		}
		else
		{
			foreach (var regressor in regressModel)
			{
				if (regressor.Factor == FactorRegressModel.FactorSymbol.Y || regressors.Contains(regressor))
				{
					continue;
				}

				var tmpRegressorList = regressors.Append(regressor).ToList();
				var correlationLists = tmpRegressorList.Select(r => tmpRegressorList.Select(r.Correlliation));
				var determinant = Matrix<double>.Build.DenseOfColumns(correlationLists).Determinant();
				if (determinant > maxCoefficient)
				{
					maxCoefficient = determinant;
					nextRegressor = regressor;
				}
			}
		}

		if (nextRegressor == null || maxCoefficient < rDeterminationPast)
		{
			break;
		}
		
		regressors.Add(nextRegressor);
		endogenousFactor = regressors.Count == 1
			? nextRegressor
			: regressors.Aggregate((l, r) => l * r);
		
		rDeterminationPast = maxCoefficient;

		iterationNo++;
	};
	
	regressors.ForEach(r => Console.WriteLine($"Factor with index {r.Factor} is included"));
}

Console.ReadKey();

static void ShowRegressModel(IEnumerable<FactorRegressModel> regressModel, IEnumerable<Data> data)
{
	var model = regressModel.ToList();
	var valueList = data.ToList();
	
	model.ForEach(m => Console.Write($"{m.Factor} |"));
	Console.WriteLine();
	
	valueList.ForEach(d => Console.WriteLine($@"
{d.X1} | {d.X3} | {d.X4} | {d.X5} | {d.X6} | {d.X7} | {d.X8} | {d.X9} | {d.X10} | {d.Y} |
"));
	model.ForEach(m => Console.Write($"{m.Summary} |"));
	Console.WriteLine();
	
	model.ForEach(m => Console.Write($"{m.Average} |"));
	Console.WriteLine();
	
	model.ForEach(m => Console.Write($"{m.AvgSqrt} |"));
	Console.WriteLine();
	
	model.ForEach(m => Console.Write($"{m.Dispersion} |"));
	Console.WriteLine();
}

record Data(double X1, double X3, double X4, 
			double X5, double X6, double X7, 
			double X8, double X9 , double X10, 
			double Y);

class FactorRegressModel
{
	public enum FactorSymbol
	{
		X1, X2, X4,
		X3, X5, X6,
		X7, X8, X9,
		Y, Multiple
	}
	
	private readonly IEnumerable<double> _values;
	private readonly IEnumerable<double> _values2;

	public IEnumerable<double> Values => _values;
	public FactorSymbol Factor { get; }

	public double Summary => _values.Sum();
	public double Average => _values.Average();

	public double Dispersion => _values2.Average() - Math.Pow(Average, 2.0);
	public double AvgSqrt => Math.Sqrt(Dispersion);

	public FactorRegressModel(IEnumerable<Data> data, int factorNo)
	{
		_values = factorNo switch
		{
			1 => data.Select(d => d.X1),
			2 => data.Select(d => d.X3),
			3 => data.Select(d => d.X4),
			4 => data.Select(d => d.X5),
			5 => data.Select(d => d.X6),
			6 => data.Select(d => d.X7),
			7 => data.Select(d => d.X8),
			8 => data.Select(d => d.X9),
			9 => data.Select(d => d.X10),
			10 => data.Select(d => d.Y),
			_ => throw new ArgumentOutOfRangeException(nameof(factorNo), factorNo, null)
		};
		
		Factor = factorNo switch
		{
			1 => FactorSymbol.X1,
			2 => FactorSymbol.X2,
			3 => FactorSymbol.X3,
			4 => FactorSymbol.X4,
			5 => FactorSymbol.X5,
			6 => FactorSymbol.X6,
			7 => FactorSymbol.X7,
			8 => FactorSymbol.X8,
			9 => FactorSymbol.X9,
			10 => FactorSymbol.Y,
			_ => throw new ArgumentOutOfRangeException(nameof(factorNo), factorNo, null)
		};

		_values2 = _values.Select(value => Math.Pow(value, 2.0));
	}

	public double Correlliation(FactorRegressModel right)
	{
		var multipleFactor = this * right;
		return (multipleFactor.Average - Average * right.Average) / (AvgSqrt * right.AvgSqrt);
	}

	public static FactorRegressModel operator *(FactorRegressModel leftFactor, FactorRegressModel rightFactor)
	{
		var count = leftFactor._values.Count();
		var values = new double[count];
		for (int i = 0; i < count; i++)
		{
			values[i] = leftFactor._values.ElementAt(i) * rightFactor._values.ElementAt(i);
		}

		return new FactorRegressModel(values, FactorSymbol.Multiple);
	}
	
	private FactorRegressModel(IEnumerable<double> values, FactorSymbol factor)
	{
		_values = values;
		_values2 = _values.Select(value => Math.Pow(value, 2.0));
		Factor = factor;
	}
	
}
