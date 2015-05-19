﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
namespace team2project.Models
{
    public class CommentViewModel
    {
        public CommentViewModel(string eventId) 
        {
            Id = Guid.NewGuid().ToString();
            PostingTime = DateTime.Now;
            EventId = eventId;
        }

        [ScaffoldColumn(false)]
        public string Id { get; set; }

        [ScaffoldColumn(false)]
        public string EventId { get; set; }

        [StringLength(50)]
        [DataType(DataType.Text)]
        public string AuthorName { get; set; }

        [Required (ErrorMessage = "This field is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Lenght must be between 2 and 50")]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        [ScaffoldColumn(false)]
        public DateTime PostingTime { get; set; }

    }
}