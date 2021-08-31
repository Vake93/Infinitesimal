# Infinitesimal
Infinitesimal is a Source Generator that turns up the ASP.NET minimal APIs to 11

![](https://raw.githubusercontent.com/Vake93/Infinitesimal/main/img/Infinitesimal.png)

# How to use
- Create a new ASP.NET 6 Project

  ```dotnet new web```
- Add the Infinitesimal NuGet Package
 
  ```dotnet add package Infinitesimal --version 0.0.1```
  
- Update the ```Program.cs``` with
  ```C#
  InfinitesimalWebApp.Run(args);
  [HttpGet("/")] static string Greet() => "This app is Infinitesimal!";
  ```
- Run the project

  ```dotnet run```
  
  # Alternatively...
  Can also be used with Minimal APIs as follows:
  ![](https://raw.githubusercontent.com/Vake93/Infinitesimal/main/img/Minimal.png)