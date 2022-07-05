using Makale_BLL;
using Makale_Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace MakaleWebProject.Models
{
    public class CacheHelper
    {
        public static List<Kategori> KategoriCache()
        {
            var sonuc = WebCache.Get("kategori-cache");

            if (sonuc==null)
            {
                KategoriYonet kategoriYonet = new KategoriYonet();
                sonuc = kategoriYonet.KategoriGetir();

                WebCache.Set("kategori-cache", sonuc, 20, true);
            }
            return sonuc;
        }

        public static void CacheTemizle()
        {
            WebCache.Remove("kategori-cache");
        }



    }
}