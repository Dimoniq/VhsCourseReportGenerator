using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Core.BusinessLogic;
using Core.Models;
using Newtonsoft.Json;
using Selenium;

namespace Core
{
  internal class Program
  {
    private static void Main()
    {
      KillAllWebDrivers();

      var jsonConfig = File.ReadAllText("Configuration.json");
      var configs = JsonConvert.DeserializeObject<Configuration>(jsonConfig,
        new JsonSerializerSettings {DateFormatString = "dd.MM.yyyy"});

      var vhsService = new VhsService(new WebDriver());

      try
      {
        var logIn = vhsService.LogIn(configs.Credentials);
        if (!logIn)
        {
          Console.WriteLine("Press any key to exit.");
          Console.ReadKey();
          return;
        }

        var myCourses = vhsService.GetCourseNames();
        Console.WriteLine("Your courses are: ");
        myCourses.ForEach(c =>
        {
          Console.WriteLine($"{c.Name} - {string.Join(", ", c.Groups)}");
        });
      

        var reports = new List<CourseReport>();

        foreach (var myCourse in myCourses)
        {
          var reportsForCourse = vhsService.GetReportForCourse(myCourse, configs.Weeks);
          reports.AddRange(reportsForCourse);
          Console.WriteLine("");
        }

        var excelService = new ExcelService();
        excelService.SaveToExcel(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
          $"Report_{DateTime.Now:dd-MM-yyyy_HH-mm}.xlsx"), reports);
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        Console.WriteLine("Press ENTER key to exit.");
        Console.ReadKey();
      }
    }

    private static void KillAllWebDrivers()
    {
      foreach (var process in Process.GetProcessesByName("Chromedriver"))
      {
        process.Kill();
      }
    }
  }
}