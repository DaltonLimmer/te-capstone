﻿using Capstone.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Capstone.Web.Models
{
    public class Leaderboard
    {
        public List<User> Users { get; set; }
        public Course course { get; set; }

    }
}