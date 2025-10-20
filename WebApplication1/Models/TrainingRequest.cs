using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("training_requests")]
public partial class TrainingRequest
{
    [Key]
    [Column("request_id")]
    public int RequestId { get; set; }

    [Column("org_id")]
    public int OrgId { get; set; }

    [Column("assignment_id")]
    public int AssignmentId { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("request_date")]
    public DateOnly RequestDate { get; set; }

    [Column("number_of_people")]
    public int NumberOfPeople { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("TrainingRequests")]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey("OrgId")]
    [InverseProperty("TrainingRequests")]
    public virtual Organization Org { get; set; } = null!;

    [ForeignKey("AssignmentId")]
    [InverseProperty("TrainingRequests")]
    public virtual TeacherAssignment Assignment { get; set; } = null!;

    [InverseProperty("Request")]
    public virtual ICollection<RequestParticipant> RequestParticipants { get; set; } = new List<RequestParticipant>();
}
