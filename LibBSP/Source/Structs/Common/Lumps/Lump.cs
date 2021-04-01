using System;
using System.Collections.Generic;
using LibBSP.Source.Structs.BSP;

namespace LibBSP.Source.Structs.Common.Lumps {
	public class Lump<T> : List<T>, ILump {

		/// <summary>
		/// The <see cref="BSP.Bsp"/> this <see cref="ILump"/> came from.
		/// </summary>
		public Bsp Bsp { get; protected set; }

		/// <summary>
		/// The <see cref="LumpInfo"/> associated with this <see cref="ILump"/>.
		/// </summary>
		public LumpInfo LumpInfo { get; protected set; }
		
		/// <summary>
		/// Creates an empty <c>Lump</c> of <typeparamref name="T"/> objects.
		/// </summary>
		/// <param name="bsp">The <see cref="BSP"/> which <paramref name="data"/> came from.</param>
		/// <param name="lumpInfo">The <see cref="LumpInfo"/> object for this <c>Lump</c>.</param>
		public Lump(Bsp bsp = null, LumpInfo lumpInfo = default) {
			Bsp = bsp;
			LumpInfo = lumpInfo;
		}

		/// <summary>
		/// Creates a new <c>Lump</c> that contains elements copied from the passed <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <param name="items">The elements to copy into this <c>Lump</c>.</param>
		/// <param name="bsp">The <see cref="BSP"/> which <paramref name="data"/> came from.</param>
		/// <param name="lumpInfo">The <see cref="LumpInfo"/> object for this <c>Lump</c>.</param>
		public Lump(IEnumerable<T> items, Bsp bsp = null, LumpInfo lumpInfo = default) : base(items) {
			Bsp = bsp;
			LumpInfo = lumpInfo;
		}

		/// <summary>
		/// Creates an empty <c>Lump</c> of <typeparamref name="T"/> objects with the specified initial capactiy.
		/// </summary>
		/// <param name="capacity">The number of elements that can initially be stored.</param>
		/// <param name="bsp">The <see cref="BSP"/> which <paramref name="data"/> came from.</param>
		/// <param name="lumpInfo">The <see cref="LumpInfo"/> object for this <c>Lump</c>.</param>
		public Lump(int capacity, Bsp bsp = null, LumpInfo lumpInfo = default) : base(capacity) {
			Bsp = bsp;
			LumpInfo = lumpInfo;
		}

		/// <summary>
		/// Parses the passed <c>byte</c> array into a <c>Lump</c> of <typeparamref name="T"/> objects.
		/// </summary>
		/// <param name="data">Array of <c>byte</c>s to parse.</param>
		/// <param name="structLength">Number of <c>byte</c>s to copy into the elements. Negative values indicate a variable length, which is not supported by this constructor.</param>
		/// <param name="bsp">The <see cref="BSP.Bsp"/> which <paramref name="data"/> came from.</param>
		/// <param name="lumpInfo">The <see cref="LumpInfo"/> object for this <c>Lump</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="data" /> was <c>null</c>.</exception>
		/// <exception cref="NotSupportedException"><paramref name="structLength"/> is negative.</exception>
		public Lump(byte[] data, int structLength, Bsp bsp = null, LumpInfo lumpInfo = default) : base(data.Length / structLength) {
			if (data == null) {
				throw new ArgumentNullException();
			}
			if (structLength <= 0) {
				throw new NotSupportedException("Cannot use the base Lump constructor for variable length lumps (structLength was negative). Create a derived class with a new constructor instead.");
			}

			Bsp = bsp;
			LumpInfo = lumpInfo;
			for (int i = 0; i < data.Length / structLength; ++i) {
				byte[] bytes = new byte[structLength];
				Array.Copy(data, (i * structLength), bytes, 0, structLength);
				Add((T)Activator.CreateInstance(typeof(T), bytes, this));
			}
		}

	}
}
