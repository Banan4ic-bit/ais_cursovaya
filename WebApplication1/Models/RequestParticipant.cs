using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("request_participants")]
public partial class RequestParticipant
{
    [Key]
    [Column("participant_id")]
    public int ParticipantId { get; set; }

    [Column("request_id")]
    public int RequestId { get; set; }

    [Column("full_name")]
    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [Column("position")]
    [StringLength(255)]
    public string Position { get; set; } = null!;

    [ForeignKey("RequestId")]
    [InverseProperty("RequestParticipants")]
    public virtual TrainingRequest Request { get; set; } = null!;
}
