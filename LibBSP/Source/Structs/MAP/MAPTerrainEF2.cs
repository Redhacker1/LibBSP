#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
#define UNITY
#endif

using System;
using System.Globalization;
using System.Numerics;

namespace LibBSP.Source.Structs.MAP {
#if UNITY
	using Vector4 = UnityEngine.Vector4;
	using Vector3 = UnityEngine.Vector3;
#elif GODOT
	using Vector4 = Godot.Quat;
	using Vector3 = Godot.Vector3;
#else
	using Vector3 = Vector3;
	using Vector4 = Vector4;
#endif

	/// <summary>
	/// Class containing all data necessary to render a Terrain from Star Trek EF2.
	/// </summary>
	[Serializable] public class MapTerrainEf2 {
		static IFormatProvider _format = CultureInfo.CreateSpecificCulture("en-US");

		public int side;
		public string texture;
		public float textureShiftS;
		public float textureShiftT;
		public float texRot;
		public float texScaleX;
		public float texScaleY;
		public int flags;
		public float sideLength;
		public Vector3 start;
		public Vector4 @if;
		public Vector4 lf;
		public float[,] heightMap;
		public float[,] alphaMap;

		/// <summary>
		/// Creates a new empty <see cref="MapTerrainEf2"/> object. Internal data will have to be set manually.
		/// </summary>
		public MapTerrainEf2() { }

		/// <summary>
		/// Constructs a new <see cref="MapTerrainEf2"/> object using the supplied string array as data.
		/// </summary>
		/// <param name="lines">Data to parse.</param>
		public MapTerrainEf2(string[] lines) {

			texture = lines[2];

			switch (lines[0]) {
				case "terrainDef": {
					for (int i = 2; i < lines.Length; ++i) {
						string[] line = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						switch (line[0]) {
							case "TEX(": {
								texture = line[1];
								textureShiftS = float.Parse(line[2], _format);
								textureShiftT = float.Parse(line[3], _format);
								texRot = float.Parse(line[4], _format);
								texScaleX = float.Parse(line[5], _format);
								texScaleY = float.Parse(line[6], _format);
								flags = int.Parse(line[8]);
								break;
							}
							case "TD(": {
								sideLength = int.Parse(line[1], _format);
								start = new Vector3(float.Parse(line[2], _format), float.Parse(line[3], _format), float.Parse(line[4], _format));
								break;
							}
							case "IF(": {
								@if = new Vector4(float.Parse(line[1], _format), float.Parse(line[2], _format), float.Parse(line[3], _format), float.Parse(line[4], _format));
								break;
							}
							case "LF(": {
								lf = new Vector4(float.Parse(line[1], _format), float.Parse(line[2], _format), float.Parse(line[3], _format), float.Parse(line[4], _format));
								break;
							}
							case "V(": {
								++i;
								line = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
								if (side == 0) {
									side = line.Length;
								}
								heightMap = new float[side, side];
								for (int j = 0; j < side; ++j) {
									for (int k = 0; k < side; ++k) {
										heightMap[j, k] = float.Parse(line[k], _format);
									}
									++i;
									line = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
								}
								break;
							}
							case "A(": {
								++i;
								line = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
								if (side == 0) {
									side = line.Length;
								}
								alphaMap = new float[side, side];
								for (int j = 0; j < side; ++j) {
									for (int k = 0; k < side; ++k) {
										alphaMap[j, k] = float.Parse(line[k], _format);
									}
									++i;
									line = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
								}
								break;
							}
						}
					}
					break;
				}
				default: {
					throw new ArgumentException($"Unknown terrain type {lines[0]}!");
				}
			}

		}

	}
}
