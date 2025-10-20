using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("course_price_changes")]
public partial class CoursePriceChange
{
    [Key]
    [Column("doc_id")]
    public int DocId { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("doc_date")]
    public DateOnly DocDate { get; set; }

    [Column("price")]
    [Precision(10, 2)]
    public decimal Price { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("CoursePriceChanges")]
    public virtual Course Course { get; set; } = null!;
}
