using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.API.Controllers
{
	[Route("api/authors/{authorId}/books")]
	public class BooksController : Controller
	{
		private ILibraryRepository _libraryRepo;
		private ILogger<BooksController> _logger;
		private IUrlHelper _urlHelper;

		public BooksController(ILibraryRepository libraryRepository, ILogger<BooksController> logger, IUrlHelper urlHelper)
		{
			_libraryRepo = libraryRepository;
			_logger = logger;
			_urlHelper = urlHelper;
		}

		[HttpGet(Name = "GetBooksForAuthor")]
		public IActionResult GetBooksForAuthor(Guid authorId)
		{
			if (!_libraryRepo.AuthorExists(authorId))
				return NotFound();

			var authorBooksFromRepo = _libraryRepo.GetBooksForAuthor(authorId);
			var authorBooks = Mapper.Map<IEnumerable<BookDto>>(authorBooksFromRepo);

			authorBooks = authorBooks.Select(book =>
			{
				book = CreateLinksForBook(book);
				return book;
			});

			var wrapper = new LinkedCollectionResourceWrapperDto<BookDto>(authorBooks);

			return Ok(CreateLinksForBooks(wrapper));
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
			return Ok(CreateLinksForBook(book));
		}

		[HttpPost(Name = "CreateBookForAuthor")]
		public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
		{
			if (book == null)
				return BadRequest();

			if (book.Description == book.Title)
				ModelState.AddModelError(nameof(BookForCreationDto), "The provided description should be different from the title.");

			if (!ModelState.IsValid)
				return new UnprocessableEntityObjectResult(ModelState);

			if (!_libraryRepo.AuthorExists(authorId))
				return NotFound();

			var bookEntity = Mapper.Map<Book>(book);
			_libraryRepo.AddBookForAuthor(authorId, bookEntity);

			if (!_libraryRepo.Save())
				throw new Exception($"Creating a book for author {authorId} failed on save.");

			var bookToReturn = Mapper.Map<BookDto>(bookEntity);
			return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, CreateLinksForBook(bookToReturn));
		}

		[HttpDelete("{id}", Name = "DeleteBookForAuthor")]
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

			_logger.LogInformation(100, $"Book {id} for author {authorId} was deleted.");
			return NoContent();
		}

		[HttpPut("{id}", Name = "UpdateBookForAuthor")]
		public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto book)
		{
			if (book == null)
				return BadRequest();

			if (book.Description == book.Title)
				ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description should be different from the title.");

			if (!ModelState.IsValid)
				return new UnprocessableEntityObjectResult(ModelState);

			if (!_libraryRepo.AuthorExists(authorId))
				return NotFound();

			var bookFromAuthorFromRepo = _libraryRepo.GetBookForAuthor(authorId, id);
			if (bookFromAuthorFromRepo == null)
			{
				var bookToAdd = Mapper.Map<Book>(book);
				bookToAdd.Id = id;
				_libraryRepo.AddBookForAuthor(authorId, bookToAdd);
				if (!_libraryRepo.Save())
					throw new Exception($"Upserting book {id} for author {authorId} failed to save.");
				var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
				return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
			}

			Mapper.Map(book, bookFromAuthorFromRepo);
			_libraryRepo.UpdateBookForAuthor(bookFromAuthorFromRepo);
			if (!_libraryRepo.Save())
				throw new Exception($"Updating book {id} for author {authorId} failed on save.");
			return NoContent();
		}

		[HttpPatch("{id}", Name = "PartiallyUpdateBookForAuthor")]
		public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id, [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
		{
			if (patchDoc == null)
				return BadRequest();

			if (!_libraryRepo.AuthorExists(authorId))
				return NotFound();

			var bookFromAuthorFromRepo = _libraryRepo.GetBookForAuthor(authorId, id);
			if (bookFromAuthorFromRepo == null)
			{
				var bookDto = new BookForUpdateDto();
				patchDoc.ApplyTo(bookDto, ModelState);

				if (bookDto.Description == bookDto.Title)
					ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description should be different from the title.");
				TryValidateModel(bookDto);
				if (!ModelState.IsValid)
					return new UnprocessableEntityObjectResult(ModelState);

				var bookToAdd = Mapper.Map<Book>(bookDto);
				bookToAdd.Id = id;

				_libraryRepo.AddBookForAuthor(authorId, bookToAdd);
				if (!_libraryRepo.Save())
					throw new Exception($"Upserting book {id} for author {authorId} failed on save.");

				var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
				return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
			}

			var bookToPatch = Mapper.Map<BookForUpdateDto>(bookFromAuthorFromRepo);
			patchDoc.ApplyTo(bookToPatch, ModelState);

			if (bookToPatch.Description == bookToPatch.Title)
				ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description should be different from the title.");

			TryValidateModel(bookToPatch);

			if (!ModelState.IsValid)
				return new UnprocessableEntityObjectResult(ModelState);

			Mapper.Map(bookToPatch, bookFromAuthorFromRepo);
			_libraryRepo.UpdateBookForAuthor(bookFromAuthorFromRepo);
			if (!_libraryRepo.Save())
				throw new Exception($"Patching book {id} for author {authorId}");

			return NoContent();
		}

		private BookDto CreateLinksForBook(BookDto book)
		{
			book.Links.Add(new LinkDto(_urlHelper.Link("GetBookForAuthor", new { id = book.Id }), "self", "GET"));
			book.Links.Add(new LinkDto(_urlHelper.Link("DeleteBookForAuthor", new { id = book.Id }), "delete_book", "DELETE"));
			book.Links.Add(new LinkDto(_urlHelper.Link("UpdateBookForAuthor", new { id = book.Id }), "update_book", "PUT"));
			book.Links.Add(new LinkDto(_urlHelper.Link("PartiallyUpdateBookForAuthor", new { id = book.Id }), "partially_update_book", "PATCH"));
			return book;
		}

		private LinkedCollectionResourceWrapperDto<BookDto> CreateLinksForBooks(LinkedCollectionResourceWrapperDto<BookDto> booksWrapper)
		{
			booksWrapper.Links.Add(new LinkDto(_urlHelper.Link("GetBooksForAuthor", new { }), "self", "GET"));
			return booksWrapper;
		}
	}
}
