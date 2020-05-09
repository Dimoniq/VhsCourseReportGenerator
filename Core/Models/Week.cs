using System;

namespace Core.Models
{
  public class Week
  {
    public string Name { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int ExercisesCompleted { get; set; }
    public Week Clone()
    {
      return new Week {Name = this.Name, Start = this.Start, End = this.End};
    }
  }
}