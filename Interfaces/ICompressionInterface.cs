using System;
using System.Collections.Generic;
using System.Text;

namespace Projekat_1.Interfaces
{
    internal interface ICompressionInterface
    {
        byte[] Compress(byte[] input);
        byte[] Decompress(byte[] input);
        String Name { get; }
    }
}
