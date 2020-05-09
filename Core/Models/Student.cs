using System.Collections.Generic;

namespace Core.Models
{
  public class Student
  {
    public Student(string firstName, string lastName, IEnumerable<Week> weeks)
    {
      this.FirstName = firstName;
      this.LastName = lastName;
      this.Weeks = new List<Week>();
      foreach (var week in weeks)
      {
        this.Weeks.Add(week.Clone());
      }
    }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<Week> Weeks { get; set; }
  }
}