﻿
using Capstone.Web.Models;
using Capstone.Web.Models.ViewModels;
using Capstone.Web.DALs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Cryptography;


namespace Capstone.Web.Controllers
{
    public class HomeController : Controller
    {
        private GolfSqlDal dal;

        public HomeController(GolfSqlDal dal)
        {
            this.dal = dal;
        }

        public HomeController()
        {

        }

        public ActionResult Index()
        {
            return View("Index");
        }

        [ChildActionOnly]
        public ActionResult Navigation()
        {
            if (Session[SessionKeys.Username] == null)
            {
                return PartialView("_NavAnon");
            }
            else
            {
                return PartialView("_NavAuth");
            }
        }

        [ChildActionOnly]
        public ActionResult NavAdmin()
        {
            if ((bool)Session[SessionKeys.IsAdmin])
            {
                return PartialView("_NavAdmin");
            }
            else
            {
                return null;
            }

        }

        public ActionResult DashboardContent()
        {
            if (Session[SessionKeys.Username] == null)
            {
                return PartialView("_Splash");
            }
            else
            {
                DashboardRobot dashBot = null;
                if (Session[SessionKeys.Username] != null)
                {
                    string username = Session[SessionKeys.Username] as string;
                    dashBot = new DashboardRobot(dal.GetUserProfile(username));
                }
                return PartialView("_UserDashboard", dashBot);
            }

        }

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index");
        }

        public ActionResult Login()
        {
            return View();
        }
       
        [HttpPost]
        public ActionResult PostUserLogin(Login model)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", model);
            }

            bool validCredentials = dal.ValidLogin(model);
            if (validCredentials)
            {
                UserRole userRole = dal.GetUserRole(model.Username);
                SetSession(userRole);
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("invalid-credentials", "Invalid login credentials");
                return View("Login", model);
            }

        }

        private void SetSession(UserRole model)
        {
            Session[SessionKeys.UserId] = model.Id;
            Session[SessionKeys.Username] = model.Username;
            Session[SessionKeys.IsAdmin] = model.IsAdministrator;
        }

        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PostUserRegistration(Registration model)
        {
            if (!ModelState.IsValid)
            {
                return View("Registration", model);
            }

            if (dal.GetUsername(model.UserName) != null)
            {
                ModelState.AddModelError("username-exists", "Username unavailable");
                return View("Registration", model);
            }
            else
            {
                dal.SaveUser(model);
                SetMessage("Registration successfull!", MessageType.Success);
            }

            return RedirectToAction("Login");
        }

        public ActionResult LeagueLeaderBoard(int leagueId)
        {
                List<LeaderboardUser> users = dal.GetLeagueUsers(leagueId);
                Course course = dal.GetCourseAssociatedWithLeague(leagueId);
                Leaderboard leaderboard = new Leaderboard
                {
                    Users = users,
                    course = course
                };

                return View("LeagueLeaderboard", leaderboard);
        }

        public ActionResult CreateMatch(int leagueId)
        {
            Match match = new Match()
            {
                LeagueId = leagueId
            };
            return View("CreateMatch", match);
        }

        [HttpPost]
        public ActionResult CreateMatch(Match match)
        {
            match.ID = dal.CreateMatch(match);
            bool matchCreated = (match.ID > -1);

            bool playersAdded = false;
            if (matchCreated)
            {
                playersAdded = dal.InitLeagueMatch(match);
            }

            if (playersAdded)
            {
                SetMessage("Match scheduled!", MessageType.Success);
            }
            else
            {
                SetMessage("There was an error scheduling your match.", MessageType.Error);
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult CreateLeague()
        {
            List<Course> courseList = dal.GetAllCourses();
            string user = Session[SessionKeys.Username].ToString();
            League league = new League
            {
                UserName = user,
                courses = courseList
            };
            return View("CreateLeague", league);
        }

        public ActionResult AddNewCourse()
        {
            return View("AddNewCourse");
        }

        public enum MessageType
        {
            Success,
            Warning,
            Error
        }

        public const string SUCCESS_MESSAGE_KEY = "postSuccessMessage.Text";
        public const string SUCCESS_MESSAGE_TYPE_KEY = "postSuccessMessage.Type";


        protected void SetMessage(string text, MessageType type = MessageType.Success)
        {
            TempData[SUCCESS_MESSAGE_KEY] = text;
            TempData[SUCCESS_MESSAGE_TYPE_KEY] = type;
        }

        [HttpPost]
        public ActionResult AddNewCourse(Course course)
        {
            dal.AddNewCourse(course);

            //Check that it was successfully added
            bool isSuccessful = true;

            //If successful:

            if (isSuccessful)
            {
                SetMessage("Course has been successfully added!", MessageType.Success);
            }
            else
            {
                SetMessage("There was an error adding your course!", MessageType.Error);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult CreateLeague(League league)
        {
            league.UserName = Session[SessionKeys.Username].ToString();

            UserAndLeague model = new UserAndLeague()
            {
                UserId = (int)Session[SessionKeys.UserId],
                LeagueId = -1
            };

            bool createSuccessful = dal.CreateLeague(league);

            bool addSuccessful = false;
            if (createSuccessful)
            {
                model.LeagueId = dal.GetLeagueId(league.Name);
                if (model.LeagueId > 0) //valid league id will be greater than 0
                {
                    addSuccessful = dal.JoinLeague(model);
                }
            }

            if (createSuccessful && addSuccessful)
            {
                SetMessage("League has been successfully created!", MessageType.Success);
            }
            else
            {
                SetMessage("There was an error creating your league!", MessageType.Error);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult LogMatchScore(LogMatch logMatch)
        {
            bool isSuccessful = dal.LogMatchScore(logMatch);

            if (isSuccessful)
            {
                SetMessage("Score has been successfully logged!", MessageType.Success);
            }
            else
            {
                SetMessage("There was an error logging your score!", MessageType.Error);
            }
            return RedirectToAction("Index", "Home");
        }

        [ChildActionOnly]
        public ActionResult NavLeagueOrg()
        {
            bool isOrg = false;
            try
            {
                isOrg = (bool)Session[SessionKeys.IsOrg];
            }
            catch
            {
                isOrg = false;
            }

            if (isOrg)
            {
                return PartialView("_NavLeagueOrg");
            }
            else
            {
                return null;
            }
        }

        public ActionResult LogMatch()
        {
            return View();
        }

        public ActionResult AddUsersToLeague(int leagueId)
        {
            TempData["LeagueId"] = leagueId;

            return View("AddUsersToLeague");
        }

        [HttpPost]
        public ActionResult AddUsersToLeague(AddUsersToLeague model)
        {
            int leagueId = (int)TempData["LeagueId"];

            UserProfile user = dal.GetUserProfile(model.Username);
            bool success = dal.AddUsersToLeague(user.Id, leagueId);

            if (success)
            {
                SetMessage("Golfer has been successfully added to league!", MessageType.Success);
            }
            else
            {
                SetMessage("There was an error adding your golfer! Please make sure the user has an active account, then try the username again.", MessageType.Error);
            }

            return RedirectToAction("Index", "Home");
        }

        public PartialViewResult UserHail()
        {
            if (Session[SessionKeys.Username] != null)
            {
                return PartialView("_Greeting", Session[SessionKeys.Username].ToString());
            }
            return null;
        }

        public ActionResult SelectMatch(int leagueId)
        {
            List<UserMatch> matches = dal.GetUnscoredMatches(leagueId);
            return View(matches);
        }

        [HttpPost]
        public ActionResult PostScore(UserMatch model, int score)
        {
            model.Score = score;
            bool scoreWritten = false;
            if (model.Score > 0)
            {
                scoreWritten = dal.UpdateUserMatchScore(model);
            }

            if (scoreWritten)
            {
                SetMessage("Score logged!", MessageType.Success);
            }
            else
            {
                SetMessage("There was an error updating the score.", MessageType.Error);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}