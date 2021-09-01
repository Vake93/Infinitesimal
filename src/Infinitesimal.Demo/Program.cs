var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapSwagger();
app.UseSwaggerUI();
app.MapFunctions();

app.Run();

[HttpGet("/")]
static string Greet() => "Hello World!";

[HttpGet("/{name}")]
static string GreetWithName(string name) => $"Hello {name}!";

[HttpGet("/contacts")]
static IEnumerable<Contact> ListContacts() => ContactStore.Data;

[HttpGet("/contacts/{id}")]
static IResult GetContact(Guid id) 
    => ContactStore.Data.FirstOrDefault(c => c.Id == id) is Contact contact ? 
        Results.Ok(contact) : 
        Results.NotFound();

[HttpPost("/contacts")]
static IResult CreateContact(Contact contact)
{
    if (contact.Id == Guid.Empty)
        contact = contact with { Id = Guid.NewGuid() };

    ContactStore.Data.Add(contact);
    return Results.Ok(contact);
}

[HttpPut("/contacts/{id}")]
static IResult UpdateContact(Guid id, Contact contact)
{
    contact = contact with { Id = id };

    var oldContact = ContactStore.Data.FirstOrDefault(c => c.Id == id);

    if (oldContact is null)
        return Results.NotFound();

    ContactStore.Data.Remove(oldContact);
    ContactStore.Data.Add(contact);

    return Results.Ok(contact);
}

[HttpDelete("/contacts/{id}")]
static IResult DeleteContact(Guid id)
{
    var oldContact = ContactStore.Data.FirstOrDefault(c => c.Id == id);

    if (oldContact is null)
        return Results.NotFound();

    ContactStore.Data.Remove(oldContact);

    return Results.Ok();
}

record Contact (Guid Id, string FirstName, string? LastName, string? Email, string? Mobile, string? Phone);

class ContactStore
{
    public static HashSet<Contact> Data { get; } = new ();
}