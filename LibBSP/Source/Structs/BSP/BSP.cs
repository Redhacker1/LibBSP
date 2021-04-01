#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5 || UNITY_5_3_OR_NEWER
#define UNITY
#if !UNITY_5_6_OR_NEWER
#define OLDUNITY
#endif
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using LibBSP.Source.Attributes;
using LibBSP.Source.Extensions;
using LibBSP.Source.Structs.BSP.Lumps;
using LibBSP.Source.Structs.Common;
using LibBSP.Source.Structs.Common.Lumps;
using LibBSP.Source.Util;
using CustomAttributeExtensions = LibBSP.Source.Extensions.CustomAttributeExtensions;

namespace LibBSP.Source.Structs.BSP {
#if UNITY
	using Plane = UnityEngine.Plane;
#if !OLDUNITY
	using Vertex = UnityEngine.UIVertex;
#endif
#elif GODOT
	using Plane = Godot.Plane;
#else
	using Plane = Plane;
#endif

	/// <summary>
	/// Enum of the known different map formats.
	/// </summary>
	public enum MapType
	{
		Undefined = 0,
		Quake = 29,
		// TYPE_GOLDSRC = 30, // Uses mostly the same structures as Quake
		Nightfire = 42,
		Vindictus = 346131372,
		Stef2 = 556942937,
		Mohaa = 892416069,
		// TYPE_MOHBT = 1095516506, // Similar enough to MOHAA to use the same structures
		Stef2Demo = 1263223129,
		Fakk = 1263223152,
		TacticalInterventionEncrypted = 1268885814,
		CoD2 = 1347633741,
		SiN = 1347633747, // The headers for SiN and Jedi Outcast are exactly the same
		Raven = 1347633748,
		CoD4 = 1347633759,
		Source17 = 1347633767,
		Source18 = 1347633768,
		Source19 = 1347633769,
		Source20 = 1347633770,
		Source21 = 1347633771,
		Source22 = 1347633772,
		Source23 = 1347633773,
		L4D2 = 1347633774,
		Quake2 = 1347633775,
		Source27 = 1347633777,
		Daikatana = 1347633778,
		SoF = 1347633782, // Uses the same header as Q3.
		Quake3 = 1347633783,
		// TYPE_RTCW = 1347633784, // Uses same structures as Quake 3
		CoD = 1347633796,
		Titanfall = 1347633807,
		DMoMaM = 1347895914,
	}

	/// <summary>
	/// Struct containing basic information for a lump in a BSP file.
	/// </summary>
	public struct LumpInfo {
		public int ident;
		public int flags;
		public int version;
		public int offset;
		public int length;
		public FileInfo lumpFile;
	}

	/// <summary>
	/// Holds data for any and all supported BSP formats. Any unused lumps in a given format
	/// will be left as null.
	/// </summary>
	public class Bsp : Dictionary<int, LumpInfo> {
		MapType _version;

		// Map structures
		// Quake 1/GoldSrc
		Entities _entities;
		Lump<Plane> _planes;
		Textures _textures;
		Lump<Vertex> _vertices;
		Lump<Node> _nodes;
		Lump<TextureInfo> _texInfo;
		Lump<Face> _faces;
		Lump<Leaf> _leaves;
		NumList _markSurfaces;
		Lump<Edge> _edges;
		NumList _surfEdges;

		Lump<Model> _models;
		// public byte[] pvs;
		// Quake 2
		Lump<Brush> _brushes;
		Lump<BrushSide> _brushSides;

		NumList _markBrushes;
		// MoHAA
		Lump<LodTerrain> _lodTerrains;

		Lump<StaticModel> _staticModels;
		// CoD
		Lump<Patch> _patches;
		Lump<Vertex> _patchVerts;
		NumList _patchIndices;

		NumList _leafPatches;
		// Nightfire
		Textures _materials;

		NumList _indices;
		// Source
		Lump<Face> _originalFaces;
		NumList _texTable;
		Lump<TextureData> _texDatas;
		Lump<Displacement> _dispInfos;
		Lump<DisplacementVertex> _dispVerts;

		NumList _displacementTriangles;
		// public SourceOverlays overlays;
		Lump<Cubemap> _cubemaps;
		GameLump _gameLump;
		StaticProps _staticProps;

		/// <summary>
		/// The <see cref="BspReader"/> object in use by this <see cref="Bsp"/> class.
		/// </summary>
		public BspReader Reader { get; private set; }

		/// <summary>
		/// The version of this BSP. Do not change this unless you want to force reading a BSP as a certain format.
		/// </summary>
		public MapType Version {
			get {
				if (_version == MapType.Undefined) {
					_version = Reader.GetVersion();
				}
				return _version;
			}
			set => _version = value;
		}

		/// <summary>
		/// Is the BSP file in big endian format?
		/// </summary>
		public bool BigEndian => Reader.BigEndian;

		/// <summary>
		/// The <see cref="Common.Lumps.Entities"/> object in the BSP file, if available.
		/// </summary>
		public Entities Entities {
			get {
				if (_entities == null) {
					int index = Entity.GetIndexForLump(Version);
					if (index >= 0) {
						_entities = Entity.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _entities;
			}
		}

		/// <summary>
		/// A <see cref="Lump{Plane}"/> of <see cref="Plane"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<Plane> Planes {
			get {
				if (_planes == null) {
					int index = PlaneExtensions.GetIndexForLump(Version);
					if (index >= 0) {
						_planes = PlaneExtensions.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _planes;
			}
		}

		/// <summary>
		/// The <see cref="Lumps.Textures"/> object in the BSP file, if available.
		/// </summary>
		public Textures Textures {
			get {
				if (_textures == null) {
					int index = Texture.GetIndexForLump(Version);
					if (index >= 0) {
						_textures = Texture.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _textures;
			}
		}

		/// <summary>
		/// A <see cref="Lump{Vertex}"/> of <see cref="Vertex"/> objects in the BSP file representing the vertices of the BSP, if available.
		/// </summary>
		public Lump<Vertex> Vertices {
			get {
				if (_vertices == null) {
					int index = VertexExtensions.GetIndexForLump(Version);
					if (index >= 0) {
						_vertices = VertexExtensions.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _vertices;
			}
		}

		/// <summary>
		/// A <see cref="Lump{Node}"/> of <see cref="Node"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<Node> Nodes {
			get {
				if (_nodes == null) {
					int index = Node.GetIndexForLump(Version);
					if (index >= 0) {
						_nodes = Node.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _nodes;
			}
		}

		/// <summary>
		/// A <see cref="Lump{TextureInfo}"/> of <see cref="TextureInfo"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<TextureInfo> TexInfo {
			get {
				if (_texInfo == null) {
					int index = TextureInfo.GetIndexForLump(Version);
					if (index >= 0) {
						_texInfo = TextureInfo.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _texInfo;
			}
		}

		/// <summary>
		/// A <see cref="Lump{Face}"/> of <see cref="Face"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<Face> Faces {
			get {
				if (_faces == null) {
					int index = Face.GetIndexForLump(Version);
					if (index >= 0) {
						_faces = Face.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _faces;
			}
		}

		/// <summary>
		/// A <see cref="Lump{Leaf}"/> of <see cref="Leaf"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<Leaf> Leaves {
			get {
				if (_leaves == null) {
					int index = Leaf.GetIndexForLump(Version);
					if (index >= 0) {
						_leaves = Leaf.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _leaves;
			}
		}

		/// <summary>
		/// A <see cref="Lump{Edge}"/> of <see cref="Edge"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<Edge> Edges {
			get {
				if (_edges == null) {
					int index = Edge.GetIndexForLump(Version);
					if (index >= 0) {
						_edges = Edge.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _edges;
			}
		}

		/// <summary>
		/// A <see cref="Lump{Model}"/> of <see cref="Model"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<Model> Models {
			get {
				if (_models == null) {
					int index = Model.GetIndexForLump(Version);
					if (index >= 0) {
						_models = Model.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _models;
			}
		}

		/// <summary>
		/// A <see cref="Lump{Brush}"/> of <see cref="Brush"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<Brush> Brushes {
			get {
				if (_brushes == null) {
					int index = Brush.GetIndexForLump(Version);
					if (index >= 0) {
						_brushes = Brush.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _brushes;
			}
		}

		/// <summary>
		/// A <see cref="Lump{BrushSide}"/> of <see cref="BrushSide"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<BrushSide> BrushSides {
			get {
				if (_brushSides == null) {
					int index = BrushSide.GetIndexForLump(Version);
					if (index >= 0) {
						_brushSides = BrushSide.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _brushSides;
			}
		}

		/// <summary>
		/// A <see cref="Lumps.Textures"/> object representing Materials (shaders), if available.
		/// </summary>
		public Textures Materials {
			get {
				if (_materials == null) {
					int index = Texture.GetIndexForMaterialLump(Version);
					if (index >= 0) {
						_materials = Texture.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _materials;
			}
		}

		/// <summary>
		/// A <see cref="Lump{Face}"/> of <see cref="Face"/> objects in the BSP file representing the Original Faces, if available.
		/// </summary>
		public Lump<Face> OriginalFaces {
			get {
				if (_originalFaces == null) {
					int index = Face.GetIndexForOriginalFacesLump(Version);
					if (index >= 0) {
						_originalFaces = Face.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _originalFaces;
			}
		}

		/// <summary>
		/// A <see cref="Lump{TextureData}"/> of <see cref="TextureData"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<TextureData> TexDatas {
			get {
				if (_texDatas == null) {
					int index = TextureData.GetIndexForLump(Version);
					if (index >= 0) {
						_texDatas = TextureData.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _texDatas;
			}
		}

		/// <summary>
		/// A <see cref="Lump{Displacement}"/> of <see cref="Displacement"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<Displacement> DispInfos {
			get {
				if (_dispInfos == null) {
					int index = Displacement.GetIndexForLump(Version);
					if (index >= 0) {
						_dispInfos = Displacement.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _dispInfos;
			}
		}

		/// <summary>
		/// The <see cref="Lump{DisplacementVertex}"/> object in the BSP file, if available.
		/// </summary>
		public Lump<DisplacementVertex> DispVerts {
			get {
				if (_dispVerts == null) {
					int index = DisplacementVertex.GetIndexForLump(Version);
					if (index >= 0) {
						_dispVerts = DisplacementVertex.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _dispVerts;
			}
		}

		/// <summary>
		/// A <see cref="Lump{Cubemap}"/> of <see cref="Cubemap"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<Cubemap> Cubemaps {
			get {
				if (_cubemaps == null) {
					int index = Cubemap.GetIndexForLump(Version);
					if (index >= 0) {
						_cubemaps = Cubemap.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _cubemaps;
			}
		}

		/// <summary>
		/// A <see cref="NumList"/> object containing the Mark Surfaces (Leaf Surfaces) lump, if available.
		/// </summary>
		public NumList MarkSurfaces {
			get {
				if (_markSurfaces == null) {
					int index = NumList.GetIndexForMarkSurfacesLump(Version, out NumList.DataType type);
					if (index >= 0) {
						_markSurfaces = NumList.LumpFactory(Reader.ReadLump(this[index]), type, this, this[index]);
					}
				}
				return _markSurfaces;
			}
		}

		/// <summary>
		/// A <see cref="NumList"/> object containing the Surface Edges lump, if available.
		/// </summary>
		public NumList SurfEdges {
			get {
				if (_surfEdges == null) {
					int index = NumList.GetIndexForSurfEdgesLump(Version, out NumList.DataType type);
					if (index >= 0) {
						_surfEdges = NumList.LumpFactory(Reader.ReadLump(this[index]), type, this, this[index]);
					}
				}
				return _surfEdges;
			}
		}

		/// <summary>
		/// A <see cref="NumList"/> object containing the Mark Brushes (Leaf Brushes) lump, if available.
		/// </summary>
		public NumList MarkBrushes {
			get {
				if (_markBrushes == null) {
					int index = NumList.GetIndexForMarkBrushesLump(Version, out NumList.DataType type);
					if (index >= 0) {
						_markBrushes = NumList.LumpFactory(Reader.ReadLump(this[index]), type, this, this[index]);
					}
				}
				return _markBrushes;
			}
		}

		/// <summary>
		/// A <see cref="Lump{StaticModel}"/> of <see cref="StaticModel"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<StaticModel> StaticModels {
			get {
				if (_staticModels == null) {
					int index = StaticModel.GetIndexForLump(Version);
					if (index >= 0) {
						_staticModels = StaticModel.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _staticModels;
			}
		}

		/// <summary>
		/// A <see cref="Lump{LODTerrain}"/> of <see cref="LodTerrain"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<LodTerrain> LodTerrains {
			get {
				if (_lodTerrains == null) {
					int index = LodTerrain.GetIndexForLump(Version);
					if (index >= 0) {
						_lodTerrains = LodTerrain.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _lodTerrains;
			}
		}

		/// <summary>
		/// A <see cref="Lump{Patch}"/> of <see cref="Patch"/> objects in the BSP file, if available.
		/// </summary>
		public Lump<Patch> Patches {
			get {
				if (_patches == null) {
					int index = Patch.GetIndexForLump(Version);
					if (index >= 0) {
						_patches = Patch.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _patches;
			}
		}
		
		/// <summary>
		/// A <see cref="Lump{Vertex}"/> of <see cref="Vertex"/> objects in the BSP file representing the patch vertices of the BSP, if available.
		/// </summary>
		public Lump<Vertex> PatchVerts {
			get {
				if (_patchVerts == null) {
					int index = VertexExtensions.GetIndexForPatchVertsLump(Version);
					if (index >= 0) {
						// Hax: CoD maps will read Vertex lump with version 1 as simply Vector3s rather than vertices.
						LumpInfo lumpInfo = this[index];
						lumpInfo.version = 1;
						_patchVerts = VertexExtensions.LumpFactory(Reader.ReadLump(this[index]), this, lumpInfo);
					}
				}
				return _patchVerts;
			}
		}

		/// <summary>
		/// A <see cref="NumList"/> object containing the Patch Vertex Indices lump, if available.
		/// </summary>
		public NumList PatchIndices {
			get {
				if (_patchIndices == null) {
					int index = NumList.GetIndexForPatchIndicesLump(Version, out NumList.DataType type);
					if (index >= 0) {
						_patchIndices = NumList.LumpFactory(Reader.ReadLump(this[index]), type, this, this[index]);
					}
				}
				return _patchIndices;
			}
		}

		/// <summary>
		/// A <see cref="NumList"/> object containing the Leaf Patches lump, if available.
		/// </summary>
		public NumList LeafPatches {
			get {
				if (_leafPatches == null) {
					int index = NumList.GetIndexForLeafPatchesLump(Version, out NumList.DataType type);
					if (index >= 0) {
						_leafPatches = NumList.LumpFactory(Reader.ReadLump(this[index]), type, this, this[index]);
					}
				}
				return _leafPatches;
			}
		}

		/// <summary>
		/// A <see cref="NumList"/> object containing the Face Vertex Indices lump, if available.
		/// </summary>
		public NumList Indices {
			get {
				if (_indices == null) {
					int index = NumList.GetIndexForIndicesLump(Version, out NumList.DataType type);
					if (index >= 0) {
						_indices = NumList.LumpFactory(Reader.ReadLump(this[index]), type, this, this[index]);
					}
				}
				return _indices;
			}
		}

		/// <summary>
		/// A <see cref="NumList"/> object containing the Texture offsets table lump, if available.
		/// </summary>
		public NumList TexTable {
			get {
				if (_texTable == null) {
					int index = NumList.GetIndexForTexTableLump(Version, out NumList.DataType type);
					if (index >= 0) {
						_texTable = NumList.LumpFactory(Reader.ReadLump(this[index]), type, this, this[index]);
					}
				}
				return _texTable;
			}
		}

		/// <summary>
		/// A <see cref="NumList"/> object containing the Displacement Triangles lump, if available.
		/// </summary>
		public NumList DisplacementTriangles {
			get {
				if (_displacementTriangles == null) {
					int index = NumList.GetIndexForDisplacementTrianglesLump(Version, out NumList.DataType type);
					if (index >= 0) {
						_displacementTriangles = NumList.LumpFactory(Reader.ReadLump(this[index]), type, this, this[index]);
					}
				}
				return _displacementTriangles;
			}
		}

		/// <summary>
		/// The <see cref="Lumps.GameLump"/> object in the BSP file containing internal lumps, if available.
		/// </summary>
		public GameLump GameLump {
			get {
				if (_gameLump == null) {
					int index = GameLump.GetIndexForLump(Version);
					if (index >= 0) {
						_gameLump = GameLump.LumpFactory(Reader.ReadLump(this[index]), this, this[index]);
					}
				}
				return _gameLump;
			}
		}

		/// <summary>
		/// The <see cref="Lumps.StaticProps"/> object in the BSP file extracted from the <see cref="GameLump"/>, if available.
		/// </summary>
		public StaticProps StaticProps {
			get {
				if (_staticProps == null) {
					if (GameLump != null && GameLump.ContainsKey(GameLumpType.Prps)) {
						LumpInfo info = GameLump[GameLumpType.Prps];
						byte[] thisLump;
						// GameLump lumps may have their offset specified from either the beginning of the GameLump, or the beginning of the file.
						if (GameLump.GetLowestLumpOffset() < this[GameLump.GetIndexForLump(Version)].offset) {
							thisLump = Reader.ReadLump(new LumpInfo
							{
								ident = info.ident,
								flags = info.flags,
								version = info.version,
								offset = info.offset + this[GameLump.GetIndexForLump(Version)].offset,
								length = info.length
							});
						} else {
							thisLump = Reader.ReadLump(info);
						}
						_staticProps = StaticProp.LumpFactory(thisLump, this, info);
					}
				}
				return _staticProps;
			}
		}

		/// <summary>
		/// Gets the path to this BSP file.
		/// </summary>
		public string FilePath { get; private set; }

		/// <summary>
		/// Gets the file name of this map.
		/// </summary>
		public string MapName => Path.GetFileName(FilePath);

		/// <summary>
		/// Gets the file name of this map without the ".BSP" extension.
		/// </summary>
		public string MapNameNoExtension => Path.GetFileNameWithoutExtension(FilePath);

		/// <summary>
		/// Gets the folder path where this map is located.
		/// </summary>
		public string Folder => Path.GetDirectoryName(FilePath);

		/// <summary>
		/// Gets the <see cref="LumpInfo"/> object associated with the lump with index "<paramref name="index"/>".
		/// </summary>
		/// <param name="index">Index of the lump to get information for.</param>
		/// <returns>A <see cref="LumpInfo"/> object containing information about lump "<paramref name="index"/>".</returns>
		public new LumpInfo this[int index] {
			get {
				if (!ContainsKey(index)) {
					base[index] = Reader.GetLumpInfo(index, Version);
				}
				return base[index];
			}
		}

		/// <summary>
		/// Creates a new <see cref="Bsp"/> instance pointing to the file at <paramref name="filePath"/>. The
		/// <c>List</c>s in this class will be read and populated when accessed through their properties.
		/// </summary>
		/// <param name="filePath">The path to the .BSP file.</param>
		public Bsp(string filePath) : base(16) {
			Reader = new BspReader(new FileInfo(filePath));
			FilePath = filePath;
		}

		/// <summary>
		/// Creates a new <see cref="Bsp"/> instance using the file referenced by <paramref name="file"/>. The
		/// <c>List</c>s in this class will be read and populated when accessed through their properties.
		/// </summary>
		/// <param name="file">A reference to the .BSP file.</param>
		public Bsp(FileInfo file) : base(16) {
			Reader = new BspReader(file);
			FilePath = file.FullName;
		}

		/// <summary>
		/// Gets the number of lumps in a given BSP version.
		/// </summary>
		/// <param name="version">The version to get the number of lumps for.</param>
		/// <returns>The number of lumps used by a BSP of version <paramref name="version"/>.</returns>
		public static int GetNumLumps(MapType version) {
			switch (version) {
				case MapType.Quake: {
					return 15;
				}
				case MapType.Daikatana:
				case MapType.Quake2: {
					return 16;
				}
				case MapType.Quake3: {
					return 17;
				}
				case MapType.Raven:
				case MapType.Nightfire: {
					return 18;
				}
				case MapType.Fakk:
				case MapType.SiN: {
					return 20;
				}
				case MapType.SoF: {
					return 22;
				}
				case MapType.Mohaa: {
					return 28;
				}
				case MapType.Stef2:
				case MapType.Stef2Demo: {
					return 30;
				}
				case MapType.CoD: {
					return 31;
				}
				case MapType.CoD2: {
					return 39;
				}
				case MapType.CoD4: {
					return 55;
				}
				case MapType.TacticalInterventionEncrypted:
				case MapType.DMoMaM:
				case MapType.Source17:
				case MapType.Source18:
				case MapType.Source19:
				case MapType.Source20:
				case MapType.Source21:
				case MapType.Source22:
				case MapType.Source23:
				case MapType.Source27:
				case MapType.L4D2:
				case MapType.Vindictus: {
					return 64;
				}
				case MapType.Titanfall: {
					return 128;
				}
				case MapType.Undefined:
					break;
				default: {
					return -1;
				}
			}

			return -1;
		}

		/// <summary>
		/// Gets all objects of type <typeparamref name="T"/> referenced through passed object <paramref name="o"/>
		/// contained in the lump <paramref name="lumpName"/> stored in this <see cref="Bsp"/> class. This is done by
		/// reflecting the <c>Type</c> of <paramref name="o"/> and looping through its public properties to find
		/// a member with an <see cref="IndexAttribute"/> attribute and a member with a <see cref="CountAttribute"/> attribute
		/// both corresponding to <paramref name="lumpName"/>. The index and count are obtained and used to construct
		/// a new <c>List&lt;<typeparamref name="T"/>&gt;</c> object containing the corresponding objects.
		/// </summary>
		/// <typeparam name="T">The type of <c>object</c> stored in the lump <paramref name="lumpName"/>.</typeparam>
		/// <param name="o">The <c>object</c> which contains and index and count corresponding to <paramref name="lumpName"/>.</param>
		/// <param name="lumpName">The name of the property in this <see cref="Bsp"/> object to get a <c>List</c> of objects from.</param>
		/// <returns>The <c>List&lt;<typeparamref name="T"/>&gt;</c> of objects in the lump from the index and length specified in <paramref name="o"/>.</returns>
		/// <exception cref="ArgumentException">The <see cref="Bsp"/> class contains no property corresponding to <paramref name="lumpName"/>.</exception>
		/// <exception cref="ArgumentException">The <c>object</c> referenced by <paramref name="o"/> is missing one or both members with <c>IndexAttribute</c> or <c>CountAttribute</c> attributes corresponding to <paramref name="lumpName"/>.</exception>
		/// <exception cref="ArgumentNullException">One or both of <paramref name="o"/> or <paramref name="lumpName"/> is null.</exception>
		public List<T> GetReferencedObjects<T>(object o, string lumpName) {
			if (o == null) {
				throw new ArgumentNullException("Object cannot be null.");
			}
			if (lumpName == null) {
				throw new ArgumentNullException("Lump name cannot be null.");
			}
			// First, find the property in this class corresponding to lumpName, and grab its "get" method
			PropertyInfo targetLump = typeof(Bsp).GetProperty(lumpName, BindingFlags.Public | BindingFlags.Instance);
			if (targetLump == null) {
				throw new ArgumentException("The lump " + lumpName + " does not exist in the BSP class.");
			}

			// Next, find the properties in the passed object corresponding to lumpName, through the Index and Length custom attributes
			Type objectType = o.GetType();
			PropertyInfo[] objectProperties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			PropertyInfo indexProperty = null;
			PropertyInfo countProperty = null;
			foreach (PropertyInfo info in objectProperties) {
				IndexAttribute indexAttribute = CustomAttributeExtensions.GetCustomAttribute<IndexAttribute>(info);
				if (indexAttribute != null) {
					if (indexAttribute.lumpName == lumpName) {
						indexProperty = info;
						if (countProperty != null) {
							break;
						}
					}
				}
				CountAttribute lengthAttribute = CustomAttributeExtensions.GetCustomAttribute<CountAttribute>(info);
				if (lengthAttribute != null) {
					if (lengthAttribute.lumpName == lumpName) {
						countProperty = info;
						if (indexProperty != null) {
							break;
						}
					}
				}
			}
			if (indexProperty == null || countProperty == null) {
				throw new ArgumentException("An object of type " + objectType.Name + " does not implement both an Index and Count for lump " + lumpName + ".");
			}

			// Get the index and length from the object
			int index = (int)(indexProperty.GetGetMethod().Invoke(o, null));
			int count = (int)(countProperty.GetGetMethod().Invoke(o, null));

			// Get the lump from this class
			IList<T> theLump = targetLump.GetGetMethod().Invoke(this, null) as IList<T>;

			// Copy items from the lump into a return list.
			// IList<T> lacks AddRange and this is faster and creates less garbage than any Linq trickery I could come up with.
			// Passing references to IList<T> out of this method just eats obscene amounts of memory until the system runs out.
			List<T> ret = new List<T>(count);
			for (int i = 0; i < count; ++i)
			{
				if (theLump != null) ret.Add(theLump[index + i]);
			}
			return ret;
		}

	}
}
