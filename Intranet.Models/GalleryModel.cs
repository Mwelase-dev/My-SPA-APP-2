using System;

namespace Intranet.Models
{
    public class GalleryModel
    {
        public virtual String GalleryDesc { get; set; }
    }
    
    public class GalleryImageModel
    {
        public virtual String FileName { get; set; }
    }
}