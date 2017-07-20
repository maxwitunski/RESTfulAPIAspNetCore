using Library.API.Entities;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
			IEnumerable<Author> authorsFromRepo = _libraryRepo.GetAuthors();
			return new JsonResult(authorsFromRepo);
		}
    }
}
