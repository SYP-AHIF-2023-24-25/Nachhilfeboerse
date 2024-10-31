﻿using Base.Core.Entities;
using Core.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Exercise : EntityObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Language Language { get; set; }
        public List<Tag> Tags { get; set; }

        
        public int TeacherId { get; set; }
        [ForeignKey(nameof(TeacherId))]
        public User? Teacher { get; set; }
        

        public ArrayOfSnippets ArrayOfSnippets { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
