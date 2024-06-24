using Microsoft.AspNetCore.Mvc;
using Sciensoft.Hateoas.WebSample.Models;
using Sciensoft.Hateoas.WebSample.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sciensoft.Hateoas.WebSample.Controllers
{
	[Route("api/books")]
	public class BookController : ControllerBase
	{
		public const string UpdateBookById = nameof(UpdateBookById);
		public const string DeleteBookById = nameof(DeleteBookById);
		public const string CreateNewBook = nameof(CreateNewBook);

		[HttpGet]
		public ActionResult<IEnumerable<BookViewModel>> Get()
			=> Ok(InMemoryBookCollection.Books.Select(b => b.Value));

		[HttpGet("{id:guid}")]
		public ActionResult<BookViewModel> Get(Guid id)
		{
			var model = InMemoryBookCollection.Books.FirstOrDefault(x => x.Key.Equals(id));

			if (model.Value == null)
			{
				return NotFound(id);
			}

			return model.Value;
		}

		[HttpPost(Name = CreateNewBook)]
		public IActionResult Post([FromBody] BookViewModel book)
		{
			Debug.Assert(book != null);

			if (!InMemoryBookCollection.Books.TryAdd(book.Id, book))
			{
				throw new InvalidOperationException($"Book with Id '{book.Id}' already exists. Try PUT operation to update the item.");
			}

			return CreatedAtAction(nameof(Get), book.Id);
		}

		[HttpPut("{id:guid}", Name = UpdateBookById)]
		public IActionResult Put(Guid id, [FromBody] BookViewModel book)
		{
			Debug.Assert(book != null);

			book.Id = id;
			var model = InMemoryBookCollection.Books.FirstOrDefault(x => x.Key.Equals(id));

			if (model.Value == null)
			{
				return NotFound(id);
			}

			if (!InMemoryBookCollection.Books.TryAdd(id, book))
			{
				InMemoryBookCollection.Books[id] = book;
			}

			return CreatedAtAction(nameof(Get), id);
		}

		[HttpDelete("{id:guid}", Name = DeleteBookById)]
		public IActionResult Delte(Guid id)
		{
			var model = InMemoryBookCollection.Books.FirstOrDefault(x => x.Key.Equals(id));

			if (model.Value == null)
			{
				return NotFound();
			}

			InMemoryBookCollection.Books.Remove(id);

			return Ok();
		}
	}
}
