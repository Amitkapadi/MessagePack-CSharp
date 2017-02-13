﻿using MessagePack.Decoders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessagePack
{
    /// <summary>
    /// Encode/Decode Utility of MessagePack Spec.
    /// https://github.com/msgpack/msgpack/blob/master/spec.md
    /// </summary>
    public static class MessagePackBinary
    {
        const int MaxSize = byte.MaxValue;

        static readonly IBooleanDecoder[] booleanDecoders = new IBooleanDecoder[MaxSize];
        static readonly IByteDecoder[] byteDecoders = new IByteDecoder[MaxSize];
        static readonly IBytesDecoder[] bytesDecoders = new IBytesDecoder[MaxSize];

        // static readonly Func<byte[], int, int>[] IntegerDecoder = new Func<byte[], int, int>[255];

        static MessagePackBinary()
        {
            // Init LookupTable.
            for (int i = 0; i < MaxSize; i++)
            {
                booleanDecoders[i] = Decoders.InvalidBoolean.Instance;
                byteDecoders[i] = Decoders.InvalidByte.Instance;
                bytesDecoders[i] = Decoders.InvalidBytes.Instance;
            }

            // Emit Codes.
            for (int i = MessagePackCode.MinFixInt; i <= MessagePackCode.MaxFixInt; i++)
            {
                // IntegerDecoder[i] = DecodeFixInteger;
                byteDecoders[i] = Decoders.FixByte.Instance;
            }
            for (int i = MessagePackCode.MinFixMap; i <= MessagePackCode.MaxFixMap; i++)
            {
                // typeLookupTable[i] = MessagePackType.Map;
            }
            for (int i = MessagePackCode.MinFixArray; i <= MessagePackCode.MaxFixArray; i++)
            {
                //typeLookupTable[i] = MessagePackType.Array;
            }
            for (int i = MessagePackCode.MinFixStr; i <= MessagePackCode.MaxFixStr; i++)
            {
                //typeLookupTable[i] = MessagePackType.String;
            }

            bytesDecoders[MessagePackCode.Nil] = Decoders.NilBytes.Instance;

            //typeLookupTable[MessagePackCode.NeverUsed] = MessagePackType.Unknown;
            booleanDecoders[MessagePackCode.False] = Decoders.False.Instance;
            booleanDecoders[MessagePackCode.True] = Decoders.True.Instance;
            bytesDecoders[MessagePackCode.Bin8] = Decoders.Bin8Bytes.Instance;
            bytesDecoders[MessagePackCode.Bin16] = Decoders.Bin16Bytes.Instance;
            bytesDecoders[MessagePackCode.Bin32] = Decoders.Bin32Bytes.Instance;
            //typeLookupTable[MessagePackCode.Ext8] = MessagePackType.Extension;
            //typeLookupTable[MessagePackCode.Ext16] = MessagePackType.Extension;
            //typeLookupTable[MessagePackCode.Ext32] = MessagePackType.Extension;
            //typeLookupTable[MessagePackCode.Float32] = MessagePackType.Float;
            //typeLookupTable[MessagePackCode.Float64] = MessagePackType.Float;

            byteDecoders[MessagePackCode.UInt8] = Decoders.UInt8Byte.Instance;

            //typeLookupTable[MessagePackCode.UInt16] = MessagePackType.Integer;
            //typeLookupTable[MessagePackCode.UInt32] = MessagePackType.Integer;
            //typeLookupTable[MessagePackCode.UInt64] = MessagePackType.Integer;

            // byteDecoders[MessagePackCode.Int8] = Decoders.UInt8Byte;


            //typeLookupTable[MessagePackCode.Int16] = MessagePackType.Integer;
            //typeLookupTable[MessagePackCode.Int32] = MessagePackType.Integer;
            //typeLookupTable[MessagePackCode.Int64] = MessagePackType.Integer;
            //typeLookupTable[MessagePackCode.FixExt1] = MessagePackType.Extension;
            //typeLookupTable[MessagePackCode.FixExt2] = MessagePackType.Extension;
            //typeLookupTable[MessagePackCode.FixExt4] = MessagePackType.Extension;
            //typeLookupTable[MessagePackCode.FixExt8] = MessagePackType.Extension;
            //typeLookupTable[MessagePackCode.FixExt16] = MessagePackType.Extension;
            //typeLookupTable[MessagePackCode.Str8] = MessagePackType.String;
            //typeLookupTable[MessagePackCode.Str16] = MessagePackType.String;
            //typeLookupTable[MessagePackCode.Str32] = MessagePackType.String;
            //typeLookupTable[MessagePackCode.Array16] = MessagePackType.Array;
            //typeLookupTable[MessagePackCode.Array32] = MessagePackType.Array;
            //typeLookupTable[MessagePackCode.Map16] = MessagePackType.Map;
            //typeLookupTable[MessagePackCode.Map32] = MessagePackType.Map;

            for (int i = MessagePackCode.MinNegativeFixInt; i <= MessagePackCode.MaxNegativeFixInt; i++)
            {
                //typeLookupTable[i] = MessagePackType.Integer;
            }
        }

        public static MessagePackType GetMessagePackType(byte[] bytes, int offset)
        {
            return MessagePackCode.ToMessagePackType(bytes[offset]);
        }

        public static void EnsureCapacity(ref byte[] bytes, int offset, int appendLength)
        {
            var newLength = offset + appendLength;

            // If null(most case fisrt time) fill byte.
            if (bytes == null)
            {
                bytes = new byte[newLength];
                return;
            }

            // like MemoryStream.EnsureCapacity
            var current = bytes.Length;
            if (newLength > current)
            {
                int num = newLength;
                if (num < 256)
                {
                    num = 256;
                    FastResize(ref bytes, num);
                    return;
                }
                if (num < current * 2)
                {
                    num = current * 2;
                }

                FastResize(ref bytes, num);
            }
        }

        // Buffer.BlockCopy version of Array.Resize
        public static void FastResize(ref byte[] array, int newSize)
        {
            if (newSize < 0) throw new ArgumentOutOfRangeException("newSize");

            byte[] array2 = array;
            if (array2 == null)
            {
                array = new byte[newSize];
                return;
            }

            if (array2.Length != newSize)
            {
                byte[] array3 = new byte[newSize];
                Buffer.BlockCopy(array2, 0, array3, 0, (array2.Length > newSize) ? newSize : array2.Length);
                array = array3;
            }
        }

        public static int WriteNil(ref byte[] bytes, int offset)
        {
            EnsureCapacity(ref bytes, offset, 1);

            bytes[offset] = MessagePackCode.Nil;
            return 1;
        }

        /// <summary>
        /// Unsafe! don't ensure capacity and don't return size.
        /// </summary>
        public static void WriteNilUnsafe(ref byte[] bytes, int offset, bool value)
        {
            bytes[offset] = MessagePackCode.Nil;
        }

        public static Nil ReadNil(byte[] bytes, int offset, out int readSize)
        {
            if (bytes[offset] == MessagePackCode.Nil)
            {
                readSize = 1;
                return Nil.Default;
            }
            else
            {
                throw new InvalidOperationException(string.Format("code is invalid. code:{0} type:{1}", bytes[offset], MessagePackCode.ToMessagePackType(bytes[offset])));
            }
        }

        public static bool IsNil(byte[] bytes, int offset)
        {
            return bytes[offset] == MessagePackCode.Nil;
        }

        public static int WriteBoolean(ref byte[] bytes, int offset, bool value)
        {
            EnsureCapacity(ref bytes, offset, 1);

            bytes[offset] = (value ? MessagePackCode.True : MessagePackCode.False);
            return 1;
        }

        /// <summary>
        /// Unsafe! don't ensure capacity and don't return size.
        /// </summary>
        public static void WriteBooleanUnsafe(ref byte[] bytes, int offset, bool value)
        {
            bytes[offset] = (value ? MessagePackCode.True : MessagePackCode.False);
        }

        /// <summary>
        /// Unsafe! don't ensure capacity and don't return size.
        /// </summary>
        public static void WriteBooleanTrueUnsafe(ref byte[] bytes, int offset)
        {
            bytes[offset] = MessagePackCode.True;
        }

        /// <summary>
        /// Unsafe! don't ensure capacity and don't return size.
        /// </summary>
        public static void WriteBooleanFalseUnsafe(ref byte[] bytes, int offset)
        {
            bytes[offset] = MessagePackCode.False;
        }

        public static bool ReadBoolean(byte[] bytes, int offset, out int readSize)
        {
            readSize = 1;
            return booleanDecoders[bytes[offset]].Read();
        }

        public static int WriteByte(ref byte[] bytes, int offset, byte value)
        {
            if (value <= MessagePackCode.MaxFixInt)
            {
                EnsureCapacity(ref bytes, offset, 1);
                bytes[offset] = value;
                return 1;
            }
            else
            {
                EnsureCapacity(ref bytes, offset, 2);
                bytes[offset] = MessagePackCode.UInt8;
                bytes[offset + 1] = value;
                return 2;
            }
        }

        public static byte ReadByte(byte[] bytes, int offset, out int readSize)
        {
            return byteDecoders[bytes[offset]].Read(bytes, offset, out readSize);
        }

        public static int WriteBytes(ref byte[] bytes, int offset, byte[] value)
        {
            if (value == null)
            {
                return WriteNil(ref bytes, offset);
            }

            var len = value.Length;
            if (len <= byte.MaxValue)
            {
                var size = value.Length + 2;
                EnsureCapacity(ref bytes, offset, size);

                bytes[offset] = MessagePackCode.Bin8;
                bytes[offset + 1] = (byte)len;

                Buffer.BlockCopy(value, 0, bytes, offset + 2, value.Length);
                return size;
            }
            else if (len <= UInt16.MaxValue)
            {
                var size = value.Length + 3;
                EnsureCapacity(ref bytes, offset, size);

                bytes[offset] = MessagePackCode.Bin16;
                bytes[offset + 1] = (byte)(len >> 8);
                bytes[offset + 2] = (byte)(len);

                Buffer.BlockCopy(value, 0, bytes, offset + 3, value.Length);
                return size;
            }
            else
            {
                var size = value.Length + 5;
                EnsureCapacity(ref bytes, offset, size);

                bytes[offset] = MessagePackCode.Bin32;
                bytes[offset + 1] = (byte)(len >> 24);
                bytes[offset + 2] = (byte)(len >> 16);
                bytes[offset + 3] = (byte)(len >> 8);
                bytes[offset + 4] = (byte)(len);

                Buffer.BlockCopy(value, 0, bytes, offset + 5, value.Length);
                return size;
            }
        }

        public static byte[] ReadBytes(byte[] bytes, int offset, out int readSize)
        {
            return bytesDecoders[bytes[offset]].Read(bytes, offset, out readSize);
        }

        // TODO:..................

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static int WriteSByte(ref byte[] bytes, int offset, sbyte value)
        {
            EnsureCapacity(ref bytes, offset, 1);

            bytes[offset] = (byte)value;
            return 1;
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static sbyte ReadSByte(ref byte[] bytes, int offset)
        {
            return (sbyte)bytes[offset];
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe int WriteSingle(ref byte[] bytes, int offset, float value)
        {
            EnsureCapacity(ref bytes, offset, 4);

            if (offset % 4 == 0)
            {
                fixed (byte* ptr = bytes)
                {
                    *(float*)(ptr + offset) = value;
                }
            }
            else
            {
                uint num = *(uint*)(&value);
                bytes[offset] = (byte)num;
                bytes[offset + 1] = (byte)(num >> 8);
                bytes[offset + 2] = (byte)(num >> 16);
                bytes[offset + 3] = (byte)(num >> 24);
            }

            return 4;
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe float ReadSingle(ref byte[] bytes, int offset)
        {
            if (offset % 4 == 0)
            {
                fixed (byte* ptr = bytes)
                {
                    return *(float*)(ptr + offset);
                }
            }
            else
            {
                uint num = (uint)((int)bytes[offset] | (int)bytes[offset + 1] << 8 | (int)bytes[offset + 2] << 16 | (int)bytes[offset + 3] << 24);
                return *(float*)(&num);
            }
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe int WriteDouble(ref byte[] bytes, int offset, double value)
        {
            EnsureCapacity(ref bytes, offset, 8);

            if (offset % 8 == 0)
            {
                fixed (byte* ptr = bytes)
                {
                    *(double*)(ptr + offset) = value;
                }
            }
            else
            {
                ulong num = (ulong)(*(long*)(&value));
                bytes[offset] = (byte)num;
                bytes[offset + 1] = (byte)(num >> 8);
                bytes[offset + 2] = (byte)(num >> 16);
                bytes[offset + 3] = (byte)(num >> 24);
                bytes[offset + 4] = (byte)(num >> 32);
                bytes[offset + 5] = (byte)(num >> 40);
                bytes[offset + 6] = (byte)(num >> 48);
                bytes[offset + 7] = (byte)(num >> 56);
            }

            return 8;
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe double ReadDouble(ref byte[] bytes, int offset)
        {
            if (offset % 8 == 0)
            {
                fixed (byte* ptr = bytes)
                {
                    return *(double*)(ptr + offset);
                }
            }
            else
            {
                uint num = (uint)((int)bytes[offset] | (int)bytes[offset + 1] << 8 | (int)bytes[offset + 2] << 16 | (int)bytes[offset + 3] << 24);
                ulong num2 = (ulong)((int)bytes[offset + 4] | (int)bytes[offset + 5] << 8 | (int)bytes[offset + 6] << 16 | (int)bytes[offset + 7] << 24) << 32 | (ulong)num;
                return *(double*)(&num2);
            }
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe int WriteInt16(ref byte[] bytes, int offset, short value)
        {
            EnsureCapacity(ref bytes, offset, 2);

            fixed (byte* ptr = bytes)
            {
                *(short*)(ptr + offset) = value;
            }

            return 2;
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe short ReadInt16(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(short*)(ptr + offset);
            }
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe int WriteInt32(ref byte[] bytes, int offset, int value)
        {
            EnsureCapacity(ref bytes, offset, 4);

            fixed (byte* ptr = bytes)
            {
                *(int*)(ptr + offset) = value;
            }

            return 4;
        }

        /// <summary>
        /// Unsafe! don't ensure capacity and don't return size.
        /// </summary>
#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe void WriteInt32Unsafe(ref byte[] bytes, int offset, int value)
        {
            fixed (byte* ptr = bytes)
            {
                *(int*)(ptr + offset) = value;
            }
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe int ReadInt32(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(int*)(ptr + offset);
            }
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe int WriteInt64(ref byte[] bytes, int offset, long value)
        {
            EnsureCapacity(ref bytes, offset, 8);

            fixed (byte* ptr = bytes)
            {
                *(long*)(ptr + offset) = value;
            }

            return 8;
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe long ReadInt64(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(long*)(ptr + offset);
            }
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe int WriteUInt16(ref byte[] bytes, int offset, ushort value)
        {
            EnsureCapacity(ref bytes, offset, 2);

            fixed (byte* ptr = bytes)
            {
                *(ushort*)(ptr + offset) = value;
            }

            return 2;
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe ushort ReadUInt16(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(ushort*)(ptr + offset);
            }
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe int WriteUInt32(ref byte[] bytes, int offset, uint value)
        {
            EnsureCapacity(ref bytes, offset, 4);

            fixed (byte* ptr = bytes)
            {
                *(uint*)(ptr + offset) = value;
            }

            return 4;
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe uint ReadUInt32(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(uint*)(ptr + offset);
            }
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe int WriteUInt64(ref byte[] bytes, int offset, ulong value)
        {
            EnsureCapacity(ref bytes, offset, 8);

            fixed (byte* ptr = bytes)
            {
                *(ulong*)(ptr + offset) = value;
            }

            return 8;
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe ulong ReadUInt64(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(ulong*)(ptr + offset);
            }
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static int WriteChar(ref byte[] bytes, int offset, char value)
        {
            return WriteUInt16(ref bytes, offset, (ushort)value);
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static char ReadChar(ref byte[] bytes, int offset)
        {
            return (char)ReadUInt16(ref bytes, offset);
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static int WriteString(ref byte[] bytes, int offset, string value)
        {
            var ensureSize = StringEncoding.UTF8.GetMaxByteCount(value.Length);
            EnsureCapacity(ref bytes, offset, ensureSize);

            return StringEncoding.UTF8.GetBytes(value, 0, value.Length, bytes, offset);
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static string ReadString(ref byte[] bytes, int offset, int count)
        {
            return StringEncoding.UTF8.GetString(bytes, offset, count);
        }

        // decimal underlying "flags, hi, lo, mid" fields are sequential and same layuout with .NET Framework and Mono(Unity)
#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe int WriteDecimal(ref byte[] bytes, int offset, decimal value)
        {
            EnsureCapacity(ref bytes, offset, 16);

            fixed (byte* ptr = bytes)
            {
                *(Decimal*)(ptr + offset) = value;
            }

            return 16;
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe decimal ReadDecimal(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(Decimal*)(ptr + offset);
            }
        }

        // Guid's underlying _a,...,_k field is sequential and same layuout as .NET Framework and Mono(Unity)
#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe int WriteGuid(ref byte[] bytes, int offset, Guid value)
        {
            EnsureCapacity(ref bytes, offset, 16);

            fixed (byte* ptr = bytes)
            {
                *(Guid*)(ptr + offset) = value;
            }

            return 16;
        }

#if !UNITY
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe Guid ReadGuid(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(Guid*)(ptr + offset);
            }
        }

        #region Timestamp/Duration

        public static unsafe int WriteTimeSpan(ref byte[] bytes, int offset, TimeSpan timeSpan)
        {
            checked
            {
                long ticks = timeSpan.Ticks;
                long seconds = ticks / TimeSpan.TicksPerSecond;
                int nanos = (int)(ticks % TimeSpan.TicksPerSecond) * Duration.NanosecondsPerTick;

                EnsureCapacity(ref bytes, offset, 12);
                fixed (byte* ptr = bytes)
                {
                    *(long*)(ptr + offset) = seconds;
                    *(int*)(ptr + offset + 8) = nanos;
                }

                return 12;
            }
        }

        public static unsafe TimeSpan ReadTimeSpan(ref byte[] bytes, int offset)
        {
            checked
            {
                fixed (byte* ptr = bytes)
                {
                    var seconds = *(long*)(ptr + offset);
                    var nanos = *(int*)(ptr + offset + 8);

                    if (!Duration.IsNormalized(seconds, nanos))
                    {
                        throw new InvalidOperationException("Duration was not a valid normalized duration");
                    }
                    long ticks = seconds * TimeSpan.TicksPerSecond + nanos / Duration.NanosecondsPerTick;
                    return TimeSpan.FromTicks(ticks);
                }
            }
        }

        public static unsafe int WriteDateTime(ref byte[] bytes, int offset, DateTime dateTime)
        {
            dateTime = dateTime.ToUniversalTime();

            // Do the arithmetic using DateTime.Ticks, which is always non-negative, making things simpler.
            long secondsSinceBclEpoch = dateTime.Ticks / TimeSpan.TicksPerSecond;
            int nanoseconds = (int)(dateTime.Ticks % TimeSpan.TicksPerSecond) * Duration.NanosecondsPerTick;

            EnsureCapacity(ref bytes, offset, 12);
            fixed (byte* ptr = bytes)
            {
                *(long*)(ptr + offset) = (secondsSinceBclEpoch - Timestamp.BclSecondsAtUnixEpoch);
                *(int*)(ptr + offset + 8) = nanoseconds;
            }

            return 12;
        }

        public static unsafe DateTime ReadDateTime(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                var seconds = *(long*)(ptr + offset);
                var nanos = *(int*)(ptr + offset + 8);

                if (!Timestamp.IsNormalized(seconds, nanos))
                {
                    throw new InvalidOperationException(string.Format(@"Timestamp contains invalid values: Seconds={0}; Nanos={1}", seconds, nanos));
                }
                return Timestamp.UnixEpoch.AddSeconds(seconds).AddTicks(nanos / Duration.NanosecondsPerTick);
            }
        }

        internal static class Timestamp
        {
            internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            internal const long BclSecondsAtUnixEpoch = 62135596800;
            internal const long UnixSecondsAtBclMaxValue = 253402300799;
            internal const long UnixSecondsAtBclMinValue = -BclSecondsAtUnixEpoch;
            internal const int MaxNanos = Duration.NanosecondsPerSecond - 1;

            internal static bool IsNormalized(long seconds, int nanoseconds)
            {
                return nanoseconds >= 0 &&
                    nanoseconds <= MaxNanos &&
                    seconds >= UnixSecondsAtBclMinValue &&
                    seconds <= UnixSecondsAtBclMaxValue;
            }
        }

        internal static class Duration
        {
            public const int NanosecondsPerSecond = 1000000000;
            public const int NanosecondsPerTick = 100;
            public const long MaxSeconds = 315576000000L;
            public const long MinSeconds = -315576000000L;
            internal const int MaxNanoseconds = NanosecondsPerSecond - 1;
            internal const int MinNanoseconds = -NanosecondsPerSecond + 1;

            internal static bool IsNormalized(long seconds, int nanoseconds)
            {
                // Simple boundaries
                if (seconds < MinSeconds || seconds > MaxSeconds ||
                    nanoseconds < MinNanoseconds || nanoseconds > MaxNanoseconds)
                {
                    return false;
                }
                // We only have a problem is one is strictly negative and the other is
                // strictly positive.
                return Math.Sign(seconds) * Math.Sign(nanoseconds) != -1;
            }
        }

        #endregion
    }
}

namespace MessagePack.Decoders
{
    internal interface IBooleanDecoder
    {
        bool Read();
    }

    internal class True : IBooleanDecoder
    {
        internal static IBooleanDecoder Instance = new True();

        True() { }

        public bool Read()
        {
            return true;
        }
    }

    internal class False : IBooleanDecoder
    {
        internal static IBooleanDecoder Instance = new False();

        False() { }

        public bool Read()
        {
            return false;
        }
    }

    internal class InvalidBoolean : IBooleanDecoder
    {
        internal static IBooleanDecoder Instance = new InvalidBoolean();

        InvalidBoolean() { }

        public bool Read()
        {
            throw new InvalidOperationException("code is invalid.");
        }
    }

    internal interface IByteDecoder
    {
        byte Read(byte[] bytes, int offset, out int readSize);
    }

    internal class FixByte : IByteDecoder
    {
        internal static readonly IByteDecoder Instance = new FixByte();

        FixByte()
        {

        }

        public byte Read(byte[] bytes, int offset, out int readSize)
        {
            readSize = 1;
            return bytes[offset];
        }
    }

    internal class UInt8Byte : IByteDecoder
    {
        internal static readonly IByteDecoder Instance = new UInt8Byte();

        UInt8Byte()
        {

        }

        public byte Read(byte[] bytes, int offset, out int readSize)
        {
            readSize = 2;
            return bytes[offset + 1];
        }
    }

    internal class InvalidByte : IByteDecoder
    {
        internal static readonly IByteDecoder Instance = new InvalidByte();

        InvalidByte()
        {

        }

        public byte Read(byte[] bytes, int offset, out int readSize)
        {
            throw new InvalidOperationException(string.Format("code is invalid. code:{0} type:{1}", bytes[offset], MessagePackCode.ToMessagePackType(bytes[offset])));
        }
    }

    internal interface IBytesDecoder
    {
        byte[] Read(byte[] bytes, int offset, out int readSize);
    }

    internal class NilBytes : IBytesDecoder
    {
        internal static readonly IBytesDecoder Instance = new NilBytes();

        NilBytes()
        {

        }

        public byte[] Read(byte[] bytes, int offset, out int readSize)
        {
            readSize = 1;
            return null;
        }
    }

    internal class Bin8Bytes : IBytesDecoder
    {
        internal static readonly IBytesDecoder Instance = new Bin8Bytes();

        Bin8Bytes()
        {

        }

        public byte[] Read(byte[] bytes, int offset, out int readSize)
        {
            var length = bytes[offset + 1];
            var newBytes = new byte[length];
            Buffer.BlockCopy(bytes, offset + 2, newBytes, 0, length);

            readSize = length + 2;
            return newBytes;
        }
    }

    internal class Bin16Bytes : IBytesDecoder
    {
        internal static readonly IBytesDecoder Instance = new Bin16Bytes();

        Bin16Bytes()
        {

        }

        public byte[] Read(byte[] bytes, int offset, out int readSize)
        {
            var length = ((int)bytes[offset + 1] << 8 | (int)bytes[offset + 2]);
            var newBytes = new byte[length];
            Buffer.BlockCopy(bytes, offset + 3, newBytes, 0, length);

            readSize = length + 3;
            return newBytes;
        }
    }

    internal class Bin32Bytes : IBytesDecoder
    {
        internal static readonly IBytesDecoder Instance = new Bin32Bytes();

        Bin32Bytes()
        {

        }

        public byte[] Read(byte[] bytes, int offset, out int readSize)
        {
            var length = ((int)bytes[offset + 1] << 24 | (int)bytes[offset + 2] << 16 | (int)bytes[offset + 3] << 8 | (int)bytes[offset + 4]);
            var newBytes = new byte[length];
            Buffer.BlockCopy(bytes, offset + 5, newBytes, 0, length);

            readSize = length + 5;
            return newBytes;
        }
    }

    internal class InvalidBytes : IBytesDecoder
    {
        internal static readonly IBytesDecoder Instance = new InvalidBytes();

        InvalidBytes()
        {

        }

        public byte[] Read(byte[] bytes, int offset, out int readSize)
        {
            throw new InvalidOperationException(string.Format("code is invalid. code:{0} type:{1}", bytes[offset], MessagePackCode.ToMessagePackType(bytes[offset])));
        }
    }
}