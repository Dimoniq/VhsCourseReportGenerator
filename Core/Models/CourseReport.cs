using System.Collections.Generic;

namespace Core.Models
{
  public class CourseReport
  {
    public CourseReport(string courseName)
    {
      this.CourseName = courseName;
      this.Students = new List<Student>();
    }

    public string CourseName { get; set; }
    public List<Student> Students { get; set; }
  }
}