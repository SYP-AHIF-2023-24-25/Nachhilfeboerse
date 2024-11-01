﻿using Base.Core.Contracts;
using Base.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Assignments : EntityObject
    {
        [ForeignKey(nameof(ExerciseId))]
        public Exercise? Exercise { get; set; }
        public int ExerciseId { get; set; }
        public DateTime DateDue { get; set; }
        public List<User> Students { get; set; }

        public int TeacherId { get; set; }
        [ForeignKey(nameof(TeacherId))]
        public User? Teacher { get; set; }

        public string Name { get; set; }
    }
}
