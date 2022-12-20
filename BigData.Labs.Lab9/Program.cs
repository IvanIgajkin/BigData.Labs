using BigData.Labs.Models;
using BigData.Labs.Utils;
using MathNet.Numerics.LinearAlgebra;

const double eps = 1e-2;

IEnumerable<IEnumerable<double>> GetByMcMethod()
{
	var dataModel = UtilsMethods.GetDataModel($"{Environment.CurrentDirectory}/data.csv").ToList();

	double GetSigma(FactorRegressModel factor) =>
		Math.Sqrt(factor.Values.Select(x => Math.Pow(x - factor.Average, 2.0)).Sum() / factor.Values.Count());
	
	var resultAsList = new List<List<double>>();

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
		
		for (var row = 0; row < reducedMtrx.RowCount; row++)
		{
			var tmp = new List<double>();
			for (var col = 0; col < reducedMtrx.ColumnCount; col++)
			{
				tmp.Add(reducedMtrx[row, col]);
			}
			
			resultAsList.Add(tmp);
		}
	}

	return resultAsList;
}

var mainData = GetByMcMethod();
if (mainData.Any())
{
	var centroids = mainData
		.Select((md, idx) => idx % 6 == 0 ? md : null)
		.Where(x => x != null) as IEnumerable<IEnumerable<double>>;

	var flags = centroids.Select(cd => true);
	var iter = 0;

	do
	{
		var groups = new Dictionary<int, IEnumerable<IEnumerable<double>>>();
		for (var key = 0; key < centroids.Count(); key++)
		{
			groups.Add(key, new[] { centroids.ElementAt(key) });
		}

		for (var idx = 0; idx < mainData.Count(); idx++)
		{
			if (idx % 6 != 0)
			{
				var md = mainData.ElementAt(idx).ToArray();
				var dx = double.MaxValue;
				var gKey = default(int);
				for (var cidx = 0; cidx < centroids.Count(); cidx++)
				{
					var cd = centroids.ElementAt(cidx).ToArray();
					var tmp = Math.Sqrt(Math.Pow(cd[0] - md[0], 2) + Math.Pow(cd[0] - md[0], 2));
					if (tmp < dx)
					{
						dx = tmp;
						gKey = cidx;
					}
				}

				groups[gKey] = groups[gKey].Append(md);
			}
		}

		var newCentroids = groups.Select(g => new[]
		{
			g.Value.Average(x => x.ElementAt(0)), g.Value.Average(x => x.ElementAt(1))
		});
		
		var oldCentroids = centroids;
		flags = newCentroids.Select((centroid, idx) =>
		{
			var newCd = centroid.ToArray();
			var cd = oldCentroids.ElementAt(idx).ToArray();
			return Math.Abs(cd[0] - newCd[0]) > eps || Math.Abs(cd[1] - newCd[1]) > eps;
		});
		
		centroids = newCentroids;
		iter++;

	} while (flags.Contains(true));

	UtilsMethods.SaveDataModel(centroids);
}
