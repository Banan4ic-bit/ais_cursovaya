using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("teachers")]
public partial class Teacher
{
    [Key]
    [Column("teacher_id")]
    public int TeacherId { get; set; }

    [Column("full_name")]
    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [Column("dob")]
    public DateOnly Dob { get; set; }

    [Column("gender")]
    [StringLength(10)]
    public string Gender { get; set; } = null!;

    [Column("education")]
    [StringLength(255)]
    public string Education { get; set; } = null!;

    [Column("category")]
    [StringLength(50)]
    public string Category { get; set; } = null!;

    [Column("is_archived")]
    public bool IsArchived { get; set; } = false;


    [InverseProperty("Teacher")]
    public virtual ICollection<TeacherAssignment> TeacherAssignments { get; set; } = new List<TeacherAssignment>();
}
