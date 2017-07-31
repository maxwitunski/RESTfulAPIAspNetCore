using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
using Microsoft.AspNetCore.Http;
using Library.API.Helpers;

namespace Library.API.Controllers
{
	[Route("api/authors")]
	public class AuthorsController : Controller
	{
		private ILibraryRepository _libraryRepo;
		private IUrlHelper _urlHelper;
		private IPropertyMappingService _propertyMappingService;
		private ITypeHelperService _typeHelperService;

		public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper, IPropertyMappingService propertyMappingService, ITypeHelperService typeHelperService)
		{
			_libraryRepo = libraryRepository;
			_urlHelper = urlHelper;
			_propertyMappingService = propertyMappingService;
			_typeHelperService = typeHelperService;
		}

		[HttpGet(Name = "GetAuthors")]
		public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters)
		{
			if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
				return BadRequest();

			if (!_typeHelperService.TypeHasProperties<AuthorDto>(authorsResourceParameters.Fields))
				return BadRequest();

			var authorsFromRepo = _libraryRepo.GetAuthors(authorsResourceParameters);

			var previousPageLink = authorsFromRepo.HasPrevious ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage) : null;
			var nextPageLink = authorsFromRepo.HasNext ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage) : null;
			var paginationMetadata = new
			{
				totalCount = authorsFromRepo.TotalCount,
				pageSize = authorsFromRepo.PageSize,
				currentPage = authorsFromRepo.CurrentPage,
				totalPages = authorsFromRepo.TotalPages,
				previousPageLink = previousPageLink,
				nextPageLink = nextPageLink
			};
			Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

			var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
			return Ok(authors.ShapeData(authorsResourceParameters.Fields));
		}

		private string CreateAuthorsResourceUri(AuthorsResourceParameters authorsResourceParameters, ResourceUriType type)
		{
			switch (type)
			{
				case ResourceUriType.PreviousPage:
					return _urlHelper.Link("GetAuthors",
						new
						{
							fields = authorsResourceParameters.Fields,
							orderBy = authorsResourceParameters.OrderBy,
							searchQuery = authorsResourceParameters.SearchQuery,
							genre = authorsResourceParameters.Genre,
							pageNumber = authorsResourceParameters.PageNumber - 1,
							pageSize = authorsResourceParameters.PageSize
						});
				case ResourceUriType.NextPage:
					return _urlHelper.Link("GetAuthors",
						new
						{
							fields = authorsResourceParameters.Fields,
							orderBy = authorsResourceParameters.OrderBy,
							searchQuery = authorsResourceParameters.SearchQuery,
							genre = authorsResourceParameters.Genre,
							pageNumber = authorsResourceParameters.PageNumber + 1,
							pageSize = authorsResourceParameters.PageSize
						});
				default:
					return _urlHelper.Link("GetAuthors",
						new
						{
							fields = authorsResourceParameters.Fields,
							orderBy = authorsResourceParameters.OrderBy,
							searchQuery = authorsResourceParameters.SearchQuery,
							genre = authorsResourceParameters.Genre,
							pageNumber = authorsResourceParameters.PageNumber,
							pageSize = authorsResourceParameters.PageSize
						});
			}

		}

		[HttpGet("{id}", Name = "GetAuthor")]
		public IActionResult GetAuthor(Guid id, [FromQuery] string fields)
		{
			if (!_typeHelperService.TypeHasProperties<AuthorDto>(fields))
				return BadRequest();
			var authorFromRepo = _libraryRepo.GetAuthor(id);
			if (authorFromRepo == null)
				return NotFound();

			var author = Mapper.Map<AuthorDto>(authorFromRepo);
			return Ok(author.ShapeData(fields));
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
