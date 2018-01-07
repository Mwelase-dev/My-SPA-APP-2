using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Intranet.Models;

namespace Intranet.Business
{
    public static class UoWGallery
    {
        //TODO: Needs to be a config setting
        private const string GalleryPath = @"c:\";





        // Gets the list of folders
        public static IEnumerable<GalleryModel> GetFolderList()
        {
            IList<GalleryModel> folderList = new List<GalleryModel>();
            var lst = new DirectoryInfo(ConfigurationManager.AppSettings["Gallery"]).GetDirectories()
                                                    .OrderByDescending(t => t.Name)
                                                    .ToList();
            if (lst.Count > 0)
            {
                foreach (var folder in lst)
                {
                    folderList.Add(new GalleryModel
                        {
                            GalleryDesc = folder.Name
                        });
                }
            }
            return folderList;
        }

        // Gets the images for a specific gallery
        public static IEnumerable<GalleryImageModel> GetImageList(string galleryName)
        {
            String lsFolder = String.Format(@"{0}\{1}", ConfigurationManager.AppSettings["Gallery"], galleryName);
            var lst = new DirectoryInfo(lsFolder).GetFiles("*.jpg")
                                                 .OrderByDescending(t => t.Name)
                                                 .ToList();

            return lst.Select(file => new GalleryImageModel {FileName = file.Name}).ToList();
        }
    }
}