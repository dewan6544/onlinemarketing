using onlinemarketing.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace onlinemarketing.Controllers
{
    public class UserController : Controller
    {
        onlinemarketingEntities6 db = new onlinemarketingEntities6();
        // GET: User
        public ActionResult Index(int? page)
        {
            if (TempData["cart"]!=null)
            {
                double x = 0;
                List<cart> li2 = TempData["cart"] as List<cart>;

                foreach(var item in li2)
                {
                    x +=Convert.ToInt32(item.o_bil);
                }
                TempData["total"] = x;


            }
            TempData.Keep();
            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.categories.Where(x => x.cat_status == 1).OrderByDescending(x => x.cat_id).ToList();
            IPagedList<category> cate = list.ToPagedList(pageindex, pagesize);

            return View(cate);
        }

        [HttpGet]

        public ActionResult CreateAdd()
        {

            if (Session["u_email"] == null)
            {
                return RedirectToAction("Login", "user");
            }
            else
            {
                List<category> li = db.categories.ToList();
                ViewBag.categorylist = new SelectList(li, "cat_id", "cat_name");
                ViewBag.svm = Session["u_email"];
                return View();
            }
           

            return View();
        }

        [HttpPost]
        public ActionResult CreateAdd(product p, HttpPostedFileBase imgfile)
        {
            List<category> li = db.categories.ToList();
            ViewBag.categorylist = new SelectList(li, "cat_id", "cat_name");


            string path = uploadimage(imgfile);

            if (path.Equals("-1"))
            {
                ViewBag.error = "image could not be uploaded";

            }
            else

            {


                product pr = new product();
                pr.pro_name = p.pro_name;
                pr.pro_price = p.pro_price;
                pr.pro_image = path;
                pr.cat_id_fk = p.pro_user_id_fk;

                pr.pro_desc = p.pro_desc;
                pr.pro_user_id_fk = Convert.ToInt32(Session["u_id"].ToString());
                db.products.Add(pr);
                db.SaveChanges();

                Response.Redirect("DisplayAdd");
            }
            return View();
        }

        public ActionResult DisplayAdd(int? id, int? page)
        {
            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.products.Where(x => x.cat_id_fk == id).OrderByDescending(x => x.pro_id).ToList();
            IPagedList<product> cate = list.ToPagedList(pageindex, pagesize);

            return View(cate);
        }

        [HttpPost]
        public ActionResult DisplayAdd(int? id, int? page, string search)
        {

            TempData.Keep();

            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.products.Where(x => x.pro_name.Contains(search)).OrderByDescending(x => x.pro_id).ToList();
            IPagedList<product> cate = list.ToPagedList(pageindex, pagesize);

            return View(cate);
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]

        public ActionResult Register(tbl_user us, HttpPostedFileBase imgfile)
        {

            string path = uploadimage(imgfile);

            if (path.Equals("-1"))
            {
                ViewBag.error = "image could not be uploaded";

            }
            else if (ModelState.IsValid)
            {
                tbl_user u = new tbl_user();
                u.u_name = us.u_name;
                u.u_password = us.u_password;
                u.u_contact = us.u_contact;
                u.u_email = us.u_email;
                u.u_image = path;
                db.tbl_user.Add(u);
                db.SaveChanges();



                return RedirectToAction("Login");
                return View();


            }
            return View();
        }



        public ActionResult Login()
        {
            if (Session["u_email"] != null)
            {
                return RedirectToAction("CreateAdd", "user", new { svm = Session["u_email"].ToString() });
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Login(tbl_user svm)
        {

            tbl_user ad = db.tbl_user.Where(x => x.u_email == svm.u_email && x.u_password == svm.u_password).SingleOrDefault();
            if (ad != null)
            {
                Session["u_id"] = ad.u_id.ToString();
                Session["user"] = ad.u_name;

                HttpCookie cookie = new HttpCookie("name");
                cookie.Values["name"] = ad.u_name;

                HttpCookie role = new HttpCookie("access");
                role.Values["access"] = "user";

                cookie.Expires = DateTime.Now.AddMinutes(10000);// update this later
                role.Expires = DateTime.Now.AddMinutes(10000);
                Response.Cookies.Add(cookie);
                Response.Cookies.Add(role);

                Session["u_id"] = ad.u_id.ToString();
                Session["user"] = ad.u_name;

                Session["u_email"] = svm.u_email;

                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.error = "Invalid User Name or Password";
            }




            return View();
        }

        [HttpGet]

        public string uploadimage(HttpPostedFileBase file)

        {

            Random r = new Random();

            string path = "-1";

            int random = r.Next();

            if (file != null && file.ContentLength > 0)

            {

                string extension = Path.GetExtension(file.FileName);

                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))

                {

                    try

                    {



                        path = Path.Combine(Server.MapPath("~/Content/upload"), random + Path.GetFileName(file.FileName));

                        file.SaveAs(path);

                        path = "~/Content/upload/" + random + Path.GetFileName(file.FileName);



                        //    ViewBag.Message = "File uploaded successfully";

                    }

                    catch (Exception ex)

                    {

                        path = "-1";

                    }

                }

                else

                {

                    Response.Write("<script>alert('Only jpg ,jpeg or png formats are acceptable....'); </script>");

                }

            }



            else

            {

                Response.Write("<script>alert('Please select a file'); </script>");

                path = "-1";

            }







            return path;


        }

        public ActionResult SignOut()
        {

            HttpCookie username = Request.Cookies["name"];
            username.Expires = DateTime.Now.AddDays(-1);
            HttpCookie role = Request.Cookies["access"];
            role.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(username);
            Response.Cookies.Add(role);


            Session.RemoveAll();
            Session.Abandon();

            return RedirectToAction("Index");
        }

        public ActionResult ViewAdds(int? id)
        {

            ad_view_model adm = new ad_view_model();

            product p = db.products.Where(x => x.pro_id == id).SingleOrDefault();
            adm.pro_id = p.pro_id;
            adm.pro_name = p.pro_name;
            adm.pro_image = p.pro_image;
            adm.pro_price = p.pro_price;
            adm.pro_desc = p.pro_desc;

            category cat = db.categories.Where(x => x.cat_id == p.cat_id_fk).SingleOrDefault();
            adm.cat_name = cat.cat_name;
            tbl_user u = db.tbl_user.Where(x => x.u_id == p.pro_user_id_fk).SingleOrDefault();
            adm.u_name = u.u_name;
            adm.u_image = u.u_image;
            adm.u_contact = u.u_contact;
            adm.pro_user_id_fk = u.u_id;
            return View(adm);



        }
        public ActionResult Add_Delete(int? id)
        {
            product p = db.products.Where(x => x.pro_id == id).SingleOrDefault();
            db.products.Remove(p);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        /*  public ActionResult Ad_tocart(int? id)

          {
              if (TempData["cart"]==null)
              {
                  li.Add(c);
                  TempData["cart"] = li;
              }
              else
              {
                  List<cart> li2 = TempData["cart"] as List<cart>;
                  li2.Add(c);
                  TempData["cart"] = li2;
              }
              TempData.Keep();
              return RedirectToAction("Index");
          }*/

        
        public ActionResult Add_tocart(int? id)
        {
            product p = db.products.Where(x => x.pro_id == id).SingleOrDefault();
            return View(p);
        }

        List<cart> li = new List<cart>();
        [HttpPost]
        public ActionResult Add_tocart(product pr, string qty, int id)
        {
            product p = db.products.Where(x => x.pro_id == id).SingleOrDefault();
            cart c = new cart();
            c.pro_id = p.pro_id;
            c.pro_name = p.pro_name;
            c.pro_price = p.pro_price;
            c.o_qty = Convert.ToInt32(qty);
            c.o_bil = c.pro_price * c.o_qty;


            if (TempData["cart"] == null)
            {
                li.Add(c);
                TempData["cart"] = li;
            }
            else
            {
                List<cart> li2 = TempData["cart"] as List<cart>;
                int flag = 0;
                foreach (var item in li2)
                {
                    if(item.pro_id==c.pro_id)
                    {
                        item.o_qty += c.o_qty;
                        item.o_bil += c.o_bil;
                        flag = 1;

                    }
                }
                if(flag==0)
                {
                    li2.Add(c);
                }

                TempData["cart"] = li2;

            }
            TempData.Keep();


            return RedirectToAction("Index");
        }

        public ActionResult checkout()
        {

            TempData.Keep();


            return View();
        }




    }
}