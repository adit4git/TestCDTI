using CandidateProject.EntityModels;
using CandidateProject.ViewModels;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CandidateProject.Controllers
{
    public class CartonController : Controller
    {

        private const string ErrMsgcd1 = "ERRMSG1";
        private const string ErrMsgcd2= "ERRMSG2";
        private const string SuccessMsgcd1 = "SCCD1";
        private const string ErrMsg1 = "This carton already has 10 items and is at capacity.";
        private const string ErrMsg2 = "This Equipment is already part of another carton";
        private const string SuccessMsg1 = "Equiptment removed successfully from Carton";
        private CartonContext db = new CartonContext();

        // GET: Carton
        public ActionResult Index()
        {
            var cartons = db.Cartons
                .Include(cd => cd.CartonDetails)
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber,
                    NoofEquipment = c.CartonDetails.Count,
                    AtCapacity = c.CartonDetails.Count == 10,
                   
                })
                .ToList();

            return View(cartons);
        }

        // GET: Carton/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // GET: Carton/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Carton/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CartonNumber")] Carton carton)
        {
            if (ModelState.IsValid)
            {
                db.Cartons.Add(carton);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(carton);
        }

        // GET: Carton/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // POST: Carton/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CartonNumber")] CartonViewModel cartonViewModel)
        {
            if (ModelState.IsValid)
            {
                var carton = db.Cartons.Find(cartonViewModel.Id);
                carton.CartonNumber = cartonViewModel.CartonNumber;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cartonViewModel);
        }

        // GET: Carton/Delete/5
        public ActionResult Delete(int? id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Carton carton = db.Cartons.Find(id);
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // POST: Carton/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //AM: TODO BUG1 = Allow for Deletion of cartons with Equipment by first clearing the equipments within carton, followed by deletion of carton - DONE
            //Carton carton = db.Cartons.Find(id)
            var carton = db.Cartons
                   .Include(c => c.CartonDetails)
                   .Where(c => c.Id == id)
                   .SingleOrDefault();
            if (carton == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);



            if (carton.CartonDetails != null && carton.CartonDetails.Count > 0)
                db.CartonDetails.RemoveRange(carton.CartonDetails);


            db.Cartons.Remove(carton);
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

        public ActionResult AddEquipment(int? id, string valMsg)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if(valMsg != null)
            {  
                if(valMsg == ErrMsgcd1)
                    ModelState.AddModelError("CartonNumber", ErrMsg1);
                if (valMsg == ErrMsgcd2)
                    ModelState.AddModelError("CartonNumber", ErrMsg2);
            }

        

            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonDetailsViewModel()
                {
                    CartonNumber = c.CartonNumber,
                    CartonId = c.Id
                })
                .SingleOrDefault();

           

                if (carton == null)
            {
                return HttpNotFound();
            }

            
            var equipment = db.Equipments
                .Where(e => !db.CartonDetails.Where(cd => cd.CartonId == id).Select(cd => cd.EquipmentId).Contains(e.Id))                
                .Select(e => new EquipmentViewModel()
                {
                    Id = e.Id,
                    ModelType = e.ModelType.TypeName,
                    SerialNumber = e.SerialNumber
                })
                .ToList();

            carton.Equipment = equipment;
            return View(carton);
        }

        public ActionResult AddEquipmentToCarton([Bind(Include = "CartonId,EquipmentId")] AddEquipmentViewModel addEquipmentViewModel)
        {
           
           
            if (ModelState.IsValid)
            {
                var carton = db.Cartons
                    .Include(c => c.CartonDetails)
                    .Where(c => c.Id == addEquipmentViewModel.CartonId)
                    .SingleOrDefault();
                if (carton == null)
                {
                    return HttpNotFound();
                }
                //AM: TODO BUG3 = DONT ALLOW USER TO ADD MORE THAN 10 Equipment to a carton - DONE
                //Check if carton is at capacity
                if (carton.CartonDetails.Count == 10)
                {
                    return RedirectToAction("AddEquipment", new { id = addEquipmentViewModel.CartonId, valMsg = ErrMsgcd1 });
                }

                var ExistingEquipment = db.CartonDetails                      
                    .Where(cd => cd.EquipmentId == addEquipmentViewModel.EquipmentId)
                    .SingleOrDefault();

                //AM: TODO BUG2 =  Equipment already part of any other Carton  - DONE         
                if (ExistingEquipment != null)
                {  
                    return RedirectToAction("AddEquipment", new { id = addEquipmentViewModel.CartonId, valMsg = ErrMsgcd2 });
                }

                var equipment = db.Equipments
                    .Where(e => e.Id == addEquipmentViewModel.EquipmentId)
                    .SingleOrDefault();
                if (equipment == null)
                {
                    return HttpNotFound();
                }
                var detail = new CartonDetail()
                {
                    Carton = carton,
                    Equipment = equipment
                };
                carton.CartonDetails.Add(detail);
                db.SaveChanges();
            }
            return RedirectToAction("AddEquipment", new { id = addEquipmentViewModel.CartonId });
        }

        public ActionResult ViewCartonEquipment(int? id, string valMSG)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if(valMSG!=null)
            {
                if(valMSG == SuccessMsgcd1)
                {
                    ModelState.AddModelError("CartonNumber", SuccessMsg1);
                }             
                   
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonDetailsViewModel()
                {
                    CartonNumber = c.CartonNumber,
                    CartonId = c.Id,
                    Equipment = c.CartonDetails
                        .Select(cd => new EquipmentViewModel()
                        {
                            Id = cd.EquipmentId,
                            ModelType = cd.Equipment.ModelType.TypeName,
                            SerialNumber = cd.Equipment.SerialNumber
                        })
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        public ActionResult RemoveEquipmentOnCarton([Bind(Include = "CartonId,EquipmentId")] RemoveEquipmentViewModel removeEquipmentViewModel)
        {
            //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //AM: TODO REMAININGDEV 1 =  Implement Remove function  - DONE
            if (ModelState.IsValid)
            {
                var carton = db.Cartons
                   .Include(x => x.CartonDetails)
                   .Where(x => x.Id == removeEquipmentViewModel.CartonId)
                   .FirstOrDefault();



                if (carton == null )
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);


              

                if (carton.CartonDetails == null || carton.CartonDetails.Count < 1)
                {
                    //No Equipment added yet
                    return RedirectToAction("Index");
                }

                var cartonDetail = carton.CartonDetails.FirstOrDefault(e => e.EquipmentId == removeEquipmentViewModel.EquipmentId);

                
                if (cartonDetail == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);


                db.CartonDetails.Remove(cartonDetail);
                db.SaveChanges();
            }
            return RedirectToAction("ViewCartonEquipment", new { id = removeEquipmentViewModel.CartonId, valMSG = SuccessMsgcd1 });
        }

        //AM: TODO ENHANCEMENTS 2 =  Implement Remove All function  - DONE
        public ActionResult RemoveAllEquipment(int? id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {

                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var carton = db.Cartons

                    .Include(x => x.CartonDetails)

                    .Where(x => x.Id == id)

                    .FirstOrDefault();



                if (carton == null)
                {

                    return HttpNotFound();
                }

                if (carton.CartonDetails == null || carton.CartonDetails.Count < 1)
                {
                    //No Equipment added yet
                    return RedirectToAction("Index");
                }



                db.CartonDetails.RemoveRange(carton.CartonDetails);

                db.SaveChanges();

           
            }
            return RedirectToAction("Index");
        }


    }
}
