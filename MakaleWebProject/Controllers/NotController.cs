using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
//using MakaleWebProject.Data;
using Makale_Entities;
using Makale_BLL;
using MakaleWebProject.Filter;
using MakaleWebProject.Models;

namespace MakaleWebProject.Controllers
{
    [Exc]
    public class NotController : Controller
    {

        MakaleYonet makaleYonet = new MakaleYonet();
        KategoriYonet kategoriYonet = new KategoriYonet();

        [Aut]
        public ActionResult Index()
        {
            //var nots = makaleYonet.MakaleGetir();
            Kullanici user = (Kullanici)Session["login"];
            var nots = makaleYonet.ListQeryable().Include("Kullanici").Where(x=>x.Kullanici.Id==user.Id).OrderByDescending(x=>x.DegistirmeTarihi); //Kullanici tablosu ile join yaptık.

            return View(nots.ToList());
        }
        public ActionResult Begendiklerim()
        { 
            LikeYonet likeYonet = new LikeYonet();
            Kullanici user = Session["login"] as Kullanici;


            var makale = likeYonet.ListQeryable().Include("Kullanici").Include("Makale").Where(x => x.Kullanici.Id == user.Id).Select(x => x.Makale).
                Include("Kategori").Include("Kullanici").OrderByDescending(x => x.DegistirmeTarihi);

           


            return View("Index", makale.ToList());


        }


        [Aut]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Not not = makaleYonet.NotBul(id.Value);
            if (not == null)
            {
                return HttpNotFound();
            }
            return View(not);
        }

        [Aut]
        public ActionResult Create()
        {
            ViewBag.KategoriID = new SelectList(CacheHelper.KategoriCache(),"Id","KategoriBaslik");
            return View();
        }

        [Aut]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Not not)
        {
            ModelState.Remove("KayitTarihi");
            ModelState.Remove("DegistirmeTarihi");
            ModelState.Remove("DegistirenKullanici");
            ViewBag.KategoriID = new SelectList(CacheHelper.KategoriCache(), "Id", "KategoriBaslik", not.KategoriID);

            if (ModelState.IsValid)
            {
                not.Kullanici = (Kullanici)Session["login"];
                BusinessLayerResult<Not> sonuc = makaleYonet.NotKaydet(not);
                if (sonuc.hata.Count > 0)
                {
                    sonuc.hata.ForEach(x => ModelState.AddModelError("", x));
                    return View(not);
                }                            
                return RedirectToAction("Index");
            }

           
            return View(not);
        }

        // GET: Not/Edit/5
        [Aut]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Not not = makaleYonet.NotBul(id.Value);
            if (not == null)
            {
                return HttpNotFound();
            }
            ViewBag.KategoriID = new SelectList(CacheHelper.KategoriCache(), "Id", "KategoriBaslik", not.KategoriID);
            return View(not);
        }

        [Aut]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Not not)
        {
            ModelState.Remove("KayitTarihi");
            ModelState.Remove("DegistirmeTarihi");
            ModelState.Remove("DegistirenKullanici");
            ViewBag.KategoriID = new SelectList(CacheHelper.KategoriCache(), "Id", "KategoriBaslik", not.KategoriID);

            if (ModelState.IsValid)
            {
                not.Kullanici = (Kullanici)Session["login"];

                BusinessLayerResult<Not> sonuc = makaleYonet.NotKaydet(not);
                if (sonuc.hata.Count > 0)
                {
                    sonuc.hata.ForEach(x => ModelState.AddModelError("", x));
                    return View(not);
                }
                return RedirectToAction("Index");
            }
            return View(not);
        }

        [Aut]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Not not = makaleYonet.NotBul(id.Value);
            if (not == null)
            {
                return HttpNotFound();
            }
            return View(not);
        }

        [Aut]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BusinessLayerResult<Not> sonuc = makaleYonet.NotSil(id);          
            return RedirectToAction("Index");
        }

       
        LikeYonet likeyonet = new LikeYonet(); 
        [Aut]
        public ActionResult LikeMakale(int[] dizi)
        {            
            List<int> likedizi = new List<int>();
            Kullanici user =Session["login"] as Kullanici;

            if (user!=null)
            {
                if (dizi!=null)
                {
                   likedizi = likeyonet.List(x => x.Kullanici.Id == user.Id && dizi.Contains(x.Makale.Id)).Select(x=>x.Makale.Id).ToList();
                }
            }
            return Json(new { sonuc =likedizi});
        }
        [Aut]
        [HttpPost]
        public ActionResult LikeDurumUpdate(int notid,bool like)
        {
            int sonuc = 0;
            Kullanici user = Session["login"] as Kullanici;
            Begeni begeni = likeyonet.BegeniGetir(notid, user.Id);
            Not makale = makaleYonet.NotBul(notid);

            if (begeni!=null && like==false)
            {
                sonuc=likeyonet.BegeniSil(begeni);
            }
            else if (begeni==null && like==true)
            {
                sonuc=likeyonet.BegeniEkle(new Begeni() 
                { 
                    Kullanici=user,
                    Makale=makale
                });
            }

            if (sonuc>0)
            {
                if (like)
                {
                    makale.BegeniSayisi++;
                }
                else
                {
                    makale.BegeniSayisi--;
                }
                BusinessLayerResult<Not> result= makaleYonet.NotUpdate(makale);
                if (result.hata.Count==0)
                {
                    return Json(new { hata = false, sonuc = makale.BegeniSayisi });
                }               
            }
            return Json(new { hata = true, sonuc = makale.BegeniSayisi });
        }
    }
}
