﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Capstone.Web.Models;
using Capstone.Web.Models.ViewModels;
using System.Security.Cryptography;


namespace Capstone.Web.DALs.Implementation
{
    public class GolfSqlDalImp : GolfSqlDal
    {
        private readonly string getUserModelSql = @"select id, username, firstname, lastname, password, salt from users where (username = @username);";

        private readonly string connectionString;

        public GolfSqlDalImp(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool AddNewCourse(Course course)
        {
            bool isSuccessful = true;

            string SQL_AddNewCourse = @"Insert into courses (name, par, holeCount, totalLengthYards) 
            values (@name, @par, @holeCount, @totalLengthYards)";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(SQL_AddNewCourse, conn);

                    cmd.Parameters.Add(new SqlParameter("@name", course.Name));
                    cmd.Parameters.Add(new SqlParameter("@par", course.Par));
                    cmd.Parameters.Add(new SqlParameter("@holeCount", course.NumberOfHoles));
                    cmd.Parameters.Add(new SqlParameter("@totalLengthYards", course.LengthInYards));
                    cmd.ExecuteNonQuery();
                }
            }

            catch (SqlException e)
            {
                Console.WriteLine(e.Message);
                isSuccessful = false;
            }

            return isSuccessful;
        }

        public User GetUser(string username)
        {
            User user = new User();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(getUserModelSql, conn);
                cmd.Parameters.AddWithValue("@username", username);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    user = AssembleUser(reader);
                }
                conn.Close();
            }
            return user;
        }

        public User VerifyLogin(Login model)
        {
            User user = GetUser(model.Username);            
            Authenticator auth = new Authenticator(user.Salt, user.Password);
            if (auth.AssertValidPassword(model.Password) == false)
            {
                user = null;
            }

            return user;
        }

        public User GetUsername(string username)

        {
            User user = null;
            string getUsernameSql = @"select id, username, firstname, lastname from users where username = @username;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(getUsernameSql, conn);
                cmd.Parameters.AddWithValue("@username", username);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    user = AssembleUser(reader);
                }
                conn.Close();
            }
            return user;
        }

        public bool CreateMatch(Match match)
        {
            bool isSuccessful = true;

            string SQL_CreateMatch = @"Insert into matches (date, numOfPlayers) 
            values (@date, @numOfPlayers)";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(SQL_CreateMatch, conn);

                    cmd.Parameters.Add(new SqlParameter("@name", match.Reservation));
                    cmd.Parameters.Add(new SqlParameter("@numOfPlayers", match.NumberOfPlayers));
                    cmd.ExecuteNonQuery();
                }
            }

            catch (SqlException e)
            {
                Console.WriteLine(e.Message);
                isSuccessful = false;
            }

            return isSuccessful;
        }

        public bool SaveUser(Registration model)
        {
            bool registrationSuccess = false;
            string saveUserSql = @"insert into users (firstname, lastname, username, password, salt) values (@firstname, @lastname, @username, @password, @salt);";

            Authenticator auth = new Authenticator(model.Password);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(saveUserSql, conn);

                cmd.Parameters.AddWithValue("@firstname", model.FirstName);
                cmd.Parameters.AddWithValue("@lastname", model.LastName);
                cmd.Parameters.AddWithValue("@username", model.UserName);
                cmd.Parameters.AddWithValue("@password", auth.Hash);
                cmd.Parameters.AddWithValue("@salt", auth.Salt);

                int affectedRows = cmd.ExecuteNonQuery();
                if(affectedRows == 1)
                {
                    registrationSuccess = true;
                }
                conn.Close();
            }

            return registrationSuccess;
        }

        private User AssembleUser(SqlDataReader reader)
        {
            User user = new User()
            {
                Id = Convert.ToInt32(reader["id"]),
                Username = Convert.ToString(reader["username"]),
                FirstName = Convert.ToString(reader["firstname"]),
                LastName = Convert.ToString(reader["lastname"]),
                Password = Convert.ToString(reader["password"]),
                Salt = Convert.ToString(reader["salt"])
            };

            return user;
        }

        private class Authenticator
        {
            private static int length = 24;
            private static int saltSize = 24;
            private static int iterations = 100;

            public string Hash { get; }
            public string Salt { get; }

            public Authenticator(string password)
            {
                Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(password, saltSize, iterations);
                this.Hash = Convert.ToString(rfc.GetBytes(length));
                this.Salt = Convert.ToString(rfc.Salt);
            }

            public Authenticator(string dbSalt, string dbHash)
            {
                this.Salt = dbSalt;
                this.Hash = dbHash;
            }

            public bool AssertValidPassword(string password)
            {
                byte[] saltBytes = Encoding.UTF8.GetBytes(this.Salt);
                
                Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(password, saltBytes, iterations);

                string hash = Convert.ToString(rfc.GetBytes(length));
                return string.Equals(this.Hash, hash);
            }

            public bool AreTheseEqual()
            {
                bool verdict = false;

                string pass = "qwerty1234";

                Rfc2898DeriveBytes rfc01 = new Rfc2898DeriveBytes(pass, saltSize, iterations);

                Rfc2898DeriveBytes rfc02 = new Rfc2898DeriveBytes(pass, rfc01.Salt, iterations);

                string hash01 = Convert.ToBase64String(rfc01.GetBytes(length));
                string hash02 = Convert.ToBase64String(rfc02.GetBytes(length));
                verdict = string.Equals(hash01, hash02);

                return verdict;
            }
        }
    }
}