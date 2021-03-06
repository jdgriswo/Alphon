﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    /// <summary>
    /// Information about an enrollment
    /// </summary>
    public class Enrollment
    {
        public int Id { get; set; }
        public Account account { get; set; }
        public Section section { get; set; }
        public int status { get; set; }
        public DateTime dateadded { get; set; }
    }
}