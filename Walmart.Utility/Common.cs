using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Walmart.Utility
{
    public class Common
    {


        public Bitmap GetResizeImage(Bitmap bm, int newWidth, int newHeight)
        {
            var newSize = new Size(newWidth, newHeight);

            return new Bitmap(bm, newSize);
        }

        public bool resizeImage(string imagePath, string id, ref string newImagePath)
        {
            try
            {
               
                System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath);
                Bitmap b = new Bitmap(img);
                System.Drawing.Image i = resizeImage(b, new Size(591, 709));
                string newpath = imagePath ;
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                if (!File.Exists(imagePath))
                {
                    
                    i.Save(imagePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            //Get the image current width  
            int sourceWidth = imgToResize.Width;
            //Get the image current height  
            int sourceHeight = imgToResize.Height;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            //Calulate  width with new desired size  
            nPercentW = ((float)size.Width / (float)sourceWidth);
            //Calculate height with new desired size  
            nPercentH = ((float)size.Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //New Width  
            int destWidth = (int)(sourceWidth * nPercent);
            //New Height  
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            // Draw image with new width and height  
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return (System.Drawing.Image)b;
        }

    }
}
