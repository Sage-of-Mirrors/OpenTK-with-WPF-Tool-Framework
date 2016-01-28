using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms.Integration;
using System.IO;
using Microsoft.Win32;
using GameFormatReader.Common;
using GameFormatReader.GCWii.Binaries.GC;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenGL_in_WPF_Framework
{
    class ViewModel
    {
        private RARC m_loadedRarc;

        private Renderer m_renderer;

        internal void CreateGraphicsContext(GLControl ctrl, WindowsFormsHost host)
        {
            m_renderer = new Renderer(ctrl, host);
        }

        internal void Open()
        {
            OpenFileDialog openFile = new OpenFileDialog();

            if (openFile.ShowDialog() == true)
            {
                string fileName = openFile.FileName;

                string tempFileName = ""; 

                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);

                    string compressedFileTest = reader.ReadString(4);

                    if (compressedFileTest == "Yay0")
                    {
                        byte[] uncompressedArc = DecodeYay0(reader);

                        reader.Close();
                        
                        tempFileName = System.IO.Path.GetTempFileName();

                        fileName = tempFileName;

                        FileInfo info = new FileInfo(fileName);

                        info.Attributes = FileAttributes.Temporary;

                        using (FileStream tempStream = new FileStream(tempFileName, FileMode.Create))
                        {
                            EndianBinaryWriter tempWriter = new EndianBinaryWriter(tempStream, Endian.Big);

                            tempWriter.Write(uncompressedArc);

                            tempWriter.Flush();

                            tempWriter.Close();
                        }
                    }

                    else if (compressedFileTest == "Yaz0")
                    {
                        byte[] uncompressedArc = DecodeYaz0(reader);

                        reader.Close();

                        tempFileName = System.IO.Path.GetTempFileName();

                        fileName = tempFileName;

                        FileInfo info = new FileInfo(fileName);

                        info.Attributes = FileAttributes.Temporary;

                        using (FileStream tempStream = new FileStream(tempFileName, FileMode.Create))
                        {
                            EndianBinaryWriter tempWriter = new EndianBinaryWriter(tempStream, Endian.Big);

                            tempWriter.Write(uncompressedArc);

                            tempWriter.Flush();

                            tempWriter.Close();
                        }
                    }
                }

                m_loadedRarc = new RARC(fileName);

                if (File.Exists(tempFileName))
                    File.Delete(tempFileName);
            }
        }

        #region Nintendo-Specific

        internal byte[] DecodeYay0(EndianBinaryReader reader)
        {
            int uncompressedSize = reader.ReadInt32();

            int linkTableOffset = reader.ReadInt32();

            int nonLinkedTableOffset = reader.ReadInt32();

            int maskBitCounter = 0;

            int currentOffsetInDestBuffer = 0;

            int currentMask = 0;

            byte[] uncompData = new byte[uncompressedSize];

            do
            {
                if (maskBitCounter == 0)
                {
                    currentMask = reader.ReadInt32();

                    maskBitCounter = 32;
                }

                if (((uint)currentMask & (uint)0x80000000) == 0x80000000)
                {
                    uncompData[currentOffsetInDestBuffer] = reader.ReadByteAt(nonLinkedTableOffset);

                    currentOffsetInDestBuffer++;

                    nonLinkedTableOffset++;
                }

                else
                {
                    ushort link = reader.ReadUInt16At(linkTableOffset);

                    linkTableOffset += 2;

                    int offset = currentOffsetInDestBuffer - (link & 0xfff);

                    int count = link >> 12;

                    if (count == 0)
                    {
                        byte countModifier;

                        countModifier = reader.ReadByteAt(nonLinkedTableOffset);

                        nonLinkedTableOffset++;

                        count = countModifier + 18;
                    }

                    else
                        count += 2;

                    int blockCopy = offset;

                    for (int i = 0; i < count; i++)
                    {
                        uncompData[currentOffsetInDestBuffer] = uncompData[blockCopy - 1];

                        currentOffsetInDestBuffer++;

                        blockCopy++;
                    }
                }

                currentMask <<= 1;

                maskBitCounter--;

            } while (currentOffsetInDestBuffer < uncompressedSize);

            return uncompData;
        }

        internal byte[] DecodeYaz0(EndianBinaryReader reader)
        {
            int uncompressedSize = reader.ReadInt32();

            byte[] dest = new byte[uncompressedSize];

            int srcPlace = 0x10, dstPlace = 0; //current read/write positions

            int validBitCount = 0; //number of valid bits left in "code" byte

            byte currCodeByte = 0;

            while (dstPlace < uncompressedSize)
            {
                //read new "code" byte if the current one is used up
                if (validBitCount == 0)
                {
                    currCodeByte = reader.ReadByteAt(srcPlace);

                    ++srcPlace;

                    validBitCount = 8;
                }

                if ((currCodeByte & 0x80) != 0)
                {
                    //straight copy
                    dest[dstPlace] = reader.ReadByteAt(srcPlace);

                    dstPlace++;

                    srcPlace++;
                }

                else
                {
                    //RLE part
                    byte byte1 = reader.ReadByteAt(srcPlace);

                    byte byte2 = reader.ReadByteAt(srcPlace + 1);

                    srcPlace += 2;

                    int dist = ((byte1 & 0xF) << 8) | byte2;

                    int copySource = dstPlace - (dist + 1);

                    int numBytes = byte1 >> 4;

                    if (numBytes == 0)
                    {
                        numBytes = reader.ReadByteAt(srcPlace) + 0x12;
                        srcPlace++;
                    }

                    else
                        numBytes += 2;

                    //copy run
                    for (int i = 0; i < numBytes; ++i)
                    {
                        dest[dstPlace] = dest[copySource];

                        copySource++;

                        dstPlace++;
                    }
                }

                //use next bit from "code" byte
                currCodeByte <<= 1;

                validBitCount -= 1;
            }

            return dest;
        }

        #endregion
    }
}
