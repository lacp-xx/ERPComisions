using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ERPComisions.Models;
using Model;

namespace ERPComisions.Controllers
{
    public class PlanCommissionsController : Controller
    {
        private ERPComisionsContext db = new ERPComisionsContext();

        // GET: PlanCommissions
        public ActionResult Index()
        {
            var planCommissions = db.PlanCommissions.Include(p => p.Carrier);
            return View(planCommissions.ToList());
        }

        // GET: PlanCommissions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanCommission planCommission = db.PlanCommissions.Find(id);
            if (planCommission == null)
            {
                return HttpNotFound();
            }
            return View(planCommission);
        }

        // GET: PlanCommissions/Create
        public ActionResult Create()
        {
            ViewBag.CarrierId = new SelectList(db.Carriers, "Id", "Name");
            return View();
        }

        // POST: PlanCommissions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,MinValue,MaxValue,PlanCommisionValue,DealerCommisionValue,StartDate,EndDate,PaymentCalculationType,CommissionType,CarrierId")] PlanCommission planCommission)
        {
            if (ModelState.IsValid)
            {
                db.PlanCommissions.Add(planCommission);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CarrierId = new SelectList(db.Carriers, "Id", "Name", planCommission.CarrierId);
            return View(planCommission);
        }

        // GET: PlanCommissions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanCommission planCommission = db.PlanCommissions.Find(id);
            if (planCommission == null)
            {
                return HttpNotFound();
            }
            ViewBag.CarrierId = new SelectList(db.Carriers, "Id", "Name", planCommission.CarrierId);
            return View(planCommission);
        }

        // POST: PlanCommissions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,MinValue,MaxValue,PlanCommisionValue,DealerCommisionValue,StartDate,EndDate,PaymentCalculationType,CommissionType,CarrierId")] PlanCommission planCommission)
        {
            if (ModelState.IsValid)
            {
                db.Entry(planCommission).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CarrierId = new SelectList(db.Carriers, "Id", "Name", planCommission.CarrierId);
            return View(planCommission);
        }

        // GET: PlanCommissions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanCommission planCommission = db.PlanCommissions.Find(id);
            if (planCommission == null)
            {
                return HttpNotFound();
            }
            return View(planCommission);
        }

        // POST: PlanCommissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PlanCommission planCommission = db.PlanCommissions.Find(id);
            db.PlanCommissions.Remove(planCommission);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
