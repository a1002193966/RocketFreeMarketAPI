﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class CommentDTO
    {
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime CommentDate { get; set; }
    }
}