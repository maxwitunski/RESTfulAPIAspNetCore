using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
using Microsoft.AspNetCore.Http;

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

		[HttpGet("{id}", Name = "GetAuthor")]
		public IActionResult GetAuthor(Guid id)
		{
			var authorFromRepo = _libraryRepo.GetAuthor(id);
			if (authorFromRepo == null)
				return NotFound();

			var author = Mapper.Map<AuthorDto>(authorFromRepo);
			return Ok(author);
		}

		[HttpPost]
		public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
		{
			if (author == null)
				return BadRequest();

			var authorEntity = Mapper.Map<Author>(author);
			_libraryRepo.AddAuthor(authorEntity);
			if (!_libraryRepo.Save())
				throw new Exception("Creating an author failed on save.");

			var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);
			return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id }, authorToReturn);
		}

		[HttpPost("{id}")]
		public IActionResult BlockAuthorCreation(Guid id)
		{
			if (_libraryRepo.AuthorExists(id))
				return new StatusCodeResult(StatusCodes.Status409Conflict);
			return NotFound();
		}

		[HttpDelete("{id}")]
		public IActionResult DeleteAuthor(Guid id)
		{
			var authorFromRepo = _libraryRepo.GetAuthor(id);
			if (authorFromRepo == null)
				return NotFound();

			_libraryRepo.DeleteAuthor(authorFromRepo);
			if (!_libraryRepo.Save())
				throw new Exception($"Deleting author {id} failed on save.");

			return NoContent();
		}
	}
}
