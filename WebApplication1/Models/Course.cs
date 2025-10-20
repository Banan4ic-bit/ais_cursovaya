using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("courses")]
public partial class Course
{
    [Key]
    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("type")]
    [StringLength(100)]
    public string Type { get; set; } = null!;

    [Column("days")]
    public int Days { get; set; }

    [Column("capacity")]
    public int? Capacity { get; set; }

    [Column("price")]
    [Precision(10, 2)]
    public decimal Price { get; set; }

    [Column("price_with_vat")]
    [Precision(10, 2)]
    public decimal PriceWithVat { get; set; }

    [InverseProperty("Course")]
    public virtual ICollection<CoursePriceChange> CoursePriceChanges { get; set; } = new List<CoursePriceChange>();

    [InverseProperty("Course")]
    public virtual ICollection<TeacherAssignment> TeacherAssignments { get; set; } = new List<TeacherAssignment>();

    [InverseProperty("Course")]
    public virtual ICollection<TrainingRequest> TrainingRequests { get; set; } = new List<TrainingRequest>();
}
