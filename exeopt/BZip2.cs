// BZip2.cs
//
// This file is from a collection of authors. See
// http://www.icsharpcode.net/OpenSource/SharpZipLib/Default.aspx
// for the original distribution
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.

using System;
using System.IO;

namespace SharpZipLib {

    public sealed class BZip2 {

        public static void Decompress(Stream instream,Stream outstream) {
            System.IO.Stream bos=outstream;
            System.IO.Stream bis=instream;
            BZip2InputStream bzis=new BZip2InputStream(bis);
            int ch=bzis.ReadByte();
            while(ch!=-1) {
                bos.WriteByte((byte)ch);
                ch=bzis.ReadByte();
            }
            bos.Flush();
        }
    }

    public sealed class BZip2Constants {

        public readonly static int[] rNums= {
			619, 720, 127, 481, 931, 816, 813, 233, 566, 247,
			985, 724, 205, 454, 863, 491, 741, 242, 949, 214,
			733, 859, 335, 708, 621, 574,  73, 654, 730, 472,
			419, 436, 278, 496, 867, 210, 399, 680, 480,  51,
			878, 465, 811, 169, 869, 675, 611, 697, 867, 561,
			862, 687, 507, 283, 482, 129, 807, 591, 733, 623,
			150, 238,  59, 379, 684, 877, 625, 169, 643, 105,
			170, 607, 520, 932, 727, 476, 693, 425, 174, 647,
			 73, 122, 335, 530, 442, 853, 695, 249, 445, 515,
			909, 545, 703, 919, 874, 474, 882, 500, 594, 612,
			641, 801, 220, 162, 819, 984, 589, 513, 495, 799,
			161, 604, 958, 533, 221, 400, 386, 867, 600, 782,
			382, 596, 414, 171, 516, 375, 682, 485, 911, 276,
			 98, 553, 163, 354, 666, 933, 424, 341, 533, 870,
			227, 730, 475, 186, 263, 647, 537, 686, 600, 224,
			469,  68, 770, 919, 190, 373, 294, 822, 808, 206,
			184, 943, 795, 384, 383, 461, 404, 758, 839, 887,
			715,  67, 618, 276, 204, 918, 873, 777, 604, 560,
			951, 160, 578, 722,  79, 804,  96, 409, 713, 940,
			652, 934, 970, 447, 318, 353, 859, 672, 112, 785,
			645, 863, 803, 350, 139,  93, 354,  99, 820, 908,
			609, 772, 154, 274, 580, 184,  79, 626, 630, 742,
			653, 282, 762, 623, 680,  81, 927, 626, 789, 125,
			411, 521, 938, 300, 821,  78, 343, 175, 128, 250,
			170, 774, 972, 275, 999, 639, 495,  78, 352, 126,
			857, 956, 358, 619, 580, 124, 737, 594, 701, 612,
			669, 112, 134, 694, 363, 992, 809, 743, 168, 974,
			944, 375, 748,  52, 600, 747, 642, 182, 862,  81,
			344, 805, 988, 739, 511, 655, 814, 334, 249, 515,
			897, 955, 664, 981, 649, 113, 974, 459, 893, 228,
			433, 837, 553, 268, 926, 240, 102, 654, 459,  51,
			686, 754, 806, 760, 493, 403, 415, 394, 687, 700,
			946, 670, 656, 610, 738, 392, 760, 799, 887, 653,
			978, 321, 576, 617, 626, 502, 894, 679, 243, 440,
			680, 879, 194, 572, 640, 724, 926,  56, 204, 700,
			707, 151, 457, 449, 797, 195, 791, 558, 945, 679,
			297,  59,  87, 824, 713, 663, 412, 693, 342, 606,
			134, 108, 571, 364, 631, 212, 174, 643, 304, 329,
			343,  97, 430, 751, 497, 314, 983, 374, 822, 928,
			140, 206,  73, 263, 980, 736, 876, 478, 430, 305,
			170, 514, 364, 692, 829,  82, 855, 953, 676, 246,
			369, 970, 294, 750, 807, 827, 150, 790, 288, 923,
			804, 378, 215, 828, 592, 281, 565, 555, 710,  82,
			896, 831, 547, 261, 524, 462, 293, 465, 502,  56,
			661, 821, 976, 991, 658, 869, 905, 758, 745, 193,
			768, 550, 608, 933, 378, 286, 215, 979, 792, 961,
			 61, 688, 793, 644, 986, 403, 106, 366, 905, 644,
			372, 567, 466, 434, 645, 210, 389, 550, 919, 135,
			780, 773, 635, 389, 707, 100, 626, 958, 165, 504,
			920, 176, 193, 713, 857, 265, 203,  50, 668, 108,
			645, 990, 626, 197, 510, 357, 358, 850, 858, 364,
			936, 638
		};

        public readonly static int baseBlockSize=100000;

        public readonly static int MAX_ALPHA_SIZE=258;

        public readonly static int MAX_CODE_LEN=23;

        public readonly static int RUNA=0;

        public readonly static int RUNB=1;

        public readonly static int N_GROUPS=6;

        public readonly static int G_SIZE=50;

        public readonly static int N_ITERS=4;

        public readonly static int MAX_SELECTORS=(2+(900000/G_SIZE));

        public readonly static int NUM_OVERSHOOT_BYTES=20;

        private BZip2Constants() { }
    }

	public class BZip2Exception : SharpZipBaseException
	{

		public BZip2Exception()
		{
		}

		public BZip2Exception(string message) : base(message)
		{
		}
		
	}

	public class BZip2InputStream : Stream
	{

		public override bool CanRead {
			get {
				return baseStream.CanRead;
			}
		}

		public override bool CanSeek {
			get {
				return baseStream.CanSeek;
			}
		}

		public override bool CanWrite {
			get {
				return false;
			}
		}

		public override long Length {
			get {
				return baseStream.Length;
			}
		}

		public override long Position {
			get {
				return baseStream.Position;
			}
			set {
				throw new NotSupportedException("BZip2InputStream position cannot be set");
			}
		}

		public override void Flush()
		{
			if (baseStream != null) {
				baseStream.Flush();
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("BZip2InputStream Seek not supported");
		}

		public override void SetLength(long val)
		{
			throw new NotSupportedException("BZip2InputStream SetLength not supported");
		}

		public override void Write(byte[] array, int offset, int count)
		{
			throw new NotSupportedException("BZip2InputStream Write not supported");
		}

		public override void WriteByte(byte val)
		{
			throw new NotSupportedException("BZip2InputStream WriteByte not supported");
		}

		public override int Read(byte[] b, int offset, int count)
		{
			for (int i = 0; i < count; ++i) {
				int rb = ReadByte();
				if (rb == -1) {
					return i;
				}
				b[offset + i] = (byte)rb;
			}
			return count;
		}

		public override void Close()
		{
			if (baseStream != null) {
				baseStream.Close();
			}
		}
		
		void MakeMaps() 
		{
			nInUse = 0;
			for (int i = 0; i < 256; ++i) {
				if (inUse[i]) {
					seqToUnseq[nInUse] = (byte)i;
					unseqToSeq[i] = (byte)nInUse;
					nInUse++;
				}
			}
		}

		int last;

		int origPtr;

		int blockSize100k;
		
		bool blockRandomised;
		
		int bsBuff;
		int bsLive;
		IChecksum mCrc = new StrangeCRC();
		
		bool[] inUse = new bool[256];
		int    nInUse;
		
		byte[] seqToUnseq = new byte[256];
		byte[] unseqToSeq = new byte[256];
		
		byte[] selector    = new byte[BZip2Constants.MAX_SELECTORS];
		byte[] selectorMtf = new byte[BZip2Constants.MAX_SELECTORS];
		
		int[] tt;
		byte[] ll8;

		int[] unzftab = new int[256];
		
		int[][] limit     = new int[BZip2Constants.N_GROUPS][];
		int[][] baseArray = new int[BZip2Constants.N_GROUPS][];
		int[][] perm      = new int[BZip2Constants.N_GROUPS][];
		int[] minLens     = new int[BZip2Constants.N_GROUPS];
		
		Stream baseStream;
		bool   streamEnd = false;
		
		int currentChar = -1;
		
		const int START_BLOCK_STATE = 1;
		const int RAND_PART_A_STATE = 2;
		const int RAND_PART_B_STATE = 3;
		const int RAND_PART_C_STATE = 4;
		const int NO_RAND_PART_A_STATE = 5;
		const int NO_RAND_PART_B_STATE = 6;
		const int NO_RAND_PART_C_STATE = 7;
		
		int currentState = START_BLOCK_STATE;
		
		int storedBlockCRC, storedCombinedCRC;
		int computedBlockCRC;
		uint computedCombinedCRC;
		
		int count, chPrev, ch2;
		int tPos;
		int rNToGo = 0;
		int rTPos  = 0;
		int i2, j2;
		byte z;

		public BZip2InputStream(Stream stream) 
		{
			// init arrays
			for (int i = 0; i < BZip2Constants.N_GROUPS; ++i) {
				limit[i] = new int[BZip2Constants.MAX_ALPHA_SIZE];
				baseArray[i]  = new int[BZip2Constants.MAX_ALPHA_SIZE];
				perm[i]  = new int[BZip2Constants.MAX_ALPHA_SIZE];
			}
			
			ll8 = null;
			tt  = null;
			BsSetStream(stream);
			Initialize();
			InitBlock();
			SetupBlock();
		}

		public override int ReadByte()
		{
			if (streamEnd) {
				return -1; // ok
			}
			
			int retChar = currentChar;
			switch (currentState) {
				case RAND_PART_B_STATE:
					SetupRandPartB();
					break;
				case RAND_PART_C_STATE:
					SetupRandPartC();
					break;
				case NO_RAND_PART_B_STATE:
					SetupNoRandPartB();
					break;
				case NO_RAND_PART_C_STATE:
					SetupNoRandPartC();
					break;
				case START_BLOCK_STATE:
				case NO_RAND_PART_A_STATE:
				case RAND_PART_A_STATE:
					break;
				default:
					break;
			}
			return retChar;
		}
		
		void Initialize() 
		{
			char magic1 = BsGetUChar();
			char magic2 = BsGetUChar();
			
			char magic3 = BsGetUChar();
			char magic4 = BsGetUChar();
			
			if (magic1 != 'B' || magic2 != 'Z' || magic3 != 'h' || magic4 < '1' || magic4 > '9') {
				streamEnd = true;
				return;
			}
			
			SetDecompressStructureSizes(magic4 - '0');
			computedCombinedCRC = 0;
		}
		
		void InitBlock() 
		{
			char magic1 = BsGetUChar();
			char magic2 = BsGetUChar();
			char magic3 = BsGetUChar();
			char magic4 = BsGetUChar();
			char magic5 = BsGetUChar();
			char magic6 = BsGetUChar();
			
			if (magic1 == 0x17 && magic2 == 0x72 && magic3 == 0x45 && magic4 == 0x38 && magic5 == 0x50 && magic6 == 0x90) {
				Complete();
				return;
			}
			
			if (magic1 != 0x31 || magic2 != 0x41 || magic3 != 0x59 || magic4 != 0x26 || magic5 != 0x53 || magic6 != 0x59) {
				BadBlockHeader();
				streamEnd = true;
				return;
			}
			
			storedBlockCRC  = BsGetInt32();
			
			blockRandomised = (BsR(1) == 1);
			
			GetAndMoveToFrontDecode();
			
			mCrc.Reset();
			currentState = START_BLOCK_STATE;
		}
		
		void EndBlock() 
		{
			computedBlockCRC = (int)mCrc.Value;
			
			/*-- A bad CRC is considered a fatal error. --*/
			if (storedBlockCRC != computedBlockCRC) {
				CrcError();
			}
			
			// 1528150659
			computedCombinedCRC = ((computedCombinedCRC << 1) & 0xFFFFFFFF) | (computedCombinedCRC >> 31);
			computedCombinedCRC = computedCombinedCRC ^ (uint)computedBlockCRC;
		}
		
		void Complete() 
		{
			storedCombinedCRC = BsGetInt32();
			if (storedCombinedCRC != (int)computedCombinedCRC) {
				CrcError();
			}
			
			streamEnd = true;
		}
		
		static void CompressedStreamEOF() 
		{
			throw new BZip2Exception("BZip2 input stream end of compressed stream");
		}
		
		static void BlockOverrun() 
		{
			throw new BZip2Exception("BZip2 input stream block overrun");
		}
		
		static void BadBlockHeader() 
		{
			throw new BZip2Exception("BZip2 input stream bad block header");
		}
		
		static void CrcError() 
		{
			throw new BZip2Exception("BZip2 input stream crc error");
		}
		
		
		void BsSetStream(Stream f) 
		{
			baseStream = f;
			bsLive = 0;
			bsBuff = 0;
		}
		
		void FillBuffer()
		{
			int thech = 0;
			
			try {
				thech = baseStream.ReadByte();
			} catch (Exception) {
				CompressedStreamEOF();
			}
			
			if (thech == -1) {
				CompressedStreamEOF();
			}
			
			bsBuff = (bsBuff << 8) | (thech & 0xFF);
			bsLive += 8;
		}
		
		int BsR(int n) 
		{
			while (bsLive < n) {
				FillBuffer();
			}
			
			int v = (bsBuff >> (bsLive - n)) & ((1 << n) - 1);
			bsLive -= n;
			return v;
		}
		
		char BsGetUChar() 
		{
			return (char)BsR(8);
		}
		
		int BsGetint() 
		{
			int u = 0;
			u = (u << 8) | BsR(8);
			u = (u << 8) | BsR(8);
			u = (u << 8) | BsR(8);
			u = (u << 8) | BsR(8);
			return u;
		}
		
		int BsGetIntVS(int numBits) 
		{
			return (int)BsR(numBits);
		}
		
		int BsGetInt32() 
		{
			return (int)BsGetint();
		}
		
		void HbCreateDecodeTables(int[] limit, int[] baseArray, int[] perm, char[] length, int minLen, int maxLen, int alphaSize) 
		{
			int pp = 0;
			
			for (int i = minLen; i <= maxLen; ++i) {
				for (int j = 0; j < alphaSize; ++j) {
					if (length[j] == i) {
						perm[pp] = j;
						++pp;
					}
				}
			}
			
			for (int i = 0; i < BZip2Constants.MAX_CODE_LEN; i++) {
				baseArray[i] = 0;
			}
			
			for (int i = 0; i < alphaSize; i++) {
				++baseArray[length[i] + 1];
			}
			
			for (int i = 1; i < BZip2Constants.MAX_CODE_LEN; i++) {
				baseArray[i] += baseArray[i - 1];
			}
			
			for (int i = 0; i < BZip2Constants.MAX_CODE_LEN; i++) {
				limit[i] = 0;
			}
			
			int vec = 0;
			
			for (int i = minLen; i <= maxLen; i++) {
				vec += (baseArray[i + 1] - baseArray[i]);
				limit[i] = vec - 1;
				vec <<= 1;
			}
			
			for (int i = minLen + 1; i <= maxLen; i++) {
				baseArray[i] = ((limit[i - 1] + 1) << 1) - baseArray[i];
			}
		}
		
		void RecvDecodingTables() 
		{
			char[][] len = new char[BZip2Constants.N_GROUPS][];
			for (int i = 0; i < BZip2Constants.N_GROUPS; ++i) {
				len[i] = new char[BZip2Constants.MAX_ALPHA_SIZE];
			}
			
			bool[] inUse16 = new bool[16];
			
			/*--- Receive the mapping table ---*/
			for (int i = 0; i < 16; i++) {
				inUse16[i] = (BsR(1) == 1);
			} 
			
			for (int i = 0; i < 16; i++) {
				if (inUse16[i]) {
					for (int j = 0; j < 16; j++) {
						inUse[i * 16 + j] = (BsR(1) == 1);
					}
				} else {
					for (int j = 0; j < 16; j++) {
						inUse[i * 16 + j] = false;
					}
				}
			}
			
			MakeMaps();
			int alphaSize = nInUse + 2;
			
			/*--- Now the selectors ---*/
			int nGroups    = BsR(3);
			int nSelectors = BsR(15);
			
			for (int i = 0; i < nSelectors; i++) {
				int j = 0;
				while (BsR(1) == 1) {
					j++;
				}
				selectorMtf[i] = (byte)j;
			}
			
			/*--- Undo the MTF values for the selectors. ---*/
			byte[] pos = new byte[BZip2Constants.N_GROUPS];
			for (int v = 0; v < nGroups; v++) {
				pos[v] = (byte)v;
			}
			
			for (int i = 0; i < nSelectors; i++) {
				int  v   = selectorMtf[i];
				byte tmp = pos[v];
				while (v > 0) {
					pos[v] = pos[v - 1];
					v--;
				}
				pos[0]      = tmp;
				selector[i] = tmp;
			}
			
			/*--- Now the coding tables ---*/
			for (int t = 0; t < nGroups; t++) {
				int curr = BsR(5);
				for (int i = 0; i < alphaSize; i++) {
					while (BsR(1) == 1) {
						if (BsR(1) == 0) {
							curr++;
						} else {
							curr--;
						}
					}
					len[t][i] = (char)curr;
				}
			}
			
			for (int t = 0; t < nGroups; t++) {
				int minLen = 32;
				int maxLen = 0;
				for (int i = 0; i < alphaSize; i++) {
					maxLen = Math.Max(maxLen, len[t][i]);
					minLen = Math.Min(minLen, len[t][i]);
				}
				HbCreateDecodeTables(limit[t], baseArray[t], perm[t], len[t], minLen, maxLen, alphaSize);
				minLens[t] = minLen;
			}
		}
		
		void GetAndMoveToFrontDecode() 
		{
			byte[] yy = new byte[256];
			int nextSym;
			
			int limitLast = BZip2Constants.baseBlockSize * blockSize100k;
			origPtr = BsGetIntVS(24);
			
			RecvDecodingTables();
			int EOB = nInUse+1;
			int groupNo = -1;
			int groupPos = 0;
			
			for (int i = 0; i <= 255; i++) {
				unzftab[i] = 0;
			}
			
			for (int i = 0; i <= 255; i++) {
				yy[i] = (byte)i;
			}
			
			last = -1;
			
			if (groupPos == 0) {
				groupNo++;
				groupPos = BZip2Constants.G_SIZE;
			}
			
			groupPos--;
			int zt = selector[groupNo];
			int zn = minLens[zt];
			int zvec = BsR(zn);
			int zj;
			
			while (zvec > limit[zt][zn]) {
				if (zn > 20) { // the longest code
					throw new BZip2Exception("Bzip data error");  // -jr- 17-Dec-2003 from bzip 1.02 why 20???
				}
				zn++;
				while (bsLive < 1) {
					FillBuffer();
				}
				zj = (bsBuff >> (bsLive-1)) & 1;
				bsLive--;
				zvec = (zvec << 1) | zj;
			}
			if (zvec - baseArray[zt][zn] < 0 || zvec - baseArray[zt][zn] >= BZip2Constants.MAX_ALPHA_SIZE) {
				throw new BZip2Exception("Bzip data error");  // -jr- 17-Dec-2003 from bzip 1.02
			}
			nextSym = perm[zt][zvec - baseArray[zt][zn]];
			
			while (true) {
				if (nextSym == EOB) {
					break;
				}
				
				if (nextSym == BZip2Constants.RUNA || nextSym == BZip2Constants.RUNB) {
					int s = -1;
					int n = 1;
					do {
						if (nextSym == BZip2Constants.RUNA) {
							s += (0 + 1) * n;
						} else if (nextSym == BZip2Constants.RUNB) {
							s += (1 + 1) * n;
						}

						n <<= 1;
						
						if (groupPos == 0) {
							groupNo++;
							groupPos = BZip2Constants.G_SIZE;
						}
						
						groupPos--;
						
						zt = selector[groupNo];
						zn = minLens[zt];
						zvec = BsR(zn);
						
						while (zvec > limit[zt][zn]) {
							zn++;
							while (bsLive < 1) {
								FillBuffer();
							}
							zj = (bsBuff >> (bsLive - 1)) & 1;
							bsLive--;
							zvec = (zvec << 1) | zj;
						}
						nextSym = perm[zt][zvec - baseArray[zt][zn]];
					} while (nextSym == BZip2Constants.RUNA || nextSym == BZip2Constants.RUNB);
					
					s++;
					byte ch = seqToUnseq[yy[0]];
					unzftab[ch] += s;
					
					while (s > 0) {
						last++;
						ll8[last] = ch;
						s--;
					}
					
					if (last >= limitLast) {
						BlockOverrun();
					}
					continue;
				} else {
					last++;
					if (last >= limitLast) {
						BlockOverrun();
					}
					
					byte tmp = yy[nextSym - 1];
					unzftab[seqToUnseq[tmp]]++;
					ll8[last] = seqToUnseq[tmp];
					
					for (int j = nextSym-1; j > 0; --j) {
						yy[j] = yy[j - 1];
					}
					yy[0] = tmp;
					
					if (groupPos == 0) {
						groupNo++;
						groupPos = BZip2Constants.G_SIZE;
					}
					
					groupPos--;
					zt = selector[groupNo];
					zn = minLens[zt];
					zvec = BsR(zn);
					while (zvec > limit[zt][zn]) {
						zn++;
						while (bsLive < 1) {
							FillBuffer();
						}
						zj = (bsBuff >> (bsLive-1)) & 1;
						bsLive--;
						zvec = (zvec << 1) | zj;
					}
					nextSym = perm[zt][zvec - baseArray[zt][zn]];
					continue;
				}
			}
		}
		
		void SetupBlock() 
		{
			int[] cftab = new int[257];
			
			cftab[0] = 0;
			Array.Copy(unzftab, 0, cftab, 1, 256);
			
			for (int i = 1; i <= 256; i++) {
				cftab[i] += cftab[i - 1];
			}
			
			for (int i = 0; i <= last; i++) {
				byte ch = ll8[i];
				tt[cftab[ch]] = i;
				cftab[ch]++;
			}
			
			cftab = null;
			
			tPos = tt[origPtr];
			
			count = 0;
			i2    = 0;
			ch2   = 256;   /*-- not a char and not EOF --*/
			
			if (blockRandomised) {
				rNToGo = 0;
				rTPos = 0;
				SetupRandPartA();
			} else {
				SetupNoRandPartA();
			}
		}
		
		void SetupRandPartA() 
		{
			if (i2 <= last) {
				chPrev = ch2;
				ch2  = ll8[tPos];
				tPos = tt[tPos];
				if (rNToGo == 0) {
					rNToGo = BZip2Constants.rNums[rTPos];
					rTPos++;
					if (rTPos == 512) {
						rTPos = 0;
					}
				}
				rNToGo--;
				ch2 ^= (int)((rNToGo == 1) ? 1 : 0);
				i2++;
				
				currentChar  = ch2;
				currentState = RAND_PART_B_STATE;
				mCrc.Update(ch2);
			} else {
				EndBlock();
				InitBlock();
				SetupBlock();
			}
		}
		
		void SetupNoRandPartA() 
		{
			if (i2 <= last) {
				chPrev = ch2;
				ch2  = ll8[tPos];
				tPos = tt[tPos];
				i2++;
				
				currentChar = ch2;
				currentState = NO_RAND_PART_B_STATE;
				mCrc.Update(ch2);
			} else {
				EndBlock();
				InitBlock();
				SetupBlock();
			}
		}
		
		void SetupRandPartB() 
		{
			if (ch2 != chPrev) {
				currentState = RAND_PART_A_STATE;
				count = 1;
				SetupRandPartA();
			} else {
				count++;
				if (count >= 4) {
					z = ll8[tPos];
					tPos = tt[tPos];
					if (rNToGo == 0) {
						rNToGo = BZip2Constants.rNums[rTPos];
						rTPos++;
						if (rTPos == 512) {
							rTPos = 0;
						}
					}
					rNToGo--;
					z ^= (byte)((rNToGo == 1) ? 1 : 0);
					j2 = 0;
					currentState = RAND_PART_C_STATE;
					SetupRandPartC();
				} else {
					currentState = RAND_PART_A_STATE;
					SetupRandPartA();
				}
			}
		}
		
		void SetupRandPartC() 
		{
			if (j2 < (int)z) {
				currentChar = ch2;
				mCrc.Update(ch2);
				j2++;
			} else {
				currentState = RAND_PART_A_STATE;
				i2++;
				count = 0;
				SetupRandPartA();
			}
		}
		
		void SetupNoRandPartB() 
		{
			if (ch2 != chPrev) {
				currentState = NO_RAND_PART_A_STATE;
				count = 1;
				SetupNoRandPartA();
			} else {
				count++;
				if (count >= 4) {
					z = ll8[tPos];
					tPos = tt[tPos];
					currentState = NO_RAND_PART_C_STATE;
					j2 = 0;
					SetupNoRandPartC();
				} else {
					currentState = NO_RAND_PART_A_STATE;
					SetupNoRandPartA();
				}
			}
		}
		
		void SetupNoRandPartC() 
		{
			if (j2 < (int)z) {
				currentChar = ch2;
				mCrc.Update(ch2);
				j2++;
			} else {
				currentState = NO_RAND_PART_A_STATE;
				i2++;
				count = 0;
				SetupNoRandPartA();
			}
		}
		
		void SetDecompressStructureSizes(int newSize100k) 
		{
			if (!(0 <= newSize100k   && newSize100k <= 9 && 0 <= blockSize100k && blockSize100k <= 9)) {
				throw new BZip2Exception("Invalid block size");
			}
			
			blockSize100k = newSize100k;
			
			if (newSize100k == 0) {
				return;
			}
			
			int n = BZip2Constants.baseBlockSize * newSize100k;
			ll8 = new byte[n];
			tt  = new int[n];
		}
	}

	public interface IChecksum
	{

		long Value 
		{
			get;
		}

		void Reset();

		void Update(int bval);

		void Update(byte[] buffer);

		void Update(byte[] buf, int off, int len);
	}

	public class SharpZipBaseException : ApplicationException
	{

		public SharpZipBaseException()
		{
		}

		public SharpZipBaseException(string msg) : base(msg)
		{
		}

		public SharpZipBaseException(string message, Exception innerException)	: base(message, innerException)
		{
		}
	}

	public class StrangeCRC : IChecksum
	{
		readonly static uint[] crc32Table = {
			0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9,
			0x130476dc, 0x17c56b6b, 0x1a864db2, 0x1e475005,
			0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61,
			0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd,
			0x4c11db70, 0x48d0c6c7, 0x4593e01e, 0x4152fda9,
			0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,
			0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011,
			0x791d4014, 0x7ddc5da3, 0x709f7b7a, 0x745e66cd,
			0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039,
			0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5,
			0xbe2b5b58, 0xbaea46ef, 0xb7a96036, 0xb3687d81,
			0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,
			0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49,
			0xc7361b4c, 0xc3f706fb, 0xceb42022, 0xca753d95,
			0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1,
			0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d,
			0x34867077, 0x30476dc0, 0x3d044b19, 0x39c556ae,
			0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,
			0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16,
			0x018aeb13, 0x054bf6a4, 0x0808d07d, 0x0cc9cdca,
			0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde,
			0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02,
			0x5e9f46bf, 0x5a5e5b08, 0x571d7dd1, 0x53dc6066,
			0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,
			0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e,
			0xbfa1b04b, 0xbb60adfc, 0xb6238b25, 0xb2e29692,
			0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6,
			0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a,
			0xe0b41de7, 0xe4750050, 0xe9362689, 0xedf73b3e,
			0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,
			0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686,
			0xd5b88683, 0xd1799b34, 0xdc3abded, 0xd8fba05a,
			0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637,
			0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb,
			0x4f040d56, 0x4bc510e1, 0x46863638, 0x42472b8f,
			0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,
			0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47,
			0x36194d42, 0x32d850f5, 0x3f9b762c, 0x3b5a6b9b,
			0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff,
			0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623,
			0xf12f560e, 0xf5ee4bb9, 0xf8ad6d60, 0xfc6c70d7,
			0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,
			0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f,
			0xc423cd6a, 0xc0e2d0dd, 0xcda1f604, 0xc960ebb3,
			0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7,
			0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b,
			0x9b3660c6, 0x9ff77d71, 0x92b45ba8, 0x9675461f,
			0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,
			0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640,
			0x4e8ee645, 0x4a4ffbf2, 0x470cdd2b, 0x43cdc09c,
			0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8,
			0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24,
			0x119b4be9, 0x155a565e, 0x18197087, 0x1cd86d30,
			0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,
			0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088,
			0x2497d08d, 0x2056cd3a, 0x2d15ebe3, 0x29d4f654,
			0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0,
			0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c,
			0xe3a1cbc1, 0xe760d676, 0xea23f0af, 0xeee2ed18,
			0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,
			0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0,
			0x9abc8bd5, 0x9e7d9662, 0x933eb0bb, 0x97ffad0c,
			0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668,
			0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4
		};
		
		int globalCrc;

		public StrangeCRC() 
		{
			Reset();
		}

		public void Reset()
		{
			globalCrc = -1;
		}

		public long Value {
			get {
				return ~globalCrc;
			}
		}
		
		public void Update(int inCh)
		{
			int temp = (globalCrc >> 24) ^ inCh;
			if (temp < 0) {
				temp = 256 + temp;
			}
			globalCrc = (int)((globalCrc << 8) ^ crc32Table[temp]);
		}

		public void Update(byte[] buf)
		{
			Update(buf, 0, buf.Length);
		}
		
		public void Update(byte[] buf, int off, int len)
		{
			if (buf == null) {
				throw new ArgumentNullException("buf");
			}
			
			if (off < 0 || len < 0 || off + len > buf.Length) {
				throw new ArgumentOutOfRangeException();
			}
			
			for (int i = 0; i < len; ++i) {
				Update(buf[off++]);
			}
		}
	}
}
