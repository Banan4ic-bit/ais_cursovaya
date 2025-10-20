using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Keyless]
public partial class CourseScheduleView
{
    [Column("course_id")]
    public int? CourseId { get; set; }

    [Column("course_name")]
    [StringLength(255)]
    public string? CourseName { get; set; }

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("teacher_id")]
    public int? TeacherId { get; set; }

    [Column("teacher_name")]
    [StringLength(255)]
    public string? TeacherName { get; set; }

    [Column("participants_count")]
    public long? ParticipantsCount { get; set; }
}
