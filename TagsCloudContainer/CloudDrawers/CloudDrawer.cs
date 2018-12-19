using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using TagsCloudContainer.Settings;
using TagsCloudContainer.Tags;

namespace TagsCloudContainer.CloudDrawers
{
    public class CloudDrawer : ICloudDrawer
    {
        private readonly ImageSettings imageSettings;

        public CloudDrawer(ImageSettings imageSettings)
        {
            this.imageSettings = imageSettings;
        }

        public Result<None> Draw(IEnumerable<Tag> tagsCloud)
        {
            using (var bmp = new Bitmap(imageSettings.Height, imageSettings.Width))
            using (var graphics = Graphics.FromImage(bmp))
            {
                graphics.FillRectangle(imageSettings.Theme.BackgroundColor, 0, 0, imageSettings.Height,
                    imageSettings.Width);
                foreach (var tag in tagsCloud)
                    graphics.DrawString(tag.Word, tag.Font, imageSettings.Theme.WordColor, tag.Rectangle.Location);
                try
                {
                    bmp.Save(imageSettings.OutputFile, ImageFormat.Png);
                    return new Result<None>();
                }
                catch (Exception exception)
                {
                    return Result.Fail<None>(exception.Message);
                }
            }
        }
    }
}