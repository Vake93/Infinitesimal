InfinitesimalWebApp.Run(args);

[HttpGet("/")] static string Greet() => "Hello World!";

[HttpGet("/{name}")] static string GreetWithName(string name) => $"Hello {name}!";