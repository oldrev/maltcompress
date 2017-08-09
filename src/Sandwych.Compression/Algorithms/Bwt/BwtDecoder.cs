// https://gist.github.com/Lordron/5039958

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.Compression.Algorithms.Bwt
{
    public class BwtDecoder : AbstractCoder
    {
        private int[] _buckets;
        private int[] _indices;

        public override void Code(Stream inStream, Stream outStream, ICodingProgress progress = null)
        {
            throw new NotImplementedException();
        }

        public void DecodeBlock(byte[] buf_encoded, byte[] buf_decoded, int size, int primary_index)
        {
            byte[] F = new byte[size];
            int[] buckets = new int[0x100];
            int[] indices = new int[size];

            for (int i = 0; i < 0x100; i++)
            {
                buckets[i] = 0;
            }

            for (int i = 0; i < size; i++)
            {
                buckets[buf_encoded[i]]++;
            }

            for (int i = 0, k = 0; i < 0x100; i++)
            {
                for (int j = 0; j < buckets[i]; j++)
                {
                    F[k++] = (byte)i;
                }
            }

            for (int i = 0, j = 0; i < 0x100; i++)
            {
                while (i > F[j] && j < size - 1)
                {
                    j++;
                }
                buckets[i] = j;
            }

            for (int i = 0; i < size; i++)
                indices[buckets[buf_encoded[i]]++] = i;

            for (int i = 0, j = primary_index; i < size; i++)
            {
                buf_decoded[i] = buf_encoded[j];
                j = indices[j];
            }
        }



    }
}
