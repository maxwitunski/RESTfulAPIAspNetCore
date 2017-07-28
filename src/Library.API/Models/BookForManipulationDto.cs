﻿using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
	public abstract class BookForManipulationDto
	{
		[Required(ErrorMessage = "You should fill out a title.")]
		[MaxLength(100, ErrorMessage = "The title shouldn't have more that 100 characters.")]
		public string Title { get; set; }

		[MaxLength(500, ErrorMessage = "The description should be no longer than 500 characters.")]
		public virtual string Description { get; set; }
	}
}
