# J3antovar.Hateoas-way

![Sciensoft.Hateoas Build Status](https://dev.azure.com/Sciensoft/Sciensoft/_apis/build/status/Sciensoft.Hateoas?branchName=master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Sciensoft.Hateoas&metric=alert_status)](https://sonarcloud.io/dashboard?id=Sciensoft.Hateoas)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Sciensoft.Hateoas&metric=security_rating)](https://sonarcloud.io/dashboard?id=Sciensoft.Hateoas)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=Sciensoft.Hateoas&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=Sciensoft.Hateoas)

A library to help you achieve HATEOAS using a fluent language and lambda expression for configuring your ASP.NET Core RESTful/Web APIs. Based on the REST application architecture style, Uniform Interface, constraint **Hypermedia As The Engine Of Application State (HATEOAS)**.

**The good thing is, there is no need to inheritance or additional code in your models or addition of extra result filters to support its functionality. They all come beautifully out of the box with `Sciensoft.Hateoas`**.

Sciensoft.Hateoas threats lambda as a first-class citizen, so your configuration starts with a lambda expression. This library DO NOT enforce <a href="https://rebrand.ly/restful-explained" target="_blank">REST constraints</a> or <a href="https://rebrand.ly/richardson-maturity-model" target="_blank">Richardson Maturity Level</a>, and this has to be done by you, Sciensoft.Hateoas helps you only with the implementation of HATEOAS in your resource.

Learn more about RESTful API <a href="https://restfulapi.net/" target="_blank">here</a> and Lambda Expressions <a href="https://rebrand.ly/dotnet-lambda-expressions" target="_blank">here</a>.

NOTE: This is a new fork based in the original model by Sciensoft.Hateoas 

## Get Started

Sciensoft.Hateoas gets installed using <a href="https://www.nuget.org/packages/Sciensoft.Hateoas/" target="_blank">Nuget</a> package manager.

```bash
Install-Package Sciensoft.Hateoas
```

Or dotnet CLI `dotnet add package Sciensoft.Hateoas`.

### Configuration

Using a fluent language, allows you to easily configure by adding the service to .NET Core dependency injection pipeline.

```csharp
public void ConfigureServices(IServiceCollection services)
{
  services
    .AddMvc()
    .AddLinks(policy =>
    {
      policy
        .AddPolicy<BookViewModel>(model =>
        {
          model
            .AddSelf(m => m.Id, "This is a GET self link.")
            .AddRoute(m => m.Id, BookController.UpdateBookById)
            .AddRoute(m => m.Id, BookController.DeleteBookById)
            .AddCollectionLevel(m=> m, BookController.CreateNewBook, method: HttpMethods.Post, message: "Creates new resource.") //Added by J3antov4r
			.AddCustomPath(m => m.Id, "Edit", method: HttpMethods.Post, message: "Edits resource")
            .AddCustomPath(m => $"/change/resource/state/?id={m.Id}", "ChangeResourceState", method: HttpMethods.Post, message: "Any operation in your resource.")
            .AddExternalUri(m => m.Id, "https://my-domain.com/api/books/", "Custom Domain External Link")
            .AddExternalUri(m => $"/search?q={m.Title}", "https://google.com", "Google Search External Links", message: "This will do a search on Google engine.");
        });
    });
}
```

Here is how your controller looks like, no additional injection or attribute decoration is required. Please check our [Sample Project](./samples/Sciensoft.Hateoas.WebSample) out!

```csharp
[Route("api/books")]
public class BookController : ControllerBase
{
    public const string UpdateBookById = nameof(UpdateBookById);
    public const string DeleteBookById = nameof(DeleteBookById);

    [HttpGet]
    public ActionResult<IEnumerable<BookViewModel>> Get()
    { /* Code omitted for simplicity */ }

    [HttpGet("{id:guid}")]
    public ActionResult<BookViewModel> Get(Guid id)
    { /* Code omitted for simplicity */ }

    [HttpPost]
    public IActionResult Post([FromBody] BookViewModel book)
    { /* Code omitted for simplicity */ }

    [HttpPut("{id:guid}", Name = UpdateBookById)]
    public IActionResult Put(Guid id, [FromBody] BookViewModel book)
    { /* Code omitted for simplicity */ }

    [HttpDelete("{id:guid}", Name = DeleteBookById)]
    public IActionResult Delte(Guid id)
    { /* Code omitted for simplicity */ }
}
```

**JSON Result:**

```json
{
    "data": {
        "Id": "8f46d29e-6c0d-4511-85e7-b1d7ae42934a",
        "Title": "The Girl Who Lived: A Thrilling Suspense Novel",
        "Author": "Christopher Greyson",
        "Tags": [
            "Fiction",
            "Crime",
            "Murder",
            "Thriller"
        ]
    },
    "links": [
        {
            "method": "GET",
            "uri": "http://localhost:6080/api/books/83389205-b1c9-4523-a3bb-85d7255546f9",
            "relation": "Self",
            "message": "This is a GET self link."
        },
        {
            "method": "PUT",
            "uri": "http://localhost:6080/api/books/83389205-b1c9-4523-a3bb-85d7255546f9",
            "relation": "UpdateBookById"
        },
        {
            "method": "DELETE",
            "uri": "http://localhost:6080/api/books/83389205-b1c9-4523-a3bb-85d7255546f9",
            "relation": "DeleteBookById"
        },
        {
            "method": "POST",
            "uri": "http://localhost:6080/api/books/83389205-b1c9-4523-a3bb-85d7255546f9",
            "relation": "Edit",
            "message": "Edits resource"
        },
        {
            "method": "POST",
            "uri": "http://localhost:6080/change/resource/state/?id=83389205-b1c9-4523-a3bb-85d7255546f9",
            "relation": "ChangeResourceState",
            "message": "Any operation in your resource."
        },
        {
            "method": "GET",
            "uri": "https://my-domain.com/api/books/83389205-b1c9-4523-a3bb-85d7255546f9",
            "relation": "Custom Domain External Link"
        },
        {
            "method": "GET",
            "uri": "https://google.com/search?q=The Girl Beneath the Sea (Underwater Investigation Unit Book 1)",
            "relation": "Google Search External Links",
            "message": "This will do a search on Google engine."
        }
    ]
}
```

## Original Features

- Collections result with links,
- Json.NET and System.Text.Json settings support,
- Self-link generation,
- Named Route link generation,
- Custom link generation with support to path override,
- External links configuration,
- Configuration with Lambda Expression,
- Attribute Routing support, and
- Conventional Routing support.

## J3antov4r Features

- Now original model object are into "data" json property
- Added new Policy to add Links at Collection Level

### Roadmap

- Add support for extending link generation.
- Add support to bypass model link generation.
- Add support to .NET Authorization.
- Add support to Content Negotiation type in the read-model.

## Contributions

Before start contributing, check our [CONTRIBUTING] guideline out, also, before doing any significant change, have a look at the existing Issues and Pull Requests, one of them may be tackling the same thing.

## Issues

To open an issue or even suggest a new feature, please use the [Issues] tab.

## License

Copyright 2019 Sciensoft

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at [http://www.apache.org/licenses/LICENSE-2.0](http://www.apache.org/licenses/LICENSE-2.0).

[## LINKS ##]: ----------------------------------------------------------

[Learn-RestfulApi]:https://restfulapi.net/
[Lambda-Expressions]:https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/lambda-expressions
[Richardson-Maturity-Level]:https://martinfowler.com/articles/richardsonMaturityModel.html
[REST-Constraints]:https://medium.com/extend/what-is-rest-a-simple-explanation-for-beginners-part-2-rest-constraints-129a4b69a582
[CONTRIBUTING]: ./CONTRIBUTING.md
[Issues]: ./../../../issues
