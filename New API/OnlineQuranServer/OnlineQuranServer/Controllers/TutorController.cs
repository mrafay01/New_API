using Antlr.Runtime;
using OnlineQuranServer.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.WebSockets;

namespace OnlineQuranServer.Controllers
{
    public class TutorController : ApiController
    {

        QURAN_TUTOR_DBEntities3 db = new QURAN_TUTOR_DBEntities3();



        [HttpGet]
        public HttpResponseMessage UserLogin(string userName,string password)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

              var info = db.Teachers.FirstOrDefault(t=>t.username==userName&&t.password==password);
                if(info != null)
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { id= info.id, name= info.name,type="T" });
                }

                var pinfo = db.Parents.FirstOrDefault(t => t.username == userName && t.password == password);
                if (pinfo != null)
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { id = pinfo.id, name = pinfo.name, type = "P" });
                }

                var sinfo = db.Students.FirstOrDefault(t => t.username == userName && t.password == password);
                if (sinfo != null)
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { id = sinfo.id, name = sinfo.name, type = "S" });
                }

                return Request.CreateResponse(HttpStatusCode.NotFound, "Invalid Uername/Password");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        [HttpGet]
        public HttpResponseMessage AllTeachers()
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                var result = db.Teachers.ToList();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage AllTeachersQuery()
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                var result = db.Database.SqlQuery<Teacher>("select * from Teacher");

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage RegisterTeacher(Teacher tInfo)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                db.Teachers.Add(tInfo);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, tInfo);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage RegisterParent(Parent pInfo)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                db.Parents.Add(pInfo);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, pInfo);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage RegisterStudent(Student sInfo)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                db.Students.Add(sInfo);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, sInfo);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage AttachParentToStudent(string studentUserName,int parent_id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                var studentInfo = db.Students.First(s => s.username == studentUserName);
                studentInfo.parent_id = parent_id;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Attached");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        public HttpResponseMessage GetParentStudents(int parent_id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                var students = db.Students.Where(s => s.parent_id == parent_id).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, students);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        public HttpResponseMessage GetStudentCurrentEnrollments(int student_id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                var detail = db.Enrollments.Where(e => e.student_id == student_id && e.completed.Value == false)
                    .Join(db.EnrollmentSlots, e => e.id, es => es.enrollment_id, (e, es) => new {e.start_date,e.finish_date, es.slot_id, e.course_id })
                    .Join(db.Slots, es => es.slot_id, s => s.id, (es, s) => new
                    {
                        es.course_id,
                        CourseName = db.Courses.FirstOrDefault(c => c.id == es.course_id).name,
                        TeacherName = db.Teachers.FirstOrDefault(t => t.id == s.teacher_id).name,
                        s.from_time,
                        s.to_time,
                        s.Day,
                        es.start_date,
                        es.finish_date
                    }
                    );

                return Request.CreateResponse(HttpStatusCode.OK, detail);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        [HttpGet]
        public HttpResponseMessage GetStudentPastEnrollments(int student_id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                var detail = db.Enrollments.Where(e => e.student_id == student_id && e.completed.Value == true)
                    .Join(db.EnrollmentSlots, e => e.id, es => es.enrollment_id, (e, es) => new { e.start_date, e.finish_date, es.slot_id, e.course_id })
                    .Join(db.Slots, es => es.slot_id, s => s.id, (es, s) => new
                    {
                        es.course_id,
                        CourseName = db.Courses.First(c => c.id == es.course_id).name,
                        TeacherName = db.Teachers.First(t => t.id == s.teacher_id).name,
                        s.from_time,
                        s.to_time,
                        s.Day,
                        es.start_date,
                        es.finish_date
                    }
                    );

                return Request.CreateResponse(HttpStatusCode.OK, detail);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage AllCourses()
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                var result = db.Courses.ToList();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        public HttpResponseMessage AllTeachersCourseWise(int courseId)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                var result = db.TeacherCourses.Where(a => a.course_id == courseId)
                    .Join(db.Teachers, a => a.teacher_id, b => b.id, (a, b) => b).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        public HttpResponseMessage TeacherRegisterCourse(int teacherId, int courseId)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                var a = new TeacherCourse { course_id = courseId, teacher_id = teacherId };


                db.TeacherCourses.Add(a);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, a);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage ManageSingleSlot(Slot slot)
        {
            //if slot.IsActive is True, and exists in db, then make IsActive=True if false, Otherise will add to the db
            //if slot.IsActive is False, and exists in db. then make is False in db.

            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                var existingSlot = db.Slots.FirstOrDefault(s => s.from_time == slot.from_time && s.to_time == slot.to_time && s.teacher_id == slot.teacher_id && s.Day == slot.Day);

                if (existingSlot != null)
                {
                    if(existingSlot.IsBusy.Value && slot.IsActive.Value==false)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "The slot is current busy, can't Inactive.");
                    }

                    existingSlot.IsActive = slot.IsActive;
                }
                else if (slot.IsActive.Value)
                {
                    slot.IsBusy = false;
                    db.Slots.Add(slot);
                }
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, slot);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage ManageMultipleSlot(Slot[] slots)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                foreach (Slot slot in slots)
                {
                    ManageSingleSlot(slot);
                }

                return Request.CreateResponse(HttpStatusCode.OK, slots);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

       
        public bool DayCheck(int teacher_id, TimeSpan startTime, TimeSpan endTime, string day)
        {
            var slot = db.Slots.FirstOrDefault(s => s.from_time == startTime && s.to_time == endTime && s.teacher_id == teacher_id && s.Day == day);
            return slot != null;
        }

        /// <summary>
        /// This Function is only for teachers, to get their slots details.
        /// </summary>
        /// <param name="teacher_id"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTeacherSlots(int teacher_id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var days = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                var startTime = new TimeSpan(0, 0, 0);

                var records = new List<SlotEx>();

                for (int h = 0; h < 24; h++)
                {
                    var endTime = new TimeSpan(0, 0, 0);
                    if (h != 23)
                        endTime = startTime.Add(new TimeSpan(1, 0, 0));
                    var slotInfo = new SlotEx
                    {
                        timeSlot = $"{startTime.Hours.ToString("00")}:{startTime.Minutes.ToString("00")}-{endTime.Hours.ToString("00")}:{endTime.Minutes.ToString("00")}",
                        from_time = $"{startTime.Hours.ToString("00")}:{startTime.Minutes.ToString("00")}",
                        to_time = $"{endTime.Hours.ToString("00")}:{endTime.Minutes.ToString("00")}",
                        Mon = DayCheck(teacher_id, startTime, endTime, "Mon"),
                        Tue = DayCheck(teacher_id, startTime, endTime, "Tue"),
                        Wed = DayCheck(teacher_id, startTime, endTime, "Wed"),
                        Thu = DayCheck(teacher_id, startTime, endTime, "Thu"),
                        Fri = DayCheck(teacher_id, startTime, endTime, "Fri"),
                        Sat = DayCheck(teacher_id, startTime, endTime, "Sat"),
                        Sun = DayCheck(teacher_id, startTime, endTime, "Sun")
                    };
                    records.Add(slotInfo);
                    startTime = endTime;
                }

                return Request.CreateResponse(HttpStatusCode.OK, records);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        [HttpGet]
        public HttpResponseMessage SearchTeacherSlots(int course_id, int student_id, string from_time, string to_time,  string day)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var studentZone = db.Students.First(s => s.id == student_id).TimeZone.Value;
                var fromString = from_time.Split(new char[] { ':' });
                var toString = to_time.Split(new char[] { ':' });

                var from_time_search = TimeSpan.FromHours(Convert.ToInt32(fromString[0]));
                var to_time_search = TimeSpan.FromHours(Convert.ToInt32(toString[0]));
                if (studentZone < 0)
                {
                    from_time_search = from_time_search.Add(TimeSpan.FromHours(studentZone*-1));
                    to_time_search = to_time_search.Add(TimeSpan.FromHours(studentZone * -1));
                }
                else
                {
                    from_time_search = from_time_search.Subtract(TimeSpan.FromHours(studentZone));
                    to_time_search = to_time_search.Subtract(TimeSpan.FromHours(studentZone));
                }

                //first of all search for available slots
                var slotsToZoneShift = db.Slots.Where(s=>s.IsActive.Value && !s.IsBusy.Value).ToList().Join(db.Teachers, s => s.teacher_id, t => t.id, (s, t) => new Slot
                {
                    id = s.id,
                    from_time = t.TimeZone.Value < 0 ? s.from_time.Value.Add(new TimeSpan(t.TimeZone.Value * -1, 0, 0)) : s.from_time.Value.Subtract(new TimeSpan(t.TimeZone.Value, 0, 0)),
                    to_time = t.TimeZone.Value < 0 ? s.to_time.Value.Add(new TimeSpan(t.TimeZone.Value * -1, 0, 0)) : s.to_time.Value.Subtract(new TimeSpan(t.TimeZone.Value, 0, 0)),
                    teacher_id = s.teacher_id,
                    Day = s.Day
                }).ToList().Where(r => r.from_time.Value.Subtract(from_time_search).TotalHours == 0 && r.Day == day)
                .Select(s => new Slot {
                    id= s.id,
                    from_time = studentZone < 0 ? s.from_time.Value.Subtract(new TimeSpan(studentZone * -1, 0, 0)) : s.from_time.Value.Add(new TimeSpan(studentZone, 0, 0)),
                    to_time = studentZone < 0 ? s.to_time.Value.Subtract(new TimeSpan(studentZone * -1, 0, 0)) : s.to_time.Value.Add(new TimeSpan(studentZone, 0, 0)),
                    teacher_id = s.teacher_id,
                    Day = s.Day

                }).Join(db.TeacherCourses.Where(tc=>tc.course_id==course_id),s=>s.teacher_id,tc=>tc.teacher_id,(s,tc)=>s).ToList();



                var finalResult = db.Teachers.ToList().Join(slotsToZoneShift, t => t.id, s => s.teacher_id, (t, s) => new { 
                t.name,
                s.from_time,
                s.to_time,
                s.Day,
                s.id,
                teracher_id=t.id,
                t.ratings
                }).ToList();
                


                return Request.CreateResponse(HttpStatusCode.OK, finalResult);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage EnrollCourse(int slot_id,int student_id,int course_id, int teacher_id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                //first find the teacher_id agaist the slot.
                //add into enrollment table
                Enrollment enrollment = new Enrollment { course_id = course_id, student_id = student_id, start_date = DateTime.Now, teacher_id = teacher_id, completed = false };// (student_id, teacher_id, course_id, DateTime.Now, null, false);
                db.Enrollments.Add(enrollment);
                //add into the EnrollmentSlot table.

               // var enroll_id = db.Enrollments.First(e=>e.student_id == student_id && e.teacher_id == teacher_id && e.course_id == course_id).id;

                EnrollmentSlot enrollmentSlot = new EnrollmentSlot(enrollment.id, slot_id);
                db.EnrollmentSlots.Add(enrollmentSlot);
                // change the IsBusy to True in slot
                db.Slots.First(s=>s.id == slot_id).IsBusy=true;

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }






    }
}
