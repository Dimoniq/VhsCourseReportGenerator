using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Models;
using OfficeOpenXml;

namespace Core.BusinessLogic
{
  public class ExcelService
  {
    public void SaveToExcel(string path, List<CourseReport> report)
    {
      if (!report.Any())
      {
        Console.WriteLine("Nothing to save.");
        return;
      }

      using (var p = new ExcelPackage())
      {
        foreach (var courseReport in report)
        {
          var ws = p.Workbook.Worksheets.Add(courseReport.CourseName);
          ws.Cells[1, 1].Value = "Vorname";
          ws.Cells[1, 1].Style.Font.Bold = true;

          ws.Cells[1, 2].Value = "Nachname";
          ws.Cells[1, 2].Style.Font.Bold = true;

          for (var i = 0; i < courseReport.Students.Count; i++)
          {
            var student = courseReport.Students[i];
            ws.Cells[i + 2, 1].Value = student.FirstName;
            ws.Cells[i + 2, 2].Value = student.LastName;
            for (var week = 0; week < student.Weeks.Count; week++)
            {
              ws.Cells[i + 2, week + 3].Value = student.Weeks[week].ExercisesCompleted;
              if (i == 0)
              {
                ws.Cells[1, week + 3].Value = student.Weeks[week].Name;
                ws.Cells[1, week + 3].Style.Font.Bold = true;
              }
            }
          }
          for (int i = 1; i < 20; i++)
          {
            ws.Column(i).AutoFit();
          }
        }

        p.SaveAs(new FileInfo(path));
        Console.WriteLine($"Results have been saved to {path}");
      }
    }
  }
}