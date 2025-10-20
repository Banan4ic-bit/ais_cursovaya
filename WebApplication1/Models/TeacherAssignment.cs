using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("teacher_assignments")]
public partial class TeacherAssignment
{
    [Key]
    [Column("assignment_id")]
    public int AssignmentId { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("teacher_id")]
    public int TeacherId { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("TeacherAssignments")]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey("TeacherId")]
    [InverseProperty("TeacherAssignments")]
    public virtual Teacher Teacher { get; set; } = null!;

    // ✅ Добавляем связь с заявками (TrainingRequests)
    [InverseProperty("Assignment")]
    public virtual ICollection<TrainingRequest> TrainingRequests { get; set; } = new List<TrainingRequest>();
}
