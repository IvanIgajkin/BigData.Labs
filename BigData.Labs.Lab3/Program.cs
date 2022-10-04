// See https://aka.ms/new-console-template for more information

using MathNet.Numerics.LinearAlgebra;
using BigData.Labs.Models;
using BigData.Labs.Utils;

var regressModel = UtilsMethods.GetDataModel($"{Environment.CurrentDirectory}/data.csv").ToList();

if (regressModel.Any())
{
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
				if (Math.Pow(determinant, 2) > maxCoefficient)
				{
					maxCoefficient = Math.Pow(determinant, 2);
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
