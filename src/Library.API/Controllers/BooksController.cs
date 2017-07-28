using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Library.API.Controllers
{
	[Route("api/authors/{authorId}/books")]
	public class BooksController : Controller
	{
		private ILibraryRepository _libraryRepo;

		public BooksController(ILibraryRepository libraryRepository)
		{
			_libraryRepo = libraryRepository;
		}

		[HttpGet()]
		public IActionResult GetBooksForAuthor(Guid authorId)
		{
			if (!_libraryRepo.AuthorExists(authorId))
				return NotFound();

			var authorBooksFromRepo = _libraryRepo.GetBooksForAuthor(authorId);
			var authorBooks = Mapper.Map<IEnumerable<BookDto>>(authorBooksFromRepo);
			return Ok(authorBooks);
		}

		[HttpGet("{id}")]
		public IActionResult GetBookForAuthor(Guid authorId, Guid id)
		{
			if (!_libraryRepo.AuthorExists(authorId))
				return NotFound();

			var authorBookFromRepo = _libraryRepo.GetBookForAuthor(authorId, id);
			if (authorBookFromRepo == null)
				return NotFound();

			var book = Mapper.Map<BookDto>(authorBookFromRepo);
			return Ok(book);
		}
	}
}
