// LzInWindow.cs

using System;
using System.IO;

namespace Sandwych.Compression.Algorithms.Lzma.LZ {
    public class InWindow {
        Stream _stream;
        UInt32 _posLimit; // offset (from _buffer) of first byte when new block reading must be done
        bool _streamEndWasReached; // if (true) then _streamPos shows real end of stream

        UInt32 _pointerToLastSafePosition;
        UInt32 _keepSizeBefore; // how many BYTEs must be kept in buffer before _pos
        UInt32 _keepSizeAfter; // how many BYTEs must be kept buffer after _pos

        public Byte[] BufferBase { get; private set; } = null; // pointer to buffer with data
        public UInt32 BufferOffset { get; private set; }
        public UInt32 BlockSize { get; private set; }// Size of Allocated memory block
        public UInt32 Pos { get; private set; } // offset (from _buffer) of curent byte
        public UInt32 StreamPos { get; private set; } // offset (from _buffer) of first not read byte from Stream

        public void MoveBlock() {
            UInt32 offset = (UInt32)(BufferOffset) + Pos - _keepSizeBefore;
            // we need one additional byte, since MovePos moves on 1 byte.
            if (offset > 0) {
                offset--;
            }

            UInt32 numBytes = (UInt32)(BufferOffset) + StreamPos - offset;

            // check negative offset ????
            for (UInt32 i = 0; i < numBytes; i++) {
                BufferBase[i] = BufferBase[offset + i];
            }
            BufferOffset -= offset;
        }

        public virtual void ReadBlock() {
            if (_streamEndWasReached)
                return;
            while (true) {
                int size = (int)((0 - BufferOffset) + BlockSize - StreamPos);
                if (size == 0)
                    return;
                int numReadBytes = _stream.Read(BufferBase, (int)(BufferOffset + StreamPos), size);
                if (numReadBytes == 0) {
                    _posLimit = StreamPos;
                    UInt32 pointerToPostion = BufferOffset + _posLimit;
                    if (pointerToPostion > _pointerToLastSafePosition)
                        _posLimit = (UInt32)(_pointerToLastSafePosition - BufferOffset);

                    _streamEndWasReached = true;
                    return;
                }
                StreamPos += (UInt32)numReadBytes;
                if (StreamPos >= Pos + _keepSizeAfter)
                    _posLimit = StreamPos - _keepSizeAfter;
            }
        }

        void Free() {
            BufferBase = null;
        }

        public void Create(UInt32 keepSizeBefore, UInt32 keepSizeAfter, UInt32 keepSizeReserv) {
            _keepSizeBefore = keepSizeBefore;
            _keepSizeAfter = keepSizeAfter;
            UInt32 blockSize = keepSizeBefore + keepSizeAfter + keepSizeReserv;
            if (BufferBase == null || BlockSize != blockSize) {
                Free();
                BlockSize = blockSize;
                BufferBase = new Byte[BlockSize];
            }
            _pointerToLastSafePosition = BlockSize - keepSizeAfter;
        }

        public void SetStream(Stream stream) {
            _stream = stream;
        }

        public void ReleaseStream() {
            _stream = null;
        }

        public void Init() {
            BufferOffset = 0;
            Pos = 0;
            StreamPos = 0;
            _streamEndWasReached = false;
            ReadBlock();
        }

        public void MovePos() {
            Pos++;
            if (Pos > _posLimit) {
                UInt32 pointerToPostion = BufferOffset + Pos;
                if (pointerToPostion > _pointerToLastSafePosition) {
                    MoveBlock();
                }
                ReadBlock();
            }
        }

        public Byte GetIndexByte(Int32 index) => BufferBase[BufferOffset + Pos + index];

        // index + limit have not to exceed _keepSizeAfter;
        public UInt32 GetMatchLen(Int32 index, UInt32 distance, UInt32 limit) {
            if (_streamEndWasReached) {
                if ((Pos + index) + limit > StreamPos) {
                    limit = StreamPos - (UInt32)(Pos + index);
                }
            }
            distance++;
            // Byte *pby = _buffer + (size_t)_pos + index;
            UInt32 pby = BufferOffset + Pos + (UInt32)index;

            UInt32 i;
            for (i = 0; i < limit && BufferBase[pby + i] == BufferBase[pby + i - distance]; i++) {
            }
            return i;
        }

        public UInt32 GetNumAvailableBytes() => StreamPos - Pos;

        public void ReduceOffsets(Int32 subValue) {
            BufferOffset += (UInt32)subValue;
            _posLimit -= (UInt32)subValue;
            Pos -= (UInt32)subValue;
            StreamPos -= (UInt32)subValue;
        }
    }
}
