using Ionic.Zlib;
using ManagedSquish;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LibraryEditor
{
    public sealed class MLibrary
    {
        public const int LibVersion = 1;
        public static bool Load = true;
        public string FileName;

        public List<MImage> Images = new List<MImage>();
        public List<int> IndexList = new List<int>();
        public int Count;
        private bool _initialized;

        private BinaryReader _reader_wzl, _reader_wzx;
        private FileStream _stream_wzl, _stream_wzx;

        public MLibrary(string filename)//sharpdx    slimdx 这2个都可以托管绘制图形，跟dx差不多。
        {
            FileName = filename;//"D:\\热血传奇客户端\\热血传奇\\Data\\Tiles.Lib"   "D:\\热血传奇客户端\\热血传奇\\Data\\Mon19.Lib"
            Initialize();
            Close();
        }

        public void Initialize()
        {
            //int CurrentVersion; D:\热血传奇客户端\十五周年客户端\热血传奇\cbohum5.wzx
            _initialized = true;

            if (!File.Exists(FileName))//"D:\\热血传奇客户端\\热血传奇\\Data\\Mon19.Lib"
                return;

            //--------------------------------------------
            Images = new List<MImage>();
            IndexList = new List<int>();


            _stream_wzx = new FileStream(Path.ChangeExtension(FileName, null) + ".wzx", FileMode.Open, FileAccess.Read);////"D:\\热血传奇客户端\\热血传奇\\Data\\Tiles.wil"
            _stream_wzx.Seek(0, SeekOrigin.Begin);
            try
            {
                using (BinaryReader reader = new BinaryReader(_stream_wzx))
                {
                    _stream_wzx.Seek(48, SeekOrigin.Begin);//80// 【Title和indexCount一共48字节,后面就是数据】  wix 其中前44字节为 INDX v1.0-WEMADE Entertainment inc.  后4字节为资源索引数组的长度占4个字节。。48字节以后的就是资源索引了。
                    //  int aa = reader.ReadInt32();
                    // long a1 = (_stream_wzx.Length - 48 )/ 4;

                    _stream_wzx = null;

                    //for (int i = 0; i < Count; i++)  //索引集合
                    //{
                    //    IndexList.Add(reader.ReadInt32());
                    //    Images.Add(null);
                    //}
                    while (reader.BaseStream.Position <= reader.BaseStream.Length - 4)//2288
                    {
                        IndexList.Add(reader.ReadInt32());  //加载list<int> 整形索引集合
                        Images.Add(null);
                    }

                }
            }
            finally
            {
                if (_stream_wzx != null)
                    _stream_wzx.Dispose();
            }

            //-------------------------------------wil-----------------------------------------
            _stream_wzl = new FileStream(Path.ChangeExtension(FileName, null) + ".wzl", FileMode.Open, FileAccess.ReadWrite);
            _reader_wzl = new BinaryReader(_stream_wzl);

            _stream_wzl.Seek(0, SeekOrigin.Begin);


            for (int i = 0; i < IndexList.Count; i++)//560   13200
            {
                CheckImage(i);
            }



        }

        private void CheckImage(int index)
        {

            if (!_initialized)
                Initialize();

            if (Images == null || index < 0 || index >= Images.Count)
                return;

            if (Images[index] == null)
            {
                _stream_wzl.Position = IndexList[index];
                Images[index] = new MImage(_reader_wzl, IndexList[index], _stream_wzl, index);
            }

            if (!Load) return;

            //MImage mi = Images[index];
            //if (!mi.TextureValid)
            //{
            //    _stream_wil.Seek(IndexList[index] + 12, SeekOrigin.Begin);//向前偏移12个
            //    mi.CreateTexture(_reader_wil);// 使用 FBytes   创建bitmap图片
            //}
        }

        public int[] _palette;

        public void Close()
        {
            if (_stream_wzl != null)
                _stream_wzl.Dispose();
            // if (_reader != null)
            //     _reader.Dispose();
        }

        public void Save()
        {
            Close();

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            Count = Images.Count;
            IndexList.Clear();

            int offSet = 8 + Count * 4;
            for (int i = 0; i < Count; i++)
            {
                IndexList.Add((int)stream.Length + offSet); //计算图片索引集合
                Images[i].Save(writer);//这个作用只是为了计算stream.Length的长度。没其他作用
            }

            writer.Flush();
            byte[] fBytes = stream.ToArray();//  图片的数据     {byte[18610343]}
            //  writer.Dispose();

            _stream_wzl = File.Create(FileName);//"D:\\热血传奇客户端\\热血传奇\\Data\\Mon19.Lib"
            writer = new BinaryWriter(_stream_wzl);
            writer.Write(LibVersion);       //写入版本号
            writer.Write(Count);            //写入图片数量
            for (int i = 0; i < Count; i++)
                writer.Write(IndexList[i]); //写入索引值

            writer.Write(fBytes);// 数据值         {byte[18610343]}
            writer.Flush();
            writer.Close();
            writer.Dispose();
            Close();
        }


        //public Point GetOffSet(int index)
        //{
        //    if (!_initialized)
        //        Initialize();

        //    if (Images == null || index < 0 || index >= Images.Count)
        //        return Point.Empty;

        //    if (Images[index] == null)
        //    {
        //        _stream_wzl.Seek(IndexList[index], SeekOrigin.Begin);
        //        Images[index] = new MImage(_reader_wzl);
        //    }

        //    return new Point(Images[index].X, Images[index].Y);
        //}

        //public Size GetSize(int index)
        //{
        //    if (!_initialized)
        //        Initialize();
        //    if (Images == null || index < 0 || index >= Images.Count)
        //        return Size.Empty;

        //    if (Images[index] == null)
        //    {
        //        _stream_wzl.Seek(IndexList[index], SeekOrigin.Begin);
        //        Images[index] = new MImage(_reader_wzl);
        //    }

        //    return new Size(Images[index].Width, Images[index].Height);
        //}

        public MImage GetMImage(int index)
        {
            if (index < 0 || index >= Images.Count)
                return null;

            return Images[index];
        }

        public Bitmap GetPreview(int index)
        {
            if (index < 0 || index >= Images.Count)
                return new Bitmap(1, 1);

            MImage image = Images[index];

            if (image == null || image.Image == null)
                return new Bitmap(1, 1);

            if (image.Preview == null)
                image.CreatePreview();

            return image.Preview;
        }

        public void AddImage(Bitmap image, short x, short y)
        {
            MImage mImage = new MImage(image) { X = x, Y = y };

            Count++;
            Images.Add(mImage);
        }

        public void ReplaceImage(int Index, Bitmap image, short x, short y)
        {
            MImage mImage = new MImage(image) { X = x, Y = y };

            Images[Index] = mImage;
        }

        public void InsertImage(int index, Bitmap image, short x, short y)
        {
            MImage mImage = new MImage(image) { X = x, Y = y };

            Count++;
            Images.Insert(index, mImage);
        }

        public void RemoveImage(int index)
        {
            if (Images == null || Count <= 1)
            {
                Count = 0;
                Images = new List<MImage>();
                return;
            }
            Count--;

            Images.RemoveAt(index);
        }

        public static bool CompareBytes(byte[] a, byte[] b)
        {
            if (a == b) return true;

            if (a == null || b == null || a.Length != b.Length) return false;

            for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;

            return true;
        }

        public void RemoveBlanks(bool safe = false)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (Images[i].FBytes == null || Images[i].FBytes.Length <= 8)
                {
                    if (!safe)
                        RemoveImage(i);
                    else if (Images[i].X == 0 && Images[i].Y == 0)
                        RemoveImage(i);
                }
            }
        }

        public sealed class MImage
        {
            public short Width { get; set; }
            public short Height { get; set; }
            public short X { get; set; }
            public short Y { get; set; }
            public short ShadowX { get; set; }
            public short ShadowY { get; set; }
            public byte Shadow { get; set; }
            public int Length { get; set; }
            public byte[] FBytes { get; set; }
            public bool TextureValid { get; set; }

            public Bitmap Image, Preview;//查看

            //layer 2:
            public short MaskWidth { get; set; }
            public short MaskHeight { get; set; }
            public short MaskX { get; set; }
            public short MaskY { get; set; }

            public int MaskLength { get; set; }
            public byte[] MaskFBytes { get; set; }
            public Bitmap MaskImage;
            public Boolean HasMask { get; set; }

            public bool bo16bit { get; set; }
            public int nSize { get; set; }

            public static int[] _palette = new int[256] { 0, -8388608, -16744448, -8355840, -16777088, -8388480, -16744320, -4144960, -11173737, -6440504, -8686733, -13817559, -10857902, -10266022, -12437191, -14870504, -15200240, -14084072, -15726584, -886415, -2005153, -42406, -52943, -2729390, -7073792, -7067368, -13039616, -9236480, -4909056, -4365486, -12445680, -21863, -10874880, -9225943, -5944783, -7046285, -4369871, -11394800, -8703720, -13821936, -7583183, -7067392, -4378368, -3771566, -9752296, -3773630, -3257856, -5938375, -10866408, -14020608, -15398912, -12969984, -16252928, -14090240, -11927552, -6488064, -2359296, -2228224, -327680, -6524078, -7050422, -9221591, -11390696, -7583208, -7846895, -11919104, -14608368, -2714534, -3773663, -1086720, -35072, -5925756, -12439263, -15200248, -14084088, -14610432, -13031144, -7576775, -12441328, -9747944, -8697320, -7058944, -7568261, -9739430, -11910599, -14081768, -12175063, -4872812, -8688806, -3231340, -5927821, -7572646, -4877197, -2710157, -1071798, -1063284, -8690878, -9742791, -4352934, -10274560, -2701651, -11386327, -7052520, -1059155, -5927837, -10266038, -4348549, -10862056, -4355023, -13291223, -7043997, -8688822, -5927846, -10859991, -6522055, -12439280, -1069791, -15200256, -14081792, -6526208, -7044006, -11386344, -9741783, -8690911, -6522079, -2185984, -10857927, -13555440, -3228293, -10266055, -7044022, -3758807, -15688680, -12415926, -13530046, -15690711, -16246768, -16246760, -16242416, -15187415, -5917267, -9735309, -15193815, -15187382, -13548982, -10238242, -12263937, -7547153, -9213127, -532935, -528500, -530688, -9737382, -10842971, -12995089, -11887410, -13531979, -13544853, -2171178, -4342347, -7566204, -526370, -16775144, -16246727, -16248791, -16246784, -16242432, -16756059, -16745506, -15718070, -15713941, -15707508, -14591323, -15716006, -15711612, -13544828, -15195855, -11904389, -11375707, -14075549, -15709474, -14079711, -11908551, -14079720, -11908567, -8684734, -6513590, -10855895, -12434924, -13027072, -10921728, -3525332, -9735391, -14077696, -13551344, -13551336, -12432896, -11377896, -10849495, -13546984, -15195904, -15191808, -15189744, -10255286, -9716406, -10242742, -10240694, -10838966, -11891655, -10238390, -10234294, -11369398, -13536471, -10238374, -11354806, -15663360, -15193832, -11892662, -11868342, -16754176, -16742400, -16739328, -16720384, -16716288, -16712960, -11904364, -10259531, -8680234, -9733162, -8943361, -3750194, -7039844, -6515514, -13553351, -14083964, -15204220, -11910574, -11386245, -10265997, -3230217, -7570532, -8969524, -2249985, -1002454, -2162529, -1894477, -1040, -6250332, -8355712, -65536, -16711936, -256, -16776961, -65281, -16711681, -1 };

            private int WidthBytes(int nBit, int nWidth)
            {
                return (((nWidth * nBit) + 31) >> 5) * 4;
            }

            public short readShort(byte[] bytes, int index, Boolean reverse)
            {
                if (reverse)
                    return (short)(
                                     (bytes[index] & 0xff)
                                   | (bytes[index + 1] << 8));   //16位数据
                else
                    return (short)((bytes[index] << 8) | (bytes[index + 1] & 0xff));
            }

            public static BitmapData data;
            public static MemoryStream output;
            public static Ionic.Zlib.ZlibStream deflateStream;

            public static int peet, index;

            public static int temp_Width;


            byte[] wwwwww;

            public byte[] bytes;





            //<summary>
            // 将 CreatePreview的创建取消了，所以图片有点大小不
            //</summary>
            //<param name="reader"></param>
            //<param name="index_long"></param>
            //<param name="fStream"></param>
            //<param name="_index"></param>
            public unsafe MImage(BinaryReader reader, long index_long, FileStream fStream, int _index)
            {

                if (reader.BaseStream.Position == 0) return;

                bo16bit = reader.ReadByte() == 5;
                reader.ReadBytes(3);

                Width = reader.ReadInt16();//80    8
                Height = reader.ReadInt16();//71   13
                X = reader.ReadInt16();
                Y = reader.ReadInt16();
                nSize = reader.ReadInt32();//2833

                if (Width * Height < 4)
                {
                    return;
                }


                fStream.Seek(index_long + (16), SeekOrigin.Begin);//80     127866- 127757=    109


                if (nSize == 0)
                {
                    bytes = reader.ReadBytes(this.bo16bit ? Width * Height * 2 : Width * Height);
                }
                else
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        using (ZlibStream deflateStream = new ZlibStream(output, Ionic.Zlib.CompressionMode.Decompress))
                        {
                            deflateStream.Write(reader.ReadBytes(nSize), 0, nSize);//得到图片数据大小,并且写入
                            bytes = output.ToArray();//--------得到解压后的原始数据      {by
                        }
                    }
                }

                this.Image = new Bitmap(Width, Height);
                BitmapData data = Image.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                int index = 0;
                if (bytes.Length == Height * Width * 2.5)
                {
                    int HW_2 = Height * Width * 2;
                    int* scan0 = (int*)data.Scan0;
                    {
                        for (int y = Height - 1; y >= 0; y--)
                        {
                            //---------------------------------------------------------------------------
                            for (int x = 0; x < Width; x++)
                            {
                                if (bo16bit)
                                {
                                    scan0[y * Width + x] = convert16bitTo32bit2(bytes[index++] + ((bytes[index++] << 8)), bytes[HW_2 + y * Width / 2 + x / 2], x);
                                }
                                else
                                    scan0[y * Width + x] = _palette[bytes[index++]];
                            }
                            if ((Width % 4) > 0)
                                index += WidthBytes(bo16bit ? 16 : 8, Width) - (Width * (bo16bit ? 2 : 1));
                            //------------------------------------------------------------------------------------------

                        }
                    }
                }
                else
                {
                    int* scan0 = (int*)data.Scan0;
                    {
                        for (int y = Height - 1; y >= 0; y--)
                        {
                            //---------------------------------------------------------------------------
                            for (int x = 0; x < Width; x++)
                            {
                                if (bo16bit)
                                {
                                    scan0[y * Width + x] = convert16bitTo32bit3(bytes[index++] + (bytes[index++] << 8));
                                }
                                else
                                    scan0[y * Width + x] = _palette[bytes[index++]];
                            }
                            if ((Width % 4) > 0)
                                index += WidthBytes(bo16bit ? 16 : 8, Width) - (Width * (bo16bit ? 2 : 1));
                            //------------------------------------------------------------------------------------------

                        }
                    }
                }

                Image.UnlockBits(data);
            }




            private int convert16bitTo32bit3(int color)
            {
                byte red = (byte)((color & 0xf800) >> 8);
                byte green = (byte)((color & 0x07e0) >> 3);
                byte blue = (byte)((color & 0x001f) << 3);

                if (red == 0 && green == 0 && blue == 0) return 0;
                return ((red << 0x10) | (green << 0x8) | blue) | (255 << 24);
            }



            private int convert16bitTo32bit2(int temp, byte bb, int x) //16位分为5位红，5位蓝，6位绿。          2的16次方，可以表现65536种颜色
            {

                byte red = (byte)((temp & 0xf800) >> 8); // 0xf800  红   1111 1000 0000 0000 （右移动8位之后，就是0000 0000 1111 1000 然后强制转换为一个字节byte  既1111 1000）
                byte green = (byte)((temp & 0x07e0) >> 3);//0x07e0  绿         111 1110 0000 （右移3位就是 000 1111 1100转为byte）
                byte blue = (byte)((temp & 0x001f) << 3); //0x001f  蓝                1 1111  （左移3位就是    1111 1000转为byte）

                //int aa1 = ((bb & 0xf0) >> 4) * 17;
                //int aa2 = (int)((bb & 0xf) * 17);
                //byte alpha = (x % 2 != 0) ? ((byte)aa2) : ((byte)aa1);

                byte alpha = (x % 2 != 0) ? ((byte)((bb & 0xf) * 17)) : ((byte)(((bb & 0xf0) >> 4) * 17)); // 透明值

                return (
                        (red << 0x10)     // 0x10      1 0000    十进制 为 16   左移16位（原来在8位。左移16位就在24位上---16--24之间）
                      | (green << 0x8)   //  1000    十进制 为 8    左移16位（green原来在8位。左移16位就在24位上---8--16之间,blue在0--8之间）
                      | blue)  //0x8         1000    十进制 为 8    左移16位（green原来在8位。左移16位就在24位上---8--16之间,blue在0--8之间）
                      | (alpha << 24);//the final or is setting alpha to max so it'll display (since mir2 images have no alpha layer)

            }


            public int convert16bitTo32bit(int color) //16位分为5位红，5位蓝，6位绿。          2的16次方，可以表现65536种颜色
            {

                //if ((System.Math.Abs(red - green) == 0) && (System.Math.Abs(red - blue) == 0) && (System.Math.Abs(blue - green) == 0))
                //{
                //    return int.MaxValue;
                //}

                //   int temp = ((((byte)((color & 0xf800) >> 8)) << 0x10) | (((byte)((color & 0x07e0) >> 3)) << 0x8) | ((byte)((color & 0x001f) << 3))) | (255 << 24);
                int temp = (((((color & 0xf800) >> 8)) << 0x10) | ((((color & 0x07e0) >> 3)) << 0x8) | (((color & 0x001f) << 3))) | (255 << 24);
                //if ((temp & 0xFFFFFF) == 0)
                //{
                //    return 0;
                //}
                return temp;
            }





            public MImage(byte[] image, short Width, short Height)//only use this when converting from old to new type!
            {
                FBytes = image;
                this.Width = Width;
                this.Height = Height;
            }

            public MImage(Bitmap image)
            {
                if (image == null)
                {
                    FBytes = new byte[0];
                    return;
                }

                Width = (short)image.Width;
                Height = (short)image.Height;

                Image = FixImageSize(image);
                FBytes = ConvertBitmapToArray(Image);
            }

            public MImage(Bitmap image, Bitmap Maskimage)
            {
                if (image == null)
                {
                    FBytes = new byte[0];
                    return;
                }

                Width = (short)image.Width;
                Height = (short)image.Height;
                Image = FixImageSize(image);
                FBytes = ConvertBitmapToArray(Image);
                if (Maskimage == null)
                {
                    MaskFBytes = new byte[0];
                    return;
                }
                HasMask = true;
                MaskWidth = (short)Maskimage.Width;
                MaskHeight = (short)Maskimage.Height;
                MaskImage = FixImageSize(Maskimage);
                MaskFBytes = ConvertBitmapToArray(MaskImage);
            }

            /// <summary>
            /// 固定图片大小
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            private Bitmap FixImageSize(Bitmap input)
            {
                int w = input.Width + (4 - input.Width % 4) % 4;
                int h = input.Height + (4 - input.Height % 4) % 4;

                if (input.Width != w || input.Height != h)
                {
                    Bitmap temp = new Bitmap(w, h);
                    using (Graphics g = Graphics.FromImage(temp))
                    {
                        g.Clear(Color.Transparent);
                        g.DrawImage(input, 0, 0);
                        g.Save();
                    }
                    input.Dispose();
                    input = temp;
                }

                return input;
            }
            /// <summary>
            /// 将bitmap数据转换为byte数组
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            private unsafe byte[] ConvertBitmapToArray(Bitmap input)
            {
                byte[] output;

                BitmapData data = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadOnly,
                                                 PixelFormat.Format32bppArgb);
                //---------------------------------像素数据----------------------------------------------
                byte[] pixels = new byte[input.Width * input.Height * 4];//width 124   height 156

                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                input.UnlockBits(data);

                for (int i = 0; i < pixels.Length; i += 4)//  pixels====  {byte[77376]}
                {


                    //本函数完成的功能是对图像进行灰度处理，我们的基本想法可是将每个象素点的三种颜色成分的值取平均值。
                    //然而由于人眼的敏感性，这样完全取平均值的做法的效果并不好，所以在程序中我取了三个效果最好的参数：.299，.587，.114。
                    //不过在这里要向读者指明的是，在GDI+中图像存储的格式是BGR而非RGB，即其顺序为：Blue、Green、Red。
                    //所以在for循环内部一定要设置好red、green、blue等变量的值，切不可颠倒。

                    //Reverse Red/Blue    pixels[0]红     pixels[1]绿    pixels[2]蓝     pixels[3]透明     所以红和蓝交换的话，就是0和2交换
                    byte b = pixels[i];       //0
                    pixels[i] = pixels[i + 2];//2 
                    pixels[i + 2] = b;
                    //  原来是【红绿蓝】，GDI+ 存储格式为【蓝绿红】
                    if (pixels[i] == 0 && pixels[i + 1] == 0 && pixels[i + 2] == 0)
                        pixels[i + 3] = 0;    //3    Make Transparent   // transparent  [trans'par·ent || træns'perənt /træns'pærənt] adj.  透明的, 明晰的, 显然的
                }
                //----------------------------------------------------------------------------------------------
                int count = Squish.GetStorageRequirements(input.Width, input.Height, SquishFlags.Dxt1);

                output = new byte[count];
                fixed (byte* dest = output)
                fixed (byte* source = pixels)
                {
                    Squish.CompressImage((IntPtr)source, input.Width, input.Height, (IntPtr)dest, SquishFlags.Dxt1);
                }
                return output;
                //   return pixels;
            }

            public void Save(BinaryWriter writer)
            {

                //    Width = reader.ReadInt16();
                //    Height = reader.ReadInt16();
                //    X = reader.ReadInt16();
                //    Y = reader.ReadInt16();
                //    ShadowX = reader.ReadInt16();
                //    ShadowY = reader.ReadInt16();
                //    Shadow = reader.ReadByte();   //   writer.Write(HasMask ? (byte)(Shadow | 0x80) : (byte)Shadow); //1字节   false
                //    Length = reader.ReadInt32();
                //    //check if there's a second layer and read it
                //    HasMask = ((Shadow >> 7) == 1) ? true : false;
                //    if (HasMask)
                //    {
                //        reader.ReadBytes(Length);
                //        MaskWidth = reader.ReadInt16();
                //        MaskHeight = reader.ReadInt16();
                //        MaskX = reader.ReadInt16();
                //        MaskY = reader.ReadInt16();
                //        MaskLength = reader.ReadInt32();
                //    }

                writer.Write(Width);//2字节   124
                writer.Write(Height);//2字节    153
                writer.Write(X);//2字节   -21
                writer.Write(Y);//2字节    -64
                writer.Write(ShadowX);//2字节   0 shadow  [shad·ow || 'ʃædəʊ]  n.  阴影, 影像, 影子  v.  遮蔽, 预示, 使朦胧; 渐变, 变阴暗
                writer.Write(ShadowY);//2字节   0


                writer.Write(HasMask ? (byte)(Shadow | 0x80) : (byte)Shadow); //1字节   false

                writer.Write(FBytes.Length); //图片数据长度   9672



                writer.Write(FBytes);        //图片数据
                if (HasMask)  //false
                {
                    writer.Write(MaskWidth);
                    writer.Write(MaskHeight);
                    writer.Write(MaskX);
                    writer.Write(MaskY);
                    writer.Write(MaskFBytes.Length);
                    writer.Write(MaskFBytes);
                }
            }

            public unsafe void CreateTexture(BinaryReader reader)
            {
                //-----------------------图片纹理  【开始】----------------------------------------
                int w = Width + (4 - Width % 4) % 4;
                int h = Height + (4 - Height % 4) % 4;

                if (w == 0 || h == 0)
                {
                    return;
                }
                Image = new Bitmap(w, h);

                BitmapData data = Image.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite,
                                                 PixelFormat.Format32bppArgb);  //4字节的32位像素。  dest[0];红   dest[1];绿  dest[2];蓝  dest[3];透明

                fixed (byte* source = FBytes)// FBytes 是像素bytes数据
                    Squish.DecompressImage(data.Scan0, w, h, (IntPtr)source, SquishFlags.Dxt1);

                byte* dest = (byte*)data.Scan0;

                for (int i = 0; i < h * w * 4; i += 4)  //宽*高*4====结果为字节总数  为什么乘以4   假如宽10 高10 ，则有100个点像素，每个像素是4个字节（红绿蓝透明），所以乘以4
                {
                    //Reverse Red/Blue
                    byte b = dest[i];
                    dest[i] = dest[i + 2];
                    dest[i + 2] = b;
                }

                Image.UnlockBits(data);
                //-----------------------图片纹理  【结束】----------------------------------------

                if (HasMask)
                {
                    //-----------------------影子纹理  【开始】----------------------------------------
                    w = MaskWidth + (4 - MaskWidth % 4) % 4;
                    h = MaskHeight + (4 - MaskHeight % 4) % 4;

                    if (w == 0 || h == 0)
                    {
                        return;
                    }
                    MaskImage = new Bitmap(w, h);//影子纹理

                    data = MaskImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite,
                                                     PixelFormat.Format32bppArgb);

                    fixed (byte* source = MaskFBytes)
                        Squish.DecompressImage(data.Scan0, w, h, (IntPtr)source, SquishFlags.Dxt1);

                    dest = (byte*)data.Scan0;

                    for (int i = 0; i < h * w * 4; i += 4)
                    {
                        //Reverse Red/Blue
                        byte b = dest[i];
                        dest[i] = dest[i + 2];
                        dest[i + 2] = b;
                    }

                    MaskImage.UnlockBits(data);
                    //-----------------------阴影纹理  【结束】----------------------------------------
                }
            }

            /// <summary>
            /// 绘制宽64高64的影子，把原来图片进行缩放
            /// </summary>
            public void CreatePreview()
            {


                if (Image == null)
                {
                    Preview = new Bitmap(1, 1);
                    return;
                }
                Preview = new Bitmap(64, 64);

                using (Graphics g = Graphics.FromImage(Preview))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.Clear(Color.Transparent);
                    if (Width % 2 == 1)
                    {
                        Width = (short)(Width + 1);
                    }
                    int w = Math.Min((int)Width, 64);

                    int h = Math.Min((int)Height, 64);
                    //  在指定位置并且按指定大小绘制指定的 System.Drawing.Image 的指定部分。
                    g.DrawImage(Image, new Rectangle((64 - w) / 2, (64 - h) / 2, w, h), new Rectangle(0, 0, Width, Height), GraphicsUnit.Pixel);

                    g.Save();
                }
            }



        }
    }
}