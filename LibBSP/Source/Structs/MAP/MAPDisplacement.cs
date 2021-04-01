#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
#define UNITY
#endif

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using LibBSP.Source.Extensions;

namespace LibBSP.Source.Structs.MAP {
#if UNITY
	using Vector3 = UnityEngine.Vector3;
#elif GODOT
	using Vector3 = Godot.Vector3;
#else
	using Vector3 = Vector3;
#endif

	/// <summary>
	/// Class containing all data necessary to render a displacement from Source engine.
	/// </summary>
	[Serializable] public class MapDisplacement {
		static IFormatProvider _format = CultureInfo.CreateSpecificCulture("en-US");

		public int power;
		public Vector3 start;
		public Vector3[,] normals;
		public float[,] distances;
		public float[,] alphas;

		/// <summary>
		/// Creates a new empty <see cref="MapDisplacement"/> object. Internal data will have to be set manually.
		/// </summary>
		public MapDisplacement() { }

		/// <summary>
		/// Constructs a <see cref="MapDisplacement"/> object using the provided <c>string</c> array as the data.
		/// </summary>
		/// <param name="lines">Data to parse.</param>
		public MapDisplacement(string[] lines) {
			Dictionary<int, string[]> normalsTokens = new Dictionary<int, string[]>(5);
			Dictionary<int, string[]> distancesTokens = new Dictionary<int, string[]>(5);
			Dictionary<int, string[]> alphasTokens = new Dictionary<int, string[]>(5);
			int braceCount = 0;
			bool inNormals = false;
			bool inDistances = false;
			bool inAlphas = false;
			foreach (string line in lines) {
				switch (line)
				{
					case "{":
						++braceCount;
						continue;
					case "}":
					{
						--braceCount;
						if (braceCount == 1) {
							inNormals = false;
							inDistances = false;
							inAlphas = false;
						}
						continue;
					}
					case "normals":
						inNormals = true;
						continue;
					case "distances":
						inDistances = true;
						continue;
					case "alphas":
						inAlphas = true;
						continue;
				}

				if (braceCount == 1) {
					string[] tokens = line.SplitUnlessInContainer(' ', '\"', StringSplitOptions.RemoveEmptyEntries);
					switch (tokens[0]) {
						case "power": {
							power = int.Parse(tokens[1]);
							int side = (int)Math.Pow(2, power) + 1;
							normals = new Vector3[side, side];
							distances = new float[side, side];
							alphas = new float[side, side];
							break;
						}
						case "startposition": {
							string[] point = tokens[1].Substring(1, tokens[1].Length - 2).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
							start = new Vector3(float.Parse(point[0], _format), float.Parse(point[1], _format), float.Parse(point[2], _format));
							break;
						}
					}
				} else if (braceCount > 1) {
					if (inNormals) {
						string[] tokens = line.SplitUnlessInContainer(' ', '\"', StringSplitOptions.RemoveEmptyEntries);
						int row = int.Parse(tokens[0].Substring(3));
						string[] points = tokens[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						normalsTokens[row] = points;
					} else if (inDistances) {
						string[] tokens = line.SplitUnlessInContainer(' ', '\"', StringSplitOptions.RemoveEmptyEntries);
						int row = int.Parse(tokens[0].Substring(3));
						string[] nums = tokens[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						distancesTokens[row] = nums;
					} else if (inAlphas) {
						string[] tokens = line.SplitUnlessInContainer(' ', '\"', StringSplitOptions.RemoveEmptyEntries);
						int row = int.Parse(tokens[0].Substring(3));
						string[] nums = tokens[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						alphasTokens[row] = nums;
					}
				}
			}

			if (power == 0) {
				throw new ArgumentException("Bad data given to MAPDisplacement, no power specified!");
			}

			if (start.X() == float.NaN) {
				throw new ArgumentException("Bad data given to MAPDisplacement, no starting point specified!");
			}

			foreach (int i in normalsTokens.Keys) {
				for (int j = 0; j < normalsTokens[i].Length / 3; j++) {
					normals[i, j] = new Vector3(float.Parse(normalsTokens[i][j * 3], _format), float.Parse(normalsTokens[i][(j * 3) + 1], _format), float.Parse(normalsTokens[i][(j * 3) + 2], _format));
					distances[i, j] = float.Parse(distancesTokens[i][j], _format);
					alphas[i, j] = float.Parse(alphasTokens[i][j], _format);
				}
			}
		}

	}
}
