using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using AutoMapper;

namespace Library.API.Controllers
{
	[Route("api/authors")]
	public class AuthorsController : Controller
	{
		private ILibraryRepository _libraryRepo;

		public AuthorsController(ILibraryRepository libraryRepository)
		{
			_libraryRepo = libraryRepository;
		}

		[HttpGet]
		public IActionResult GetAuthors()
		{
			var authorsFromRepo = _libraryRepo.GetAuthors();
			var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
			return Ok(authors);
		}

		[HttpGet("{id}")]
		public IActionResult GetAuthor(Guid id)
		{
			var authorFromRepo = _libraryRepo.GetAuthor(id);
			if (authorFromRepo == null)
				return NotFound();

			var author = Mapper.Map<AuthorDto>(authorFromRepo);
			return Ok(author);
		}
	}
}
