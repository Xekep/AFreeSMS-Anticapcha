using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Collections.Generic;

namespace anticapcha1
{
    static class Anticapcha
    {
        private static readonly string url = "http://www.afreesms.com/image.php";
        private static readonly int SymbolsColor = Color.FromArgb(102, 153, 0).ToArgb();

        public static Bitmap CapchaOriginImage { get; private set; }
        public static Bitmap CapchaImageContur { get; private set; }
        public static Bitmap CapchaImage { get; private set; }
        public static string CapchaText { get; private set; }
        public static List<Rectangle> Coordinates { get; private set; } = new List<Rectangle>();
        private static List<uint> CurrentHashes = new List<uint>();
        private static Dictionary<uint, char> Hashes = new Dictionary<uint, char>();

        public static bool NewCapcha()
        {
            try
            {
                using (WebClient wClient = new WebClient())
                {
                    byte[] imageByte = wClient.DownloadData(url);

                    using (MemoryStream ms = new MemoryStream(imageByte, 0, imageByte.Length))
                    {
                        ms.Write(imageByte, 0, imageByte.Length);
                        CapchaImage = (Bitmap) Image.FromStream(ms, true);
                        RecognizeCapcha();
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        public static void EnteredCaptcha(string text)
        {
            if (Coordinates.Count == text.Length)
            {
                char[] charArray = text.ToCharArray();
                for (int i = 0; i < charArray.Length; i++)
                        Hashes[CurrentHashes[i]] = charArray[i];
            }
        }

        private static void RecognizeCapcha()
        {
            // Очищаем массивы
            Coordinates.Clear();
            CurrentHashes.Clear();
            // Меняем PixelFormat
            CapchaImage = CapchaImage.Clone(new Rectangle(0, 0, CapchaImage.Width, CapchaImage.Height), PixelFormat.Format32bppArgb);
            CapchaOriginImage = (Bitmap)CapchaImage.Clone();
            CapchaImageContur = (Bitmap)CapchaImage.Clone();
            // Убираем лишние пиксели
            for (int x = 0; x < CapchaImage.Width; x++)
            {
                for (int y = 0; y < CapchaImage.Height; y++)
                {
                    if (CapchaImage.GetPixel(x, y).ToArgb() != SymbolsColor)
                        CapchaImage.SetPixel(x, y, Color.White);
                    else
                        CapchaImage.SetPixel(x, y, Color.Black);
                }
            }
            // Прозрачность
            CapchaImage.MakeTransparent(Color.White);
            // Поиск координат символов
            Rectangle SymbolPoint = Rectangle.Empty;
            while (true)
            {
                if ((SymbolPoint = FindNextSymbol(SymbolPoint.Right + 1)) != Rectangle.Empty)
                    Coordinates.Add(SymbolPoint);
                   else
                break;
            }
            // Вписать в прямоугольники символы
            foreach (Rectangle Coordinate in Coordinates)
            {
                for (int x = 0; x < Coordinate.Width; x++)
                {
                    for (int y = 0; y < Coordinate.Height; y++)
                    {
                        CapchaImageContur.SetPixel(Coordinate.X + x, Coordinate.Y, Color.Red);
                        CapchaImageContur.SetPixel(Coordinate.X, Coordinate.Y + y, Color.Red);
                        CapchaImageContur.SetPixel(Coordinate.X + x, Coordinate.Y + Coordinate.Height, Color.Red);
                        CapchaImageContur.SetPixel(Coordinate.X + Coordinate.Width, Coordinate.Y + y, Color.Red);
                    }
                }
            }
            // Подсчет контрольных сумм
            string text = "";

            UInt32[] crc_table = new UInt32[256];
            UInt32 crc;

            for (UInt32 i = 0; i < 256; i++)
            {
                crc = i;
                for (UInt32 j = 0; j < 8; j++)
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;

                crc_table[i] = crc;
            };

            foreach (Rectangle Coordinate in Coordinates)
            {
                crc = 0xFFFFFFFF;
                int minX = Coordinate.X;
                int maxX = minX + Coordinate.Width;
                int minY = Coordinate.Y;
                int maxY = minY + Coordinate.Height;

                for (int x = minX; x < maxX; x++)
                {
                    for (int y = minY; y < maxY; y++)
                    {
                        byte[] byteArray = BitConverter.GetBytes(CapchaImage.GetPixel(x, y).ToArgb());
                        foreach (byte _byte in byteArray)
                            crc = crc_table[(crc ^ _byte) & 0xFF] ^ (crc >> 8);
                    }
                }

                crc ^= 0xFFFFFFFF;

                char Value;
                if (!Hashes.TryGetValue(crc, out Value))
                    Value = '0';
                CurrentHashes.Add(crc);
                text += Value;               
            }
            CapchaText = text;
        }

        private static Rectangle FindNextSymbol(int startX)
        {
            bool found = false;
            int minX, minY, maxX, maxY;

            minX = minY = int.MaxValue;
            maxX = maxY = int.MinValue;

            int _x = startX < CapchaImage.Width ? startX : 0;

            for (int x = _x; x < CapchaImage.Width; x++)
            {
                var count = 0;
                for (int y = 0; y < CapchaImage.Height; y++)
                {
                    if (CapchaImage.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                    {
                        found = true;
                        if (x > maxX) maxX = x;
                        if (x < minX) minX = x;

                        if (y > maxY) maxY = y;
                        if (y < minY) minY = y;
                    }
                    else
                        count++;
                }
                if (found && count == CapchaImage.Height)
                    break;
            }

            if (found)
                return new Rectangle(minX, minY, maxX - minX, maxY - minY);
            else
                return Rectangle.Empty;
        }
    }
}
