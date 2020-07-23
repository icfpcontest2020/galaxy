using System;
using System.Text;

namespace PlanetWars.Contracts.AlienContracts.Serialization
{
    public static class EncodingExtensions
    {
        public static string AlienEncode(this Data? data)
        {
            var result = new StringBuilder();
            Encode(result, data);
            return result.ToString();
        }

        public static Data? AlienDecode(this string data)
        {
            var position = 0;
            return DecodeData(data, ref position);
        }

        private static Data? DecodeData(string data, ref int position)
        {
            var token = ReadOne(data, ref position);
            if (token.type == TokenType.EmptyList)
                return null;

            if (token.type == TokenType.Pair)
            {
                var head = DecodeData(data, ref position);
                var tail = DecodeData(data, ref position);
                return new PairData(head, tail);
            }

            if (token.type == TokenType.NegativeNumber || token.type == TokenType.PositiveNumber)
                return new NumData(token.number);

            throw new FormatException(token.ToString());
        }

        private static (TokenType type, long number) ReadOne(string data, ref int position)
        {
            try
            {
                long a = data[position++] - '0';
                if (a != 0 && a != 1)
                    throw new FormatException($"Invalid char at {position}");

                long b = data[position++] - '0';
                if (b != 0 && b != 1)
                    throw new FormatException($"Invalid char at {position}");

                var tokenType = (TokenType)(a * 2 + b);
                if (tokenType == TokenType.EmptyList || tokenType == TokenType.Pair)
                    return (tokenType, 0);

                var start = position;
                while (data[position] == '1')
                    position++;
                if (data[position] != '0')
                    throw new FormatException($"Invalid char at {position}");

                var bytesCount = position - start;
                position++;
                long code = 0;
                for (int i = 0; i < bytesCount << 2; i++)
                {
                    long bit = data[position++] - '0';
                    if (bit != 0 && bit != 1)
                        throw new FormatException($"Invalid char at {position}");
                    code = (code << 1) | bit;
                }
                if (tokenType == TokenType.NegativeNumber)
                    code = -code;
                return (tokenType, code);
            }
            catch (IndexOutOfRangeException)
            {
                throw new FormatException($"Data was unexpectedly terminated at {position}");
            }
        }

        private enum TokenType
        {
            EmptyList = 0,
            PositiveNumber = 1,
            NegativeNumber = 2,
            Pair = 3,
        }

        private static void Encode(StringBuilder result, Data? data)
        {
            switch (data)
            {
                case null:
                    EncodeEmptyList(result);
                    return;
                case PairData pair:
                    EncodePair(result);
                    Encode(result, pair.Value);
                    Encode(result, pair.Next);
                    return;
                case NumData num:
                    EncodeNumber(result, num.Value);
                    return;
                default:
                    throw new InvalidOperationException(data.ToString());
            }
        }

        private static void EncodePair(StringBuilder result)
        {
            result.Append("11");
        }

        private static void EncodeEmptyList(StringBuilder result)
        {
            result.Append("00");
        }

        private static void EncodeNumber(StringBuilder result, long number)
        {
            if (number == 0)
            {
                result.Append("010");
                return;
            }
            if (number == 1)
            {
                result.Append("01100001");
                return;
            }
            if (number == 2)
            {
                result.Append("01100010");
                return;
            }
            if (number == -1)
            {
                result.Append("10100001");
                return;
            }
            if (number == -2)
            {
                result.Append("10100010");
                return;
            }

            result.Append(number >= 0 ? "01" : "10");

            var bitsCount = 0;
            number = Math.Abs(number);
            var cur = number;
            while (cur != 0)
            {
                bitsCount++;
                cur >>= 1;
            }

            var bytesCount = bitsCount >> 2;
            var padding = bitsCount & 3;
            if (padding > 0)
            {
                bytesCount++;
                padding = 4 - padding;
            }

            result.Append('1', bytesCount);
            result.Append('0', padding + 1);

            cur = 1L << (bitsCount - 1);
            while (cur != 0)
            {
                result.Append((number & cur) != 0 ? '1' : '0');
                cur >>= 1;
            }
        }
    }
}