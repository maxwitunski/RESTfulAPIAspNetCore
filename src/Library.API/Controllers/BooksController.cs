﻿using AutoMapper;
using Library.API.Entities;
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

		[HttpGet("{id}", Name = "GetBookForAuthor")]
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

		[HttpPost]
		public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
		{
			if (book == null)
				return BadRequest();
			if (!_libraryRepo.AuthorExists(authorId))
				return NotFound();

			var bookEntity = Mapper.Map<Book>(book);
			_libraryRepo.AddBookForAuthor(authorId, bookEntity);

			if (!_libraryRepo.Save())
				throw new Exception($"Creating a book for author {authorId} failed on save.");

			var bookToReturn = Mapper.Map<BookDto>(bookEntity);
			return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
		}

		[HttpDelete("{id}")]
		public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
		{
			if (!_libraryRepo.AuthorExists(authorId))
				return NotFound();

			var bookFromAuthorFromRepo = _libraryRepo.GetBookForAuthor(authorId, id);
			if (bookFromAuthorFromRepo == null)
				return NotFound();

			_libraryRepo.DeleteBook(bookFromAuthorFromRepo);

			if (!_libraryRepo.Save())
				throw new Exception($"Deleting book {id} for author {authorId} failed on save.");

			return NoContent();
		}
	}
}
