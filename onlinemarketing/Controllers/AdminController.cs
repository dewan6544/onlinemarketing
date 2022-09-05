using onlinemarketing.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
 using PagedList.Mvc;
using PagedList;

namespace onlinemarketing.Controllers
{
    public class AdminController : Controller
    {

        onlinemarketingEntities6 db = new onlinemarketingEntities6();

        // GET: Admin
        [OutputCache(Duration = 0)]
        public ActionResult Login()
        {
            if (isLoggedIn())
            {
                return RedirectToAction("Index", "User");
            }

            return View();
        }

        [HttpPost]

        public ActionResult Login(admin adm)
        {
            admin ad = db.admins.Where(x => x.ad_name == adm.ad_name && x.ad_password == adm.ad_password).SingleOrDefault();
            



            if (ad != null)
            {
                HttpCookie cookie = new HttpCookie("name");
                cookie.Values["name"] = ad.ad_name;

                HttpCookie role = new HttpCookie("access");
                role.Values["access"] = "admin";

                cookie.Expires = DateTime.Now.AddMinutes(10000);// update this later
                role.Expires = DateTime.Now.AddMinutes(10000);
                Response.Cookies.Add(cookie);
                Response.Cookies.Add(role);

                Session["ad_id"] = ad.ad_id.ToString();
                return RedirectToAction("Index","User");
            }
            else
            {
                ViewBag.error = "Invalid User Name or Password";
            }

            return View();
        }

        public ActionResult SignOut()
        {

            HttpCookie username = Request.Cookies["name"];
            username.Expires = DateTime.Now.AddDays(-1);
            HttpCookie role = Request.Cookies["access"];
            role.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(username);
            Response.Cookies.Add(role);

            return RedirectToAction("Index", "User");
        }


        public ActionResult Category()
        {
         
            return View();
        }
        [HttpPost]

        public ActionResult Category(category cat, HttpPostedFileBase imgfile)
        {

            admin ad = new admin();
            string path = uploadimage(imgfile);

            if (path.Equals("-1"))
            {
                ViewBag.error = "image could not be uploaded";

            }
            else
            {
                category ca = new category();
                ca.cat_name = cat.cat_name;
                ca.cat_image = path;
                ca.cat_status = 1;
                ca.ad_id_fk = Convert.ToInt32(Session["ad_id"].ToString());



              
               db.categories.Add(ca);


                db.SaveChanges();


                return RedirectToAction("ViewCategory");
            }


            return View();

        }

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

        public ActionResult ViewCategory(int? page)
        {
            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.categories.Where(x => x.cat_status == 1).OrderByDescending(x => x.cat_id).ToList();
            IPagedList<category> cate = list.ToPagedList(pageindex, pagesize);

            return View(cate);
        }
        public ActionResult CreateAdd()

        {

            return View();

        }

        public bool isLoggedIn()
        {
            HttpCookie access = Request.Cookies["access"];
            string role = access != null ? access.Value.Split('=')[1] : "undefined";

            if (role.Equals("undefined"))
            {
                return false;
            }

            return true;

        }

    }

}