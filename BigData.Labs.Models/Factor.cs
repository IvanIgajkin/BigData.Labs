namespace BigData.Labs.Models;

public class FactorRegressModel
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
	
	public FactorRegressModel(IEnumerable<double> values, FactorSymbol factor)
	{
		_values = values;
		_values2 = _values.Select(value => Math.Pow(value, 2.0));
		Factor = factor;
	}
	
}