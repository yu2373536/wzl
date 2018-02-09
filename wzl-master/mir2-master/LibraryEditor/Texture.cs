//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Library_Editor
//{
//  /**
// * 热血传奇图片数据<br>
// * 使用三字节sRGB方式存放色彩数据<br>
// * 图片不支持透明色，背景为黑色<br>
// * 使用双缓冲加速图像处理
// * 
// * @author 云中双月
// */
//public  class Texture   {

//    private static int EMPTY_COLOR_INDEX = 0;
//    /**
//     * 空图片
//     */
//    public static  Texture EMPTY = new Texture(new byte[]{SDK.palletes[EMPTY_COLOR_INDEX][1],SDK.palletes[EMPTY_COLOR_INDEX][2],SDK.palletes[EMPTY_COLOR_INDEX][3]}, 1, 1);
	
//    private byte[] pixels;
//    private int width;
//    private int height;
//    private  Boolean dirty;
	
//    private Boolean emptyHoldFlag;
//    private static byte[] emptyPixels;
//    private static long clearCount;
//    private static Object clear_locker = new Object();
//    private Object proc_locker = new Object();
	
//    /**
//     * 获取图片宽度
//     * 
//     * @return
//     * 		图片宽度(像素)
//     */
//    public int getWidth() {
//        return width;
//    }
	
//    /**
//     * 获取图片高度
//     * 
//     * @return
//     * 		图片高度(像素)
//     */
//    public int getHeight() {
//        return height;
//    }
	
//    /**
//     * 获取图片色彩数据<br>
//     * 每一个像素点以R G B三个byte的分量存储<br>
//     * 即返回的数据长度为图片宽度*图片高度*3大小<br>
//     * 从图片左上角到右下角
//     * 
//     * @return
//     * 		图片全部颜色数据
//     */
//    public byte[] getRGBs() {
//        return pixels;
//    }
	
//    /**
//     * 获取图片特定点色彩数据
//     * 
//     * @param x
//     * 		横坐标(像素)
//     * @param y
//     * 		纵坐标(像素)
//     * @return
//     * 		特定点色彩数据，三个字节依次表示RGB分量
//     */
//    public byte[] getRGB(int x, int y) {
//        if(x > width - 1 || y > height - 1) return new byte[]{0,0,0};
//        int _idx = (x + y * width) * 3;
//        byte[] ret = new byte[3];
//        ret[0] = pixels[_idx];
//        ret[1] = pixels[_idx + 1];
//        ret[2] = pixels[_idx + 2];
//        return ret;
//    }
	
//    /**
//     * 从RGB字节数组创建图片数据
//     * 
//     * @param sRGB
//     * 		图片色彩数据数据<br>
//     * 		每个像素占用三个字节进行存储，从图片左上角到右下角，必须是RGB顺序
//     * @param width
//     * 		图片宽度
//     * @param height
//     * 		图片高度
//     * 
//     * @throws IllegalArgumentException 传入的像素数据长度不符合要求
//     */
//    public Texture(byte[] sRGB, int width, int height)
//    {
//        this(sRGB, width, height, true);
//    }
	
//    /**
//     * 从RGB字节数组创建图片数据
//     * 
//     * @param sRGB
//     * 		图片色彩数据数据<br>
//     * 		每个像素占用三个字节进行存储，从图片左上角到右下角，必须是RGB顺序
//     * @param width
//     * 		图片宽度
//     * @param height
//     * 		图片高度
//     * @param emptyHoldFlag
//     * 		是否存储一个空的字节数组，用于在将图片清空时快速反应
//     * 
//     * @throws IllegalArgumentException 传入的像素数据长度不符合要求
//     */
//    public Texture(byte[] sRGB, int width, int height, Boolean emptyHoldFlag) 
//    {
//        //if(sRGB != null && width > 0 && height > 0 && sRGB.Length != (width * height * 3))
//        //    throw new IllegalArgumentException("sRGB Length not match width * height * 3 !!!");
//        this.pixels = sRGB;
//        this.width = width;
//        this.height = height;
//        this.emptyHoldFlag = emptyHoldFlag;
//    }
	
//    /**
//     * 判断当前图片是否为空
//     * 
//     * @return true表示当前图片为空，不可用于任何处理/绘制/序列化
//     */
//    public  Boolean empty() {
//        return this == EMPTY || pixels == null || pixels.Length == 0 || width < 1 || height < 1;
//    }
	
//    /**
//     * 判断当前图片是否被修改过<br>
//     * 当前函数返回之后，图片会被置为未修改，即下次调用会返回false
//     * 
//     * @return 上次调用此函数之后图片是否被修改过
//     */
//    public  Boolean dirty()
//    {
//        lock (proc_locker) 
//        {
//            Boolean _dirty = dirty;
//            dirty = false;
//            return _dirty;
//        }
//    }




//    protected void ize() {
//        if(emptyHoldFlag) {
//            lock (clear_locker) {
//                clearCount--;
//                if(clearCount < 1) {
//                    clearCount = 0;
//                    emptyPixels = null;
//                }
//            }
//        }
//    }
	
//    /**
//     * 创建当前图片数据的克隆，完整克隆<br>
//     * 如需创建当前图片部分区域的克隆，则使用{@link #clip(int, int, int, int)}
//     * 
//     * @return 当前图片完整克隆
//     * 
//     * @see #clip(int, int, int, int)
//     */
//    //@Override
//    protected Object clone() 
//    {
//        if(empty())
//            return EMPTY;
//        lock (proc_locker) 
//        {
//            byte[] sRGB = new byte[pixels.Length];
//            Array.Copy(pixels, 0, sRGB, 0, pixels.Length);
//            return new Texture(sRGB, width, height);
//        }
//    }
	
//    /**
//     * 创建当前图片数据的克隆，部分克隆<br>
//     * 如果区域的右方或下方超出图片宽高则忽略超出部分，但左上方不可超出，如果超出则直接不进行处理<br>
//     * 如需创建当前图完整克隆，则使用{@link #clone()}
//     * 
//     * @param x
//     * 		克隆区域起始x坐标
//     * @param y
//     * 		克隆区域起始y坐标
//     * @param w
//     * 		克隆区域宽度
//     * @param h
//     * 		克隆区域高度
//     * 
//     * @return 当前图片部分区域克隆
//     * 
//     * @see #clone()
//     */
//    public  Texture clip(int x, int y, int w, int h) 
//    {
//        if(empty())
//            return EMPTY;
//        if(x < 0 || x > width || y < 0 || y > height) return EMPTY;
//        lock (proc_locker) 
//        {
//            int rx = x + w;
//            if(rx >= width)
//                rx = width - 1;
//            int by = y + h;
//            if(by >= height)
//                by = height - 1;
//            byte[] npixels = new byte[(rx - x) * (by - y) * 3];
//            for(int i = y; i < by; ++i) {
//                for(int j = x; j < rx; ++j) {
//                    int _idx = (j + i * width) * 3;
//                    npixels[(j - x + (i - y) * width) * 3] = pixels[_idx];
//                    npixels[(j - x + (i - y) * width) * 3 + 1] = pixels[_idx + 1];
//                    npixels[(j - x + (i - y) * width) * 3 + 2] = pixels[_idx + 2];
//                }
//            }
//            return new Texture(npixels, rx -x, by -y);
//        }
//    }
	
//    /**
//     * 清除图片色彩数据<br>
//     * 清除图片全部色彩数据<br>
//     * 如果需要清除部分区域色彩数据则使用{@link #clear(int, int, int, int)}
//     * 
//     * @see #clear(int, int, int, int)
//     */
//    public  void clear() 
//    {
//        if(empty()) return;
//        lock (proc_locker) 
//        {
//            if(!emptyHoldFlag) 
//            {
//                byte[] _emptyPixels = new byte[pixels.Length];
//                Array.Copy(_emptyPixels, 0, pixels, 0, _emptyPixels.Length);
//            } 
//            else 
//            {
//                lock (clear_locker) {
//                    if(emptyPixels == null || emptyPixels.Length < pixels.Length)
//                        emptyPixels = new byte[pixels.Length];
//                    Array.Copy(emptyPixels, 0, pixels, 0, pixels.Length);
//                    clearCount++;
//                }
//            }
//            dirty = true;
//        }
//    }
	
//    /**
//     * 清除图片色彩数据<br>
//     * 清除图片内部分区域色彩数据<br>
//     * 如果区域的右方或下方超出图片宽高则忽略超出部分，但左上方不可超出，如果超出则直接不进行处理<br>
//     * 如果需要清除全部色彩数据则使用{@link #clear()}
//     * 
//     * @param x
//     * 		要清除的区域起始x坐标
//     * @param y
//     * 		要清除的区域起始y坐标
//     * @param w
//     * 		要清除的区域宽度
//     * @param h
//     * 		要清除的区域高度
//     * 
//     * @see #clear()
//     */
//    public  void clear(int x, int y, int w, int h) {
//        if(empty()) return;
//        if(x < 0 || x > width || y < 0 || y > height) return;
//        lock (proc_locker) {
//            int rx = x + w;
//            if(rx >= width)
//                rx = width - 1;
//            int by = y + h;
//            if(by >= height)
//                by = height - 1;
//            for(int i = y; i < by; ++i) {
//                for(int j = x; j < rx; ++j) {
//                    int _idx = (j + i * width) * 3;
//                    pixels[_idx] = pixels[_idx + 1] = pixels[_idx + 2] = 0;
//                }
//            }
//            dirty = true;
//        }
//    }
	
//    /**
//     * 将图片转换为灰白<br>
//     * 将图片全部区域转换为灰白<br>
//     * 如果需要转换部分区域为灰白则使用{@link #toGray(int, int, int, int)}
//     * 
//     * @see #toGray(int, int, int, int)
//     */
//    public  void toGray() {
//        if(empty()) return;
//        lock (proc_locker) {
//            for(int i = 0; i < pixels.Length - 2; i += 3) {
//                pixels[i] *= 0.299;
//                pixels[i + 1] *= 0.587;
//                pixels[i + 2] *= 0.114;
//            }
//            dirty = true;
//        }
//    }
	
//    /**
//     * 将图片转换为灰白<br>
//     * 将图片部分区域转换为灰白<br>
//     * 如果区域的右方或下方超出图片宽高则忽略超出部分，但左上方不可超出，如果超出则直接不进行处理<br>
//     * 如果需要转换全部区域为灰白则使用{@link #toGray()}
//     * 
//     * @param x
//     * 		要转换的区域起始x坐标
//     * @param y
//     * 		要转换的区域起始y坐标
//     * @param w
//     * 		要转换的区域宽度
//     * @param h
//     * 		要转换的区域高度
//     * 
//     * @see #toGray()
//     */
//    public  void toGray(int x, int y, int w, int h) {
//        if(empty()) return;
//        if(x < 0 || x > width || y < 0 || y > height) return;
//        lock (proc_locker) {
//            int rx = x + w;
//            if(rx >= width)
//                rx = width - 1;
//            int by = y + h;
//            if(by >= height)
//                by = height - 1;
//            for(int i = y; i < by; ++i) {
//                for(int j = x; j < rx; ++j) {
//                    int _idx = (j + i * width) * 3;
//                    pixels[_idx] =	(byte)(pixels[_idx]*0.299f);
//                    pixels[_idx + 1] = (byte)(pixels[_idx] *  0.587);
//                    pixels[_idx + 2] = (byte)(pixels[_idx] *  0.114); 
//                }
//            }
//            dirty = true;
//        }
//    }
	
//    /**
//     * 将图片进行反色处理<br>
//     * 将图片全部区域进行反色处理<br>
//     * 如果需要对部分区域进行反色处理则使用{@link #inverse(int, int, int, int)}
//     * 
//     * @see #inverse(int, int, int, int)
//     */
//    public  void inverse() {
//        if(empty()) return;
//        lock (proc_locker) {
//            for(int i = 0; i < pixels.Length; ++i) {
//                pixels[i] ^= 0xff;
//            }
//            dirty = true;
//        }
//    }
	
//    /**
//     * 将图片进行反色处理<br>
//     * 将图片部分区域进行反色处理<br>
//     * 如果区域的右方或下方超出图片宽高则忽略超出部分，但左上方不可超出，如果超出则直接不进行处理<br>
//     * 如果需要对全部区域进行反色处理则使用{@link #inverse()}
//     * 
//     * @param x
//     * 		要转换的区域起始x坐标
//     * @param y
//     * 		要转换的区域起始y坐标
//     * @param w
//     * 		要转换的区域宽度
//     * @param h
//     * 		要转换的区域高度
//     * 
//     * @see #inverse()
//     */
//    public  void inverse(int x, int y, int w, int h) {
//        if(empty()) return;
//        if(x < 0 || x > width || y < 0 || y > height) return;
//        lock (proc_locker) {
//            int rx = x + w;
//            if(rx >= width)
//                rx = width - 1;
//            int by = y + h;
//            if(by >= height)
//                by = height - 1;
//            for(int i = y; i < by; ++i) {
//                for(int j = x; j < rx; ++j) {
//                    int _idx = (j + i * width) * 3;
//                    pixels[_idx] ^= 0xff;
//                    pixels[_idx + 1] ^= 0xff;
//                    pixels[_idx + 2] ^= 0xff;
//                }
//            }
//            dirty = true;
//        }
//    }
	
//    /**
//     * 将图片进行透明度处理<br>
//     * 将图片全部区域进行透明度处理<br>
//     * 如果需要对部分区域进行透明度处理则使用{@link #alpha(float, int, int, int, int)}
//     * 
//     * @param alpha 透明度
//     * 
//     * @see #alpha(float, int, int, int, int)
//     */
//    public  void alpha(float alpha) {
//        if(empty()) return;
//        lock (proc_locker) {
//            for(int i = 0; i < pixels.Length; ++i) {
//                pixels[i] = (byte)(pixels[i] * alpha);
//            }
//            dirty = true;
//        }
//    }
	
//    /**
//     * 将图片进行透明度处理<br>
//     * 将图片部分区域进行透明度处理<br>
//     * 如果区域的右方或下方超出图片宽高则忽略超出部分，但左上方不可超出，如果超出则直接不进行处理<br>
//     * 如果需要对全部区域进行透明度处理则使用{@link #alpha(float)}
//     * 
//     * @param alpha 透明度
//     * 
//     * @param x
//     * 		要处理的区域起始x坐标
//     * @param y
//     * 		要处理的区域起始y坐标
//     * @param w
//     * 		要处理的区域宽度
//     * @param h
//     * 		要处理的区域高度
//     * 
//     * @see #alpha(float)
//     */
//    public  void alpha(float alpha, int x, int y, int w, int h) {
//        if(empty()) return;
//        if(x < 0 || x > width || y < 0 || y > height) return;
//        lock (proc_locker) {
//            int rx = x + w;
//            if(rx >= width)
//                rx = width - 1;
//            int by = y + h;
//            if(by >= height)
//                by = height - 1;
//            for(int i = y; i < by; ++i) {
//                for(int j = x; j < rx; ++j) {
//                    int _idx = (j + i * width) * 3;
//                    pixels[_idx] =	(byte)(pixels[_idx]* alpha);
//                    pixels[_idx + 1] =(byte)(pixels[_idx + 1]* alpha);
//                    pixels[_idx + 2] =(byte)(pixels[_idx + 2]* alpha);
//                }
//            }
//            dirty = true;
//        }
//    }
	
//    /**
//     * 将一副目标图像混合到当前图像上<br>
//     * 使用普通的图像叠加方式<br>
//     * 即直接使用目标rgb作为新图片的rgb<br>
//     * 如果需要使用Overlay方式，则使用{@link #blendAdd(Texture, int, int, float)}方式<br>
//     * 如果需要支持透明色，则使用{@link #blendNormalTransparent(Texture, int, int, float, byte, byte, byte)}
//     * 此操作不改变目标图像数据，即使传递了alpha参数
//     * 
//     * @param tar
//     * 		目标图像
//     * @param locx
//     * 		图像叠加起始X坐标
//     * @param locy
//     * 		图像叠加起始Y坐标
//     * @param alpha
//     * 		目标图像透明度
//     * 
//     * @see #blendAdd(Texture, int, int, float)
//     * @see #blendAddTransparent(Texture, int, int, float, byte, byte, byte)
//     * @see #blendNormalTransparent(Texture, int, int, float, byte, byte, byte)
//     */
//    public  void blendNormal(Texture tar, int locx, int locy, float alpha) {
//        if(empty()) return;
//        if(tar.empty()) return;
//        lock (proc_locker) {
//            int x = locx;
//            int y = locy;
//            if(x < 0 || x > width || y < 0 || y < height) return;
//            int rx = x + tar.width;
//            if(rx >= width)
//                rx = width - 1;
//            int by = y + tar.height;
//            if(by >= height)
//                by = height - 1;
//            for(int i = y; i < by; ++i) {
//                for(int j = x; j < rx; ++j) {
//                    int _idx_this = (j + i * width) * 3;
//                    int _idx_that = (j - x + (i - y) * width) * 3;
//                    pixels[_idx_this] = (byte) (tar.pixels[_idx_that] * alpha);
//                    pixels[_idx_this + 1] = (byte) (tar.pixels[_idx_that + 1] * alpha);
//                    pixels[_idx_this + 2] = (byte) (tar.pixels[_idx_that + 2] * alpha);
//                }
//            }
//            dirty = true;
//        }
//    }
	
//    /**
//     * 将一副目标图像混合到当前图像上<br>
//     * 使用普通的图像叠加方式<br>
//     * 即直接使用目标rgb作为新图片的rgb<br>
//     * 如果需要使用Overlay方式，则使用{@link #blendAddTransparent(Texture, int, int, float, byte, byte, byte)}方式<br>
//     * 此操作不改变目标图像数据，即使传递了alpha参数<br>
//     * 支持透明色，即如果目标坐标目标图片的颜色是给定值则忽略
//     * 
//     * @param tar
//     * 		目标图像
//     * @param locx
//     * 		图像叠加起始X坐标
//     * @param locy
//     * 		图像叠加起始Y坐标
//     * @param alpha
//     * 		目标图像透明度
//     * @param r
//     * 		透明色R分量
//     * @param g
//     * 		透明色分量
//     * @param b
//     * 		透明色分量
//     * 
//     * @see #blendAdd(Texture, int, int, float)
//     * @see #blendAddTransparent(Texture, int, int, float, byte, byte, byte)
//     * @see #blendNormal(Texture, int, int, float)
//     */
//    public  void blendNormalTransparent(Texture tar, int locx, int locy, float alpha, byte r, byte g, byte b) {
//        if(empty()) return;
//        if(tar.empty()) return;
//        lock (proc_locker) {
//            int x = locx;
//            int y = locy;
//            if(x < 0 || x > width || y < 0 || y < height) return;
//            int rx = x + tar.width;
//            if(rx >= width)
//                rx = width - 1;
//            int by = y + tar.height;
//            if(by >= height)
//                by = height - 1;
//            for(int i = y; i < by; ++i) {
//                for(int j = x; j < rx; ++j) {
//                    int _idx_this = (j + i * width) * 3;
//                    int _idx_that = (j - x + (i - y) * width) * 3;
//                    byte _r = tar.pixels[_idx_that];
//                    byte _g = tar.pixels[_idx_that + 1];
//                    byte _b = tar.pixels[_idx_that + 2];
//                    if(r != _r || _g != g || _b != b) {
//                        pixels[_idx_this] = (byte) (_r * alpha);
//                        pixels[_idx_this + 1] = (byte) (_g * alpha);
//                        pixels[_idx_this + 2] = (byte) (_b * alpha);
//                    }
//                }
//            }
//            dirty = true;
//        }
//    }
	
//    /**
//     * 将一副目标图像混合到当前图像上<br>
//     * 使用Overlay的图像叠加方式<br>
//     * 即显卡的Add混合模式，在OpenGL里是glBlendFunc(GL_SRC_COLOR, GL_ONE)<br>
//     * 如果需要使用普通方式，则使用{@link #blendNormal(Texture, int, int, float)}方式<br>
//     * 如需支持透明色，则使用{@link #blendNormalTransparent(Texture, int, int, float, byte, byte, byte)}
//     * 此操作不改变目标图像数据，即使传递了alpha参数
//     * 
//     * @param tar
//     * 		目标图像
//     * @param locx
//     * 		图像叠加起始X坐标
//     * @param locy
//     * 		图像叠加起始Y坐标
//     * @param alpha
//     * 		目标图像透明度
//     * 
//     * @see #blendNormal(Texture, int, int, float)
//     * @see #blendNormalTransparent(Texture, int, int, float, byte, byte, byte)
//     * @see #blendAddTransparent(Texture, int, int, float, byte, byte, byte)
//     */
//    public  void blendAdd(Texture tar, int locx, int locy, float alpha) {
//        if(empty()) return;
//        if(tar.empty()) return;
//        lock (proc_locker) {
//            int x = locx;
//            int y = locy;
//            if(x < 0 || x > width || y < 0 || y < height) return;
//            int rx = x + tar.width;
//            if(rx >= width)
//                rx = width - 1;
//            int by = y + tar.height;
//            if(by >= height)
//                by = height - 1;
//            for(int i = y; i < by; ++i) {
//                for(int j = x; j < rx; ++j) {
//                    int _idx_this = (j + i * width) * 3;
//                    int _idx_that = (j - x + (i - y) * width) * 3;
//                    byte r = (byte) (tar.pixels[_idx_that] * alpha);
//                    byte g = (byte) (tar.pixels[_idx_that + 1] * alpha);
//                    byte b = (byte) (tar.pixels[_idx_that + 2] * alpha);
//                    pixels[_idx_this] = (byte) ((r < 128) ? (2 * pixels[_idx_this] * r / 255) : (255 - 2 * (255 - pixels[_idx_this]) * (255 - r) / 255));
//                    pixels[_idx_this + 1] = (byte) ((g < 128) ? (2 * pixels[_idx_this + 1] * g / 255) : (255 - 2 * (255 - pixels[_idx_this + 1]) * (255 - g) / 255));
//                    pixels[_idx_this + 2] = (byte) ((b < 128) ? (2 * pixels[_idx_this + 2] * b / 255) : (255 - 2 * (255 - pixels[_idx_this + 2]) * (255 - b) / 255));
//                }
//            }
//        }
//        dirty = true;
//    }
	
//    /**
//     * 将一副目标图像混合到当前图像上<br>
//     * 使用Overlay的图像叠加方式<br>
//     * 即显卡的Add混合模式，在OpenGL里是glBlendFunc(GL_SRC_COLOR, GL_ONE)<br>
//     * 如果需要使用普通方式，则使用{@link #blendNormalTransparent(Texture, int, int, float, byte, byte, byte)}方式<br>
//     * 此操作不改变目标图像数据，即使传递了alpha参数<br>
//     * 支持透明色，即如果目标坐标目标图片的颜色是给定值则忽略
//     * 
//     * @param tar
//     * 		目标图像
//     * @param locx
//     * 		图像叠加起始X坐标
//     * @param locy
//     * 		图像叠加起始Y坐标
//     * @param alpha
//     * 		目标图像透明度
//     * @param r
//     * 		透明色R分量
//     * @param g
//     * 		透明色分量
//     * @param b
//     * 		透明色分量
//     * 
//     * @see #blendNormal(Texture, int, int, float)
//     * @see #blendNormalTransparent(Texture, int, int, float, byte, byte, byte)
//     * @see #blendAdd(Texture, int, int, float)
//     */
//    public  void blendAddTransparent(Texture tar, int locx, int locy, float alpha, byte r, byte g, byte b) {
//        if(empty()) return;
//        if(tar.empty()) return;
//        lock (proc_locker) {
//            int x = locx;
//            int y = locy;
//            if(x < 0 || x > width || y < 0 || y < height) return;
//            int rx = x + tar.width;
//            if(rx >= width)
//                rx = width - 1;
//            int by = y + tar.height;
//            if(by >= height)
//                by = height - 1;
//            for(int i = y; i < by; ++i) {
//                for(int j = x; j < rx; ++j) {
//                    int _idx_this = (j + i * width) * 3;
//                    int _idx_that = (j - x + (i - y) * width) * 3;
//                    byte _r = (byte) (tar.pixels[_idx_that] * alpha);
//                    byte _g = (byte) (tar.pixels[_idx_that + 1] * alpha);
//                    byte _b = (byte) (tar.pixels[_idx_that + 2] * alpha);
//                    if(r != _r || _g != g || _b != b) {
//                        pixels[_idx_this] = (byte) ((_r < 128) ? (2 * pixels[_idx_this] * _r / 255) : (255 - 2 * (255 - pixels[_idx_this]) * (255 - _r) / 255));
//                        pixels[_idx_this + 1] = (byte) ((_g < 128) ? (2 * pixels[_idx_this + 1] * _g / 255) : (255 - 2 * (255 - pixels[_idx_this + 1]) * (255 - _g) / 255));
//                        pixels[_idx_this + 2] = (byte) ((_b < 128) ? (2 * pixels[_idx_this + 2] * _b / 255) : (255 - 2 * (255 - pixels[_idx_this + 2]) * (255 - _b) / 255));
//                    }
//                }
//            }
//        }
//        dirty = true;
//    }
//}
//}
