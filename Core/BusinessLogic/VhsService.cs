using System;
using System.Collections.Generic;
using System.Linq;
using Core.Models;
using OpenQA.Selenium;
using Selenium;

namespace Core.BusinessLogic
{
  public class VhsService
  {
    private readonly WebDriver webDriver;

    public VhsService(WebDriver webDriver)
    {
      this.webDriver = webDriver; 
    }

    public bool LogIn(Credentials credentials)
    {
      try
      {
        this.webDriver.NavigateTo("https://vhs-lernportal.de/");
        this.webDriver.TrySwitchToFrame("//iframe[@id='main_frame']");

        this.webDriver.Sleep(200);

        var loginPage = this.webDriver.GetClickableElement("//a[text()='Login']");
        loginPage.Click();
        this.webDriver.Sleep(200);

        var logInField = this.webDriver.GetVisibleElement("//input[@id='login_login']");
        logInField.SendKeys(credentials.Username);

        var passwordInField = this.webDriver.GetVisibleElement("//input[@id='login_password']");
        passwordInField.SendKeys(credentials.Password);

        var loginButton = this.webDriver.GetClickableElement("//input[@name='login_submit']");
        loginButton.Click();

        this.webDriver.Sleep(500);

        this.webDriver.GetVisibleElement("//span[text()='Meine Kurse im vhs-Lernportal' ]");
        Console.WriteLine("Login successful.");
        return true;
      }
      catch (Exception e)
      {
        Console.WriteLine("Login failed !!! Terminating the program.");
        return false;
      }
    }

    public List<SystemCourse> GetCourseNames()
    {
      Console.WriteLine("Reading course names");
      var courses = this.webDriver.GetVisibleElements("(//table[@class='table_list'])[1]//child::a").Select(c =>
        {
          return new SystemCourse {Name = c.Text, BaseUrl = c.GetAttribute("href")};
        }).ToList();

      foreach (var systemCourse in courses)
      {
        this.webDriver.NavigateTo(systemCourse.BaseUrl);
        this.webDriver.Sleep(1000);
        this.webDriver.TrySwitchToFrame("//iframe[@id='main_frame']");

        this.webDriver.Sleep(1000);
        var myCourses = this.webDriver.GetClickableElement("//ul[@id='vhs_navigation']/li/a[text()='Meine Kurse']");
        myCourses.Click();

        systemCourse.Groups = this.webDriver.GetVisibleElements("//table[@id='meine_kurse']//child::td[@class='right']//ul/li/a")
          .Select(g => g.Text).ToArray();
      }

      return courses;
    }

    public List<CourseReport> GetReportForCourse(SystemCourse systemCourse, Week[] weeks)
    {
      var reports = new List<CourseReport>();

      foreach (var group in systemCourse.Groups)
      {
        Console.WriteLine($"Generating report for the course '{systemCourse.Name}-{group}'");

        var report = new CourseReport($" {systemCourse.Name}-{group}");

        this.webDriver.NavigateTo(systemCourse.BaseUrl);
        this.webDriver.Sleep(300);
        this.webDriver.TrySwitchToFrame("//iframe[@id='main_frame']");
        this.webDriver.Sleep(300);

        var myCourses = this.webDriver.GetClickableElement("//ul[@id='vhs_navigation']/li/a[text()='Meine Kurse']");
        myCourses.Click();

        var course = this.webDriver.GetClickableElement($"//table[@id='meine_kurse']//a[text()='{group}']");
        course.Click();

        var progressOverview = this.webDriver.GetClickableElement("//a[text()='Lernende und Lernstände anzeigen']");
        progressOverview.Click();

        var students = this.webDriver.GetVisibleElements("//table[@class='table_list space']/tbody/child::tr");

        var tmpStudentList = new List<(Student, string)>();

        foreach (var studentRow in students)
        {
          var firstName = studentRow.FindElement(By.ClassName("c_vorname")).Text;
          var lastName = studentRow.FindElement(By.ClassName("c_nachname")).Text;
          var link = studentRow.FindElement(By.XPath(".//img[1]"));
          var url =
            $"https://{link.GetAttribute("src").Substring(8, 2)}.vhs-lernportal.de/wws/9.php#/wws/{link.GetAttribute("data-href")}";

          tmpStudentList.Add((new Student(firstName, lastName, weeks), url));
        }

        foreach (var studentWrapper in tmpStudentList)
        {
          var student = studentWrapper.Item1;
          Console.Write($"[{(tmpStudentList.IndexOf(studentWrapper) + 1) * 100 / tmpStudentList.Count } %] Generating the report for '{student.FirstName} {student.LastName}'  ");
          this.webDriver.NavigateTo(studentWrapper.Item2);
          this.webDriver.TrySwitchToFrame("//iframe[@id='main_frame']");
          this.webDriver.Sleep(500);

          var lectureRows = this.webDriver.GetVisibleElements("(//table[@class='table_list space'])[2]/tbody/child::tr");
          var lectureUrls = lectureRows
            .Where(lr => !string.IsNullOrWhiteSpace(lr.FindElement(By.XPath(".//td[2]")).Text))
            .Select(lr => lr.FindElement(By.XPath(".//a[1]")).GetAttribute("href")).ToList();

          var assignedExercisesUrl = this.webDriver.GetClickableElement("//a[text()='Zugewiesene Übungen']")
            .GetAttribute("href");

          var assignedCompletedExercisesUrl = this.webDriver
            .GetClickableElement("//a[text()='Abgeschlossene zugewiesene Übungen']")
            .GetAttribute("href");

          foreach (var lecture in lectureUrls)
          {
            this.webDriver.NavigateTo(lecture);
            this.webDriver.TrySwitchToFrame("//iframe[@id='main_frame']");
            this.webDriver.Sleep(500);
            var exercises = this.GetExercisesFromTable(3);
            this.UpdateStudentReport(ref student, exercises);
          }

          this.UpdateStudentReport(ref student, this.LoadExercises(assignedExercisesUrl));
          this.UpdateStudentReport(ref student, this.LoadExercises(assignedCompletedExercisesUrl));

          report.Students.Add(student);

          Console.WriteLine();
        }

        reports.Add(report);
      }

      return reports;
    }

    private List<DateTime> GetExercisesFromTable(int dateColumnIndex)
    {
      return this.webDriver
        .GetVisibleElements($"(//table[@class='table_list space'])[2]/tbody/child::tr/td[{dateColumnIndex}]")
        .Where(x => !string.IsNullOrEmpty(x.Text))
        .Select(x => DateTime.Parse(x.Text).Date)
        .ToList();
    }

    private void UpdateStudentReport(ref Student student, List<DateTime> exercises)
    {
      Console.Write(".");
      foreach (var week in student.Weeks)
      {
        week.ExercisesCompleted += exercises.Count(targetDate => week.Start <= targetDate && week.End >= targetDate);
      }
    }

    private List<DateTime> LoadExercises(string url)
    {
      this.webDriver.NavigateTo(url);
      this.webDriver.TrySwitchToFrame("//iframe[@id='main_frame']");
      this.webDriver.Sleep(500);
      return this.GetExercisesFromTable(4);
    }
  }
}