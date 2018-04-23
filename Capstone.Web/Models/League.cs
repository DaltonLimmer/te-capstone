﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Capstone.Web.Models
{
    public class League
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<User> Users { get; set; }
    }
}