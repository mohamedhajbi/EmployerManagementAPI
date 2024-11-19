﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Movie
    {

        public int Id { get; set; }

        [MaxLength(250)]

        public string? Title { get; set; }
        public int? Year { get; set; }
        public double? Rate { get; set; }

        [MaxLength(2500)]

        public string? Storeline { get; set; }
        public byte[]? Poster { get; set; }
        public byte? GenreId { get; set; }
        public Genre? Genre { get; set; }
    }
}
