using LibBSP.Source.Structs.BSP;

namespace LibBSP.Source.Structs.Common.Lumps {

	/// <summary>
	/// Interface for a Lump object.
	/// </summary>
	public interface ILump {

		/// <summary>
		/// The <see cref="BSP.Bsp"/> this <see cref="ILump"/> came from.
		/// </summary>
		Bsp Bsp { get; }

		/// <summary>
		/// The <see cref="LumpInfo"/> associated with this <see cref="ILump"/>.
		/// </summary>
		LumpInfo LumpInfo { get; }

	}
}
