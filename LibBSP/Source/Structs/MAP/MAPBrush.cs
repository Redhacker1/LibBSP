using System;
using System.Collections.Generic;

namespace LibBSP.Source.Structs.MAP {

	/// <summary>
	/// Class containing all data for a single brush, including side definitions or a patch definition.
	/// </summary>
	[Serializable] public class MapBrush {

		public List<MapBrushSide> sides = new List<MapBrushSide>(6);
		public MapPatch patch;
		public MapTerrainEf2 ef2Terrain;
		public MapTerrainMoHaa mohTerrain;

		public bool isDetail;
		public bool isWater = false;
		public bool isManVis = false;

		/// <summary>
		/// Creates a new empty <see cref="MapBrush"/> object. Internal data will have to be set manually.
		/// </summary>
		public MapBrush() { }

		/// <summary>
		/// Creates a new <see cref="MapBrush"/> object using the supplied <c>string</c> array as data.
		/// </summary>
		/// <param name="lines">Data to parse.</param>
		public MapBrush(IList<string> lines) {
			int braceCount = 0;
			bool brushDef3 = false;
			bool inPatch = false;
			bool inTerrain = false;
			List<string> child = new List<string>();
			for (int i = 0; i < lines.Count; ++i) {
				string line = lines[i];

				if (line[0] == '{') {
					braceCount++;
					if (braceCount == 1 || brushDef3) { continue; }
				} else if (line[0] == '}') {
					braceCount--;
					if (braceCount == 0 || brushDef3) { continue; }
				}

				if (braceCount == 1 || brushDef3) {
					// Source engine
					if (line.Length >= "side".Length && line.Substring(0, "side".Length) == "side") {
					}
					// id Tech does this kinda thing
					else if (line.Length >= "patch".Length && line.Substring(0, "patch".Length) == "patch") {
						inPatch = true;
						// Gonna need this line too. We can switch on the type of patch definition, make things much easier.
						child.Add(line);
					} else if (inPatch) {
						child.Add(line);
						inPatch = false;
						patch = new MapPatch(child.ToArray());
						child = new List<string>();
					} else if (line.Length >= "terrainDef".Length && line.Substring(0, "terrainDef".Length) == "terrainDef") {
						inTerrain = true;
						child.Add(line);
					} else if (inTerrain) {
						child.Add(line);
						inTerrain = false;
						// TODO: MoHRadiant terrain
						ef2Terrain = new MapTerrainEf2(child.ToArray());
						child = new List<string>();
					} else if (line.Length >= "brushDef3".Length && line.Substring(0, "brushDef3".Length) == "brushDef3") {
						brushDef3 = true;
					} else if (line == "\"BRUSHFLAGS\" \"DETAIL\"") {
						isDetail = true;
					} else if (line.Length >= "\"id\"".Length && line.Substring(0, "\"id\"".Length) == "\"id\"") {
					} else {
						child.Add(line);
						sides.Add(new MapBrushSide(child.ToArray()));
						child = new List<string>();
					}
				} else if (braceCount > 1) {
					child.Add(line);
				}
			}
		}

	}
}
